using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer
{
    public partial class PlayerData
    {
        public class PlayerInfo
        {
            private static Queue<uint> FreeIDs { get; set; } = new Queue<uint>();

            public static Dictionary<uint, PlayerInfo> All { get; private set; } = new Dictionary<uint, PlayerInfo>();

            private static uint LastAddedMaxId { get; set; }

            public static uint MoveNextId()
            {
                uint id;

                if (!FreeIDs.TryDequeue(out id))
                {
                    id = ++LastAddedMaxId;
                }

                return id;
            }

            public static void AddFreeId(uint id) => FreeIDs.Enqueue(id);

            public static void AddOnLoad(PlayerInfo pInfo)
            {
                if (pInfo == null)
                    return;

                All.Add(pInfo.CID, pInfo);

                if (pInfo.CID > LastAddedMaxId)
                    LastAddedMaxId = pInfo.CID;
            }

            public static void Add(PlayerInfo pInfo)
            {
                if (pInfo == null)
                    return;

                All.Add(pInfo.CID, pInfo);

                MySQL.CharacterAdd(pInfo);
            }

            public static void Remove(PlayerInfo pInfo)
            {
                if (pInfo == null)
                    return;

                var id = pInfo.CID;

                AddFreeId(id);

                All.Remove(id);

                //MySQL.GiftDelete(pInfo);
            }

            public static PlayerInfo Get(uint id) => All.GetValueOrDefault(id);

            public static List<PlayerInfo> GetAllByAID(uint aid) => All.Values.Where(x => x?.AID == aid).ToList();

            public PlayerData PlayerData { get; set; }

            public uint CID { get; set; }

            public uint AID { get; set; }

            public DateTime CreationDate { get; set; }

            public int AdminLevel { get; set; }

            public DateTime LastJoinDate { get; set; }

            public bool IsOnline => PlayerData != null;

            public int TimePlayed { get; set; }

            public string Name { get; set; }

            public string Surname { get; set; }

            public bool Sex { get; set; }

            public DateTime BirthDate { get; set; }

            public List<LicenseTypes> Licenses { get; set; }

            public Game.Fractions.Types Fraction { get; set; }

            public byte FractionRank { get; set; }

            public int OrganisationID { get; set; }

            public ulong Cash { get; set; }

            public uint PhoneNumber { get; set; }

            public uint PhoneBalance { get; set; }

            public Game.Bank.Account BankAccount { get; set; }

            public LastPlayerData LastData { get; set; }

            public List<uint> Familiars { get; set; }

            public Dictionary<SkillTypes, int> Skills { get; set; }

            public List<Sync.Punishment> Punishments { get; set; }

            public List<Game.Items.Gift> Gifts { get; set; }

            public Dictionary<uint, string> Contacts { get; set; }

            public List<uint> PhoneBlacklist { get; set; }

            public Game.Data.Customization.HeadBlend HeadBlend { get; set; }

            public Dictionary<int, Game.Data.Customization.HeadOverlay> HeadOverlays { get; set; }

            public float[] FaceFeatures { get; set; }

            public List<int> Decorations { get; set; }

            public Game.Data.Customization.HairStyle HairStyle { get; set; }

            public byte EyeColor { get; set; }

            public List<VehicleData.VehicleInfo> OwnedVehicles => VehicleData.VehicleInfo.GetAllByCID(CID);

            public List<Game.Estates.House> OwnedHouses => Game.Estates.House.All.Values.Where(x => x.Owner == this).ToList();

            public List<Game.Estates.Apartments> OwnedApartments => Game.Estates.Apartments.All.Values.Where(x => x.Owner == this).ToList();

            public List<Game.Estates.Garage> OwnedGarages => Game.Estates.Garage.All.Values.Where(x => x.Owner == this).ToList();

            public List<Game.Businesses.Business> OwnedBusinesses => Game.Businesses.Business.All.Values.Where(x => x.Owner == this).ToList();

            public Game.Estates.HouseBase SettledHouseBase => (Game.Estates.HouseBase)Game.Estates.House.All.Values.Where(x => x.Settlers.ContainsKey(this)).FirstOrDefault() ?? (Game.Estates.HouseBase)Game.Estates.Apartments.All.Values.Where(x => x.Settlers.ContainsKey(this)).FirstOrDefault();

            /// <summary>Предметы игрока в карманах</summary>
            /// <value>Массив объектов класса Game.Items.Item, в котором null - пустой слот</value>
            public Game.Items.Item[] Items { get; set; }

            /// <summary>Текущая одежда игрока</summary>
            /// <value>Массив объектов класса Game.Items.Clothes, в котором null - пустой слот. <br/> Индексы: 0 - шапка, 1 - верх, 2 - низ, 3 - штаны, 4 - обувь</value>
            public Game.Items.Clothes[] Clothes { get; set; }

            /// <summary>Текущие аксессуары игрока</summary>
            /// <value>Массив объектов класса Game.Items.Clothes, в котором null - пустой слот. <br/> Индексы: 0 - очки, 1 - маска, 2 - серьги, 3 - шея, 4 - часы, 5 - браслет, 6 - кольцо, 7 - перчатки</value>
            public Game.Items.Clothes[] Accessories { get; set; }

            /// <summary>Текущая сумка игрока</summary>
            /// <value>Объект класса Game.Items.Bag, null - если отсутствует</value>
            public Game.Items.Bag Bag { get; set; }

            /// <summary>Текущая кобура игрока</summary>
            /// <value>Объект класса Game.Items.Holster, null - если отсутствует</value>
            public Game.Items.Holster Holster { get; set; }

            /// <summary>Текущее оружие игрока (не включает в себя кобуру)</summary>
            /// <value>Массив объектов класса Game.Items.Weapon, в котором null - пустой слот</value>
            public Game.Items.Weapon[] Weapons { get; set; }

            /// <summary>Текущий бронежилет игрока</summary>
            /// <value>Объект класса Game.Items.BodyArmour, null - если отсутствует</value>
            public Game.Items.Armour Armour { get; set; }

            /// <summary>Мебель игрока</summary>
            public List<Game.Estates.Furniture> Furniture { get; set; }

            /// <summary>Скины на оружие игрока</summary>
            public Dictionary<Game.Items.WeaponSkin.ItemData.Types, Game.Items.WeaponSkin> WeaponSkins { get; set; }

            public Dictionary<Sync.Cooldowns.Types, DateTime> Cooldowns { get; set; }

            public List<Sync.Phone.SMS> AllSMS { get; set; }

            public Dictionary<Achievement.Types, Achievement> Achievements { get; set; }

            public Dictionary<Sync.Quest.QuestData.Types, Sync.Quest> Quests { get; set; }

            public MedicalCard MedicalCard { get; set; }

            public bool LosSantosAllowed { get; set; }

            public IEnumerable<VehicleData.VehicleInfo> VehiclesOnPound => OwnedVehicles.Where(x => x.VehicleData == null && x.IsOnVehiclePound);

            private static Dictionary<string, object> TempData { get; set; } = new Dictionary<string, object>();

            public void SetTempData(string key, object value)
            {
                if (!TempData.TryAdd(key, value))
                    TempData[key] = value;
            }

            public T GetTempData<T>(string key, T otherwise = default(T))
            {
                object value;

                if (!TempData.TryGetValue(key, out value))
                    return otherwise;

                if (value is T valueT)
                    return valueT;

                return otherwise;
            }

            public bool HasTempData(string key) => TempData.ContainsKey(key);

            public bool ResetTempData(string key)
            {
                return TempData.Remove(key);
            }

            public bool HasCooldown(Sync.Cooldowns.Types cdType)
            {
                DateTime dt;

                if (!Cooldowns.TryGetValue(cdType, out dt))
                    return false;

                return dt > Utils.GetCurrentTime();
            }

            public TimeSpan GetCooldownTimeLeft(Sync.Cooldowns.Types cdType)
            {
                DateTime dt;

                if (!Cooldowns.TryGetValue(cdType, out dt))
                    dt = DateTime.MinValue;

                return Utils.GetCurrentTime().Subtract(dt);
            }

            public void SetCooldown(Sync.Cooldowns.Types cdType, int secs)
            {
                var time = Utils.GetCurrentTime().AddSeconds(secs);

                if (!Cooldowns.TryAdd(cdType, time))
                    Cooldowns[cdType] = time;

                MySQL.CharacterCooldownSet(this, cdType);
            }

            public bool RemoveCooldown(Sync.Cooldowns.Types cdType)
            {
                if (Cooldowns.Remove(cdType))
                {
                    MySQL.CharacterCooldownRemove(this, cdType);

                    return true;
                }

                return false;
            }

            public bool TryAddCash(ulong amount, out ulong newBalance, bool notifyOnFault = true, PlayerData tData = null)
            {
                if (!Cash.TryAdd(amount, out newBalance))
                {
                    if (notifyOnFault)
                    {
                        if (PlayerData != null)
                        {

                        }
                    }

                    return false;
                }

                return true;
            }

            public bool TryRemoveCash(ulong amount, out ulong newBalance, bool notifyOnFault = true, PlayerData tData = null)
            {
                if (!Cash.TrySubtract(amount, out newBalance))
                {
                    if (notifyOnFault)
                    {
                        if (PlayerData != null)
                        {
                            PlayerData.Player.Notify("Cash::NotEnough", Cash);
                        }
                    }

                    return false;
                }

                return true;
            }

            public void SetCash(ulong value)
            {
                if (PlayerData != null)
                {
                    PlayerData.Cash = value;
                }
                else
                {
                    Cash = value;
                }

                MySQL.CharacterCashUpdate(this);
            }

            public bool TryAddPhoneBalance(uint amount, out uint newBalance, bool notifyOnFault = true)
            {
                if (!PhoneBalance.TryAdd(amount, out newBalance))
                {
                    if (notifyOnFault)
                    {
                        if (PlayerData != null)
                        {

                        }
                    }

                    return false;
                }

                return true;
            }

            public bool TryRemovePhoneBalance(uint amount, out uint newBalance, bool notifyOnFault = true)
            {
                if (!PhoneBalance.TrySubtract(amount, out newBalance))
                {
                    if (notifyOnFault)
                    {
                        if (PlayerData != null)
                        {
                            PlayerData.Player.Notify("Phone::BNE", PhoneBalance);
                        }
                    }

                    return false;
                }

                return true;
            }

            public void SetPhoneBalance(uint value)
            {
                PhoneBalance = value;

                MySQL.CharacterPhoneBalanceUpdate(this);
            }

            public PlayerInfo()
            {
                this.AllSMS = new List<Sync.Phone.SMS>() { };
            }
        }
    }
}
