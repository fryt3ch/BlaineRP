using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BlaineRP.Server.Extensions.System;
using BlaineRP.Server.Extensions.System.Collections.Generic;
using BlaineRP.Server.Game.Achievements;
using BlaineRP.Server.Game.BankSystem;
using BlaineRP.Server.Game.EntitiesData.Players.Customization;
using BlaineRP.Server.Game.EntitiesData.Vehicles;
using BlaineRP.Server.Game.Estates;
using BlaineRP.Server.Game.Fractions;
using BlaineRP.Server.Game.Gifts;
using BlaineRP.Server.Game.Management.Cooldowns;
using BlaineRP.Server.Game.Management.Punishments;
using BlaineRP.Server.Game.Phone;
using BlaineRP.Server.Game.Quests;

namespace BlaineRP.Server.Game.EntitiesData.Players
{
    public class PlayerInfo
    {
        private static readonly Dictionary<uint, PlayerInfo> _all = new Dictionary<uint, PlayerInfo>();

        public static IReadOnlyDictionary<uint, PlayerInfo> All => _all.AsReadOnly();

        public static readonly UtilsT.UidHandlers.UInt32 UidHandler = new UtilsT.UidHandlers.UInt32(Properties.Settings.Profile.Current.Game.CIDBaseOffset);

        public static PlayerInfo Get(uint cid)
        {
            return _all.GetValueOrDefault(cid);
        }

        public PlayerData PlayerData { get; set; }

        public uint CID { get; private set; }

        public uint AID { get; private set; }

        public DateTime CreationDate { get; private set; }

        public int AdminLevel { get; private set; }

        public DateTime LastJoinDate { get; set; }

        public bool IsOnline => PlayerData != null;

        public TimeSpan TimePlayed { get; set; }

        public string Name { get; private set; }

        public string Surname { get; private set; }

        public bool Sex { get; private set; }

        public DateTime BirthDate { get; private set; }

        private readonly HashSet<LicenseType> _licenses;
        public IReadOnlyCollection<LicenseType> Licenses => _licenses.AsReadOnly();

        public Game.Fractions.FractionType Fraction { get; set; }

        public byte FractionRank { get; set; }

        public int OrganisationID { get; set; }

        public ulong Cash { get; set; }

        public uint PhoneNumber { get; set; }

        public uint PhoneBalance { get; set; }

        public uint CasinoChips { get; set; }

        public BankAccount BankAccount { get; set; }

        public LastPlayerData LastData { get; set; }

        private readonly HashSet<uint> _familiars;

        public IReadOnlyCollection<uint> Familiars => _familiars.AsReadOnly();

        private readonly Dictionary<SkillTypes, int> _skills;
        public IReadOnlyDictionary<SkillTypes, int> Skills => _skills.AsReadOnly();

        public List<Punishment> Punishments { get; set; }

        public List<Gift> Gifts { get; set; }

        private readonly Dictionary<uint, string> _contacts;
        public IReadOnlyDictionary<uint, string> Contacts => _contacts.AsReadOnly();

        private readonly HashSet<uint> _phoneBlacklist;
        public IReadOnlyCollection<uint> PhoneBlacklist => _phoneBlacklist.AsReadOnly();

        public HeadBlend HeadBlend { get; set; }

        public Dictionary<int, HeadOverlay> HeadOverlays { get; set; }

        public float[] FaceFeatures { get; set; }

        public List<int> Decorations { get; set; }

        public HairStyle HairStyle { get; set; }

        public byte EyeColor { get; set; }

        public IEnumerable<VehicleInfo> OwnedVehicles => VehicleInfo.All.Values.Where(x => x != null && (x.OwnerType == OwnerTypes.Player && x.OwnerID == CID));

        public IEnumerable<Game.Estates.House> OwnedHouses => Game.Estates.House.All.Values.Where(x => x.Owner == this);

        public IEnumerable<Game.Estates.Apartments> OwnedApartments => Game.Estates.Apartments.All.Values.Where(x => x.Owner == this);

