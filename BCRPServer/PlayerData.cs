using GTANetworkAPI;
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
        public static Dictionary<Player, PlayerData> Players { get; private set; } = new Dictionary<Player, PlayerData>();

        /// <summary>Получить PlayerData игрока</summary>
        /// <returns>Объект класса PlayerData если существует, иначе - null</returns>
        public static PlayerData Get(Player player)
        {
            if (player == null)
                return null;

            return Players.ContainsKey(player) ? Players[player] : null;
        }

        /// <summary>Назначить объект класса PlayerData игроку</summary>
        public static void Set(Player player, PlayerData data)
        {
            if (player == null)
                return;

            if (Players.ContainsKey(player))
                Players[player] = data;
            else
                Players.Add(player, data);
        }

        public void Remove()
        {
            if (Player == null)
                return;

            Players.Remove(Player);

            Player.ResetData();
        }

        public void Delete()
        {
            this.Semaphore?.Dispose();
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
        #endregion

        #region Subclasses
        public class Prototype
        {
            public int CID;
            public string Name;
            public string Surname;
            public DateTime CreationDate { get; set; }
            public DateTime LastJoinDate { get; set; }
            public bool IsOnline { get; set; } = true;
            public int TimePlayed { get; set; } = 0;
            public bool Sex { get; set; } = true;
            public DateTime BirthDate { get; set; }
            public FractionTypes Fraction { get; set; } = FractionTypes.None;
            public int OrganisationID { get; set; } = -1;
            public int Cash { get; set; } = 550;
            public Game.Bank.Account BankAccount { get; set; } = null;
            public List<Punishment> Punishments { get; set; }

            public Prototype() { }
        }

        public class PlayerInfo
        {
            public int CID { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public DateTime LastJoinDate { get; set; }
            public FractionTypes Fraction { get; set; }
            public int OrganisationID { get; set; }
            public List<Punishment> Punishments { get; set; }

            public PlayerInfo(int CID, string Name, string Surname, DateTime LastJoinDate, FractionTypes Fraction, int OrganisationID, List<Punishment> Punishments)
            {
                this.CID = CID;
                this.Name = Name;
                this.Surname = Surname;
                this.LastJoinDate = LastJoinDate;
                this.Fraction = Fraction;
                this.OrganisationID = OrganisationID;

                this.Punishments = Punishments;
            }
        }

        public class Punishment
        {
            public enum Types
            {
                Ban = 0,
                Warn = 1,
                Mute = 2,
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
            public uint Dimension;
            /// <summary>Последние координаты</summary>
            public Vector3 Position;
            /// <summary>Последний поворот</summary>
            public float Heading;
            /// <summary>Последнее здоровье</summary>
            public int Health;

            /// <summary>Время в секундах, наигранное за последнюю сессию</summary>
            /// <remarks>Обнуляется каждый час</remarks>
            public int SessionTime;

            /// <summary>Был ли игрок Knocked?</summary>
            public bool Knocked;
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
        public Player Player = null;

        /// <summary>Список наказаний игрока</summary>
        public List<Punishment> Punishments;

        /// <summary>Банковский счёт игрока</summary>
        public Game.Bank.Account BankAccount;

        #region Local Data
        /// <summary>Информация об игроке с момента последнего захода на сервер</summary>
        public LastPlayerData LastData;

        /// <summary>Дата последнего захода игрока на сервер</summary>
        public DateTime LastJoinDate;

        /// <summary>Текущее оружие</summary>
        /// <value>Объект класса Game.Items.Weapon, null - если ничего</value>
        public (Game.Items.Weapon WeaponItem, CEF.Inventory.Groups Group, int Slot)? ActiveWeapon
        {
            get
            {
                if (Weapons == null)
                    return null;

                if (Weapons[0]?.Equiped == true)
                    return (Weapons[0], CEF.Inventory.Groups.Weapons, 0);
                else if (Weapons[1]?.Equiped == true)
                    return (Weapons[1], CEF.Inventory.Groups.Weapons, 1);
                else if ((Holster?.Items[0] as Game.Items.Weapon)?.Equiped == true)
                    return (Holster.Items[0] as Game.Items.Weapon, CEF.Inventory.Groups.Holster, 2);

                return null;
            }
        }

        /// <summary>Знакомые игроки</summary>
        /// <value>Список CID игроков</value>
        public List<int> Familiars { get; set; }

        /// <summary>Сущность, к которой прикреплен игрок</summary>
        public (Entity Entity, Sync.AttachSystem.Types Type)? IsAttachedTo { get => Player.GetData<(Entity, Sync.AttachSystem.Types)?>("IsAttachedTo::Entity"); set { Player.SetData("IsAttachedTo::Entity", value); } }

        /// <summary>Транспорт, находящийся в собственности у игрока</summary>
        /// <value>Список VID</value>
        public List<int> OwnedVehicles { get; set; }

        /// <summary>Текущий контейнер, который смотрит игрок</summary>
        /// <value>UID контейнера, null - если отсутствует</value>
        public uint? CurrentContainer { get; set; }

        /// <summary>Текущий бизнес, с которым взаимодействует игрок</summary>
        /// <value>UID бизнеса, null - если отсутствует</value>
        public int? CurrentBusiness { get; set; }

        /// <summary>Текущий дом, с которым взаимодействует игрок</summary>
        /// <value>UID дома, null - если отсутствует</value>
        public int? CurrentHouse { get; set; }

        #region Customization
        public HeadBlend HeadBlend { get; set; }

        public Dictionary<int, HeadOverlay> HeadOverlays { get; set; }

        public float[] FaceFeatures { get; set; }

        public List<Decoration> Decorations { get; set; }

        public Game.Data.Customization.HairStyle HairStyle { get; set; }

        public byte EyeColor { get; set; }
        #endregion

        #region Inventory
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
        public Game.Items.BodyArmour Armour { get; set; }

        /// <summary>Текущее оружие игрока (не включает в себя кобуру), которое было временно снято сервером</summary>
        /// <value>Массив объектов класса Game.Items.Weapon, в котором null - пустой слот</value>
        public Game.Items.Weapon[] TempWeapons { get; set; }
        #endregion

        public List<Game.Items.Gift> Gifts { get; set; }

        /// <summary>Активное предложение игрока</summary>
        public Task<Sync.Offers.Offer> ActiveOffer { get => Sync.Offers.Offer.GetAsync(this); }

        /// <summary>Список игроков, которые являются слушателями (микрофон)</summary>
        public List<Player> Listeners { get; set; }

        /// <summary>Время получения последнего урона от оружия</summary>
        public DateTime LastDamageTime { get; set; }

        /// <summary>Наигранное время игрока</summary>
        /// <value>Кол-во минут</value>
        public int TimePlayed { get; set; }


        /// <summary>Дата создания игрока</summary>
        public DateTime CreationDate { get; set; }


        /// <summary>Дата рождения игрока</summary>
        public DateTime BirthDate { get; set; }

        /// <summary>Имя игрока</summary>
        public string Name { get; set; }

        /// <summary>Фамилия игрока</summary>
        public string Surname { get; set; }

        /// <summary>Навыки игрока</summary>
        /// <value>Словарь, где ключ - enum SkilType, а значение - от 0 до 100</value>
        public Dictionary<SkillTypes, int> Skills { get; set; }

        /// <summary>Лицензии игрока</summary>
        /// <value>Список enum LicenseType</value>
        public List<LicenseTypes> Licenses { get; set; }

        public void AddFamiliar(PlayerData tData)
        {
            var pCid = CID;
            var tCid = tData.CID;

            if (!Familiars.Contains(tCid)) ;
            {
                Player?.TriggerEvent("Player::Familiars::Update", true, tCid);
            }

            if (!tData.Familiars.Contains(pCid))
            {
                tData.Player?.TriggerEvent("Player::Familiars::Update", true, pCid);
            }
        }

        public void RemoveFamiliar(PlayerData tData)
        {
            var pCid = CID;
            var tCid = tData.CID;

            if (Familiars.Remove(tCid)) ;
            {
                Player?.TriggerEvent("Player::Familiars::Update", false, tCid);
            }

            if (tData.Familiars.Remove(pCid))
            {
                tData.Player?.TriggerEvent("Player::Familiars::Update", false, pCid);
            }
        }

        public async Task AddLicense(LicenseTypes lType)
        {
            if (Licenses.Contains(lType))
                return;

            Licenses.Add(lType);

            await NAPI.Task.RunAsync(() => Player?.TriggerEvent("Player::Licenses::Update", true, lType));
        }

        public async Task RemoveLicense(LicenseTypes lType)
        {
            if (!Licenses.Remove(lType))
                return;

            await NAPI.Task.RunAsync(() => Player?.TriggerEvent("Player::Licenses::Update", false, lType));
        }

        public async Task UpdateSkill(SkillTypes sType, int updValue)
        {
            updValue = Skills[sType] + updValue;

            if (updValue > MaxSkills[sType])
                updValue = MaxSkills[sType];
            else if (updValue < 0)
                updValue = 0;

            Skills[sType] = updValue;

            await NAPI.Task.RunAsync(() => Player?.TriggerEvent("Player::Skills::Update", sType, updValue));
        }
        #endregion

        #region Own Shared Data
        /// <summary>Сытость игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>От 0 до 100</value>
        public int Satiety { get => Player.GetOwnSharedData<int>("Satiety"); set { Player.SetOwnSharedData("Satiety", value); } }

        /// <summary>Настроение игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>От 0 до 100</value>
        public int Mood { get => Player.GetOwnSharedData<int>("Mood"); set { Player.SetOwnSharedData("Mood", value); } }

        public int BankBalance { get => Player.GetOwnSharedData<int>("BankBalance"); set { Player.SetOwnSharedData("BankBalance", value); } }

        /// <summary>Наличные игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>Кол-во наличных средств</value>
        public int Cash { get => Player.GetOwnSharedData<int>("Cash"); set { Player.SetOwnSharedData("Cash", value); } }

        /// <summary>Организация игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>ID организации, -1 - если отсутствует</value>
        public int OrganisationID { get => Player.GetOwnSharedData<int>("OrganisationID"); set { Player.SetOwnSharedData("OrganisationID", value); } }

        /// <summary>Пристёгнут ли игрок?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool BeltOn { get => Player.GetOwnSharedData<bool>("Belt::On"); set { Player.SetOwnSharedData("Belt::On", value); } }

        /// <summary>Ранен ли игрок?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool IsWounded { get => Player.GetOwnSharedData<bool>("IsWounded"); set { Player.SetOwnSharedData("IsWounded", value); } }

        /// <summary>Текущая анимация игрока (Fast)</summary>
        /// <remarks>НЕ синхронизуется с игроками ВНЕ зоны стрима (т.к. проигрывается быстро)</remarks>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public Sync.Animations.FastTypes FastAnim { get => (Sync.Animations.FastTypes)Player.GetOwnSharedData<int>("Anim::Fast"); set { Player.SetOwnSharedData("Anim::Fast", (int)value); } }

        /// <summary>Метод для изменения кол-ва наличных у игрока</summary>
        /// <param name="value">Кол-во (- или +)</param>
        /// <param name="notify">Уведомить ли игрока?</param>
        /// <returns>true, если операция успешна, false - в противном случае</returns>
        public async ValueTask<bool> AddCash(int value, bool notify = true)
        {
            if (!await NAPI.Task.RunAsync(() =>
            {
                if (Player?.Exists != true)
                    return false;

                var oldValue = Cash;

                if (oldValue + value < 0)
                {
                    if (notify)
                        Player.Notify("Cash::NotEnough", oldValue);

                    return false;
                }

                Cash = oldValue + value;

                return true;
            }))
                return false;

            await Task.Run(() => MySQL.SaveCharacter(this, true));

            return true;
        }

        public PlayerInfo Info { get; set; }
        #endregion

        #region Shared Data
        /// <summary>CID игрока</summary>
        /// <remarks>Т.к. может использоваться для сохранения данных в БД, set - в основном потоке, get - в любом</remarks>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public int CID { get => Info.CID; set { Player.SetSharedData("CID", value); } }

        /// <summary>Пол игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>true - мужчина, false - женщина</value>
        public bool Sex { get => Player.GetSharedData<bool>("Sex"); set { Player.SetSharedData("Sex", value); Player.SetSkin(value ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01); } }

        /// <summary>Фракция игрока</summary>
        /// <remarks>Также вызывает событие Players::SetFraction на клиенте игроков в зоне стрима</remarks>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public FractionTypes Fraction { get => (FractionTypes)Player.GetSharedData<int>("Fraction"); set { Player.SetSharedData("Fraction", value); } }

        /// <summary>В маске ли игрок?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool Masked { get => Player.GetSharedData<bool>("Masked"); set { Player.SetSharedData("Masked", value); } }

        /// <summary>Без сознания ли игрок?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool Knocked { get => Player.GetSharedData<bool>("Knocked"); set { Player.SetSharedData("Knocked", value); } }

        /// <summary>Приседает ли игрок?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool CrouchOn { get => Player.GetSharedData<bool>("Crouch::On"); set { Player.SetSharedData("Crouch::On", value); } }

        /// <summary>Ползет ли игрок?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool CrawlOn { get => Player.GetSharedData<bool>("Crawl::On"); set { Player.SetSharedData("Crawl::On", value); } }

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

        /// <summary>Показывает ли игрок пальцем?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool IsFingerPointing { get => Player.GetSharedData<bool>("IsFingerPointing"); set { Player.SetSharedData("IsFingerPointing", value); } }

        /// <summary>Использует ли игрок телефон?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool PhoneOn { get => Player.GetSharedData<bool>("Phone::On"); set { Player.SetSharedData("Phone::On", value); } }

        /// <summary>Уровень администратора игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public int AdminLevel { get => Player.GetSharedData<int>("AdminLevel"); set { Player.SetSharedData("AdminLevel", value); } }

        /// <summary>Место в транспорте, на котором сидит игрок</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public int VehicleSeat { get => Player.GetSharedData<int>("VehicleSeat"); set { Player.SetSharedData("VehicleSeat", value); } }

        /// <summary>Текущая шапка игрока, необходимо для нормального отображения в игре при входе/выходе из транспорта</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public string Hat { get => Player.GetSharedData<string>("Hat"); set { Player.SetSharedData("Hat", value); } }

        /// <summary>Является ли игрок невидимым?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool IsInvisible { get => Player.GetSharedData<bool>("IsInvisible"); set { Player.SetSharedData("IsInvisible", value); } }

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
                return AttachedObjects.Where(x => x.Type != Sync.AttachSystem.Types.WeaponRightTight && x.Type != Sync.AttachSystem.Types.WeaponLeftTight && x.Type != Sync.AttachSystem.Types.WeaponRightBack && x.Type != Sync.AttachSystem.Types.WeaponLeftBack).ToList();
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

        public SemaphoreSlim Semaphore { get; set; }
        #endregion

        public PlayerData(Player Player)
        {
            this.Player = Player;

            Semaphore = new SemaphoreSlim(1, 1);

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
            Knocked = false;
            CrouchOn = false;
            CrawlOn = false;
            VoiceRange = 0f;
            BeltOn = false;
            IsFingerPointing = false;
            PhoneOn = false;
            VehicleSeat = -1;

            IsInvisible = false;
            IsInvincible = false;

            BankBalance = 0; // todo

            AttachedEntities = new List<Sync.AttachSystem.AttachmentEntityNet>();
            AttachedObjects = new List<Sync.AttachSystem.AttachmentObjectNet>();

            IsInvalid = false;
        }

        public void New(string name, string surname, int age, bool sex, HeadBlend hBlend, Dictionary<int, HeadOverlay> hOverlays, float[] faceFeatures, byte eyeColor, Game.Data.Customization.HairStyle hStyle)
        {
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

            OwnedVehicles = new List<int>();

            Cash = Settings.CHARACTER_DEFAULT_MONEY_CASH;
            Satiety = Settings.CHARACTER_DEFAULT_SATIETY;
            Mood = Settings.CHARACTER_DEFAULT_MOOD;

            Skills = Settings.CHARACTER_DEFAULT_SKILLS;
            Licenses = Settings.CHARACTER_DEFAULT_LICENSES;

            LastData = new LastPlayerData() { Dimension = Utils.Dimensions.Main, Position = Utils.DefaultSpawnPosition, Heading = Utils.DefaultSpawnHeading, Health = 100 };

            Gifts = new List<Game.Items.Gift>();

            HeadBlend = hBlend;
            HeadOverlays = hOverlays;
            FaceFeatures = faceFeatures;
            EyeColor = eyeColor;
            HairStyle = hStyle;
            Decorations = new List<Decoration>();

            Items = new Game.Items.Item[20]; 
            Clothes = new Game.Items.Clothes[5];
            Accessories = new Game.Items.Clothes[8];
            Weapons = new Game.Items.Weapon[2];
            Bag = null;
            Holster = null;
            Armour = null;

            Familiars = new List<int>();

            Punishments = new List<Punishment>();
        }

        /// <summary>Метод обозначает готовность персонажа к игре</summary>
        public async Task SetReady()
        {
            if (!await this.WaitAsync())
                return;

            var res = await NAPI.Task.RunAsync(() =>
            {
                if (Player?.Exists != true)
                    return false;

                Player.Name = Name + " " + Surname;

                UpdateCustomization();
                UpdateClothes();

                Player.TriggerEvent("Players::CloseAuth");

                return true;
            });

            if (!res)
                return;

            await Task.Run(async () =>
            {
                foreach (var vid in OwnedVehicles)
                    await VehicleData.Load(vid);

                string[] inventory = new string[7];

                inventory[0] = Weapons.Select(x => Game.Items.Item.ToClientJson(x, CEF.Inventory.Groups.Weapons)).SerializeToJson();
                inventory[1] = Game.Items.Item.ToClientJson(Armour, CEF.Inventory.Groups.Armour);
                inventory[2] = Items.Select(x => Game.Items.Item.ToClientJson(x, CEF.Inventory.Groups.Items)).SerializeToJson();
                inventory[3] = Clothes.Select(x => Game.Items.Item.ToClientJson(x, CEF.Inventory.Groups.Clothes)).SerializeToJson();
                inventory[4] = Accessories.Select(x => Game.Items.Item.ToClientJson(x, CEF.Inventory.Groups.Accessories)).SerializeToJson();
                inventory[5] = Game.Items.Item.ToClientJson(Bag, CEF.Inventory.Groups.BagItem);
                inventory[6] = Game.Items.Item.ToClientJson(Holster, CEF.Inventory.Groups.HolsterItem);

                var licenses = Licenses.SerializeToJson();
                var skills = Skills.SerializeToJson();

                var info = ((TimePlayed, CreationDate, BirthDate)).SerializeToJson();

                var vehicles = (await NAPI.Task.RunAsync(() => OwnedVehicles.Select(x => Utils.FindVehicleOnline(x)?.ID))).SerializeToJson();

                var gifts = Gifts.Select(x => (x.ID, (int)x.Type, x.GID, x.Amount, (int)x.SourceType)).SerializeToJson();

                await NAPI.Task.RunAsync(() =>
                {
                    if (Player?.Exists != true)
                        return;

                    Player.TriggerEvent("Players::CharacterPreload", Settings.SettingsToClientStr, Game.Businesses.Business.AllNames, Familiars, licenses, skills, inventory, info, vehicles, gifts);

                    Player.SetTransparency(255);

                    Additional.AntiCheat.SetPlayerHealth(Player, LastData.Health);

                    Player.Heading = LastData.Heading;

                    Player.Teleport(LastData.Position, true);

                    Player.SkyCameraMove(Additional.SkyCamera.SwitchTypes.ToPlayer, false, "Players::CharacterReady");

                    Player.TriggerEvent("FadeScreen", false);
                }, 1000);
            });

            this.Release();
        }

        /// <summary>Метод раздевает игрока и надевает всю текущую одежду</summary>
        public void UpdateClothes()
        {
            CEF.CharacterCreation.Undress(Player, Sex);

            foreach (var x in Clothes)
                x?.Wear(Player);

            foreach (var x in Accessories)
                x?.Wear(Player);

            Armour?.Wear(Player);
            Bag?.Wear(Player);
            Holster?.Wear(Player);
        }

        public void UpdateWeapons()
        {
            foreach (var x in Weapons)
                x?.Wear(Player);
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
