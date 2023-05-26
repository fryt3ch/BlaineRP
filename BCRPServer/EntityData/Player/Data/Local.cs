using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BCRPServer
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

        /// <summary>Текущее оружие</summary>
        /// <value>Объект класса Game.Items.Weapon, null - если ничего</value>
        public (Game.Items.Weapon WeaponItem, Game.Items.Inventory.Groups Group, int Slot)? ActiveWeapon
        {
            get
            {
                if (Weapons[0]?.Equiped == true)
                {
                    return (Weapons[0], Game.Items.Inventory.Groups.Weapons, 0);
                }
                else if (Weapons[1]?.Equiped == true)
                {
                    return (Weapons[1], Game.Items.Inventory.Groups.Weapons, 1);
                }
                else if (Holster?.Items[0] is Game.Items.Weapon weapon && weapon.Equiped)
                {
                    return (weapon, Game.Items.Inventory.Groups.Holster, 2);
                }

                return null;
            }
        }

        public (Game.Items.IUsable Item, int Slot)? CurrentItemInUse
        {
            get
            {
                for (int i = 0; i < Items.Length; i++)
                {
                    if (Items[i] is Game.Items.IUsable itemU && itemU.InUse)
                        return (itemU, i);
                }

                return null;
            }
        }

        /// <summary>Знакомые игроки</summary>
        /// <value>Список CID игроков</value>
        public HashSet<uint> Familiars { get => Info.Familiars; set => Info.Familiars = value; }

        /// <summary>Сущность, к которой прикреплен игрок</summary>
        public Entity IsAttachedToEntity => Player.GetEntityIsAttachedTo();

        /// <summary>Транспорт, находящийся в собственности у игрока</summary>
        public List<VehicleData.VehicleInfo> OwnedVehicles { get; set; }

        public List<Game.Estates.House> OwnedHouses { get; set; }

        public List<Game.Estates.Apartments> OwnedApartments { get; set; }

        public List<Game.Estates.Garage> OwnedGarages { get; set; }

        public List<Game.Businesses.Business> OwnedBusinesses { get; set; }

        public Game.Estates.HouseBase SettledHouseBase { get; set; }

        public VehicleData RentedVehicle => VehicleData.All.Values.Where(x => x.OwnerType == VehicleData.OwnerTypes.PlayerRent && x.OwnerID == CID).FirstOrDefault();

        public VehicleData RentedJobVehicle => VehicleData.All.Values.Where(x => x.OwnerType == VehicleData.OwnerTypes.PlayerRentJob && x.OwnerID == CID).FirstOrDefault();

        public Game.Jobs.Job CurrentJob { get => Game.Jobs.Job.Get(Player.GetData<int>("CJob")); set { if (value == null) Player.ResetData("CJob"); else Player.SetData("CJob", value.Id); } }

        /// <summary>Текущий контейнер, который смотрит игрок</summary>
        /// <value>UID контейнера, null - если отсутствует</value>
        public Game.Items.Container CurrentContainer { get => Player.GetData<Game.Items.Container>("CCont"); set { if (value == null) Player.ResetData("CCont"); else Player.SetData("CCont", value); } }

        public Game.Items.Craft.Workbench CurrentWorkbench { get => Player.GetData<Game.Items.Craft.Workbench>("CWBench"); set { if (value == null) Player.ResetData("CWBench"); else Player.SetData("CWBench", value); } }

        /// <summary>Текущий бизнес, с которым взаимодействует игрок</summary>
        public Game.Businesses.Business CurrentBusiness { get => Player.GetData<Game.Businesses.Business>("CBusiness"); set { if (value == null) Player.ResetData("CBusiness"); else Player.SetData("CBusiness", value); } }

        /// <summary>Текущий дом, с которым взаимодействует игрок</summary>
        public Game.Estates.House CurrentHouse
        {
            get
            {
                var id = Utils.GetHouseIdByDimension(Player.Dimension);

                return id == 0 ? null : Game.Estates.House.Get(id);
            }
        }

        public Game.Estates.Apartments CurrentApartments
        {
            get
            {
                var id = Utils.GetApartmentsIdByDimension(Player.Dimension);

                return id == 0 ? null : Game.Estates.Apartments.Get(id);
            }
        }

        public Game.Estates.Garage CurrentGarage
        {
            get
            {
                var id = Utils.GetGarageIdByDimension(Player.Dimension);

                return id == 0 ? null : Game.Estates.Garage.Get(id);
            }
        }

        public Game.Estates.HouseBase CurrentHouseBase => Utils.GetHouseBaseByDimension(Player.Dimension);

        public Game.Estates.Apartments.ApartmentsRoot CurrentApartmentsRoot => Utils.GetApartmentsRootByDimension(Player.Dimension);

        public int VehicleSlots => (Settings.MIN_VEHICLE_SLOTS + OwnedHouses.Where(x => x.GarageData != null).Select(x => x.GarageData.MaxVehicles).Sum() + OwnedGarages.Select(x => x.StyleData.MaxVehicles).Sum()) - OwnedVehicles.Count;

        public int HouseSlots => Settings.MAX_HOUSES - OwnedHouses.Count;
        public int ApartmentsSlots => Settings.MAX_APARTMENTS - OwnedApartments.Count;
        public int GaragesSlots => Settings.MAX_GARAGES - OwnedGarages.Count;
        public int BusinessesSlots => Settings.MAX_BUSINESSES - OwnedBusinesses.Count;

        public Game.Data.Customization.UniformTypes? CurrentUniform { get => Player.GetData<Game.Data.Customization.UniformTypes?>("CUNIF"); set { if (value == null) Player.ResetData("CUNIF"); else Player.SetData("CUNIF", value); } }

        #region Customization
        public Game.Data.Customization.HeadBlend HeadBlend { get => Info.HeadBlend; set => Info.HeadBlend = value; }

        public Dictionary<int, Game.Data.Customization.HeadOverlay> HeadOverlays { get => Info.HeadOverlays; set => Info.HeadOverlays = value; }

        public float[] FaceFeatures { get => Info.FaceFeatures; set => Info.FaceFeatures = value; }

        public List<int> Decorations { get => Info.Decorations; set => Info.Decorations = value; }

        public Game.Data.Customization.HairStyle HairStyle { get => Info.HairStyle; set => Info.HairStyle = value; }

        public byte EyeColor { get => Info.EyeColor; set => Info.EyeColor = value; }
        #endregion

        #region Inventory
        /// <summary>Предметы игрока в карманах</summary>
        /// <value>Массив объектов класса Game.Items.Item, в котором null - пустой слот</value>
        public Game.Items.Item[] Items { get => Info.Items; set => Info.Items = value; }

        /// <summary>Текущая одежда игрока</summary>
        /// <value>Массив объектов класса Game.Items.Clothes, в котором null - пустой слот. <br/> Индексы: 0 - шапка, 1 - верх, 2 - низ, 3 - штаны, 4 - обувь</value>
        public Game.Items.Clothes[] Clothes { get => Info.Clothes; set => Info.Clothes = value; }

        /// <summary>Текущие аксессуары игрока</summary>
        /// <value>Массив объектов класса Game.Items.Clothes, в котором null - пустой слот. <br/> Индексы: 0 - очки, 1 - маска, 2 - серьги, 3 - шея, 4 - часы, 5 - браслет, 6 - кольцо, 7 - перчатки</value>
        public Game.Items.Clothes[] Accessories { get => Info.Accessories; set => Info.Accessories = value; }

        /// <summary>Текущая сумка игрока</summary>
        /// <value>Объект класса Game.Items.Bag, null - если отсутствует</value>
        public Game.Items.Bag Bag { get => Info.Bag; set => Info.Bag = value; }

        /// <summary>Текущая кобура игрока</summary>
        /// <value>Объект класса Game.Items.Holster, null - если отсутствует</value>
        public Game.Items.Holster Holster { get => Info.Holster; set => Info.Holster = value; }

        /// <summary>Текущее оружие игрока (не включает в себя кобуру)</summary>
        /// <value>Массив объектов класса Game.Items.Weapon, в котором null - пустой слот</value>
        public Game.Items.Weapon[] Weapons { get => Info.Weapons; set => Info.Weapons = value; }

        /// <summary>Текущий бронежилет игрока</summary>
        /// <value>Объект класса Game.Items.BodyArmour, null - если отсутствует</value>
        public Game.Items.Armour Armour { get => Info.Armour; set => Info.Armour = value; }

        public List<Game.Estates.Furniture> Furniture { get => Info.Furniture; set => Info.Furniture = value; }

        /// <summary>Текущие предметы игрока, которые времено забрал сервер</summary>
        public List<(Game.Items.Item Item, Game.Items.Inventory.Groups Group, int Slot)> TempItems { get => Player.GetData<List<(Game.Items.Item, Game.Items.Inventory.Groups, int)>>("TempItems"); set { if (value == null) Player.ResetData("TempItems"); else Player.SetData("TempItems", value); } }

        public bool InventoryBlocked { get => Player.GetData<bool?>("Inventory::Blocked") == true; set { if (!value) Player.ResetData("Inventory::Blocked"); else Player.SetData("Inventory::Blocked", value); } }
        #endregion

        public List<Game.Items.Gift> Gifts { get => Info.Gifts; set => Info.Gifts = value; }

        /// <summary>Активное предложение игрока</summary>
        public Sync.Offers.Offer ActiveOffer => Sync.Offers.Offer.GetByPlayer(this);

        /// <summary>Активный звонок игрока</summary>
        public Sync.Phone.Call ActiveCall => Sync.Phone.Call.GetByPlayer(this);

        /// <summary>Список игроков, которые являются слушателями (микрофон)</summary>
        public List<Player> Listeners { get; set; }

        /// <summary>Время получения последнего урона от оружия</summary>
        public DateTime LastDamageTime { get; set; }

        /// <summary>Наигранное время игрока</summary>
        /// <value>Кол-во минут</value>
        public int TimePlayed { get => Info.TimePlayed; set => Info.TimePlayed = value; }

        /// <summary>Дата создания игрока</summary>
        public DateTime CreationDate { get => Info.CreationDate; set => Info.CreationDate = value; }

        /// <summary>Дата рождения игрока</summary>
        public DateTime BirthDate { get => Info.BirthDate; set => Info.BirthDate = value; }

        /// <summary>Имя игрока</summary>
        public string Name { get => Info.Name; set => Info.Name = value; }

        /// <summary>Фамилия игрока</summary>
        public string Surname { get => Info.Surname; set => Info.Surname = value; }

        /// <summary>Навыки игрока</summary>
        /// <value>Словарь, где ключ - enum SkilType, а значение - от 0 до 100</value>
        public Dictionary<SkillTypes, int> Skills { get => Info.Skills; set => Info.Skills = value; }

        /// <summary>Лицензии игрока</summary>
        /// <value>Список enum LicenseType</value>
        public List<LicenseTypes> Licenses { get => Info.Licenses; set => Info.Licenses = value; }

        public IEnumerable<Sync.World.ItemOnGround> OwnedItemsOnGround => Sync.World.GetItemsOnGroundByOwner(Info);
    }
}
