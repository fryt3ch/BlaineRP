using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BlaineRP.Server.Extensions.System;
using BlaineRP.Server.Extensions.System.Collections.Generic;
using BlaineRP.Server.Game.Achievements;
using BlaineRP.Server.Game.Animations;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.EntitiesData.Players.Customization;
using BlaineRP.Server.Game.EntitiesData.Vehicles;
using BlaineRP.Server.Game.Estates;
using BlaineRP.Server.Game.Gifts;
using BlaineRP.Server.Game.Inventory;
using BlaineRP.Server.Game.Items;
using BlaineRP.Server.Game.Management.Punishments;
using BlaineRP.Server.Game.Phone;
using BlaineRP.Server.Game.Quests;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;
using Newtonsoft.Json.Linq;

namespace BlaineRP.Server.Game.EntitiesData.Players
{
    public partial class PlayerData
    {
        private static readonly Dictionary<Player, PlayerData> _all = new Dictionary<Player, PlayerData>();

        public static IReadOnlyDictionary<Player, PlayerData> All => _all.AsReadOnly();

        /// <summary>Получить PlayerData игрока</summary>
        /// <returns>Объект класса PlayerData если существует, иначе - null</returns>
        public static PlayerData Get(Player player)
        {
            return player == null ? null : _all.GetValueOrDefault(player);
        }

        /// <summary>Назначить объект класса PlayerData игроку</summary>
        public static void Set(Player player, PlayerData data)
        {
            if (player == null)
                return;

            _all.Add(player, data);
        }

        public void Remove()
        {
            if (Player == null)
                return;

            Info.PlayerData = null;

            _all.Remove(Player);

            Player.ResetData();
        }

        /// <summary>Сущность игрока</summary>
        public Player Player { get; set; }

        public void AddRentedVehicle(VehicleData vData, int timeDel)
        {
            Player.TriggerEvent("Player::RVehs::U", vData.Vehicle.Id, vData.Info.ID);
        }

        public void RemoveRentedVehicle(VehicleData vData)
        {
            Player.TriggerEvent("Player::RVehs::U", vData.Vehicle.Id);
        }

        public void AddVehicleProperty(VehicleInfo vInfo)
        {
            if (OwnedVehicles.Contains(vInfo))
                return;

            OwnedVehicles.Add(vInfo);

            Player.TriggerEvent("Player::Properties::Update", true, PropertyTypes.Vehicle, vInfo.VID, vInfo.ID);
        }

        public void RemoveVehicleProperty(VehicleInfo vInfo)
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


        public bool TryAddCash(ulong amount, out ulong newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            return Info.TryAddCash(amount, out newBalance, notifyOnFault, tData);
        }

        public bool TryRemoveCash(ulong amount, out ulong newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            return Info.TryRemoveCash(amount, out newBalance, notifyOnFault, tData);
        }

        public void SetCash(ulong value)
        {
            Info.SetCash(value);
        }

        public bool HasBankAccount(bool notifyOnFault = true)
        {
            if (BankAccount != null)
                return true;

            if (notifyOnFault)
                Player.Notify("Bank::NoAccount");

            return false;
        }