        public IEnumerable<Game.Estates.Garage> OwnedGarages => Game.Estates.Garage.All.Values.Where(x => x.Owner == this);

        public IEnumerable<Game.Businesses.Business> OwnedBusinesses => Game.Businesses.Business.All.Values.Where(x => x.Owner == this);

        public Game.Estates.HouseBase SettledHouseBase => (Game.Estates.HouseBase)Game.Estates.House.All.Values.Where(x => x.Settlers.ContainsKey(this)).FirstOrDefault() ??
                                                          (Game.Estates.HouseBase)Game.Estates.Apartments.All.Values.Where(x => x.Settlers.ContainsKey(this)).FirstOrDefault();

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
        public List<Game.Items.WeaponSkin> WeaponSkins { get; set; }

        public Dictionary<uint, Cooldown> Cooldowns { get; set; }

        public List<SMS> AllSMS { get; set; } = new List<SMS>();

        public Dictionary<AchievementType, Achievement> Achievements { get; set; }

        public Dictionary<QuestType, Quest> Quests { get; set; }

        public MedicalCard MedicalCard { get; set; }

        public bool LosSantosAllowed { get; set; }

        public IEnumerable<VehicleInfo> VehiclesOnPound => OwnedVehicles.Where(x => x.VehicleData == null && x.IsOnVehiclePound);

        private static Dictionary<string, object> TempData { get; set; } = new Dictionary<string, object>();

        public void SetTempData(string key, object value)
        {
            if (!TempData.TryAdd(key, value))
                TempData[key] = value;
        }

        public T GetTempData<T>(string key, T otherwise = default)
        {
            object value;

            if (!TempData.TryGetValue(key, out value))
                return otherwise;

            if (value is T valueT)
                return valueT;

            return otherwise;
        }

        public bool HasTempData(string key)
        {
            return TempData.ContainsKey(key);
        }

        public bool ResetTempData(string key)
        {
            return TempData.Remove(key);
        }

        public bool HasCooldown(uint hash, DateTime currentDate, out TimeSpan timePassed, out TimeSpan timeLeft, out DateTime startDate, double timeFactor = 1d)
        {
            Cooldown cdObj;

            if (Cooldowns.TryGetValue(hash, out cdObj))
            {
                return cdObj.IsActive(currentDate, out timePassed, out timeLeft, out startDate, timeFactor);
            }
            else
            {
                timeLeft = TimeSpan.Zero;
                timePassed = TimeSpan.MaxValue;
                startDate = DateTime.MinValue;

                return false;
            }
        }

        public void SetCooldown(uint hash, DateTime startDate, TimeSpan time, bool saveDb)
        {
            Cooldown cdObj = Cooldowns.GetValueOrDefault(hash);

            if (cdObj != null)
            {
                cdObj.Update(startDate, time);

                if (saveDb)
                    MySQL.CharacterCooldownSet(this, hash, cdObj, false);
            }
            else
            {
                cdObj = new Cooldown(startDate, time);

                Cooldowns.Add(hash, cdObj);

                if (saveDb)
                    MySQL.CharacterCooldownSet(this, hash, cdObj, true);
            }
        }

        public bool RemoveCooldown(uint hash)
        {
            Cooldown cdObj;

            if (Cooldowns.Remove(hash, out cdObj))
            {
                MySQL.CharacterCooldownRemoveByGuid(this, cdObj.Guid);

                return true;
            }

            return false;
        }

