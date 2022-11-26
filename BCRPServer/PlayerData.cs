using BCRPServer.Game.Items;
using GTANetworkAPI;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BCRPServer
{
    public class PlayerData
    {
        public static Dictionary<Player, PlayerData> All { get; private set; } = new Dictionary<Player, PlayerData>();

        /// <summary>Получить PlayerData игрока</summary>
        /// <returns>Объект класса PlayerData если существует, иначе - null</returns>
        public static PlayerData Get(Player player)
        {
            if (player == null)
                return null;

            return All.GetValueOrDefault(player);
        }

        /// <summary>Назначить объект класса PlayerData игроку</summary>
        public static void Set(Player player, PlayerData data)
        {
            if (player == null)
                return;

            All.Add(player, data);
        }

        public void Remove()
        {
            if (Player == null)
                return;

            Info.PlayerData = null;

            All.Remove(Player);

            Player.ResetData();
        }

        #region Enums
        public enum LicenseTypes
        {
            /// <summary>Мопеды</summary>
            M = 0,
            /// <summary>Мотоциклы</summary>
            A,
            /// <summary>Легковой транспорт</summary>
            B,
            /// <summary>Грузовой транспорт</summary>
            C,
            /// <summary>Маршрутный транспорт</summary>
            D,
            /// <summary>Летный транспорт</summary>
            Fly,
            /// <summary>Морской транспорт</summary>
            Sea,
            /// <summary>Оружие</summary>
            Weapons,
            /// <summary>Охота</summary>
            Hunting,
            /// <summary>Бизнес</summary>
            Business,
            /// <summary>Адвокатура</summary>
            Lawyer,
        }

        public enum SkillTypes
        {
            /// <summary>Сила</summary>
            Strength = 0,
            /// <summary>Стрельба</summary>
            Shooting,
            /// <summary>Кулинария</summary>
            Cooking,
            /// <summary>Рыбалка</summary>
            Fishing
        }

        public enum FractionTypes
        {
            /// <summary>Отсутствует</summary>
            None = -1,

            PBMS = 0, // Paleto Bay Medical Service
            SSMS = 1, // Sandy Shores Medical Service
            PBSD = 2, // Paleto Bay Sheriff's Department
            SSSD = 3, // Sandy Shores Sheriff's Department
            NG = 4, // National Guard
            GOV = 5, // Government
            WeazelNews = 6, // Weazel News
            MM = 7, // Mexican Mafia
            IM = 8, // Italian Mafia
        }

        public enum PropertyTypes
        {
            /// <summary>Транспорт</summary>
            Vehicle = 0,
            /// <summary>Дом</summary>
            House,
            /// <summary>Квартира</summary>
            Apartments,
            /// <summary>Гараж</summary>
            Garage,
            /// <summary>Бизнес</summary>
            Business,
        }
        #endregion

        #region Subclasses

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

            public FractionTypes Fraction { get; set; }

            public int OrganisationID { get; set; }

            public int Cash { get; set; }

            public Bank.Account BankAccount { get; set; }

            public LastPlayerData LastData { get; set; }

            public List<uint> Familiars { get; set; }

            public Dictionary<SkillTypes, int> Skills { get; set; }

            public List<Punishment> Punishments { get; set; }

            public List<Game.Items.Gift> Gifts { get; set; }


            public HeadBlend HeadBlend { get; set; }

            public Dictionary<int, HeadOverlay> HeadOverlays { get; set; }

            public float[] FaceFeatures { get; set; }

            public List<Decoration> Decorations { get; set; }

            public Game.Data.Customization.HairStyle HairStyle { get; set; }

            public byte EyeColor { get; set; }

            public List<VehicleData.VehicleInfo> OwnedVehicles { get; set; }
            public List<Game.Houses.House> OwnedHouses { get; set; }
            public List<Game.Houses.Apartments> OwnedApartments { get; set; }
            public List<Game.Houses.Garage> OwnedGarages { get; set; }
            public List<Game.Businesses.Business> OwnedBusinesses { get; set; }

            public List<Game.Houses.House> SettledHouses { get; set; }
            public List<Game.Houses.Apartments> SettledApartments { get; set; }

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

            public PlayerInfo() {}
        }

        public class Punishment
        {
            public enum Types
            {
                /// <summary>Блокировка</summary>
                Ban = 0,
                /// <summary>Предупреждение</summary>
                Warn = 1,
                /// <summary>Мут</summary>
                Mute = 2,
                /// <summary>NonRP тюрьма</summary>
                NRPPrison = 3
            }

            /// <summary>Уникальный ID наказания</summary>
            public int ID { get; set; }

            /// <summary>Тип наказания</summary>
            public Types Type { get; set; }

            /// <summary>Причина наказания</summary>
            public string Reason { get; set; }

            /// <summary>Дата выдачи наказания</summary>
            public DateTime StartDate { get; set; }

            /// <summary>Дата окончания наказания/summary>
            public DateTime EndDate { get; set; }

            /// <summary>CID администратора, выдавшего наказание</summary>
            public int AdminID { get; set; }

            public Punishment(int ID, Types Type, string Reason, DateTime StartDate, DateTime EndDate, int AdminID)
            {
                this.ID = ID;
                this.Type = Type;
                this.Reason = Reason;
                this.StartDate = StartDate;
                this.EndDate = EndDate;
                this.AdminID = AdminID;
            }

            /// <summary>Получить оставшееся время в секундах</summary>
            /// <returns>Время в секундах</returns>
            public int GetSecondsLeft() => (int)EndDate.Subtract(StartDate).TotalSeconds;
        }

        public class LastPlayerData
        {
            /// <summary>Последнее измерение</summary>
            public uint Dimension { get; set; }

            /// <summary>Последние координаты</summary>
            public Vector3 Position { get; set; }

            /// <summary>Последний поворот</summary>
            public float Heading { get; set; }

            /// <summary>Последнее здоровье</summary>
            public int Health { get; set; }

            /// <summary>Время в секундах, наигранное за последнюю сессию</summary>
            /// <remarks>Обнуляется каждый час</remarks>
            public int SessionTime { get; set; }

            public int Satiety { get; set; }

            public int Mood { get; set; }
        }
        #endregion

        public static Dictionary<SkillTypes, int> MaxSkills = new Dictionary<SkillTypes, int>()
        {
            { SkillTypes.Strength, 100 },
            { SkillTypes.Shooting, 100 },
            { SkillTypes.Cooking, 100 },
            { SkillTypes.Fishing, 100 },
        };

        /// <summary>Сущность игрока</summary>
        public Player Player { get; set; }

        /// <summary>Список наказаний игрока</summary>
        public List<Punishment> Punishments { get => Info.Punishments; set => Info.Punishments = value; }

        /// <summary>Банковский счёт игрока</summary>
        public Bank.Account BankAccount { get => Info.BankAccount; set => Info.BankAccount = value; }

        #region Local Data
        /// <summary>Информация об игроке с момента последнего захода на сервер</summary>
        public LastPlayerData LastData { get => Info.LastData; set => Info.LastData = value; }

        /// <summary>Дата последнего захода игрока на сервер</summary>
        public DateTime LastJoinDate { get => Info.LastJoinDate; set => Info.LastJoinDate = value; }

        /// <summary>Текущее оружие</summary>
        /// <value>Объект класса Game.Items.Weapon, null - если ничего</value>
        public (Game.Items.Weapon WeaponItem, CEF.Inventory.Groups Group, int Slot)? ActiveWeapon
        {
            get
            {
                if (Weapons[0]?.Equiped == true)
                {
                    return (Weapons[0], CEF.Inventory.Groups.Weapons, 0);
                }
                else if (Weapons[1]?.Equiped == true)
                {
                    return (Weapons[1], CEF.Inventory.Groups.Weapons, 1);
                }
                else if (Holster?.Items[0] is Game.Items.Weapon weapon && weapon.Equiped)
                {
                    return (weapon, CEF.Inventory.Groups.Holster, 2);
                }

                return null;
            }
        }

        /// <summary>Знакомые игроки</summary>
        /// <value>Список CID игроков</value>
        public List<uint> Familiars { get => Info.Familiars; set => Info.Familiars = value; }

        /// <summary>Сущность, к которой прикреплен игрок</summary>
        public (Entity Entity, Sync.AttachSystem.Types Type)? IsAttachedTo { get => Player.GetData<(Entity, Sync.AttachSystem.Types)?>("IsAttachedTo::Entity"); set { Player.SetData("IsAttachedTo::Entity", value); } }

        /// <summary>Транспорт, находящийся в собственности у игрока</summary>
        public List<VehicleData.VehicleInfo> OwnedVehicles { get => Info.OwnedVehicles; set => Info.OwnedVehicles = value; }

        public List<Game.Houses.House> OwnedHouses { get => Info.OwnedHouses; set => Info.OwnedHouses = value; }

        public List<Game.Houses.Apartments> OwnedApartments { get => Info.OwnedApartments; set => Info.OwnedApartments = value; }

        public List<Game.Houses.Garage> OwnedGarages { get => Info.OwnedGarages; set => Info.OwnedGarages = value; }

        public List<Game.Businesses.Business> OwnedBusinesses { get => Info.OwnedBusinesses; set => Info.OwnedBusinesses = value; }

        /// <summary>Текущий контейнер, который смотрит игрок</summary>
        /// <value>UID контейнера, null - если отсутствует</value>
        public uint? CurrentContainer { get; set; }

        /// <summary>Текущий бизнес, с которым взаимодействует игрок</summary>
        public Game.Businesses.Business CurrentBusiness { get; set; }

        /// <summary>Текущий дом, с которым взаимодействует игрок</summary>
        public Game.Houses.House CurrentHouse { get; set; }

        public int VehicleSlots
        {
            get
            {
                return Settings.MIN_VEHICLE_SLOTS - OwnedVehicles.Count; // todo
            }
        }

        public int HouseSlots => Settings.MAX_HOUSES - OwnedHouses.Count;
        public int ApartmentsSlots => Settings.MAX_APARTMENTS - OwnedApartments.Count;
        public int GaragesSlots => Settings.MAX_GARAGES - OwnedGarages.Count;
        public int BusinessesSlots => Settings.MAX_BUSINESSES - OwnedBusinesses.Count;

        #region Customization
        public HeadBlend HeadBlend { get => Info.HeadBlend; set => Info.HeadBlend = value; }

        public Dictionary<int, HeadOverlay> HeadOverlays { get => Info.HeadOverlays; set => Info.HeadOverlays = value; }

        public float[] FaceFeatures { get => Info.FaceFeatures; set => Info.FaceFeatures = value; }

        public List<Decoration> Decorations { get => Info.Decorations; set => Info.Decorations = value; }

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

        /// <summary>Текущее оружие игрока (не включает в себя кобуру), которое было временно снято сервером</summary>
        /// <value>Массив объектов класса Game.Items.Weapon, в котором null - пустой слот</value>
        public Game.Items.Weapon[] TempWeapons { get; set; }
        #endregion

        public List<Game.Items.Gift> Gifts { get => Info.Gifts; set => Info.Gifts = value; }

        /// <summary>Активное предложение игрока</summary>
        public Sync.Offers.Offer ActiveOffer { get; set; }

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

        public void AddFamiliar(PlayerData tData)
        {
            var pCid = CID;
            var tCid = tData.CID;

            if (!Familiars.Contains(tCid)) ;
            {
                Player.TriggerEvent("Player::Familiars::Update", true, tCid);
            }

            if (!tData.Familiars.Contains(pCid))
            {
                tData.Player.TriggerEvent("Player::Familiars::Update", true, pCid);
            }
        }

        public void RemoveFamiliar(PlayerData tData)
        {
            var pCid = CID;
            var tCid = tData.CID;

            if (Familiars.Remove(tCid)) ;
            {
                Player.TriggerEvent("Player::Familiars::Update", false, tCid);
            }

            if (tData.Familiars.Remove(pCid))
            {
                tData.Player.TriggerEvent("Player::Familiars::Update", false, pCid);
            }
        }

        public void AddLicense(LicenseTypes lType)
        {
            if (Licenses.Contains(lType))
                return;

            Licenses.Add(lType);

            Player.TriggerEvent("Player::Licenses::Update", true, lType);
        }

        public void RemoveLicense(LicenseTypes lType)
        {
            if (!Licenses.Remove(lType))
                return;

            Player.TriggerEvent("Player::Licenses::Update", false, lType);
        }

        public void UpdateSkill(SkillTypes sType, int updValue)
        {
            updValue = Skills[sType] + updValue;

            if (updValue > MaxSkills[sType])
                updValue = MaxSkills[sType];
            else if (updValue < 0)
                updValue = 0;

            Skills[sType] = updValue;

            Player.TriggerEvent("Player::Skills::Update", sType, updValue);
        }

        public void AddVehicleProperty(VehicleData.VehicleInfo vInfo)
        {
            if (OwnedVehicles.Contains(vInfo))
                return;

            OwnedVehicles.Add(vInfo);

            Player.TriggerEvent("Player::Properties::Update", true, PropertyTypes.Vehicle, vInfo.VID, vInfo.ID);
        }

        public void RemoveVehicleProperty(VehicleData.VehicleInfo vInfo)
        {
            OwnedVehicles.Remove(vInfo);

            Player.TriggerEvent("Player::Properties::Update", false, PropertyTypes.Vehicle, vInfo.VID);
        }

        public void AddBusinessProperty(Game.Businesses.Business biz)
        {
            if (OwnedBusinesses.Contains(biz))
                return;

            OwnedBusinesses.Add(biz);

            Player.TriggerEvent("Player::Properties::Update", true, PropertyTypes.Business, biz.ID);
        }

        public void RemoveBusinessProperty(Game.Businesses.Business biz)
        {
            OwnedBusinesses.Remove(biz);

            Player.TriggerEvent("Player::Properties::Update", false, PropertyTypes.Business, biz.ID);
        }
        #endregion

        #region Own Shared Data
        /// <summary>Сытость игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>От 0 до 100</value>
        public int Satiety { get => Info.LastData.Satiety; set { Player.SetOwnSharedData("Satiety", value); Info.LastData.Satiety = value; } }

        /// <summary>Настроение игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>От 0 до 100</value>
        public int Mood { get => Info.LastData.Mood; set { Player.SetOwnSharedData("Mood", value); Info.LastData.Mood = value; } }

        public int BankBalance { get => Info.BankAccount?.Balance ?? 0; set { Player.SetOwnSharedData("BankBalance", value); if (Info.BankAccount != null) Info.BankAccount.Balance = value; } }

        /// <summary>Наличные игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>Кол-во наличных средств</value>
        public int Cash { get => Info.Cash; set { Player.SetOwnSharedData("Cash", value); Info.Cash = value; } }

        /// <summary>Организация игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>ID организации, -1 - если отсутствует</value>
        public int OrganisationID { get => Info.OrganisationID; set { Player.SetOwnSharedData("OrganisationID", value); Info.OrganisationID = value; } }

        /// <summary>Пристёгнут ли игрок?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool BeltOn { get => Player.GetOwnSharedData<bool>("Belt::On"); set { Player.SetOwnSharedData("Belt::On", value); } }

        /// <summary>Ранен ли игрок?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool IsWounded { get => Player.GetOwnSharedData<bool>("IsWounded"); set { Player.SetOwnSharedData("IsWounded", value); } }

        /// <summary>Ползет ли игрок?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool CrawlOn { get => Player.GetOwnSharedData<bool>("Crawl::On"); set { Player.SetOwnSharedData("Crawl::On", value); } }

        /// <summary>Текущая анимация игрока (Fast)</summary>
        /// <remarks>НЕ синхронизуется с игроками ВНЕ зоны стрима (т.к. проигрывается быстро)</remarks>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public Sync.Animations.FastTypes FastAnim { get => (Sync.Animations.FastTypes)Player.GetOwnSharedData<int>("Anim::Fast"); set { Player.SetOwnSharedData("Anim::Fast", (int)value); } }

        /// <summary>Метод для изменения кол-ва наличных у игрока</summary>
        /// <param name="value">Кол-во (- или +)</param>
        /// <param name="notify">Уведомить ли игрока?</param>
        /// <returns>true, если операция успешна, false - в противном случае</returns>
        public bool AddCash(int value, bool notify = true)
        {
            if (Cash + value < 0)
            {
                if (notify)
                    Player.Notify("Cash::NotEnough", Cash);

                return false;
            }

            Cash += value;

            MySQL.CharacterUpdateCash(this.Info);

            return true;
        }

        public PlayerInfo Info { get; set; }
        #endregion

        #region Shared Data
        /// <summary>CID игрока</summary>
        /// <remarks>Т.к. может использоваться для сохранения данных в БД, set - в основном потоке, get - в любом</remarks>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public uint CID { get => Info.CID; set { Player.SetSharedData("CID", value); Info.CID = value; } }

        /// <summary>Пол игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>true - мужчина, false - женщина</value>
        public bool Sex { get => Info.Sex; set { Player.SetSharedData("Sex", value); Player.SetSkin(value ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01); Info.Sex = value; } }

        /// <summary>Фракция игрока</summary>
        /// <remarks>Также вызывает событие Players::SetFraction на клиенте игроков в зоне стрима</remarks>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public FractionTypes Fraction { get => Info.Fraction; set { Player.SetSharedData("Fraction", value); Info.Fraction = value; } }

        /// <summary>В маске ли игрок?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool Masked { get => Player.GetSharedData<bool>("Masked"); set { Player.SetSharedData("Masked", value); } }

        /// <summary>Без сознания ли игрок?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool IsKnocked { get => Player.GetSharedData<bool>("Knocked"); set { Player.SetSharedData("Knocked", value); } }

        /// <summary>Приседает ли игрок?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool CrouchOn { get => Player.GetSharedData<bool>("Crouch::On"); set { Player.SetSharedData("Crouch::On", value); } }

        /// <summary>Дальность микрофона игрока</summary>
        /// <remarks>Если микрофон игроком не используется: 0, если в муте: -1</remarks>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public float VoiceRange { get => Player.GetSharedData<float>("VoiceRange"); set { Player.SetSharedData("VoiceRange", value); } }

        /// <summary>В муте ли игрок?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool IsMuted { get => VoiceRange < 0f; set { Sync.Microphone.DisableMicrophone(this); VoiceRange = -1; } }

        /// <summary>Проблемы ли у игрока со слухом/речью?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool IsInvalid { get => Player.GetSharedData<bool>("IsInvalid"); set { Player.SetSharedData("IsInvalid", value); } }

        /// <summary>Использует ли игрок телефон?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool PhoneOn { get => Player.GetSharedData<bool>("Phone::On"); set { Player.SetSharedData("Phone::On", value); } }

        /// <summary>Уровень администратора игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public int AdminLevel { get => Info.AdminLevel; set { Player.SetSharedData("AdminLevel", value); Info.AdminLevel = value; } }

        /// <summary>Место в транспорте, на котором сидит игрок</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public int VehicleSeat { get => Player.GetSharedData<int>("VehicleSeat"); set { Player.SetSharedData("VehicleSeat", value); } }

        /// <summary>Текущая шапка игрока, необходимо для нормального отображения в игре при входе/выходе из транспорта</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public string Hat { get => Player.GetSharedData<string>("Hat"); set { Player.SetSharedData("Hat", value); } }

        /// <summary>Является ли игрок невидимым?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool IsInvisible { get => Player.GetSharedData<bool>("IsInvisible"); set { Player.SetSharedData("IsInvisible", value); Player.SetAlpha(value ? 0 : 255); } }

        /// <summary>Является ли игрок бессмертным?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool IsInvincible { get => Player.GetSharedData<bool>("IsInvincible"); set { Player.SetSharedData("IsInvincible", value); Player.SetInvincible(value); } }

        /// <summary>Текущая анимация игрока (General)</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public Sync.Animations.GeneralTypes GeneralAnim { get => (Sync.Animations.GeneralTypes)Player.GetSharedData<int>("Anim::General"); set { Player.SetSharedData("Anim::General", (int)value); } }

        /// <summary>Текущая анимация игрока (Other)</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public Sync.Animations.OtherTypes OtherAnim { get => (Sync.Animations.OtherTypes)Player.GetSharedData<int>("Anim::Other"); set { Player.SetSharedData("Anim::Other", (int)value); } }

        /// <summary>Текущая походка игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public Sync.Animations.WalkstyleTypes Walkstyle { get => (Sync.Animations.WalkstyleTypes)Player.GetSharedData<int>("Walkstyle"); set { Player.SetSharedData("Walkstyle", (int)value); } }

        /// <summary>Текущая эмоция игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public Sync.Animations.EmotionTypes Emotion { get => (Sync.Animations.EmotionTypes)Player.GetSharedData<int>("Emotion"); set { Player.SetSharedData("Emotion", (int)value); } }

        /// <summary>Прикрепленные объекты к игроку</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>Список объектов класса Sync.AttachSystem.AttachmentNet</value>
        public List<Sync.AttachSystem.AttachmentObjectNet> AttachedObjects { get => Player.GetSharedData<Newtonsoft.Json.Linq.JArray>(Sync.AttachSystem.AttachedObjectsKey).ToList<Sync.AttachSystem.AttachmentObjectNet>(); set { Player.SetSharedData(Sync.AttachSystem.AttachedObjectsKey, value); } }
        
        /// <summary>Прикрепленные сущности к игроку</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>Список объектов класса Sync.AttachSystem.AttachmentNet</value>
        public List<Sync.AttachSystem.AttachmentEntityNet> AttachedEntities { get => Player.GetSharedData<Newtonsoft.Json.Linq.JArray>(Sync.AttachSystem.AttachedEntitiesKey).ToList<Sync.AttachSystem.AttachmentEntityNet>(); set { Player.SetSharedData(Sync.AttachSystem.AttachedEntitiesKey, value); } }

        /// <summary>Прикрепленные объекты к игроку, которые находятся в руках</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>Список объектов класса Sync.AttachSystem.AttachmentNet</value>
        public List<Sync.AttachSystem.AttachmentObjectNet> ObjectsInHand
        {
            get
            {
                return AttachedObjects.Where(x => !Sync.AttachSystem.StaticObjectsTypes.Contains(x.Type)).ToList();
            }
        }
        #endregion

        #region Stuff
        /// <summary>Занят ли игрок?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool IsBusy
        {
            get
            {
                return PhoneOn || CurrentContainer != null || IsAttachedTo != null || CurrentBusiness != null;
            }
        }

        public bool BlockRemoteCalls { get; set; }

        public byte SpamCounter { get; set; }

        public AccountData AccountData { get; set; }
        #endregion

        public PlayerData(Player Player)
        {
            this.Player = Player;

            BlockRemoteCalls = false;
            SpamCounter = 0;

            FastAnim = Sync.Animations.FastTypes.None;
            GeneralAnim = Sync.Animations.GeneralTypes.None;
            OtherAnim = Sync.Animations.OtherTypes.None;

            Walkstyle = Sync.Animations.WalkstyleTypes.None;
            Emotion = Sync.Animations.EmotionTypes.None;

            LastDamageTime = DateTime.MinValue;

            Listeners = new List<Player>();

            IsAttachedTo = null;

            CurrentContainer = null;
            CurrentBusiness = null;
            CurrentHouse = null;

            IsWounded = false;

            Hat = null;
            Masked = false;
            IsKnocked = false;
            CrouchOn = false;
            CrawlOn = false;
            VoiceRange = 0f;
            BeltOn = false;
            PhoneOn = false;
            VehicleSeat = -1;

            IsInvisible = false;
            IsInvincible = false;

            AttachedEntities = new List<Sync.AttachSystem.AttachmentEntityNet>();
            AttachedObjects = new List<Sync.AttachSystem.AttachmentObjectNet>();

            Player.SetData(Sync.AttachSystem.AttachedObjectsIDsKey, new Queue<int>());
            Player.SetData(Sync.AttachSystem.AttachedObjectsCancelsKey, new Dictionary<int, CancellationTokenSource>());

            IsInvalid = false;

            Player.SetData("CharacterNotReady", true);
        }

        public PlayerData(Player Player, PlayerInfo Info) : this(Player)
        {
            this.Info = Info;

            CID = Info.CID;

            Sex = Info.Sex;

            AdminLevel = Info.AdminLevel;
            Fraction = Info.Fraction;
            OrganisationID = Info.OrganisationID;

            Cash = Info.Cash;
            Satiety = Info.LastData.Satiety;
            Mood = Info.LastData.Mood;

            LastJoinDate = Utils.GetCurrentTime();

            Info.PlayerData = this;

            BankBalance = Info.BankAccount?.Balance ?? 0;
        }

        public PlayerData(Player Player, string name, string surname, int age, bool sex, HeadBlend hBlend, Dictionary<int, HeadOverlay> hOverlays, float[] faceFeatures, byte eyeColor, Game.Data.Customization.HairStyle hStyle, Game.Items.Clothes[] clothes) : this(Player)
        {
            Info = new PlayerInfo();

            CID = PlayerInfo.MoveNextId();

            Name = name;
            Surname = surname;
            BirthDate = Utils.GetCurrentTime().Subtract(new TimeSpan(365 * age, 0, 0, 0, 0));
            Sex = sex;

            AdminLevel = -1;
            Fraction = FractionTypes.None;
            OrganisationID = -1;
            BankAccount = null;
            LastJoinDate = Utils.GetCurrentTime();
            CreationDate = LastJoinDate;
            TimePlayed = 0;

            OwnedVehicles = new List<VehicleData.VehicleInfo>();

            Cash = Settings.CHARACTER_DEFAULT_MONEY_CASH;
            Satiety = Settings.CHARACTER_DEFAULT_SATIETY;
            Mood = Settings.CHARACTER_DEFAULT_MOOD;

            Skills = Settings.CHARACTER_DEFAULT_SKILLS;
            Licenses = Settings.CHARACTER_DEFAULT_LICENSES;

            LastData = new LastPlayerData() { Dimension = Utils.Dimensions.Main, Position = Utils.DefaultSpawnPosition, Heading = Utils.DefaultSpawnHeading, Health = 100, SessionTime = 0, Mood = Mood, Satiety = Satiety };

            Gifts = new List<Game.Items.Gift>();

            HeadBlend = hBlend;
            HeadOverlays = hOverlays;
            FaceFeatures = faceFeatures;
            EyeColor = eyeColor;
            HairStyle = hStyle;
            Decorations = new List<Decoration>();

            Items = new Game.Items.Item[20];
            Clothes = clothes;
            Accessories = new Game.Items.Clothes[8];
            Weapons = new Game.Items.Weapon[2];
            Bag = null;
            Holster = null;
            Armour = null;

            Familiars = new List<uint>();

            Punishments = new List<Punishment>();

            Info.PlayerData = this;

            BankBalance = 0;

            PlayerInfo.Add(Info);
        }

        /// <summary>Метод обозначает готовность персонажа к игре</summary>
        public void SetReady()
        {
            Player.Name = Name + " " + Surname;

            UpdateCustomization();
            UpdateClothes();

            Player.TriggerEvent("Players::CloseAuth");

            foreach (var vInfo in OwnedVehicles)
                vInfo.Spawn();

            JArray inventory = new JArray()
            {
                Weapons.Select(x => Game.Items.Item.ToClientJson(x, CEF.Inventory.Groups.Weapons)).SerializeToJson(),
                Game.Items.Item.ToClientJson(Armour, CEF.Inventory.Groups.Armour),
                Items.Select(x => Game.Items.Item.ToClientJson(x, CEF.Inventory.Groups.Items)).SerializeToJson(),
                Clothes.Select(x => Game.Items.Item.ToClientJson(x, CEF.Inventory.Groups.Clothes)).SerializeToJson(),
                Accessories.Select(x => Game.Items.Item.ToClientJson(x, CEF.Inventory.Groups.Accessories)).SerializeToJson(),
                Game.Items.Item.ToClientJson(Bag, CEF.Inventory.Groups.BagItem),
                Game.Items.Item.ToClientJson(Holster, CEF.Inventory.Groups.HolsterItem),
            };

            JObject data = new JObject();

            data.Add("Inventory", inventory);

            data.Add("Licenses", Licenses.SerializeToJson());
            data.Add("Skills", Skills.SerializeToJson());

            data.Add("TimePlayed", TimePlayed);
            data.Add("CreationDate", CreationDate);
            data.Add("BirthDate", CreationDate);

            data.Add("Org", OrganisationID == -1 ? null : "todo");

            data.Add("Familiars", Familiars.SerializeToJson());

            data.Add("Vehicles", OwnedVehicles.Select(x => $"{x.VID}_{x.ID}").SerializeToJson());
            data.Add("Businesses", OwnedBusinesses.Select(x => x.ID).SerializeToJson());
            data.Add("Houses", OwnedHouses.Select(x => x.ID).SerializeToJson());

            data.Add("Gifts", Gifts.ToDictionary(x => x.ID, x => ((int)x.Type, x.GID, x.Amount, (int)x.SourceType)).SerializeToJson()); // to change!

            NAPI.Task.Run(() =>
            {
                if (Player?.Exists != true)
                    return;

                Player.TriggerEvent("Players::CharacterPreload", Settings.SettingsToClientStr, data.SerializeToJson());

                Player.SetAlpha(255);

                Additional.AntiCheat.SetPlayerHealth(Player, LastData.Health);

                Player.Heading = LastData.Heading;

                Player.Teleport(LastData.Position, true);

                Player.SkyCameraMove(Additional.SkyCamera.SwitchTypes.ToPlayer, false, "Players::CharacterReady");

                Player.TriggerEvent("FadeScreen", false);
            }, 1000);
        }

        /// <summary>Метод раздевает игрока и надевает всю текущую одежду</summary>
        public void UpdateClothes()
        {
            CEF.CharacterCreation.Undress(Player, Sex);

            foreach (var x in Clothes)
                x?.Wear(this);

            foreach (var x in Accessories)
                x?.Wear(this);

            Armour?.Wear(this);
            Bag?.Wear(this);
            Holster?.Wear(this);
        }

        public void UpdateWeapons()
        {
            foreach (var x in Weapons)
                x?.Wear(this);
        }

        /// <summary>Метод обновляет кастомизацию игрока</summary>
        public void UpdateCustomization()
        {
            var hairStyle = HairStyle;

            Player.SetCustomization(Sex, HeadBlend, EyeColor, hairStyle.Color, hairStyle.Color2, FaceFeatures, HeadOverlays, Decorations.ToArray());

            Player.SetClothes(2, Game.Data.Customization.GetHair(Sex, hairStyle.ID), 0);

            Player.SetSharedData("Customization::HairOverlay", hairStyle.Overlay);
        }
    }
}
