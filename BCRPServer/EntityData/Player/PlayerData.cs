﻿using BCRPServer.Game.Items;
using GTANetworkAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public enum CooldownTypes
        {
            ShootingRange = 0,
        }
        #endregion

        public static Dictionary<SkillTypes, int> MaxSkills = new Dictionary<SkillTypes, int>()
        {
            { SkillTypes.Strength, 100 },
            { SkillTypes.Shooting, 100 },
            { SkillTypes.Cooking, 100 },
            { SkillTypes.Fishing, 100 },
        };

        /// <summary>Стандартное время кулдаунов (в секундах!)</summary>
        public static Dictionary<CooldownTypes, int> CooldownTimeouts = new Dictionary<CooldownTypes, int>()
        {
            { CooldownTypes.ShootingRange, 3600 },
        };

        /// <summary>Сущность игрока</summary>
        public Player Player { get; set; }

        public void AddFamiliar(PlayerData tData)
        {
            var pCid = CID;
            var tCid = tData.CID;

            if (!Familiars.Contains(tCid))
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

            if (Familiars.Remove(tCid))
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

            MySQL.CharacterSkillsUpdate(Info);
        }

        public void AddRentedVehicle(VehicleData vData, int timeDel)
        {
            Player.TriggerEvent("Player::RVehs::U", vData.Vehicle.Id, vData.ID, timeDel);
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

        public void AddFurniture(Game.Estates.Furniture furn)
        {
            if (Furniture.Contains(furn))
                return;

            Furniture.Add(furn);

            Player.TriggerEvent("Player::Furniture::Update", true, furn.UID, furn.ID);

            MySQL.CharacterFurnitureUpdate(Info);
        }

        public void RemoveFurniture(Game.Estates.Furniture furn)
        {
            Furniture.Remove(furn);

            Player.TriggerEvent("Player::Furniture::Update", false, furn.UID);

            MySQL.CharacterFurnitureUpdate(Info);
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

            MySQL.CharacterCashUpdate(this.Info);

            return true;
        }

        public bool HasEnoughCash(int value, bool notifyOnFault = true)
        {
            if (Cash >= value)
                return true;

            if (notifyOnFault)
                Player.Notify("Cash::NotEnough", Cash);

            return false;
        }

        public bool HasBankAccount(bool notifyOnFault = true)
        {
            if (BankAccount != null)
                return true;

            if (notifyOnFault)
                Player.Notify("Bank::NoAccount");

            return false;
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
                return PhoneOn || CurrentContainer != null || IsAttachedToEntity != null || CurrentBusiness != null;
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

            Player.SetData(Sync.AttachSystem.AttachedObjectsCancelsKey, new Dictionary<Sync.AttachSystem.Types, CancellationTokenSource>());

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

        public PlayerData(Player Player, string name, string surname, int age, bool sex, Game.Data.Customization.HeadBlend hBlend, Dictionary<int, Game.Data.Customization.HeadOverlay> hOverlays, float[] faceFeatures, byte eyeColor, Game.Data.Customization.HairStyle hStyle, Game.Items.Clothes[] clothes) : this(Player)
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

            LastData = new LastPlayerData() { Dimension = Utils.Dimensions.Main, Position = new Utils.Vector4(Utils.DefaultSpawnPosition, Utils.DefaultSpawnHeading), Health = 100, SessionTime = 0, Mood = Mood, Satiety = Satiety };

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

            Furniture = new List<Game.Estates.Furniture>();

            Info.WeaponSkins = new Dictionary<WeaponSkin.ItemData.Types, WeaponSkin>();

            Familiars = new List<uint>();

            Punishments = new List<Punishment>();

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
                Weapons.Select(x => Game.Items.Item.ToClientJson(x, Game.Items.Inventory.Groups.Weapons)).SerializeToJson(),
                Game.Items.Item.ToClientJson(Armour, Game.Items.Inventory.Groups.Armour),
                Items.Select(x => Game.Items.Item.ToClientJson(x, Game.Items.Inventory.Groups.Items)).SerializeToJson(),
                Clothes.Select(x => Game.Items.Item.ToClientJson(x, Game.Items.Inventory.Groups.Clothes)).SerializeToJson(),
                Accessories.Select(x => Game.Items.Item.ToClientJson(x, Game.Items.Inventory.Groups.Accessories)).SerializeToJson(),
                Game.Items.Item.ToClientJson(Bag, Game.Items.Inventory.Groups.BagItem),
                Game.Items.Item.ToClientJson(Holster, Game.Items.Inventory.Groups.HolsterItem),
            };

            JObject data = new JObject();

            data.Add("Inventory", inventory);

            if (Furniture.Count > 0)
                data.Add("Furniture", Furniture.ToDictionary(x => x.UID, x => x.ID).SerializeToJson());

            if (Info.WeaponSkins.Count > 0)
                data.Add("WSkins", Info.WeaponSkins.Select(x => x.Value.ID).SerializeToJson());

            data.Add("Licenses", Licenses.SerializeToJson());
            data.Add("Skills", Skills.SerializeToJson());

            if (Info.MedicalCard != null)
                data.Add("MedCard", Info.MedicalCard.SerializeToJson());

            data.Add("TimePlayed", TimePlayed);
            data.Add("CreationDate", CreationDate);
            data.Add("BirthDate", CreationDate);

            data.Add("Org", OrganisationID == -1 ? null : "todo");

            data.Add("Familiars", Familiars.SerializeToJson());

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

            data.Add("Gifts", Gifts.ToDictionary(x => x.ID, x => ((int)x.Type, x.GID, x.Amount, (int)x.SourceType)).SerializeToJson()); // to change!

            data.Add("Achievements", Info.Achievements.Select(x => $"{(int)x.Key}_{x.Value.Progress}_{x.Value.TypeData.Goal}").SerializeToJson());

            if (Info.Quests.Count > 0)
                data.Add("Quests", Info.Quests.Where(x => !x.Value.IsCompleted).Select(x => $"{(int)x.Key}_{x.Value.Step}_{x.Value.StepProgress}").SerializeToJson());

            NAPI.Task.Run(() =>
            {
                if (Player?.Exists != true)
                    return;

                Player.TriggerEvent("Players::CharacterPreload", Settings.SettingsToClientStr, data.SerializeToJson());

                Player.SetAlpha(255);

                Additional.AntiCheat.SetPlayerHealth(Player, LastData.Health);

                Player.Teleport(LastData.Position.Position, true, LastData.Dimension, LastData.Position.RotationZ, LastData.Dimension >= Utils.HouseDimBase);

                Player.SkyCameraMove(Additional.SkyCamera.SwitchTypes.ToPlayer, false, "Players::CharacterReady");
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

        public bool HasCooldown(CooldownTypes cdType, int notifyType = -1)
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
    }
}