        public bool HasLicense(LicenseType lType, bool notifyIfNot = true)
        {
            if (Info.Licenses.Contains(lType))
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

        public bool BlockRemoteCalls
        {
            get => Player.GetData<bool?>("BlockRC") ?? false;
            set
            {
                if (value)
                    Player.SetData("BlockRC", true);
                else
                    Player.ResetData("BlockRC");
            }
        }

        public byte SpamCounter { get; set; }

        public AccountData AccountData { get; set; }

        public VehicleData CurrentTuningVehicle
        {
            get => Player.GetData<VehicleData>("tsvdata");
            set
            {
                if (value == null)
                    Player.ResetData("tsvdata");
                else
                    Player.SetData("tsvdata", value);
            }
        }

        public PlayerData(Player Player)
        {
            this.Player = Player;

            SpamCounter = 0;

            LastDamageTime = DateTime.MinValue;

            _microphoneListeners = new HashSet<Player>();

            VoiceRange = 0f;

            AttachedEntities = new List<AttachmentEntityNet>();
            AttachedObjects = new List<AttachmentObjectNet>();

            Player.SetData(Game.Attachments.Service.AttachedObjectsTimersKey, new Dictionary<AttachmentType, Timer>());

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

            Info.LastJoinDate = Utils.GetCurrentTime();

            OwnedHouses = Info.OwnedHouses.ToList();
            OwnedGarages = Info.OwnedGarages.ToList();
            OwnedVehicles = Info.OwnedVehicles.ToList();
            OwnedApartments = Info.OwnedApartments.ToList();
            OwnedBusinesses = Info.OwnedBusinesses.ToList();

            SettledHouseBase = Info.SettledHouseBase;

            BankBalance = Info.BankAccount?.Balance ?? 0;

            Info.PlayerData = this;
        }

        /// <summary>Метод обозначает готовность персонажа к игре</summary>
        public void SetReady()
        {
            Player.SetMainData(this);

            Player.Name = $"{Info.Name} {Info.Surname}";

            UpdateCustomization();
            UpdateClothes();

            Player.TriggerEvent("Players::CloseAuth");

            foreach (VehicleInfo vInfo in OwnedVehicles)
            {
                vInfo.Spawn();
            }

            var activePunishments = Punishments.Where(x => x.IsActive()).ToList();

            foreach (Punishment x in activePunishments)
            {
                if (x.Type == PunishmentType.Mute)
                    IsMuted = true;
            }

            var data = new JObject
            {
                {
                    "Inventory", new JArray()
                    {
                        Weapons.Select(x => Item.ToClientJson(x, GroupTypes.Weapons)).SerializeToJson(),
                        Item.ToClientJson(Armour, GroupTypes.Armour),
                        Items.Select(x => Item.ToClientJson(x, GroupTypes.Items)).SerializeToJson(),
                        Clothes.Select(x => Item.ToClientJson(x, GroupTypes.Clothes)).SerializeToJson(),
                        Accessories.Select(x => Item.ToClientJson(x, GroupTypes.Accessories)).SerializeToJson(),
                        Item.ToClientJson(Bag, GroupTypes.BagItem),
                        Item.ToClientJson(Holster, GroupTypes.HolsterItem),
                    }
                },

                { "PN", Info.PhoneNumber },

                { "Licenses", Info.Licenses.SerializeToJson() },

                { "Skills", Info.Skills.SerializeToJson() },

                { "TimePlayed", Info.TimePlayed },
                { "CreationDate", Info.CreationDate },
                { "BirthDate", Info.BirthDate },
                { "Org", OrganisationID == -1 ? null : "todo" },
                { "Familiars", Info.Familiars.SerializeToJson() },

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
                data.Add("WSkins", Info.WeaponSkins.Select(x => x.ID).SerializeToJson());

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
                data.Add("Quests",
                    Info.Quests.Where(x => !x.Value.IsCompleted).Select(x => $"{(int)x.Key}~{x.Value.Step}~{x.Value.StepProgress}~{x.Value.CurrentData ?? ""}").SerializeToJson()
                );

            var rentedVehs = VehicleData.All.Values
                                        .Where(x => x.OwnerID == CID && (x.OwnerType == OwnerTypes.PlayerRent || x.OwnerType == OwnerTypes.PlayerRentJob))
                                        .ToList();

            if (rentedVehs.Count > 0)
                data.Add("RV", JArray.FromObject(rentedVehs.Select(x => $"{x.Vehicle.Id}&{x.Info.ID}").ToList()));

            Player.SetAlpha(255);

            Game.Management.AntiCheat.Service.SetPlayerHealth(Player, LastData.Health);

            Player.TriggerEvent("Players::CharacterPreload", data);

            Player.Teleport(LastData.Position.Position,
                false,
                LastData.Dimension,
                LastData.Position.RotationZ,
                LastData.Dimension >= Properties.Settings.Profile.Current.Game.HouseDimensionBaseOffset
            );
        }

        /// <summary>Метод раздевает игрока и надевает всю текущую одежду</summary>
        public void UpdateClothes()
        {
            Events.Players.CharacterCreation.Undress(Player, Sex);

            foreach (Clothes x in Clothes)
            {
                x?.Wear(this);
            }

            foreach (Clothes x in Accessories)
            {
                x?.Wear(this);
            }

            Armour?.Wear(this);
            Bag?.Wear(this);
            Holster?.Wear(this);

            //Game.Data.Customization.ApplyUniform(this, Game.Data.Customization.UniformTypes.Farmer);
        }

        public void UpdateWeapons()
        {
            foreach (Weapon x in Weapons)
            {
                x?.Wear(this);
            }
        }

        /// <summary>Метод обновляет кастомизацию игрока</summary>
        public void UpdateCustomization()
        {
            Player.SetCustomization(Sex,
                Info.HeadBlend.RageHeadBlend,
                Info.EyeColor,
                Info.HairStyle.Color,
                Info.HairStyle.Color2,
                Info.FaceFeatures,
                Info.HeadOverlays.ToDictionary(x => x.Key, x => x.Value.RageHeadOverlay),
                Defaults.Decorations
            );

            Player.SetClothes(2, Customization.Service.GetHair(Sex, Info.HairStyle.Id), 0);

            UpdateHairOverlay();

            UpdateDecorations();
        }

        public void UpdateHairOverlay()
        {
            if (Info.HairStyle.Overlay > 0)
                Player.SetSharedData("CHO", Info.HairStyle.Overlay);
            else
                Player.ResetSharedData("CHO");
        }

        public void UpdateDecorations()
        {
            if (Info.Decorations.Count > 0)
                Player.SetSharedData("DCR", Info.Decorations);
            else
                Player.ResetSharedData("DCR");
        }

        public bool CanUseInventory(bool notifyIfNot = true)
        {
            if (ActiveOffer?.TradeData != null)
                return false;

            if (IsInventoryBlocked)
            {
                if (notifyIfNot)
                    Player.Notify("Inventory::Blocked");

                return false;
            }

            return true;
        }

        public bool CanBeSettled(Game.Estates.HouseBase houseBase, bool notifyIfNot = true)
        {
            if (SettledHouseBase != null)
            {
                if (SettledHouseBase == houseBase)
                    Player.Notify("House::ASH");
                else if (SettledHouseBase.Type == houseBase.Type)
                    Player.Notify(houseBase.Type == Game.Estates.HouseBase.Types.House ? "House::ASOH" : "House::ASOA");

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
                UpdateTimer.Dispose();

            UpdateTimer = new Timer((_) =>
                {
                    NAPI.Task.Run(() =>
                        {
                            if (Player?.Exists != true)
                                return;

                            var minuteTimeSpan = TimeSpan.FromMinutes(1);

                            Info.TimePlayed = Info.TimePlayed.Add(minuteTimeSpan);
                            LastData.SessionTime = LastData.SessionTime.Add(minuteTimeSpan);

                            if (Info.TimePlayed.TotalMinutes % 2 == 0)
                            {
                                if (Satiety > 0)
                                    Satiety--;

                                if (Mood > 0)
                                    Mood--;
                            }

                            foreach (Punishment x in Punishments)
                            {
                                if (x.Type == PunishmentType.NRPPrison || x.Type == PunishmentType.Arrest || x.Type == PunishmentType.FederalPrison)
                                {
                                    if (!x.IsActive())
                                        continue;

                                    string[] strD = x.AdditionalData.Split('_');

                                    long time = long.Parse(strD[0]) + 60;

                                    if (time >= x.EndDate.GetUnixTimestamp())
                                    {
                                        x.OnFinish(Info, 0);
                                    }
                                    else
                                    {
                                        strD[0] = (long.Parse(strD[0]) + 60).ToString();

                                        x.AdditionalData = string.Join('_', strD);

                                        MySQL.UpdatePunishmentAdditionalData(x);
                                    }
                                }
                            }
                        }
                    );
                },
                null,
                60_000,
                60_000
            );
        }

        public void StopUpdateTimer()
        {
            if (UpdateTimer == null)
                return;

            UpdateTimer.Dispose();

            UpdateTimer = null;
        }

        public void SetAsKnocked(Player attacker)
        {
            if (IsWounded)
                IsWounded = false;

            this.PlayAnim(GeneralType.Knocked);

            IsKnocked = true;

            Player.TriggerEvent("Player::Knocked", true, attacker?.Id ?? ushort.MaxValue);
        }

        public void SetAsNotKnocked()
        {
            IsKnocked = false;

            Player.TriggerEvent("Player::Knocked", false);

            if (GeneralAnim == GeneralType.Knocked)
                this.StopGeneralAnim();
        }

        public bool Uncuff()
        {
            AttachmentObjectNet aData = AttachedObjects.Where(x => x.Type == AttachmentType.Cuffs || x.Type == AttachmentType.CableCuffs).FirstOrDefault();

            if (aData == null)
                return false;

            Player.DetachObject(aData.Type);

            return true;
        }

        public void ShowPassport(Player target)
        {
            target.TriggerEvent("Documents::Show",
                0,
                Info.Name,
                Info.Surname,
                Sex,
                Info.BirthDate.SerializeToJson(),
                null,
                CID,
                Info.CreationDate.SerializeToJson(),
                false,
                Info.LosSantosAllowed
            );
        }

        public void ShowLicences(Player target)
        {
            target.TriggerEvent("Documents::Show", 1, Info.Name, Info.Surname, Info.Licenses);
        }

        public void ShowFractionDocs(Player target, Game.Fractions.Fraction fData, byte fRank)
        {
            target.TriggerEvent("Documents::Show", 4, Info.Name, Info.Surname, fData.Type, fRank);
        }

        public string GetNameForPlayer(PlayerData tData, bool familiarOnly = true, bool dontMask = true, bool includeId = false)
        {
            string name = familiarOnly
                ? tData.Info.Familiars.Contains(CID) && (dontMask || WearedMask == null) ? Player.Name : Language.Strings.Get(Sex ? "NPC_NOTFAM_MALE" : "NPC_NOTFAM_FEMALE")
                : Player.Name;

            if (includeId)
                return name + $" ({Player.Id})";
            else
                return name;
        }
    }
}