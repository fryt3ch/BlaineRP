using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BlaineRP.Server.EntityData.Vehicles;
using BlaineRP.Server.Game.Estates;
using GTANetworkAPI;

namespace BlaineRP.Server.EntityData.Players
{
    public partial class PlayerData
    {
        /// <summary>Список наказаний игрока</summary>
        public List<Sync.Punishment> Punishments { get => Info.Punishments; set => Info.Punishments = value; }

        /// <summary>Банковский счёт игрока</summary>
        public Game.Bank.Account BankAccount { get => Info.BankAccount; set => Info.BankAccount = value; }

        /// <summary>Информация об игроке с момента последнего захода на сервер</summary>
        public LastPlayerData LastData { get => Info.LastData; set => Info.LastData = value; }

        /// <summary>Дата последнего захода игрока на сервер</summary>
        public DateTime LastJoinDate { get => Info.LastJoinDate; set => Info.LastJoinDate = value; }

        private Timer UpdateTimer { get; set; }

        /// <summary>Сущность, к которой прикреплен игрок</summary>
        public Entity IsAttachedToEntity => Player.GetEntityIsAttachedTo();

        /// <summary>Транспорт, находящийся в собственности у игрока</summary>
        public List<VehicleInfo> OwnedVehicles { get; set; }

        public List<Game.Estates.House> OwnedHouses { get; set; }

        public List<Game.Estates.Apartments> OwnedApartments { get; set; }

        public List<Game.Estates.Garage> OwnedGarages { get; set; }

        public List<Game.Businesses.Business> OwnedBusinesses { get; set; }

        public Game.Estates.HouseBase SettledHouseBase { get; set; }

        public VehicleData RentedVehicle => VehicleData.All.Values.Where(x => x.OwnerType == OwnerTypes.PlayerRent && x.OwnerID == CID).FirstOrDefault();

        public VehicleData RentedJobVehicle => VehicleData.All.Values.Where(x => x.OwnerType == OwnerTypes.PlayerRentJob && x.OwnerID == CID).FirstOrDefault();

        public Game.Jobs.Job CurrentJob { get => Game.Jobs.Job.Get(Player.GetData<int>("CJob")); set { if (value == null) Player.ResetData("CJob"); else Player.SetData("CJob", value.Id); } }

        /// <summary>Текущий контейнер, который смотрит игрок</summary>
        /// <value>UID контейнера, null - если отсутствует</value>
        public Game.Items.Container CurrentContainer { get => Player.GetData<Game.Items.Container>("CCont"); set { if (value == null) Player.ResetData("CCont"); else Player.SetData("CCont", value); } }

        public Game.Items.Craft.Workbench CurrentWorkbench { get => Player.GetData<Game.Items.Craft.Workbench>("CWBench"); set { if (value == null) Player.ResetData("CWBench"); else Player.SetData("CWBench", value); } }

        /// <summary>Текущий бизнес, с которым взаимодействует игрок</summary>
        public Game.Businesses.Business CurrentBusiness { get => Player.GetData<Game.Businesses.Business>("CBusiness"); set { if (value == null) Player.ResetData("CBusiness"); else Player.SetData("CBusiness", value); } }

        public Game.Estates.Garage CurrentGarage => Utils.GetGarageByDimension(Player.Dimension);

        public Game.Estates.HouseBase CurrentHouseBase => Utils.GetHouseBaseByDimension(Player.Dimension);

        public ApartmentsRoot CurrentApartmentsRoot => Utils.GetApartmentsRootByDimension(Player.Dimension);

        public int FreeVehicleSlots => (Properties.Settings.Static.MIN_VEHICLE_SLOTS + OwnedHouses.Where(x => x.GarageData != null).Select(x => x.GarageData.MaxVehicles).Sum() + OwnedGarages.Select(x => x.StyleData.MaxVehicles).Sum()) - OwnedVehicles.Count;

        public int FreeHouseSlots => Properties.Settings.Static.MAX_HOUSES - OwnedHouses.Count;
        public int FreeApartmentsSlots => Properties.Settings.Static.MAX_APARTMENTS - OwnedApartments.Count;
        public int FreeGaragesSlots => Properties.Settings.Static.MAX_GARAGES - OwnedGarages.Count;
        public int FreeBusinessesSlots => Properties.Settings.Static.MAX_BUSINESSES - OwnedBusinesses.Count;

        public Game.Data.Customization.UniformTypes CurrentUniform { get => Player.GetData<Game.Data.Customization.UniformTypes?>("CUNIF") ?? Game.Data.Customization.UniformTypes.None; set { if (value == Game.Data.Customization.UniformTypes.None) Player.ResetData("CUNIF"); else Player.SetData("CUNIF", value); } }

        public Game.Items.Item[] Items => Info.Items;

        /// <summary>Текущая одежда игрока</summary>
        /// <value>Массив объектов класса Game.Items.Clothes, в котором null - пустой слот. <br/> Индексы: 0 - шапка, 1 - верх, 2 - низ, 3 - штаны, 4 - обувь</value>
        public Game.Items.Clothes[] Clothes => Info.Clothes;

