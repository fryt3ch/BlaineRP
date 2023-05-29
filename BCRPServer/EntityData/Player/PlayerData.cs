using BCRPServer.Game.Items;
using GTANetworkAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BCRPServer
{
    public partial class PlayerData
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
            Shooting = 1,
            /// <summary>Кулинария</summary>
            Cooking = 2,
            /// <summary>Рыбалка</summary>
            Fishing = 3,
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

        public static Dictionary<SkillTypes, int> MaxSkills = new Dictionary<SkillTypes, int>()
        {
            { SkillTypes.Strength, 100 },
            { SkillTypes.Shooting, 100 },
            { SkillTypes.Cooking, 100 },
            { SkillTypes.Fishing, 100 },
        };

        /// <summary>Сущность игрока</summary>
        public Player Player { get; set; }

        public void AddFamiliar(PlayerData.PlayerInfo tInfo)
        {
            var tCid = tInfo.CID;

            if (Familiars.Add(tCid))
            {
                Player.TriggerEvent("Player::Familiars::Update", true, tCid);

                MySQL.CharacterFamiliarsUpdate(Info);
            }
        }

        public void RemoveFamiliar(PlayerData.PlayerInfo tInfo)
        {
            var tCid = tInfo.CID;

            if (Familiars.Remove(tCid))
            {
                Player.TriggerEvent("Player::Familiars::Update", false, tCid);

                MySQL.CharacterFamiliarsUpdate(Info);
            }
        }

        public void AddLicense(LicenseTypes lType)
        {
            if (Licenses.Contains(lType))
                return;

            Licenses.Add(lType);

            Player.TriggerEvent("Player::Licenses::Update", true, lType);

            MySQL.CharacterLicensesUpdate(Info);
        }

        public void RemoveLicense(LicenseTypes lType)
        {
            if (!Licenses.Remove(lType))
                return;

            Player.TriggerEvent("Player::Licenses::Update", false, lType);

            MySQL.CharacterLicensesUpdate(Info);
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

            MySQL.CharacterSkillsUpdate(Info);
        }

        public void AddRentedVehicle(VehicleData vData, int timeDel)
        {
            Player.TriggerEvent("Player::RVehs::U", vData.Vehicle.Id, vData.Info.ID);
        }

        public void RemoveRentedVehicle(VehicleData vData)
        {
            Player.TriggerEvent("Player::RVehs::U", vData.Vehicle.Id);
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

        public void AddHouseProperty(Game.Estates.House house)
        {
            if (OwnedHouses.Contains(house))
                return;

            OwnedHouses.Add(house);

            Player.TriggerEvent("Player::Properties::Update", true, PropertyTypes.House, house.Id);
        }

        public void RemoveHouseProperty(Game.Estates.House house)
        {
            OwnedHouses.Remove(house);

            Player.TriggerEvent("Player::Properties::Update", false, PropertyTypes.House, house.Id);
        }

        public void AddApartmentsProperty(Game.Estates.Apartments aps)
        {
            if (OwnedApartments.Contains(aps))
                return;

            OwnedApartments.Add(aps);

            Player.TriggerEvent("Player::Properties::Update", true, PropertyTypes.Apartments, aps.Id);
        }

        public void RemoveApartmentsProperty(Game.Estates.Apartments aps)
        {
            OwnedApartments.Remove(aps);

            Player.TriggerEvent("Player::Properties::Update", false, PropertyTypes.Apartments, aps.Id);
        }

        public void AddGarageProperty(Game.Estates.Garage garage)
        {
            if (OwnedGarages.Contains(garage))
                return;

            OwnedGarages.Add(garage);

            Player.TriggerEvent("Player::Properties::Update", true, PropertyTypes.Garage, garage.Id);
        }

        public void RemoveGarageProperty(Game.Estates.Garage garage)
        {
            OwnedGarages.Remove(garage);

            Player.TriggerEvent("Player::Properties::Update", false, PropertyTypes.Garage, garage.Id);
        }

        public void UpdateMedicalCard(MedicalCard medCard)
        {
            Info.MedicalCard = medCard;

            if (medCard == null)
                Player.TriggerEvent("Player::MedCard::Update");
            else
                Player.TriggerEvent("Player::MedCard::Update", medCard.SerializeToJson());

            MySQL.CharacterMedicalCardUpdate(Info);
        }

        #region Own Shared Data

        public bool TryAddCash(ulong amount, out ulong newBalance, bool notifyOnFault = true, PlayerData tData = null) => Info.TryAddCash(amount, out newBalance, notifyOnFault, tData);

        public bool TryRemoveCash(ulong amount, out ulong newBalance, bool notifyOnFault = true, PlayerData tData = null) => Info.TryRemoveCash(amount, out newBalance, notifyOnFault, tData);

        public void SetCash(ulong value) => Info.SetCash(value);

        public bool HasBankAccount(bool notifyOnFault = true)
        {
            if (BankAccount != null)
                return true;

            if (notifyOnFault)
                Player.Notify("Bank::NoAccount");

            return false;
        }

        public bool HasLicense(LicenseTypes lType, bool notifyIfNot = true)
        {
            if (Licenses.Contains(lType))
                return true;

            if (notifyIfNot)
                Player.Notify($"Lic::N_{(int)lType}");

            return false;
        }

        public bool HasJob(bool notifyOnFault = true)
        {
            if (CurrentJob == null)
                return false;

            if (notifyOnFault)
                Player.Notify("Job::AHJ");

            return true;
        }

        public PlayerInfo Info { get; set; }
        #endregion

        #region Stuff
        /// <summary>Занят ли игрок?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool IsBusy
        {
            get
            {
                return PhoneStateType != Sync.Players.PhoneStateTypes.Off || CurrentWorkbench != null || CurrentContainer != null || IsAttachedToEntity != null || CurrentBusiness != null || IsFrozen;
            }
        }

        public bool BlockRemoteCalls { get => Player.GetData<bool?>("BlockRC") ?? false; set { if (value) Player.SetData("BlockRC", true); else Player.ResetData("BlockRC"); } }

        public byte SpamCounter { get; set; }

        public AccountData AccountData { get; set; }

        public VehicleData CurrentTuningVehicle { get => Player.GetData<VehicleData>("tsvdata"); set { if (value == null) Player.ResetData("tsvdata"); else Player.SetData("tsvdata", value); } }
        #endregion

        public PlayerData(Player Player)
        {
            this.Player = Player;

            SpamCounter = 0;

            LastDamageTime = DateTime.MinValue;

            Listeners = new List<Player>();

            VoiceRange = 0f;

            AttachedEntities = new List<Sync.AttachSystem.AttachmentEntityNet>();
            AttachedObjects = new List<Sync.AttachSystem.AttachmentObjectNet>();

            Player.SetData(Sync.AttachSystem.AttachedObjectsTimersKey, new Dictionary<Sync.AttachSystem.Types, Timer>());

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

            OwnedHouses = Info.OwnedHouses;
            OwnedGarages = Info.OwnedGarages;
            OwnedVehicles = Info.OwnedVehicles;
            OwnedApartments = Info.OwnedApartments;
            OwnedBusinesses = Info.OwnedBusinesses;

            SettledHouseBase = Info.SettledHouseBase;

            BankBalance = Info.BankAccount?.Balance ?? 0;

            Info.PlayerData = this;
        }

        public PlayerData(Player Player, uint aid, string name, string surname, int age, bool sex, Game.Data.Customization.HeadBlend hBlend, Dictionary<int, Game.Data.Customization.HeadOverlay> hOverlays, float[] faceFeatures, byte eyeColor, Game.Data.Customization.HairStyle hStyle, Game.Items.Clothes[] clothes) : this(Player)
        {
            Info = new PlayerInfo() { AID = aid };

            LastData = new LastPlayerData() { Dimension = Utils.Dimensions.Main, Position = new Utils.Vector4(Utils.DefaultSpawnPosition, Utils.DefaultSpawnHeading), Health = 100 };

            Name = name;
            Surname = surname;
            BirthDate = Utils.GetCurrentTime().Subtract(new TimeSpan(365 * age, 0, 0, 0, 0));
            Sex = sex;

            AdminLevel = -1;
            Fraction = Game.Fractions.Types.None;
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

            Gifts = new List<Game.Items.Gift>();

            HeadBlend = hBlend;
            HeadOverlays = hOverlays;
            FaceFeatures = faceFeatures;
            EyeColor = eyeColor;
            HairStyle = hStyle;
            Decorations = new List<int>();

            Items = new Game.Items.Item[20];
            Clothes = clothes;
            Accessories = new Game.Items.Clothes[8];
            Weapons = new Game.Items.Weapon[2];
            Bag = null;
            Holster = null;
            Armour = null;

            Info.PhoneNumber = Sync.Players.GenerateNewPhoneNumber();
            Info.PhoneBalance = 0;

            Info.Contacts = new Dictionary<uint, string>();
            Info.PhoneBlacklist = new List<uint>();

            Furniture = new List<Game.Estates.Furniture>();

            Info.WeaponSkins = new Dictionary<WeaponSkin.ItemData.Types, WeaponSkin>();

            Familiars = new HashSet<uint>();

            Punishments = new List<Sync.Punishment>();

            Info.PlayerData = this;

            Info.Achievements = Achievement.GetNewDict();

            Info.Quests = Sync.Quest.GetNewDict();

            BankBalance = 0;

            OwnedHouses = new List<Game.Estates.House>();
            OwnedGarages = new List<Game.Estates.Garage>();
            OwnedVehicles = new List<VehicleData.VehicleInfo>();
            OwnedApartments = new List<Game.Estates.Apartments>();
            OwnedBusinesses = new List<Game.Businesses.Business>();

            PlayerInfo.Add(Info);

            CID = Info.CID;
        }

        /// <summary>Метод обозначает готовность персонажа к игре</summary>
        public void SetReady()
        {
            Player.SetMainData(this);

            Player.Name = $"{Name} {Surname}";

            UpdateCustomization();
            UpdateClothes();

            Player.TriggerEvent("Players::CloseAuth");

            foreach (var vInfo in OwnedVehicles)
                vInfo.Spawn();

            var activePunishments = Punishments.Where(x => x.IsActive()).ToList();

            foreach (var x in activePunishments)
            {
                if (x.Type == Sync.Punishment.Types.Mute)
                {
                    IsMuted = true;
                }
            }

            NAPI.Task.Run(() =>
            {
                if (Player?.Exists != true)
                    return;

                var data = new JObject
                {
                    {
                        "Inventory",

                        new JArray()
                        {
                            Weapons.Select(x => Game.Items.Item.ToClientJson(x, Game.Items.Inventory.Groups.Weapons)).SerializeToJson(),
                            Game.Items.Item.ToClientJson(Armour, Game.Items.Inventory.Groups.Armour),
                            Items.Select(x => Game.Items.Item.ToClientJson(x, Game.Items.Inventory.Groups.Items)).SerializeToJson(),
                            Clothes.Select(x => Game.Items.Item.ToClientJson(x, Game.Items.Inventory.Groups.Clothes)).SerializeToJson(),
                            Accessories.Select(x => Game.Items.Item.ToClientJson(x, Game.Items.Inventory.Groups.Accessories)).SerializeToJson(),
                            Game.Items.Item.ToClientJson(Bag, Game.Items.Inventory.Groups.BagItem),
                            Game.Items.Item.ToClientJson(Holster, Game.Items.Inventory.Groups.HolsterItem),
                        }
                    },

                    { "PN", Info.PhoneNumber },

                    { "Licenses", Licenses.SerializeToJson() },

                    { "Skills", Skills.SerializeToJson() },

                    { "TimePlayed", TimePlayed },
                    { "CreationDate", CreationDate },
                    { "BirthDate", BirthDate },
                    { "Org", OrganisationID == -1 ? null : "todo" },
                    { "Familiars", Familiars.SerializeToJson() },

                    { "Gifts", Gifts.ToDictionary(x => x.ID, x => ((int)x.Type, x.GID, x.Amount, (int)x.SourceType)).SerializeToJson() }, // to change!

                    { "Achievements", Info.Achievements.Select(x => $"{(int)x.Key}_{x.Value.Progress}_{x.Value.TypeData.Goal}").SerializeToJson() },
                };

                if (activePunishments.Count > 0)
                    data.Add("P", JArray.FromObject(activePunishments.Select(x => $"{x.Id}&{(int)x.Type}&{x.EndDate.GetUnixTimestamp()}&{x.AdditionalData ?? ""}")));

                if (Info.Contacts.Count > 0)
                    data.Add("Conts", JObject.FromObject(Info.Contacts));

                if (Info.PhoneBlacklist.Count > 0)
                    data.Add("PBL", JArray.FromObject(Info.PhoneBlacklist));

                if (Furniture.Count > 0)
                    data.Add("Furniture", Furniture.ToDictionary(x => x.UID, x => x.ID).SerializeToJson());

                if (Info.WeaponSkins.Count > 0)
                    data.Add("WSkins", Info.WeaponSkins.Select(x => x.Value.ID).SerializeToJson());

                if (Info.AllSMS.Count > 0)
                    data.Add("SMS", Info.AllSMS.Select(x => x.Data).SerializeToJson());

                if (Info.MedicalCard != null)
                    data.Add("MedCard", Info.MedicalCard.SerializeToJson());

                if (OwnedVehicles.Count > 0)
                    data.Add("Vehicles", OwnedVehicles.Select(x => $"{x.VID}_{x.ID}").SerializeToJson());

                if (OwnedBusinesses.Count > 0)
                    data.Add("Businesses", OwnedBusinesses.Select(x => x.ID).SerializeToJson());

                if (OwnedHouses.Count > 0)
                    data.Add("Houses", OwnedHouses.Select(x => x.Id).SerializeToJson());

                if (OwnedApartments.Count > 0)
                    data.Add("Apartments", OwnedApartments.Select(x => x.Id).SerializeToJson());

                if (OwnedGarages.Count > 0)
                    data.Add("Garages", OwnedGarages.Select(x => x.Id).SerializeToJson());

                if (SettledHouseBase != null)
                    data.Add("SHB", $"{(int)SettledHouseBase.Type}_{SettledHouseBase.Id}");

                if (Info.Quests.Count > 0)
                    data.Add("Quests", Info.Quests.Where(x => !x.Value.IsCompleted).Select(x => $"{(int)x.Key}~{x.Value.Step}~{x.Value.StepProgress}~{(x.Value.CurrentData ?? "")}").SerializeToJson());

                var rentedVehs = VehicleData.All.Values.Where(x => x.OwnerID == CID && (x.OwnerType == VehicleData.OwnerTypes.PlayerRent || x.OwnerType == VehicleData.OwnerTypes.PlayerRentJob)).ToList();

                if (rentedVehs.Count > 0)
                    data.Add("RV", JArray.FromObject(rentedVehs.Select(x => $"{x.Vehicle.Id}&{x.Info.ID}").ToList()));

                Player.SetAlpha(255);

                Additional.AntiCheat.SetPlayerHealth(Player, LastData.Health);

                Player.TriggerEvent("Players::CharacterPreload", Settings.SettingsToClientStr, data);

                Player.Teleport(LastData.Position.Position, false, LastData.Dimension, LastData.Position.RotationZ, LastData.Dimension >= Utils.HouseDimBase);
            }, 1000);
        }

        /// <summary>Метод раздевает игрока и надевает всю текущую одежду</summary>
        public void UpdateClothes()
        {
            Events.Players.CharacterCreation.Undress(Player, Sex);

            foreach (var x in Clothes)
                x?.Wear(this);

            foreach (var x in Accessories)
                x?.Wear(this);

            Armour?.Wear(this);
            Bag?.Wear(this);
            Holster?.Wear(this);

            //Game.Data.Customization.ApplyUniform(this, Game.Data.Customization.UniformTypes.Farmer);
        }

        public void UpdateWeapons()
        {
            foreach (var x in Weapons)
                x?.Wear(this);
        }

        /// <summary>Метод обновляет кастомизацию игрока</summary>
        public void UpdateCustomization()
        {
            Player.SetCustomization(Sex, HeadBlend.RageHeadBlend, EyeColor, HairStyle.Color, HairStyle.Color2, FaceFeatures, HeadOverlays.ToDictionary(x => x.Key, x => x.Value.RageHeadOverlay), Game.Data.Customization.Defaults.Decorations);

            Player.SetClothes(2, Game.Data.Customization.GetHair(Sex, HairStyle.Id), 0);

            UpdateHairOverlay();

            UpdateDecorations();
        }

        public void UpdateHairOverlay()
        {
            if (HairStyle.Overlay > 0)
                Player.SetSharedData("CHO", HairStyle.Overlay);
            else
                Player.ResetSharedData("CHO");
        }

        public void UpdateDecorations()
        {
            if (Decorations.Count > 0)
                Player.SetSharedData("DCR", Decorations);
            else
                Player.ResetSharedData("DCR");
        }

        public bool CanUseInventory(bool notifyIfNot = true)
        {
            if (ActiveOffer?.TradeData != null)
            {
                return false;
            }

            if (InventoryBlocked)
            {
                if (notifyIfNot)
                    Player.Notify("Inventory::Blocked");

                return false;
            }

            return true;
        }

        public bool HasCooldown(Sync.Cooldowns.Types cdType, int notifyType = -1)
        {
            var ts = Info.GetCooldownTimeLeft(cdType);

            if (ts.TotalSeconds > 0)
            {
                if (notifyType >= 0)
                {
                    if (notifyType != 3)
                        Player.Notify($"CDown::{notifyType}");
                    else
                        Player.Notify("CDown::3", ts.GetBeautyString());
                }

                return true;
            }

            return false;
        }

        public bool CanBeSettled(Game.Estates.HouseBase houseBase, bool notifyIfNot = true)
        {
            if (SettledHouseBase != null)
            {
                if (SettledHouseBase == houseBase)
                {
                    Player.Notify("House::ASH");
                }
                else if (SettledHouseBase.Type == houseBase.Type)
                {
                    Player.Notify(houseBase.Type == Game.Estates.HouseBase.Types.House ? "House::ASOH" : "House::ASOA");
                }

                return false;
            }

            if (houseBase.Type == Game.Estates.HouseBase.Types.House)
            {
                if (OwnedHouses.Count > 0)
                {
                    Player.Notify("House::OHSE");

                    return false;
                }
            }
            else
            {
                if (OwnedApartments.Count > 0)
                {
                    Player.Notify("House::OASE");

                    return false;
                }
            }

            return true;
        }

        public void ResetUpdateTimer()
        {
            if (UpdateTimer != null)
            {
                UpdateTimer.Dispose();
            }

            UpdateTimer = new Timer((_) =>
            {
                NAPI.Task.Run(() =>
                {
                    if (Player?.Exists != true)
                        return;

                    TimePlayed += 1;
                    LastData.SessionTime += 60;

                    if (TimePlayed % 2 == 0)
                    {
                        if (Satiety > 0)
                            Satiety--;

                        if (Mood > 0)
                            Mood--;
                    }

                    foreach (var x in Punishments)
                    {
                        if (x.Type == Sync.Punishment.Types.NRPPrison || x.Type == Sync.Punishment.Types.Arrest || x.Type == Sync.Punishment.Types.FederalPrison)
                        {
                            if (!x.IsActive())
                                continue;

                            var strD = x.AdditionalData.Split('_');

                            var time = long.Parse(strD[0]) + 60;

                            if (time >= x.EndDate.GetUnixTimestamp())
                            {
                                if (x.Type == Sync.Punishment.Types.NRPPrison)
                                {
                                    Player.TriggerEvent("Player::Punish", x.Id, (int)x.Type, ushort.MaxValue, -2, null);

                                    Utils.Demorgan.SetFromDemorgan(this);

                                    if (Info.Fraction != Game.Fractions.Types.None)
                                    {
                                        var fData = Game.Fractions.Fraction.Get(Info.Fraction);

                                        fData.OnMemberStatusChange(Info, fData.GetMemberStatus(Info));
                                    }

                                    x.AmnestyInfo = new Sync.Punishment.Amnesty();

                                    MySQL.UpdatePunishmentAmnesty(x);
                                }
                            }
                            else
                            {
                                strD[0] = (long.Parse(strD[0]) + 60).ToString();

                                x.AdditionalData = string.Join('_', strD);

                                MySQL.UpdatePunishmentAdditionalData(x);
                            }
                        }
                    }
                });
            }, null, 60_000, 60_000);
        }

        public void StopUpdateTimer()
        {
            if (UpdateTimer != null)
            {
                UpdateTimer.Dispose();

                UpdateTimer = null;
            }
        }
    }
}