        public bool TryAddCash(ulong amount, out ulong newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            if (!Cash.TryAdd(amount, out newBalance))
            {
                if (notifyOnFault)
                    if (PlayerData != null)
                    {
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
                    if (PlayerData != null)
                        PlayerData.Player.Notify("Cash::NotEnough", Cash);

                return false;
            }

            return true;
        }

        public void SetCash(ulong value)
        {
            if (PlayerData != null)
                PlayerData.Cash = value;
            else
                Cash = value;

            MySQL.CharacterCashUpdate(this);
        }

        public bool TryAddPhoneBalance(uint amount, out uint newBalance, bool notifyOnFault = true)
        {
            if (!PhoneBalance.TryAdd(amount, out newBalance))
            {
                if (notifyOnFault)
                    if (PlayerData != null)
                    {
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
                    if (PlayerData != null)
                        PlayerData.Player.Notify("Phone::BNE", PhoneBalance);

                return false;
            }

            return true;
        }

        public void SetPhoneBalance(uint value)
        {
            PhoneBalance = value;

            MySQL.CharacterPhoneBalanceUpdate(this);
        }

        public void AddFurniture(params Game.Estates.Furniture[] furn)
        {
            if (PlayerData != null)
            {
                var furnJs = new List<string>();

                for (var i = 0; i < furn.Length; i++)
                {
                    Furniture x = furn[i];

                    if (Furniture.Contains(x))
                        return;

                    Furniture.Add(x);

                    furnJs.Add($"{x.UID}&{x.ID}");
                }

                PlayerData.Player.TriggerEvent("Player::Furniture::Update", true, furnJs);
            }
            else
            {
                for (var i = 0; i < furn.Length; i++)
                {
                    Furniture x = furn[i];

                    if (Furniture.Contains(x))
                        return;

                    Furniture.Add(x);
                }
            }

            MySQL.CharacterFurnitureUpdate(this);
        }

        public void RemoveFurniture(params Game.Estates.Furniture[] furn)
        {
            if (PlayerData != null)
            {
                var furnJs = new List<string>();

                for (var i = 0; i < furn.Length; i++)
                {
                    Furniture x = furn[i];

                    if (Furniture.Remove(x))
                        furnJs.Add($"{x.UID}");
                }

                PlayerData.Player.TriggerEvent("Player::Furniture::Update", false, furnJs);
            }
            else
            {
                for (var i = 0; i < furn.Length; i++)
                {
                    Furniture x = furn[i];

                    if (Furniture.Remove(x))
                    {
                    }
                }
            }

            MySQL.CharacterFurnitureUpdate(this);
        }

        public void AddFamiliar(PlayerInfo tInfo)
        {
            if (!_familiars.Add(tInfo.CID))
                return;

            if (PlayerData != null)
            {
                PlayerData.Player.TriggerEvent("Player::Familiars::Update", true, tInfo.CID);
            }

            MySQL.CharacterFamiliarsUpdate(this);
        }

        public void RemoveFamiliar(PlayerInfo tInfo)
        {
            if (!_familiars.Remove(tInfo.CID))
                return;

            if (PlayerData != null)
            {
                PlayerData.Player.TriggerEvent("Player::Familiars::Update", false, tInfo.CID);
            }

            MySQL.CharacterFamiliarsUpdate(this);
        }

        public void AddLicense(LicenseType lType)
        {
            if (!_licenses.Add(lType))
                return;

            if (PlayerData != null)
                PlayerData.Player.TriggerEvent("Player::Licenses::Update", true, lType);

            MySQL.CharacterLicensesUpdate(this);
        }

        public void RemoveLicense(LicenseType lType)
        {
            if (!_licenses.Remove(lType))
                return;

            if (PlayerData != null)
                PlayerData.Player.TriggerEvent("Player::Licenses::Update", false, lType);

            MySQL.CharacterLicensesUpdate(this);
        }

        public void AddSms(SMS sms, bool triggerAdd)
        {
            if (AllSMS.Count >= Properties.Settings.Static.PHONE_SMS_MAX_COUNT)
            {
                AllSMS.RemoveAt(0);

                if (PlayerData != null)
                    SMS.TriggerRemove(PlayerData.Player, 0);
            }

            AllSMS.Add(sms);

            if (triggerAdd && PlayerData != null)
            {
                sms.TriggerAdd(PlayerData.Player);
            }
        }

        public void UpdateSkill(SkillTypes sType, int updValue)
        {
            updValue = _skills[sType] + updValue;

            int maxSkillValue = Properties.Settings.Static.PlayerMaxSkills.GetValueOrDefault(sType);

            if (updValue > maxSkillValue)
                updValue = maxSkillValue;
            else if (updValue < 0)
                updValue = 0;

            _skills[sType] = updValue;

            if (PlayerData != null)
                PlayerData.Player.TriggerEvent("Player::Skills::Update", sType, updValue);

            MySQL.CharacterSkillsUpdate(this);
        }

        public void AddOrUpdateContact(uint number, string name)
        {
            _contacts[number] = name;

            MySQL.CharacterContactsUpdate(this);
        }

        public bool RemoveContact(uint number)
        {
            if (!_contacts.Remove(number))
                return false;

            MySQL.CharacterContactsUpdate(this);

            return true;
        }

        public bool AddPhoneToBlacklist(uint number)
        {
            if (!_phoneBlacklist.Add(number))
                return false;

            MySQL.CharacterPhoneBlacklistUpdate(this);

            return true;
        }

        public bool RemovePhoneFromBlacklist(uint number)
        {
            if (!_phoneBlacklist.Remove(number))
                return false;

            MySQL.CharacterPhoneBlacklistUpdate(this);

            return true;
        }

        private PlayerInfo(uint cid, uint aid, DateTime creationDate, int adminLevel, DateTime lastJoinDate, TimeSpan timePlayed, string name, string surname, bool sex, DateTime birthDate, HashSet<LicenseType> licenses, MedicalCard medicalCard, bool losSantosAllowed, FractionType fraction, byte fractionRank, int organisationId, ulong cash, uint phoneNumber, uint phoneBalance, Dictionary<uint, string> contacts, HashSet<uint> phoneBlacklist, LastPlayerData lastData, HashSet<uint> familiars, Dictionary<SkillTypes, int> skills, List<Punishment> punishments, uint casinoChips, HeadBlend headBlend, Dictionary<int, HeadOverlay> headOverlays, float[] faceFeatures, List<int> decorations, HairStyle hairStyle, byte eyeColor, List<Gift> gifts, Dictionary<AchievementType, Achievement> achievements, Dictionary<QuestType, Quest> quests, Dictionary<uint, Cooldown> cooldowns)
        {
            CID = cid;
            AID = aid;
            CreationDate = creationDate;
            AdminLevel = adminLevel;
            LastJoinDate = lastJoinDate;
            TimePlayed = timePlayed;
            Name = name;
            Surname = surname;
            Sex = sex;
            BirthDate = birthDate;
            _licenses = licenses;
            MedicalCard = medicalCard;
            LosSantosAllowed = losSantosAllowed;
            Fraction = fraction;
            FractionRank = fractionRank;
            OrganisationID = organisationId;
            Cash = cash;
            PhoneNumber = phoneNumber;
            PhoneBalance = phoneBalance;
            _contacts = contacts;
            _phoneBlacklist = phoneBlacklist;
            LastData = lastData;
            _familiars = familiars;
            _skills = skills;
            Punishments = punishments;
            CasinoChips = casinoChips;
            HeadBlend = headBlend;
            HeadOverlays = headOverlays;
            FaceFeatures = faceFeatures;
            Decorations = decorations;
            HairStyle = hairStyle;
            EyeColor = eyeColor;
            Gifts = gifts;
            Achievements = achievements;
            Quests = quests;
            Cooldowns = cooldowns;
        }

        public static PlayerInfo CreateExisting(uint cid, uint aid, DateTime creationDate, int adminLevel, DateTime lastJoinDate, TimeSpan timePlayed, string name, string surname, bool sex, DateTime birthDate, HashSet<LicenseType> licenses, MedicalCard medicalCard, bool losSantosAllowed, FractionType fraction, byte fractionRank, int organisationId, ulong cash, uint phoneNumber, uint phoneBalance, Dictionary<uint, string> contacts, HashSet<uint> phoneBlacklist, LastPlayerData lastData, HashSet<uint> familiars, Dictionary<SkillTypes, int> skills, List<Punishment> punishments, uint casinoChips, HeadBlend headBlend, Dictionary<int, HeadOverlay> headOverlays, float[] faceFeatures, List<int> decorations, HairStyle hairStyle, byte eyeColor, List<Gift> gifts, Dictionary<AchievementType, Achievement> achievements, Dictionary<QuestType, Quest> quests, Dictionary<uint, Cooldown> cooldowns)
        {
            var obj = new PlayerInfo(cid: cid,
                aid: aid,
                creationDate: creationDate,
                adminLevel: adminLevel,
                lastJoinDate: lastJoinDate,
                timePlayed: timePlayed,
                name: name,
                surname: surname,
                sex: sex,
                birthDate: birthDate,
                licenses: licenses,
                medicalCard: medicalCard,
                losSantosAllowed: losSantosAllowed,
                fraction: fraction,
                fractionRank: fractionRank,
                organisationId: organisationId,
                cash: cash,
                phoneNumber: phoneNumber,
                contacts: contacts,
                phoneBalance: phoneBalance,
                phoneBlacklist: phoneBlacklist,
                lastData: lastData,
                familiars: familiars,
                skills: skills,
                punishments: punishments,
                casinoChips: casinoChips,
                headBlend: headBlend,
                headOverlays: headOverlays,
                faceFeatures: faceFeatures,
                decorations: decorations,
                hairStyle: hairStyle,
                eyeColor: eyeColor,
                gifts: gifts,
                achievements: achievements,
                quests: quests,
                cooldowns: cooldowns
            );

            UidHandler.TryUpdateLastAddedMaxUid(cid);

            _all.Add(cid, obj);

            return obj;
        }

        public static PlayerInfo CreateNew(uint aid, DateTime currentDate, string name, string surname, DateTime birthDate, bool sex, LastPlayerData lastData, HeadBlend headBlend, Dictionary<int, HeadOverlay> headOverlays, float[] faceFeatures, List<int> decorations, HairStyle hairStyle, byte eyeColor, Items.Clothes[] clothes)
        {
            var cid = UidHandler.MoveNextUid();

            var obj = new PlayerInfo(cid: cid,
                aid: aid,
                creationDate: currentDate,
                adminLevel: -1,
                lastJoinDate: currentDate,
                timePlayed: TimeSpan.Zero,
                name: name,
                surname: surname,
                sex: sex,
                birthDate: birthDate,
                licenses: Properties.Settings.Static.CharacterDefaultLicenses,
                medicalCard: null,
                losSantosAllowed: false,
                fraction: FractionType.None,
                fractionRank: 0,
                organisationId: -1,
                cash: Properties.Settings.Static.CHARACTER_DEFAULT_MONEY_CASH,
                phoneNumber: Numbers.GenerateNewPhoneNumber(),
                contacts: new Dictionary<uint, string>(),
                phoneBalance: 0,
                phoneBlacklist: new HashSet<uint>(),
                lastData: lastData,
                familiars: new HashSet<uint>(),
                skills: Properties.Settings.Static.CharacterDefaultSkills,
                punishments: new List<Punishment>(),
                casinoChips: 0,
                headBlend: headBlend,
                headOverlays: headOverlays,
                faceFeatures: faceFeatures,
                decorations: decorations,
                hairStyle: hairStyle,
                eyeColor: eyeColor,
                gifts: new List<Gift>(),
                achievements: new Dictionary<AchievementType, Achievement>(),
                quests: new Dictionary<QuestType, Quest>(),
                cooldowns: new Dictionary<uint, Cooldown>()
            );

            obj.Clothes = clothes;

            MySQL.CharacterAdd(obj);

            _all.Add(cid, obj);

            return obj;
        }
    }
}