        /// <summary>Текущие аксессуары игрока</summary>
        /// <value>Массив объектов класса Game.Items.Clothes, в котором null - пустой слот. <br/> Индексы: 0 - очки, 1 - маска, 2 - серьги, 3 - шея, 4 - часы, 5 - браслет, 6 - кольцо, 7 - перчатки</value>
        public Game.Items.Clothes[] Accessories => Info.Accessories;

        /// <summary>Текущая сумка игрока</summary>
        /// <value>Объект класса Game.Items.Bag, null - если отсутствует</value>
        public Game.Items.Bag Bag { get => Info.Bag; set => Info.Bag = value; }

        /// <summary>Текущая кобура игрока</summary>
        /// <value>Объект класса Game.Items.Holster, null - если отсутствует</value>
        public Game.Items.Holster Holster { get => Info.Holster; set => Info.Holster = value; }

        /// <summary>Текущее оружие игрока (не включает в себя кобуру)</summary>
        /// <value>Массив объектов класса Game.Items.Weapon, в котором null - пустой слот</value>
        public Game.Items.Weapon[] Weapons => Info.Weapons;

        /// <summary>Текущий бронежилет игрока</summary>
        /// <value>Объект класса Game.Items.BodyArmour, null - если отсутствует</value>
        public Game.Items.Armour Armour { get => Info.Armour; set => Info.Armour = value; }

        public List<Game.Estates.Furniture> Furniture { get => Info.Furniture; set => Info.Furniture = value; }

        /// <summary>Текущие предметы игрока, которые времено забрал сервер</summary>
        public List<(Game.Items.Item Item, Game.Items.Inventory.GroupTypes Group, int Slot)> TempItems { get => Player.GetData<List<(Game.Items.Item, Game.Items.Inventory.GroupTypes, int)>>("TempItems"); set { if (value == null) Player.ResetData("TempItems"); else Player.SetData("TempItems", value); } }

        public bool IsInventoryBlocked { get => Player.GetData<bool?>("Inventory::Blocked") == true; set { if (!value) Player.ResetData("Inventory::Blocked"); else Player.SetData("Inventory::Blocked", value); } }

        public List<Game.Items.Gift> Gifts { get => Info.Gifts; set => Info.Gifts = value; }

        /// <summary>Активное предложение игрока</summary>
        public Sync.Offers.Offer ActiveOffer => Sync.Offers.Offer.GetByPlayer(this);

        /// <summary>Активный звонок игрока</summary>
        public Sync.Phone.Call ActiveCall => Sync.Phone.Call.GetByPlayer(this);

        /// <summary>Время получения последнего урона от оружия</summary>
        public DateTime LastDamageTime { get; set; }

        public IEnumerable<Sync.World.ItemOnGround> OwnedItemsOnGround => Sync.World.GetItemsOnGroundByOwner(Info);

        private HashSet<Player> _microphoneListeners = new HashSet<Player>();

        public bool AddMicrophoneListener(Player target)
        {
            if (_microphoneListeners.Add(target))
            {
                Player.EnableVoiceTo(target);

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool RemoveMicrophoneListener(Player target)
        {
            if (_microphoneListeners.Remove(target))
            {
                Player.DisableVoiceTo(target);

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool HasMicrophoneListener(Player target) => _microphoneListeners.Contains(target);

        public void RemoveAllMicrophoneListeners()
        {
            foreach (var x in _microphoneListeners)
            {
                Player.DisableVoiceTo(x);
            }

            _microphoneListeners.Clear();
        }

        public bool TryGetCurrentItemInUse(out Game.Items.IUsable item, out int slot)
        {
            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i] is Game.Items.IUsable itemU && itemU.InUse)
                {
                    item = itemU;
                    slot = i;

                    return true;
                }
            }

            item = null;
            slot = -1;

            return false;
        }

        public bool HasAnyItemInUse() => TryGetCurrentItemInUse(out _, out _);

        public bool StopUseCurrentItem()
        {
            Game.Items.IUsable item;
            int slot;

            if (TryGetCurrentItemInUse(out item, out slot))
            {
                item.StopUse(this, Game.Items.Inventory.GroupTypes.Items, slot, true);

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryGetActiveWeapon(out Game.Items.Weapon weaponItem, out Game.Items.Inventory.GroupTypes group, out int slot)
        {
            for (int i = 0; i < Weapons.Length; i++)
            {
                var weapon = Weapons[i];

                if (weapon?.Equiped == true)
                {
                    weaponItem = weapon;
                    group = Game.Items.Inventory.GroupTypes.Weapons;
                    slot = i;

                    return true;
                }
            }

            if (Holster?.Weapon is Game.Items.Weapon holsterWeapon && holsterWeapon.Equiped)
            {
                weaponItem = holsterWeapon;
                group = Game.Items.Inventory.GroupTypes.Holster;
                slot = 2;

                return true;
            }
            else
            {
                weaponItem = null;
                group = Game.Items.Inventory.GroupTypes.Items;
                slot = -1;

                return false;
            }
        }

        public bool HasAnyActiveWeapon() => TryGetActiveWeapon(out _, out _, out _);

        public bool UnequipActiveWeapon()
        {
            Game.Items.Weapon weapon; Game.Items.Inventory.GroupTypes group; int slot;

            if (TryGetActiveWeapon(out weapon, out group, out slot))
            {
                this.InventoryAction(group, slot, 5);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
