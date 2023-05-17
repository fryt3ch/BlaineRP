using BCRPClient.CEF;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Sync
{
    // Player.LocalPlayer.TaskFollowToOffsetOfEntity(Player.LocalPlayer.Handle, 0, -1f, 0f, 1f, -1, 1f, true);
    public class Players : Events.Script
    {
        /// <summary>Готов ли персонаж к игре?</summary>
        public static bool CharacterLoaded { get; set; }

        /// <summary>Интервал, в котором будет отниматься здоровье, если игрок ранен</summary>
        private const int WoundedTime = 30000;

        /// <summary>Кол-во здоровья, которое будет отниматься каждые WoundedTime мсек., если игрок ранен</summary>
        private const int WoundedReduceHP = 5;

        /// <summary>Интервал, в котором будет отниматься здоровье, если игрок голоден</summary>
        private const int HungryTime = 120000;

        /// <summary>Кол-во здоровья, которое будет отниматься каждые WoundedTime мсек., если игрок голоден</summary>
        private const int HungryReduceHP = 5;

        /// <summary>Кол-во здоровья, ниже которого оно не будет отниматься, если игрок голоден</summary>
        private const int HungryLowestHP = 10;

        private static AsyncTask WoundedTask { get; set; }
        private static AsyncTask HungerTask { get; set; }

        private static AsyncTask RentedVehiclesCheckTask { get; set; }

        private static Dictionary<string, Action<PlayerData, object, object>> DataActions = new Dictionary<string, Action<PlayerData, object, object>>();

        private static void InvokeHandler(string dataKey, PlayerData pData, object value, object oldValue = null) => DataActions.GetValueOrDefault(dataKey)?.Invoke(pData, value, oldValue);

        private static void AddDataHandler(string dataKey, Action<PlayerData, object, object> action)
        {
            Events.AddDataHandler(dataKey, (Entity entity, object value, object oldValue) =>
            {
                if (entity is Player player)
                {
                    var data = Sync.Players.GetData(player);

                    if (data == null)
                        return;

                    action.Invoke(data, value, oldValue);
                }
            });

            DataActions.Add(dataKey, action);
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

        public enum AchievementTypes
        {
            SR1,
            SR2,
        }

        public enum PhoneStateTypes : byte
        {
            /// <summary>Телефон используется без анимаций</summary>
            JustOn = 0,
            /// <summary>Телефон используется c обычной анимацией</summary>
            Idle,
            /// <summary>Телефон используется в транспорте</summary>
            Vehicle,
            /// <summary>Телефон используется с анимацией камеры 0</summary>
            Camera,
        }
        #endregion

        public static Dictionary<SkillTypes, int> MaxSkills = new Dictionary<SkillTypes, int>()
        {
            { SkillTypes.Strength, 100 },
            { SkillTypes.Shooting, 100 },
            { SkillTypes.Cooking, 100 },
            { SkillTypes.Fishing, 100 },
        };

        public class MedicalCard
        {
            public enum DiagnoseTypes
            {
                Healthy = 0,
            }

            [JsonProperty(PropertyName = "I")]
            public DateTime IssueDate { get; set; }

            [JsonProperty(PropertyName = "F")]
            public Data.Fractions.Types IssueFraction { get; set; }

            [JsonProperty(PropertyName = "N")]
            public string DoctorName { get; set; }

            [JsonProperty(PropertyName = "D")]
            public DiagnoseTypes Diagnose { get; set; }

            public MedicalCard() { }
        }

        public class PlayerData
        {
            public Player Player { get; private set; }

            public PlayerData(Player Player) => this.Player = Player;

            #region Player Data
            public uint CID => Player.GetSharedData<object>("CID", 0).ToUInt32();

            public ulong Cash => Player.GetSharedData<object>("Cash", 0).ToUInt64();

            public ulong BankBalance => Player.GetSharedData<object>("BankBalance", 0).ToUInt64();

            public bool Sex => Player.GetSharedData<bool>("Sex", true);

            public Data.Fractions.Types Fraction => (Data.Fractions.Types)Player.GetSharedData<int>("Fraction", 0);

            public int Satiety => Player.GetSharedData<int>("Satiety", 0);

            public int Mood => Player.GetSharedData<int>("Mood", 0);

            public bool IsMasked => Player.GetDrawableVariation(1) > 0;

            public bool IsKnocked => Player.GetSharedData<bool>("Knocked", false);

            public bool CrouchOn => Player.GetSharedData<bool>("Crouch::On", false);

            public bool CrawlOn => Player.GetSharedData<bool>("Crawl::On", false);

            public string WeaponComponents => Player.GetSharedData<string>("WCD", null);

            public float VoiceRange => Player.GetSharedData<float>("VoiceRange", 0f);

            public bool IsMuted => VoiceRange < 0f;

            public bool IsCuffed => Player.GetSharedData<bool>("IsCuffed", false);

            public bool IsInvalid => Player.GetSharedData<bool>("IsInvalid", false);

            public bool IsJailed => Player.GetSharedData<bool>("IsJailed", false);

            public bool IsFrozen => Player.GetSharedData<bool>("IsFrozen", false);

            public string Hat => Player.GetSharedData<string>("Hat", null);

            public bool IsWounded => Player.GetSharedData<bool>("IsWounded", false);

            public bool BeltOn => Player.GetSharedData<bool>("Belt::On", false);

            public Sync.Phone.PhoneStateTypes PhoneStateType => (Sync.Phone.PhoneStateTypes)Player.GetSharedData<int>("PST", 0);

            public int AdminLevel => Player.GetSharedData<int>("AdminLevel", -1);

            public int VehicleSeat => Player.GetSharedData<int>("VehicleSeat", -1);

            public Vehicle AutoPilot { get => Player.GetData<Vehicle>("AutoPilot::State"); set { if (value == null) Player.ResetData("AutoPilot::State"); else Player.SetData("AutoPilot::State", value); } }

            public Sync.Animations.GeneralTypes GeneralAnim => (Sync.Animations.GeneralTypes)Player.GetSharedData<int>("Anim::General", -1);

            public Sync.Animations.FastTypes FastAnim { get => Player.GetData<Sync.Animations.FastTypes>("Anim::Fast"); set => Player.SetData("Anim::Fast", value); }

            public Sync.Animations.OtherTypes OtherAnim => (Sync.Animations.OtherTypes)Player.GetSharedData<int>("Anim::Other", -1);

            public Sync.Animations.WalkstyleTypes Walkstyle => (Sync.Animations.WalkstyleTypes)Player.GetSharedData<int>("Walkstyle", -1);

            public Sync.Animations.EmotionTypes Emotion => (Sync.Animations.EmotionTypes)Player.GetSharedData<int>("Emotion", -1);

            public bool IsInvisible => Player.GetSharedData<bool>("IsInvisible", false);

            public bool IsInvincible => Player.GetSharedData<bool>("IsInvincible", false);

            public bool IsFlyOn => Player.GetSharedData<bool>("Fly", false);

            public int MuteTime { get => Player.GetData<int>("MuteTime"); set => Player.SetData("MuteTime", value); }

            public int JailTime { get => Player.GetData<int>("JailTime"); set => Player.SetData("JailTime", value); }

            public List<(uint VID, Data.Vehicles.Vehicle Data)> OwnedVehicles { get => Player.LocalPlayer.GetData<List<(uint VID, Data.Vehicles.Vehicle Data)>>("OwnedVehicles"); set => Player.LocalPlayer.SetData("OwnedVehicles", value); }

            public List<Data.Locations.Business> OwnedBusinesses { get => Player.LocalPlayer.GetData<List<Data.Locations.Business>>("OwnedBusinesses"); set => Player.LocalPlayer.SetData("OwnedBusinesses", value); }

            public List<Data.Locations.House> OwnedHouses { get => Player.LocalPlayer.GetData<List<Data.Locations.House>>("OwnedHouses"); set => Player.LocalPlayer.SetData("OwnedHouses", value); }

            public List<Data.Locations.Apartments> OwnedApartments { get => Player.LocalPlayer.GetData<List<Data.Locations.Apartments>>("OwnedApartments"); set => Player.LocalPlayer.SetData("OwnedApartments", value); }

            public List<Data.Locations.Garage> OwnedGarages { get => Player.LocalPlayer.GetData<List<Data.Locations.Garage>>("OwnedGarages"); set => Player.LocalPlayer.SetData("OwnedGarages", value); }

            public Data.Locations.HouseBase SettledHouseBase { get => Player.LocalPlayer.GetData<Data.Locations.HouseBase>("SettledHouseBase"); set { if (value == null) Player.LocalPlayer.ResetData("SettledHouseBase"); else Player.LocalPlayer.SetData("SettledHouseBase", value); } }

            public Dictionary<uint, Data.Furniture> Furniture { get => Player.LocalPlayer.GetData<Dictionary<uint, Data.Furniture>>("Furniture"); set => Player.LocalPlayer.SetData("Furniture", value); }

            public Dictionary<Data.Items.WeaponSkin.ItemData.Types, string> WeaponSkins { get => Player.LocalPlayer.GetData<Dictionary<Data.Items.WeaponSkin.ItemData.Types, string>>("WeaponSkins"); set => Player.LocalPlayer.SetData("WeaponSkins", value); }

            public List<uint> Familiars { get => Player.LocalPlayer.GetData<List<uint>>("Familiars"); set => Player.LocalPlayer.SetData("Familiars", value); }

            public Dictionary<SkillTypes, int> Skills { get => Player.LocalPlayer.GetData<Dictionary<SkillTypes, int>>("Skills"); set => Player.LocalPlayer.SetData("Skills", value); }

            public List<LicenseTypes> Licenses { get => Player.LocalPlayer.GetData<List<LicenseTypes>>("Licenses"); set => Player.LocalPlayer.SetData("Licenses", value); }

            public MedicalCard MedicalCard { get => Player.LocalPlayer.GetData<MedicalCard>("MedicalCard"); set { if (value == null) Player.LocalPlayer.ResetData("MedicalCard"); else Player.LocalPlayer.SetData("MedicalCard", value); } }

            public Dictionary<AchievementTypes, (int Progress, bool IsRecieved)> Achievements { get => Player.LocalPlayer.GetData<Dictionary<AchievementTypes, (int, bool)>>("Achievements"); set => Player.LocalPlayer.SetData("Achievements", value); }

            public List<Sync.Quest> Quests { get => Player.LocalPlayer.GetData<List<Sync.Quest>>("Quests"); set => Player.LocalPlayer.SetData("Quests", value); }

            public Entity IsAttachedTo { get => Player.GetData<Entity>("IsAttachedTo::Entity"); set { if (value == null) Player.ResetData("IsAttachedTo::Entity"); else Player.SetData("IsAttachedTo::Entity", value); } }

            public List<Sync.AttachSystem.AttachmentObject> AttachedObjects => Sync.AttachSystem.GetEntityObjectAttachments(Player);

            public List<Sync.AttachSystem.AttachmentObject> AttachedEntities => Sync.AttachSystem.GetEntityEntityAttachments(Player);

            public List<int> Decorations => Player.GetSharedData<JArray>("DCR", null)?.ToObject<List<int>>();

            public Data.Customization.HairOverlay HairOverlay => Data.Customization.GetHairOverlay(Sex, Player.GetSharedData<int>("CHO", 0));

            public Sync.AttachSystem.AttachmentObject WearedRing => AttachedObjects.Where(x => x.Type >= Sync.AttachSystem.Types.PedRingLeft3 && x.Type <= Sync.AttachSystem.Types.PedRingRight3).FirstOrDefault();

            public Sync.Animations.Animation ActualAnimation { get => Player.GetData<Sync.Animations.Animation>("ActualAnim"); set { if (value == null) Player.ResetData("ActualAnim"); Player.SetData("ActualAnim", value); } }

            public List<CEF.PhoneApps.SMSApp.SMS> AllSMS { get => Player.GetData<List<CEF.PhoneApps.SMSApp.SMS>>("AllSMS"); set => Player.SetData("AllSMS", value); }

            public Dictionary<uint, string> Contacts { get => Player.GetData<Dictionary<uint, string>>("Contacts"); set => Player.SetData("Contacts", value); }

            public List<uint> PhoneBlacklist { get => Player.GetData<List<uint>>("PBL"); set => Player.SetData("PBL", value); }

            public uint PhoneNumber { get => Player.GetData<uint>("PhoneNumber"); set => Player.SetData("PhoneNumber", value); }

            public CEF.PhoneApps.PhoneApp.CallInfo ActiveCall { get => Player.GetData<CEF.PhoneApps.PhoneApp.CallInfo>("ActiveCall"); set { if (value == null) Player.ResetData("ActiveCall"); Player.SetData("ActiveCall", value); } }

            public Data.Jobs.Job CurrentJob { get => Player.GetData<Data.Jobs.Job>("CJob"); set { if (value == null) Player.ResetData("CJob"); Player.SetData("CJob", value); } }

            public Data.Fractions.Fraction CurrentFraction { get => Player.GetData<Data.Fractions.Fraction>("CFraction"); set { if (value == null) Player.ResetData("CFraction"); Player.SetData("CFraction", value); } }
            #endregion

            public void Reset()
            {
                if (Player == null)
                    return;

                Player.ClearTasksImmediately();

                Player.SetNoCollisionEntity(Player.LocalPlayer.Handle, false);

                Sync.Microphone.RemoveTalker(Player);
                Sync.Microphone.RemoveListener(Player, false);

                Player.ResetData();
            }
        }

        public static PlayerData GetData(Player player)
        {
            if (player == null)
                return null;

            return player.GetData<PlayerData>("SyncedData");
        }

        public static void SetData(Player player, PlayerData data)
        {
            if (player == null)
                return;

            player.SetData("SyncedData", data);
        }

        public static async System.Threading.Tasks.Task OnPlayerStreamIn(Player player)
        {
            if (player.IsLocal)
                return;

            var data = GetData(player);

            if (data != null)
            {
                data.Reset();
            }

            player.AutoVolume = false;
            player.Voice3d = false;
            player.VoiceVolume = 0f;

            data = new PlayerData(player);

            if (data.CID == 0)
                return;

            SetData(player, data);

            if (data.VehicleSeat >= 0)
                InvokeHandler("VehicleSeat", data, data.VehicleSeat, null);

            InvokeHandler("IsInvisible", data, data.IsInvisible, null);

            InvokeHandler("CHO", data, player.GetSharedData<int>("CHO", 0), null);

            InvokeHandler("DCR", data, player.GetSharedData<JArray>("DCR", null), null);

            InvokeHandler("WCD", data, data.WeaponComponents, null);

            if (data.VoiceRange > 0f)
                Sync.Microphone.AddTalker(player);

            if (data.CrouchOn)
                Crouch.On(true, player);

            var phoneStateType = data.PhoneStateType;

            if (phoneStateType != Phone.PhoneStateTypes.Off)
                Phone.SetState(player, phoneStateType);

            if (data.GeneralAnim != Animations.GeneralTypes.None)
            {
                InvokeHandler("Anim::General", data, (int)data.GeneralAnim);
            }
            else if (data.OtherAnim != Animations.OtherTypes.None)
            {
                InvokeHandler("Anim::Other", data, (int)data.OtherAnim);
            }
        }

        public static async System.Threading.Tasks.Task OnPlayerStreamOut(Player player)
        {
            var data = GetData(player);

            if (data == null)
                return;

            data.Reset();
        }

        public Players()
        {
            CharacterLoaded = false;

            (new AsyncTask(() =>
            {
                var players = RAGE.Elements.Entities.Players.Streamed;

                for (int i = 0; i < players.Count; i++)
                {
                    var pData = Sync.Players.GetData(players[i]);

                    if (pData == null)
                        continue;

                    if (pData.ActualAnimation is Sync.Animations.Animation anim)
                    {
                        if (!pData.Player.IsPlayingAnim(anim.Dict, anim.Name, 3))
                            Sync.Animations.Play(pData.Player, anim);
                    }
                }
            }, 2500, true, 0)).Run();

            #region LocalPlayer Ready
            Events.Add("Players::CloseAuth", async (object[] args) =>
            {
                CEF.Auth.CloseAll();
            });

            Events.Add("Players::CharacterPreload", async (object[] args) =>
            {
                if (CharacterLoaded)
                    return;

                while (!World.Preloaded)
                    await RAGE.Game.Invoker.WaitAsync(0);

                Player.LocalPlayer.AutoVolume = false;
                Player.LocalPlayer.VoiceVolume = 0f;

                await CEF.Browser.Render(CEF.Browser.IntTypes.Inventory_Full, true, false);

                await CEF.Browser.Render(CEF.Browser.IntTypes.Chat, true, false);

                await CEF.Browser.Render(CEF.Browser.IntTypes.Interaction, true, false);

                await CEF.Browser.Render(CEF.Browser.IntTypes.NPC, true, false);

                await CEF.Browser.Render(CEF.Browser.IntTypes.Phone, true, false);

                RAGE.Game.Invoker.Invoke(0x95C0A5BBDC189AA1);

                CEF.Browser.Window.ExecuteJs("Hud.createSpeedometer", 500);

                var player = Player.LocalPlayer;

                var data = new PlayerData(Player.LocalPlayer);

                data.FastAnim = Animations.FastTypes.None;

                var settings = RAGE.Util.Json.Deserialize<(float, float, float, float, float, float)>((string)args[0]);

                BCRPClient.Settings.STREAM_DISTANCE = settings.Item1;
                BCRPClient.Settings.ENTITY_INTERACTION_MAX_DISTANCE = settings.Item2;
                BCRPClient.Settings.ENTITY_INTERACTION_MAX_DISTANCE_RENDER = settings.Item3;
                BCRPClient.Settings.MIN_CRUISE_CONTROL_SPEED = settings.Item4;
                BCRPClient.Settings.MAX_CRUISE_CONTROL_SPEED = settings.Item5;
                BCRPClient.Settings.MAX_INVENTORY_WEIGHT = settings.Item6;

                var sData = (JObject)args[1];

                data.Familiars = RAGE.Util.Json.Deserialize<List<uint>>((string)sData["Familiars"]);

                data.Licenses = RAGE.Util.Json.Deserialize<List<LicenseTypes>>((string)sData["Licenses"]);

                data.Skills = RAGE.Util.Json.Deserialize<Dictionary<SkillTypes, int>>((string)sData["Skills"]);

                data.PhoneNumber = (uint)sData["PN"];

                if (sData.ContainsKey("P"))
                {
                    foreach (var x in ((JArray)sData["P"]).ToObject<List<string>>())
                    {
                        var t = x.Split('&');

                        Sync.Punishment.AddPunishment(new Punishment() { Id = uint.Parse(t[0]), Type = (Sync.Punishment.Types)int.Parse(t[1]), EndDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(t[2])).DateTime, AdditionalData = t[3].Length > 0 ? t[3] : null });
                    }

                    Sync.Punishment.StartCheckTask();
                }

                if (sData.ContainsKey("Conts"))
                    data.Contacts = ((JObject)sData["Conts"]).ToObject<Dictionary<uint, string>>();
                else
                    data.Contacts = new Dictionary<uint, string>();

                if (sData.ContainsKey("PBL"))
                    data.PhoneBlacklist = ((JArray)sData["PBL"]).ToObject<List<uint>>();
                else
                    data.PhoneBlacklist = new List<uint>();

                if (sData.ContainsKey("SMS"))
                    data.AllSMS = RAGE.Util.Json.Deserialize<List<string>>((string)sData["SMS"]).Select(x => new CEF.PhoneApps.SMSApp.SMS(x)).ToList();
                else
                    data.AllSMS = new List<CEF.PhoneApps.SMSApp.SMS>();

                if (sData.ContainsKey("Vehicles"))
                    data.OwnedVehicles = RAGE.Util.Json.Deserialize<List<string>>((string)sData["Vehicles"]).Select(x => { var data = x.Split('_'); return (Convert.ToUInt32(data[0]), Data.Vehicles.GetById(data[1])); }).ToList();
                else
                    data.OwnedVehicles = new List<(uint VID, Data.Vehicles.Vehicle Data)>();

                if (sData.ContainsKey("Businesses"))
                    data.OwnedBusinesses = RAGE.Util.Json.Deserialize<List<int>>((string)sData["Businesses"]).Select(x => Data.Locations.Business.All[x]).ToList();
                else
                    data.OwnedBusinesses = new List<Data.Locations.Business>();

                if (sData.ContainsKey("Houses"))
                    data.OwnedHouses = RAGE.Util.Json.Deserialize<List<uint>>((string)sData["Houses"]).Select(x => Data.Locations.House.All[x]).ToList();
                else
                    data.OwnedHouses = new List<Data.Locations.House>();

                if (sData.ContainsKey("Apartments"))
                    data.OwnedApartments = RAGE.Util.Json.Deserialize<List<uint>>((string)sData["Apartments"]).Select(x => Data.Locations.Apartments.All[x]).ToList();
                else
                    data.OwnedApartments = new List<Data.Locations.Apartments>();

                if (sData.ContainsKey("Garages"))
                    data.OwnedGarages = RAGE.Util.Json.Deserialize<List<uint>>((string)sData["Garages"]).Select(x => Data.Locations.Garage.All[x]).ToList();
                else
                    data.OwnedGarages = new List<Data.Locations.Garage>();

                if (sData.ContainsKey("MedCard"))
                    data.MedicalCard = RAGE.Util.Json.Deserialize<MedicalCard>((string)sData["MedCard"]);

                if (sData.ContainsKey("SHB"))
                {
                    var shbData = ((string)sData["SHB"]).Split('_');

                    data.SettledHouseBase = ((Sync.House.HouseTypes)int.Parse(shbData[0])) == House.HouseTypes.House ? (Data.Locations.HouseBase)Data.Locations.House.All[uint.Parse(shbData[1])] : (Data.Locations.HouseBase)Data.Locations.Apartments.All[uint.Parse(shbData[1])];
                }

                var achievements = RAGE.Util.Json.Deserialize<List<string>>((string)sData["Achievements"]).ToDictionary(x => (AchievementTypes)Convert.ToInt32(x.Split('_')[0]), y => { var data = y.Split('_'); return (Convert.ToInt32(data[1]), Convert.ToInt32(data[2])); });

                foreach (var x in achievements)
                    UpdateAchievement(data, x.Key, x.Value.Item1, x.Value.Item2);

                data.Achievements = achievements.ToDictionary(x => x.Key, y => (y.Value.Item1, y.Value.Item1 >= y.Value.Item2));

                if (sData.ContainsKey("Quests"))
                {
                    data.Quests = RAGE.Util.Json.Deserialize<List<string>>((string)sData["Quests"]).Select(y => { var data = y.Split('~'); return new Sync.Quest((Sync.Quest.QuestData.Types)int.Parse(data[0]), byte.Parse(data[1]), int.Parse(data[2]), data[3].Length > 0 ? data[3] : null); }).ToList();
                }
                else
                {
                    data.Quests = new List<Quest>();
                }

                if (sData.ContainsKey("Furniture"))
                {
                    data.Furniture = RAGE.Util.Json.Deserialize<Dictionary<uint, string>>((string)sData["Furniture"]).ToDictionary(x => x.Key, x => Data.Furniture.GetData(x.Value));
                }
                else
                {
                    data.Furniture = new Dictionary<uint, Data.Furniture>();
                }

                if (sData.ContainsKey("WSkins"))
                {
                    data.WeaponSkins = RAGE.Util.Json.Deserialize<List<string>>((string)sData["WSkins"]).ToDictionary(x => ((Data.Items.WeaponSkin.ItemData)Data.Items.GetData(x, null)).Type, x => x);
                }
                else
                {
                    data.WeaponSkins = new Dictionary<Data.Items.WeaponSkin.ItemData.Types, string>();
                }

                if (sData.ContainsKey("RV"))
                {
                    var vehs = ((JArray)sData["RV"]).ToObject<List<string>>().Select(x => x.Split('&')).ToList();

                    foreach (var x in vehs)
                    {
                        Sync.Vehicles.RentedVehicle.All.Add(new Vehicles.RentedVehicle(ushort.Parse(x[0]), Data.Vehicles.GetById(x[1])));
                    }

                    RentedVehiclesCheckTask = new AsyncTask(Sync.Vehicles.RentedVehicle.Check, 1000, true, 0);

                    RentedVehiclesCheckTask.Run();
                }

                foreach (var x in data.Skills)
                    UpdateSkill(x.Key, x.Value);

                UpdateDrivingSkill(data.Licenses.Contains(LicenseTypes.B));
                UpdateBikeSkill(data.Licenses.Contains(LicenseTypes.A));
                UpdateFlyingSkill(data.Licenses.Contains(LicenseTypes.Fly));

                CEF.Inventory.Load((JArray)sData["Inventory"]);

                CEF.Menu.Load(data, (int)sData["TimePlayed"], (DateTime)sData["CreationDate"], (DateTime)sData["BirthDate"], RAGE.Util.Json.Deserialize<Dictionary<uint, (int, string, int, int)>>((string)sData["Gifts"]));

                CEF.Menu.SetOrganisation((string)sData["Org"]);

                foreach (var x in data.Skills)
                    CEF.Menu.UpdateSkill(x.Key, x.Value);

                while (data.CID == 0)
                    await RAGE.Game.Invoker.WaitAsync(0);

                SetData(Player.LocalPlayer, data);

                CEF.Menu.SetCID(data.CID);

                InvokeHandler("AdminLevel", data, data.AdminLevel, null);

                InvokeHandler("Cash", data, data.Cash, null);
                InvokeHandler("BankBalance", data, data.BankBalance, null);

                InvokeHandler("Sex", data, data.Sex, null);

                InvokeHandler("Fraction", data, data.Fraction, null);

                InvokeHandler("Mood", data, data.Mood, null);
                InvokeHandler("Satiety", data, data.Satiety, null);

                InvokeHandler("Knocked", data, data.IsKnocked, null);

                InvokeHandler("Wounded", data, data.IsWounded, null);

                InvokeHandler("VoiceRange", data, data.VoiceRange, null);

                InvokeHandler("Anim::General", data, (int)data.GeneralAnim, null);

                InvokeHandler("CHO", data, player.GetSharedData<int>("CHO", 0), null);

                InvokeHandler("DCR", data, Player.LocalPlayer.GetSharedData<JArray>("DCR", null), null);

                (new AsyncTask(() =>
                {
                    //Events.CallRemote("Player::UpdateTime");

                    CEF.Menu.TimePlayed += 1;
                }, 60_000, true, 60_000)).Run();

                CEF.HUD.Menu.UpdateCurrentTypes(true, HUD.Menu.Types.Menu, HUD.Menu.Types.Documents, HUD.Menu.Types.BlipsMenu);

                if (data.WeaponSkins.Count > 0)
                    CEF.HUD.Menu.UpdateCurrentTypes(true, CEF.HUD.Menu.Types.WeaponSkinsMenu);

                Settings.Load();
                KeyBinds.LoadAll();

                CEF.Phone.Preload();

                await CEF.Animations.Load();

                await Events.CallRemoteProc("Players::CRI", data.IsInvalid, Settings.Other.CurrentEmotion, Settings.Other.CurrentWalkstyle);

                CharacterLoaded = true;

                CEF.Menu.UpdateSettingsData();
                CEF.Menu.UpdateKeyBindsData();

                Player.LocalPlayer.SetInvincible(false);

                GameEvents.DisableAllControls(false);

                var timeUpdateTask = new AsyncTask(() =>
                {
                    CEF.HUD.UpdateTime();
                    CEF.Phone.UpdateTime();
                }, 1_000, true, 0);

                timeUpdateTask.Run();

                CEF.HUD.ShowHUD(!Settings.Interface.HideHUD);

                Interaction.Enabled = true;
                Sync.World.EnabledItemsOnGround = true;

                CEF.Chat.Show(true);

                Additional.ExtraLabel.Initialize();

                Additional.ExtraColshape.Activate();

                Additional.Discord.SetDefault();

                await RAGE.Game.Invoker.WaitAsync(500);

                foreach (var x in data.OwnedBusinesses)
                    x.ToggleOwnerBlip(true);

                foreach (var x in data.OwnedHouses)
                    x.ToggleOwnerBlip(true);

                foreach (var x in data.OwnedApartments)
                    x.ToggleOwnerBlip(true);

                foreach (var x in data.OwnedGarages)
                    x.ToggleOwnerBlip(true);

                data.SettledHouseBase?.ToggleOwnerBlip(true);

                foreach (var x in data.Quests)
                {
                    x.Initialize();
                }

/*                foreach (var x in Data.Locations.House.All)
                {
                    new Additional.ExtraBlip(40, x.Value.Position, "Дом", 1f, 2, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION, Additional.ExtraBlip.Types.Default);
                }*/
            });
            #endregion

            #region Local Player Events

            Events.Add("opday", (args) =>
            {
                if (args == null || args.Length == 0)
                {
                    Events.CallLocal("Chat::ShowServerMessage", $"Время зарплаты | Вы ничего не получаете, так как у Вас нет банковского счёта!");
                }
                else if (args.Length == 1)
                {
                    var playedTime = args[0].ToDecimal();

                    Events.CallLocal("Chat::ShowServerMessage", $"Время зарплаты | Вы ничего не получаете, так как за этот час Вы наиграли {playedTime / 60} минут из {10} необходимых!");
                }
                else
                {
                    var joblessBenefit = args[0].ToDecimal();
                    var fractionSalary = args[1].ToDecimal();
                    var organisationSalary = args[2].ToDecimal();

                    if (joblessBenefit > 0)
                    {
                        Events.CallLocal("Chat::ShowServerMessage", $"Время зарплаты | Вы получаете {Utils.GetPriceString(joblessBenefit)} (пособие по безработице) на свой счёт!");
                    }
                    else
                    {
                        if (organisationSalary == 0)
                            Events.CallLocal("Chat::ShowServerMessage", $"Время зарплаты | Вы получаете {Utils.GetPriceString(fractionSalary)} (от фракции) на свой счёт!");
                        else if (fractionSalary != 0)
                            Events.CallLocal("Chat::ShowServerMessage", $"Время зарплаты | Вы получаете {Utils.GetPriceString(fractionSalary)} (от фракции) и {Utils.GetPriceString(organisationSalary)} (от организации) на свой счёт!");
                        else
                            Events.CallLocal("Chat::ShowServerMessage", $"Время зарплаты | Вы получаете {Utils.GetPriceString(fractionSalary)} (от организации) на свой счёт!");
                    }
                }
            });

            Events.Add("Player::ParachuteS", (args) =>
            {
                var parachuteWeaponHash = RAGE.Util.Joaat.Hash("gadget_parachute");

                if (!(bool)args[0])
                {
                    Player.LocalPlayer.RemoveWeaponFrom(parachuteWeaponHash);

                    if (!(bool)args[1])
                    {
                        Player.LocalPlayer.GetData<AsyncTask>("ParachuteATask")?.Cancel();

                        Player.LocalPlayer.ResetData("ParachuteATask");

                        if (Player.LocalPlayer.GetParachuteState() >= 0)
                            Player.LocalPlayer.ClearTasksImmediately();
                    }
                }
                else
                {
                    Player.LocalPlayer.GetData<AsyncTask>("ParachuteATask")?.Cancel();

                    Player.LocalPlayer.RemoveWeaponFrom(parachuteWeaponHash);

                    Player.LocalPlayer.GiveWeaponTo(parachuteWeaponHash, 0, false, false);

                    RAGE.Game.Player.SetPlayerParachuteVariationOverride(66, 0, 2, false);
                    RAGE.Game.Player.SetPlayerParachuteTintIndex(6);

                    AsyncTask task = null;

                    var isInFly = false;

                    var lastSentState = int.MinValue;

                    task = new AsyncTask(() =>
                    {
                        var pState = Player.LocalPlayer.GetParachuteState();

                        if (isInFly)
                        {
                            if (pState < 0 || pState == 3)
                            {
                                if (lastSentState != -1)
                                {
                                    Events.CallRemote("Player::ParachuteS", false);

                                    if (lastSentState == 1 || lastSentState == 2)
                                        task?.Cancel();

                                    lastSentState = -1;

                                    isInFly = false;
                                }
                            }
                            else if (pState == 1 || pState == 2)
                            {
                                if (lastSentState != 1)
                                {
                                    Events.CallRemote("Player::ParachuteS", true);

                                    CEF.Notification.ShowHint("Используйте F, чтобы открепиться от парашюта", true, -1);

                                    lastSentState = 1;
                                }
                            }
                        }
                        else
                        {
                            if (pState < 0 || pState == 3)
                                return;

                            isInFly = true;

                            if (lastSentState != 0)
                            {
                                //Events.CallRemote("Player::ParachuteS", 0);

                                lastSentState = 0;

                                CEF.Notification.ShowHint("Используйте ЛКМ или F, чтобы раскрыть парашют", true, -1);
                            }
                        }
                    }, 25, true, 0);

                    Player.LocalPlayer.SetData("ParachuteATask", task);

                    task.Run();
                }
            });

            Events.Add("Player::RVehs::U", (args) =>
            {
                var rId = (ushort)(int)args[0];

                var rentedVehs = Sync.Vehicles.RentedVehicle.All;

                if (args.Length > 1)
                {
                    var vTypeId = (string)args[1];

                    var vTypeData = Data.Vehicles.GetById(vTypeId);

                    rentedVehs.Add(new Vehicles.RentedVehicle(rId, vTypeData));

                    if (RentedVehiclesCheckTask == null)
                    {
                        RentedVehiclesCheckTask = new AsyncTask(Sync.Vehicles.RentedVehicle.Check, 1000, true, 0);

                        RentedVehiclesCheckTask.Run();
                    }
                }
                else
                {
                    var rVeh = rentedVehs.Where(x => x.RemoteId == rId).FirstOrDefault();

                    if (rVeh == null)
                        return;

                    rentedVehs.Remove(rVeh);

                    if (rentedVehs.Count == 0)
                    {
                        RentedVehiclesCheckTask?.Cancel();

                        RentedVehiclesCheckTask = null;
                    }
                }

                if (CEF.Phone.CurrentApp == CEF.Phone.AppTypes.Vehicles)
                    CEF.Phone.ShowApp(null, CEF.Phone.AppTypes.Vehicles);
            });

            Events.Add("Player::Waypoint::Set", (args) =>
            {
                var x = (float)args[0];
                var y = (float)args[1];

                Utils.SetWaypoint(x, y);
            });

            Events.Add("Player::Smoke::Start", (object[] args) =>
            {
                var maxTime = (int)args[0];
                var maxPuffs = (int)args[1];

                Player.LocalPlayer.SetData("Smoke::Data::Puffs", maxPuffs);
                Player.LocalPlayer.SetData("Smoke::Data::CTask", new AsyncTask(() => Events.CallRemote("Players::Smoke::Stop"), maxTime, false, 0));
            });

            Events.Add("Player::Smoke::Stop", (object[] args) =>
            {
                Player.LocalPlayer.ResetData("Smoke::Data::Puffs");

                Player.LocalPlayer.GetData<AsyncTask>("Smoke::Data::CTask")?.Cancel();
                Player.LocalPlayer.GetData<AsyncTask>("Smoke::Data::CTask1")?.Cancel();
                Player.LocalPlayer.GetData<AsyncTask>("Smoke::Data::CTask2")?.Cancel();

                Player.LocalPlayer.ResetData("Smoke::Data::CTask");
                Player.LocalPlayer.ResetData("Smoke::Data::CTask1");
                Player.LocalPlayer.ResetData("Smoke::Data::CTask2");
            });

            Events.Add("Player::Smoke::Puff", (object[] args) =>
            {
                var task1 = new AsyncTask(async () =>
                {
                    await Utils.RequestPtfx("core");

                    var fxHandle = RAGE.Game.Graphics.StartParticleFxLoopedOnEntityBone("exp_grd_bzgas_smoke", Player.LocalPlayer.Handle, 0f, 0f, 0f, 0f, 0f, 0f, Player.LocalPlayer.GetBoneIndex(20279), 0.15f, false, false, false);

                    await RAGE.Game.Invoker.WaitAsync(1000);

                    RAGE.Game.Graphics.StopParticleFxLooped(fxHandle, false);
                }, 2000, false, 0);

                var task2 = new AsyncTask(() =>
                {
                    if (!Player.LocalPlayer.HasData("Smoke::Data::Puffs"))
                        return;

                    Player.LocalPlayer.SetData("Smoke::Data::Puffs", Player.LocalPlayer.GetData<int>("Smoke::Data::Puffs") - 1);
                }, 3000, false, 0);

                task1.Run();
                task2.Run();

                Player.LocalPlayer.SetData("Smoke::Data::CTask1", task1);
                Player.LocalPlayer.SetData("Smoke::Data::CTask2", task2);
            });

            Events.Add("Player::CloseAll", args => CloseAll((bool)args[0]));

            Events.Add("Player::Quest::Upd", (object[] args) =>
            {
                var data = Sync.Players.GetData(Player.LocalPlayer);

                if (data == null)
                    return;

                var qType = (Sync.Quest.QuestData.Types)(int)args[0];

                var quests = data.Quests;

                var quest = quests.Where(x => x.Type == qType).FirstOrDefault();

                if (args.Length < 3)
                {
                    var success = (bool)args[1];

                    if (quest == null)
                        return;

                    quest.SetQuestAsCompleted(success, true);
                }
                else
                {
                    var step = (byte)args[1].ToDecimal();

                    var sProgress = (int)args[2];

                    if (quest == null)
                    {
                        quest = new Sync.Quest(qType, step, sProgress, args.Length > 3 ? (string)args[3] : null);

                        quest.SetQuestAsStarted(true);
                    }
                    else
                    {
                        if (args.Length > 3)
                        {
                            quest.SetQuestAsUpdated(step, sProgress, (string)args[3], true);
                        }
                        else
                        {
                            quest.SetQuestAsUpdatedKeepOldData(step, sProgress, true);
                        }
                    }
                }
            });

            Events.Add("Player::Achievements::Update", (object[] args) =>
            {
                var data = Sync.Players.GetData(Player.LocalPlayer);

                if (data == null)
                    return;

                var aType = (AchievementTypes)(int)args[0];
                int value = (int)args[1];
                int maxValue = (int)args[2];

                UpdateAchievement(data, aType, value, maxValue);

                var aData = Locale.General.Players.AchievementTexts.ContainsKey(aType) ? Locale.General.Players.AchievementTexts[aType] : ("null", "null");

                if (value >= maxValue)
                    CEF.Notification.Show(Notification.Types.Achievement, aData.Item1, Locale.Notifications.General.AchievementUnlockedText, 5000);
            });

            Events.Add("Player::Skills::Update", (object[] args) =>
            {
                var data = Sync.Players.GetData(Player.LocalPlayer);

                if (data == null)
                    return;

                SkillTypes sType = (SkillTypes)(int)args[0];
                int value = (int)args[1];

                var oldValue = data.Skills[sType];

                data.Skills[sType] = value;

                CEF.Menu.UpdateSkill(sType, value);

                UpdateSkill(sType, value);

                CEF.Notification.Show(Notification.Types.Information, Locale.Notifications.DefHeader, string.Format(value >= oldValue ? Locale.Notifications.General.SkillUp : Locale.Notifications.General.SkillDown, Locale.General.Players.SkillNamesGenitive.GetValueOrDefault(sType) ?? "null", Math.Abs(value - oldValue), value, MaxSkills[sType]));
            });

            Events.Add("Player::WSkins::Update", (args) =>
            {
                var data = Sync.Players.GetData(Player.LocalPlayer);

                if (data == null)
                    return;

                var add = (bool)args[0];

                var id = (string)args[1];

                var wSkins = data.WeaponSkins;

                var type = ((Data.Items.WeaponSkin.ItemData)Data.Items.GetData(id, null)).Type;

                if (add)
                {
                    if (wSkins.ContainsKey(type))
                        wSkins[type] = id;
                    else
                        wSkins.Add(type, id);

                    CEF.HUD.Menu.UpdateCurrentTypes(true, CEF.HUD.Menu.Types.WeaponSkinsMenu);
                }
                else
                {
                    wSkins.Remove(type);

                    if (wSkins.Count == 0)
                        CEF.HUD.Menu.UpdateCurrentTypes(false, CEF.HUD.Menu.Types.WeaponSkinsMenu);
                }

                data.WeaponSkins = wSkins;
            });

            Events.Add("Player::MedCard::Update", (object[] args) =>
            {
                var data = Sync.Players.GetData(Player.LocalPlayer);

                if (data == null)
                    return;

                if (args.Length > 0)
                {
                    data.MedicalCard = RAGE.Util.Json.Deserialize<MedicalCard>((string)args[0]);
                }
                else
                {
                    data.MedicalCard = null;
                }
            });

            Events.Add("Player::Licenses::Update", (object[] args) =>
            {
                var data = Sync.Players.GetData(Player.LocalPlayer);

                if (data == null)
                    return;

                bool state = (bool)args[0];

                LicenseTypes lType = (LicenseTypes)(int)args[1];

                if (state)
                {
                    if (!data.Licenses.Contains(lType))
                    {
                        data.Licenses.Add(lType);
                    }
                }
                else
                {
                    data.Licenses.Remove(lType);
                }

                if (lType == LicenseTypes.B)
                {
                    UpdateDrivingSkill(state);
                }
                else if (lType == LicenseTypes.A)
                {
                    UpdateBikeSkill(state);
                }
                else if (lType == LicenseTypes.Fly)
                {
                    UpdateFlyingSkill(state);
                }
            });

            Events.Add("Player::Familiars::Update", (object[] args) =>
            {
                var data = Sync.Players.GetData(Player.LocalPlayer);

                if (data == null)
                    return;

                bool add = (bool)args[0];
                uint cid = (uint)(int)args[1];

                if (add)
                {
                    if (!data.Familiars.Contains(cid))
                        data.Familiars.Add(cid);
                }
                else
                {
                    data.Familiars.Remove(cid);
                }
            });

            Events.Add("Player::SettledHB", (args) =>
            {
                var pType = (Sync.House.HouseTypes)(int)args[0];

                var pId = (uint)(int)args[1];

                var state = (bool)args[2];

                var house = pType == House.HouseTypes.House ? (Data.Locations.HouseBase)Data.Locations.House.All[pId] : (Data.Locations.HouseBase)Data.Locations.Apartments.All[pId];

                house.ToggleOwnerBlip(state);

                if (args.Length > 3)
                {
                    var playerInit = RAGE.Elements.Entities.Players.GetAtRemote((ushort)(int)args[3]);

                    if (state)
                    {
                        CEF.Notification.Show(Notification.Types.Information, Locale.Notifications.DefHeader, string.Format(pType == House.HouseTypes.House ? Locale.Notifications.House.SettledHouse : Locale.Notifications.House.SettledApartments, playerInit.GetName(true, false, true)));
                    }
                    else
                    {
                        if (playerInit?.Handle == Player.LocalPlayer.Handle)
                        {
                            CEF.Notification.Show(Notification.Types.Information, Locale.Notifications.DefHeader, pType == House.HouseTypes.House ? Locale.Notifications.House.ExpelledHouseSelf : Locale.Notifications.House.ExpelledApartmentsSelf);
                        }
                        else
                        {
                            CEF.Notification.Show(Notification.Types.Information, Locale.Notifications.DefHeader, string.Format(pType == House.HouseTypes.House ? Locale.Notifications.House.ExpelledHouse : Locale.Notifications.House.ExpelledApartments, playerInit.GetName(true, false, true)));
                        }
                    }
                }
                else
                {
                    if (state)
                    {
                        CEF.Notification.Show(Notification.Types.Information, Locale.Notifications.DefHeader, pType == House.HouseTypes.House ? Locale.Notifications.House.SettledHouseAuto : Locale.Notifications.House.SettledApartmentsAuto);
                    }
                    else
                    {
                        CEF.Notification.Show(Notification.Types.Information, Locale.Notifications.DefHeader, pType == House.HouseTypes.House ? Locale.Notifications.House.ExpelledHouseAuto : Locale.Notifications.House.ExpelledApartmentsAuto);
                    }
                }
            });

            Events.Add("Player::Properties::Update", (object[] args) =>
            {
                var data = Sync.Players.GetData(Player.LocalPlayer);

                if (data == null)
                    return;

                bool add = (bool)args[0];

                PropertyTypes pType = (PropertyTypes)(int)args[1];

                if (pType == PropertyTypes.Vehicle)
                {
                    var vid = (uint)(int)args[2];

                    if (add)
                    {
                        var t = (vid, Data.Vehicles.GetById((string)args[3]));

                        if (!data.OwnedVehicles.Contains(t))
                            data.OwnedVehicles.Add(t);
                    }
                    else
                    {
                        var idx = data.OwnedVehicles.FindIndex(x => x.VID == vid);

                        if (idx < 0)
                            return;

                        data.OwnedVehicles.RemoveAt(idx);
                    }

                    if (CEF.Phone.CurrentApp == CEF.Phone.AppTypes.Vehicles)
                        CEF.Phone.ShowApp(null, CEF.Phone.AppTypes.Vehicles);
                }
                else if (pType == PropertyTypes.House)
                {
                    var hid = (uint)(int)args[2];

                    var t = Data.Locations.House.All[hid];

                    if (add)
                    {
                        if (!data.OwnedHouses.Contains(t))
                            data.OwnedHouses.Add(t);
                    }
                    else
                    {
                        data.OwnedHouses.Remove(t);
                    }

                    t.ToggleOwnerBlip(add);
                }
                else if (pType == PropertyTypes.Apartments)
                {
                    var hid = (uint)(int)args[2];

                    var t = Data.Locations.Apartments.All[hid];

                    if (add)
                    {
                        if (!data.OwnedApartments.Contains(t))
                            data.OwnedApartments.Add(t);
                    }
                    else
                    {
                        data.OwnedApartments.Remove(t);
                    }

                    t.ToggleOwnerBlip(add);
                }
                else if (pType == PropertyTypes.Garage)
                {
                    var gid = (uint)(int)args[2];

                    var t = Data.Locations.Garage.All[gid];

                    if (add)
                    {
                        if (!data.OwnedGarages.Contains(t))
                            data.OwnedGarages.Add(t);
                    }
                    else
                    {
                        data.OwnedGarages.Remove(t);
                    }

                    t.ToggleOwnerBlip(add);
                }
                else if (pType == PropertyTypes.Business)
                {
                    var bid = (int)args[2];

                    var t = Data.Locations.Business.All[bid];

                    if (add)
                    {
                        if (!data.OwnedBusinesses.Contains(t))
                            data.OwnedBusinesses.Add(t);
                    }
                    else
                    {
                        data.OwnedBusinesses.Remove(t);
                    }

                    t.ToggleOwnerBlip(add);
                }

                CEF.Menu.UpdateProperties(data);
            });

            Events.Add("Player::Furniture::Update", (args) =>
            {
                var data = Sync.Players.GetData(Player.LocalPlayer);

                if (data == null)
                    return;

                bool add = (bool)args[0];

                var fUid = (uint)(int)args[1];

                if (add)
                {
                    if (data.Furniture.ContainsKey(fUid))
                        return;

                    var fData = Data.Furniture.GetData((string)args[2]);

                    data.Furniture.Add(fUid, fData);

                    if (CEF.HouseMenu.IsActive)
                        CEF.HouseMenu.AddOwnedFurniture(fUid, fData);
                }
                else
                {
                    data.Furniture.Remove(fUid);

                    if (CEF.HouseMenu.IsActive)
                        CEF.HouseMenu.RemoveOwnedFurniture(fUid);
                }
            });

            AddDataHandler("IsCuffed", (pData, value, oldValue) =>
            {
                if (pData.Player != Player.LocalPlayer)
                    return;

                var state = value as bool? ?? false;

                if (state)
                {

                }
                else
                {

                }
            });

            AddDataHandler("Fly", (pData, value, oldValue) =>
            {
                if (pData.Player != Player.LocalPlayer)
                    return;

                var state = (bool?)value ?? false;

                GameEvents.Render -= FlyRender;

                if (state)
                {
                    Player.LocalPlayer.ClearTasksImmediately();

                    GameEvents.Render += FlyRender;
                }
                else
                {

                }
            });

            AddDataHandler("IsFrozen", (pData, value, oldValue) =>
            {
                if (pData.Player.Handle != Player.LocalPlayer.Handle)
                    return;

                var state = (bool?)value ?? false;

                if (state)
                {
                    Player.LocalPlayer.ClearTasksImmediately();

                    CloseAll(false);

                    Player.LocalPlayer.FreezePosition(true);

                    GameEvents.DisableAllControls(true);

                    WeaponSystem.DisabledFiring = true;
                }
                else
                {
                    WeaponSystem.DisabledFiring = false;

                    Player.LocalPlayer.FreezePosition(false);

                    GameEvents.DisableAllControls(false);
                }
            });

            AddDataHandler("Cash", (pData, value, oldValue) =>
            {
                if (pData.Player.Handle != Player.LocalPlayer.Handle)
                    return;

                var cash = Convert.ToDecimal(value).ToUInt64();

                CEF.HUD.SetCash(cash);
                CEF.Menu.SetCash(cash);

                var oldCash = oldValue == null ? cash : Convert.ToDecimal(oldValue).ToUInt64();

                if (cash == oldCash)
                    return;

                if (cash > oldCash)
                {
                    CEF.Notification.Show(CEF.Notification.Types.Cash, string.Format(Locale.Notifications.Money.Cash.AddHeader, cash - oldCash), string.Format(Locale.Notifications.Money.Cash.Balance, cash));
                }
                else
                {
                    CEF.Notification.Show(CEF.Notification.Types.Cash, string.Format(Locale.Notifications.Money.Cash.LossHeader, oldCash - cash), string.Format(Locale.Notifications.Money.Cash.Balance, cash));
                }
            });

            AddDataHandler("BankBalance", (pData, value, oldValue) =>
            {
                if (pData.Player.Handle != Player.LocalPlayer.Handle)
                    return;

                var bank = Convert.ToDecimal(value).ToUInt64();

                CEF.HUD.SetBank(bank);
                CEF.Menu.SetBank(bank);

                CEF.ATM.UpdateMoney(bank);
                CEF.Bank.UpdateMoney(bank);
                CEF.PhoneApps.BankApp.UpdateBalance(bank);

                var oldCash = oldValue == null ? bank : Convert.ToDecimal(oldValue).ToUInt64();

                if (bank == oldCash)
                    return;

                if (bank > oldCash)
                {
                    CEF.Notification.Show(CEF.Notification.Types.Bank, string.Format(Locale.Notifications.Money.Bank.AddHeader, bank - oldCash), string.Format(Locale.Notifications.Money.Bank.Balance, bank));
                }
                else
                {
                    CEF.Notification.Show(CEF.Notification.Types.Bank, string.Format(Locale.Notifications.Money.Bank.LossHeader, oldCash - bank), string.Format(Locale.Notifications.Money.Bank.Balance, bank));
                }
            });

            AddDataHandler("IsWounded", (pData, value, oldValue) =>
            {
                var player = pData.Player;

                var state = (bool?)value ?? false;

                if (player.Handle == Player.LocalPlayer.Handle)
                {
                    if (state)
                    {
                        CEF.HUD.SwitchStatusIcon(HUD.StatusTypes.Wounded, true);

                        CEF.Notification.ShowHint(Locale.Notifications.Players.States.Wounded, false);

                        if (WoundedTask != null)
                            WoundedTask.Cancel();

                        WoundedTask = new AsyncTask(() =>
                        {
                            var pData = Sync.Players.GetData(Player.LocalPlayer);

                            if (pData == null || pData.IsInvincible)
                                return;

                            Player.LocalPlayer.SetRealHealth(Player.LocalPlayer.GetRealHealth() - WoundedReduceHP);
                        }, WoundedTime, true, WoundedTime / 2);

                        WoundedTask.Run();

                        RAGE.Game.Graphics.StartScreenEffect("DeathFailMPDark", 0, true);
                    }
                    else
                    {
                        RAGE.Game.Graphics.StopScreenEffect("DeathFailMPDark");

                        CEF.HUD.SwitchStatusIcon(HUD.StatusTypes.Wounded, false);

                        if (WoundedTask != null)
                        {
                            WoundedTask.Cancel();

                            WoundedTask = null;
                        }
                    }
                }
            });

            AddDataHandler("WCD", (pData, value, oldValue) =>
            {
                var player = pData.Player;

                if (value is string strData)
                {
                    Sync.WeaponSystem.UpdateWeaponComponents(player, strData);
                }
            });

            Events.Add("Players::WCD::U", (args) =>
            {
                var player = (Player)args[0];

                if (player?.Exists != true || player.Handle == Player.LocalPlayer.Handle)
                    return;

                if (player.GetSharedData<string>("WCD", null) is string strData)
                {
                    Sync.WeaponSystem.UpdateWeaponComponents(player, strData);
                }
            });

            AddDataHandler("Mood", (pData, value, oldValue) =>
            {
                if (pData.Player.Handle != Player.LocalPlayer.Handle)
                    return;

                var mood = (int)value;

                if (mood <= 25)
                {
                    CEF.HUD.SwitchStatusIcon(HUD.StatusTypes.Mood, true);

                    if (mood % 5 == 0)
                        CEF.Notification.ShowHint(Locale.Notifications.Players.States.LowMood, false, 5000);
                }
                else
                    CEF.HUD.SwitchStatusIcon(HUD.StatusTypes.Mood, false);

                CEF.Inventory.UpdateStates();
            });

            AddDataHandler("Satiety", (pData, value, oldValue) =>
            {
                if (pData.Player.Handle != Player.LocalPlayer.Handle)
                    return;

                var satiety = (int)value;

                if (satiety <= 25)
                {
                    CEF.HUD.SwitchStatusIcon(HUD.StatusTypes.Food, true);

                    if (satiety % 5 == 0)
                        CEF.Notification.ShowHint(Locale.Notifications.Players.States.LowSatiety, false, 5000);

                    if (satiety == 0)
                    {
                        if (HungerTask != null)
                            HungerTask.Cancel();

                        HungerTask = new AsyncTask(() =>
                        {
                            var pData = Sync.Players.GetData(Player.LocalPlayer);

                            if (pData == null || pData.IsInvincible)
                                return;

                            var currentHp = Player.LocalPlayer.GetRealHealth();

                            if (currentHp <= HungryLowestHP)
                                return;

                            if (currentHp - HungryReduceHP <= HungryLowestHP)
                                Player.LocalPlayer.SetRealHealth(HungryLowestHP);
                            else
                                Player.LocalPlayer.SetRealHealth(currentHp - HungryReduceHP);
                        }, HungryTime, true, HungryTime / 2);
                    }
                }
                else
                {
                    if (HungerTask != null)
                    {
                        HungerTask.Cancel();

                        HungerTask = null;
                    }

                    CEF.HUD.SwitchStatusIcon(HUD.StatusTypes.Mood, false);
                }

                CEF.Inventory.UpdateStates();
            });
            #endregion

            #region Streamed Players Data Change

            AddDataHandler("Emotion", (pData, value, oldValue) =>
            {
                var player = pData.Player;

                var emotion = (Animations.EmotionTypes)((int?)value ?? -1);

                if (player.Handle == Player.LocalPlayer.Handle)
                {
                    Settings.Other.CurrentEmotion = emotion;

                    CEF.Animations.ToggleAnim("e-" + emotion.ToString(), true);
                }

                Sync.Animations.Set(player, emotion);
            });

            AddDataHandler("Walkstyle", (pData, value, oldValue) =>
            {
                var player = pData.Player;

                var wStyle = (Animations.WalkstyleTypes)((int?)value ?? -1);

                if (player.Handle == Player.LocalPlayer.Handle)
                {
                    Settings.Other.CurrentWalkstyle = wStyle;

                    CEF.Animations.ToggleAnim("w-" + wStyle.ToString(), true);
                }

                if (!pData.CrouchOn)
                    Sync.Animations.Set(player, wStyle);
            });

            AddDataHandler("Anim::Other", (pData, value, oldValue) =>
            {
                var player = pData.Player;

                var anim = (Sync.Animations.OtherTypes)((int?)value ?? -1);

                if (player.Handle == Player.LocalPlayer.Handle)
                {
                    if (anim == Animations.OtherTypes.None)
                    {
                        if (oldValue is int oldAnim)
                            CEF.Animations.ToggleAnim("a-" + ((Sync.Animations.OtherTypes)oldAnim).ToString(), false);

                        GameEvents.Render -= CEF.Animations.Render;

                        var cancelAnimKb = KeyBinds.Get(KeyBinds.Types.CancelAnimation);

                        if (!cancelAnimKb.IsDisabled)
                            KeyBinds.Get(KeyBinds.Types.CancelAnimation).Disable();
                    }
                    else
                    {
                        CEF.Animations.ToggleAnim("a-" + anim.ToString(), true);
                    }
                }

                if (anim == Animations.OtherTypes.None)
                {
                    Sync.Animations.Stop(player);

                    pData.ActualAnimation = null;
                }
                else
                {
                    var animData = Sync.Animations.OtherAnimsList[anim];

                    Sync.Animations.Play(player, animData);

                    pData.ActualAnimation = animData;
                }
            });

            AddDataHandler("Anim::General", (pData, value, oldValue) =>
            {
                var player = pData.Player;

                var anim = (Sync.Animations.GeneralTypes)((int?)value ?? -1);

                if (anim == Animations.GeneralTypes.None)
                {
                    Sync.Animations.Stop(player);

                    pData.ActualAnimation = null;
                }
                else
                {
                    var animData = Sync.Animations.GeneralAnimsList[anim];

                    Sync.Animations.Play(player, animData);

                    pData.ActualAnimation = animData;
                }
            });

            AddDataHandler("DCR", (pData, value, oldValue) =>
            {
                Data.Customization.TattooData.ClearAll(pData.Player);

                if (value is JArray jArr)
                {
                    foreach (var x in jArr.ToObject<List<int>>())
                    {
                        var tattoo = Data.Customization.GetTattooData(x);

                        tattoo.TryApply(pData.Player);
                    }
                }
            });

            AddDataHandler("CHO", (pData, value, oldValue) =>
            {
                Data.Customization.HairOverlay.ClearAll(pData.Player);

                if (value is int valueInt)
                {
                    Data.Customization.GetHairOverlay(pData.Sex, valueInt)?.Apply(pData.Player);
                }
            });

            AddDataHandler("Belt::On", (pData, value, oldValue) =>
            {
                if (pData.Player.Handle != Player.LocalPlayer.Handle)
                    return;

                var state = (bool?)value ?? false;

                var player = pData.Player;

                if (state)
                {
                    GameEvents.Update -= Sync.Vehicles.BeltTick;
                    GameEvents.Update += Sync.Vehicles.BeltTick;
                }
                else
                {
                    GameEvents.Update -= Sync.Vehicles.BeltTick;
                }

                player.SetConfigFlag(32, !state);

                HUD.SwitchBeltIcon(state);
            });

            AddDataHandler("PST", (pData, value, oldValue) =>
            {
                var player = pData.Player;

                var state = (Sync.Phone.PhoneStateTypes)((int?)value ?? 0);

                Sync.Phone.SetState(player, state);
            });

            AddDataHandler("Crawl::On", (pData, value, oldValue) =>
            {
                var player = pData.Player;

                var state = (bool?)value ?? false;

                if (player.Handle == Player.LocalPlayer.Handle)
                {
                    if (state)
                    {
                        Crawl.On(true);
                    }
                    else
                    {
                        Crawl.Off(true);
                    }
                }
            });

            AddDataHandler("Crouch::On", (pData, value, oldValue) =>
            {
                var player = pData.Player;

                var state = (bool?)value ?? false;

                if (state)
                {
                    Crouch.On(true, player);
                }
                else
                {
                    Crouch.Off(true, player);
                }
            });

            AddDataHandler("IsInvalid", (pData, value, oldValue) =>
            {
                var player = pData.Player;
                var state = (bool?)value ?? false;

                if (player.Handle == Player.LocalPlayer.Handle)
                {
                    Settings.Special.DisabledPerson = state;
                }
            });

            AddDataHandler("IsInvisible", (pData, value, oldValue) =>
            {
                var player = pData.Player;

                var state = (bool?)value ?? false;

                player.SetNoCollisionEntity(Player.LocalPlayer.Handle, !state);
            });

            AddDataHandler("Sex", (pData, value, oldValue) =>
            {
                if (pData.Player.Handle != Player.LocalPlayer.Handle)
                    return;

                var state = (bool)value;

                CEF.Menu.SetSex(state);
            });

            AddDataHandler("Knocked", (pData, value, oldValue) =>
            {
                var player = pData.Player;

                var state = (bool?)value ?? false;

                if (state)
                {
                    player.SetCanRagdoll(false);
                }
                else
                {
                    player.SetCanRagdoll(true);
                }

                if (player.Handle == Player.LocalPlayer.Handle)
                {
                    if (state)
                    {
                        CEF.Death.Show();

                        GameEvents.DisableMove(true);

                        RAGE.Game.Graphics.StartScreenEffect("DeathFailMPIn", 0, true);
                    }
                    else
                    {
                        GameEvents.DisableMove(false);

                        RAGE.Game.Graphics.StopScreenEffect("DeathFailMPIn");

                        Additional.Scaleform.Get("wasted")?.Destroy();
                    }
                }
            });

            AddDataHandler("AdminLevel", (pData, value, oldValue) =>
            {
                if (pData.Player == Player.LocalPlayer)
                {
                    var level = (int?)value ?? 0;

                    SetPlayerAsAdmin(level);
                }
            });

            AddDataHandler("VoiceRange", (pData, value, oldValue) =>
            {
                var player = pData.Player;

                var vRange = (float)value;

                if (player.Handle == Player.LocalPlayer.Handle)
                {
                    // Voice Off
                    if (vRange > 0f)
                    {
                        Voice.Muted = false;

                        CEF.HUD.SwitchMicroIcon(true);

                        GameEvents.Update -= Sync.Microphone.OnTick;
                        GameEvents.Update += Sync.Microphone.OnTick;

                        Sync.Microphone.StartUpdateListeners();

                        Sync.Microphone.SetTalkingAnim(Player.LocalPlayer, true);
                    }
                    // Voice On / Muted
                    else if (vRange <= 0f)
                    {
                        Sync.Microphone.StopUpdateListeners();

                        Voice.Muted = true;

                        CEF.HUD.SwitchMicroIcon(false);

                        Sync.Microphone.SetTalkingAnim(Player.LocalPlayer, false);

                        GameEvents.Update -= Sync.Microphone.OnTick;

                        if (vRange < 0f)
                        {
                            CEF.HUD.SwitchMicroIcon(null);
                        }
                    }
                }
                else
                {
                    if (vRange > 0f)
                    {
                        Sync.Microphone.AddTalker(player);
                    }
                    else
                    {
                        Sync.Microphone.RemoveTalker(player);
                    }
                }
            });

            AddDataHandler("VehicleSeat", async (pData, value, oldValue) =>
            {
                var player = pData.Player;

                var seat = (int?)value ?? -1;

                if (seat >= 0)
                {
                    if (player.Vehicle?.Exists != true)
                        return;

                    player.SetIntoVehicle(player.Vehicle.Handle, seat - 1);

                    AsyncTask.RunSlim(() =>
                    {
                        Sync.Players.UpdateHat(player);
                    }, 250);

                    if (player.Handle == Player.LocalPlayer.Handle)
                    {
                        if (seat == 0 || seat == 1)
                        {
                            var veh = Player.LocalPlayer.Vehicle;

                            Sync.Vehicles.VehicleData vData = null;

                            do
                            {
                                vData = Sync.Vehicles.GetData(veh);

                                if (vData == null)
                                {
                                    await RAGE.Game.Invoker.WaitAsync(100);

                                    continue;
                                }
                                else
                                {
                                    HUD.SwitchSpeedometer(true);

                                    if (seat == 0)
                                        Vehicles.StartDriverSync();

                                    break;
                                }
                            }
                            while (veh?.Exists == true && veh.GetPedInSeat(seat - 1, 0) == Player.LocalPlayer.Handle);
                        }
                        else
                        {
                            HUD.SwitchSpeedometer(false);
                        }
                    }
                }
                else
                {
                    Sync.Players.UpdateHat(player);
                }
            });

            #endregion
        }

        public static void UpdateHat(Player player)
        {
            if (player == null)
                return;

            var pData = GetData(player);

            if (pData == null)
                return;

            var hData = pData.Hat?.Split('|');

            if (hData == null)
            {
                player.ClearProp(0);

                return;
            }

            player.SetPropIndex(0, int.Parse(hData[0]), int.Parse(hData[1]), true);
        }

        private static void UpdateSkill(SkillTypes sType, int value)
        {
            if (sType == SkillTypes.Strength)
            {
                value = (int)Math.Round(0.5f * value); // 20 * a

                RAGE.Game.Stats.StatSetInt(RAGE.Util.Joaat.Hash("MP0_STAMINA"), value, true);
                RAGE.Game.Stats.StatSetInt(RAGE.Util.Joaat.Hash("MP0_STRENGTH"), value, true);
                RAGE.Game.Stats.StatSetInt(RAGE.Util.Joaat.Hash("MP0_LUNG_CAPACITY"), value, true);
            }
            else if (sType == SkillTypes.Shooting)
            {
                value = (int)Math.Round(0.25f * value); // 10 * a

                RAGE.Game.Stats.StatSetInt(RAGE.Util.Joaat.Hash("MP0_SHOOTING_ABILITY"), value, true);
            }
        }

        private static void UpdateDrivingSkill(bool hasLicense) => RAGE.Game.Stats.StatSetInt(RAGE.Util.Joaat.Hash("MP0_DRIVING_ABILITY"), hasLicense ? 50 : 0, true);

        private static void UpdateFlyingSkill(bool hasLicense) => RAGE.Game.Stats.StatSetInt(RAGE.Util.Joaat.Hash("MP0_FLYING_ABILITY"), hasLicense ? 50 : 0, true);

        private static void UpdateBikeSkill(bool hasLicense) => RAGE.Game.Stats.StatSetInt(RAGE.Util.Joaat.Hash("MP0_WHEELIE_ABILITY"), hasLicense ? 50 : 0, true);

        private static void UpdateAchievement(PlayerData pData, AchievementTypes aType, int value, int maxValue)
        {
            if (pData == null)
            {
                pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;
            }

            if (pData.Achievements?.ContainsKey(aType) == true)
            {
                CEF.Menu.UpdateAchievement(aType, value, maxValue);
            }
            else
            {
                var aData = Locale.General.Players.AchievementTexts.ContainsKey(aType) ? Locale.General.Players.AchievementTexts[aType] : ("null", "null");

                CEF.Menu.AddAchievement(aType, value, maxValue, aData.Item1, aData.Item2);
            }
        }

        public static async void TryShowWeaponSkinsMenu()
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var wSkins = pData.WeaponSkins;

            if (wSkins.Count == 0)
            {
                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Inventory.NoWeaponSkins);

                return;
            }

            await CEF.ActionBox.ShowSelect
            (
                "WeaponSkinsMenuSelect", Locale.Actions.WeaponSkinsMenuSelectHeader, wSkins.Select(x => ((decimal)x.Key, $"{(Locale.Actions.WeaponSkinTypeNames.GetValueOrDefault(x.Key) ?? "null")} | {Data.Items.GetName(x.Value).Split(' ')[0]}")).ToArray(), Locale.Actions.SelectOkBtn1, Locale.Actions.SelectCancelBtn1,

                CEF.ActionBox.DefaultBindAction,

                async (rType, id) =>
                {
                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    var wSkins = pData.WeaponSkins;

                    if (rType == CEF.ActionBox.ReplyTypes.OK)
                    {
                        if (!wSkins.Keys.Where(x => (int)x == id).Any())
                        {
                            CEF.ActionBox.Close(true);

                            return;
                        }

                        if (CEF.ActionBox.LastSent.IsSpam(500, false, true))
                            return;

                        if ((bool)await Events.CallRemoteProc("WSkins::Rm", id))
                        {
                            CEF.ActionBox.Close(true);
                        }
                    }
                    else if (rType == CEF.ActionBox.ReplyTypes.Cancel)
                    {
                        CEF.ActionBox.Close(true);
                    }
                    else
                        return;
                },

                null
            );
        }

        public static void CloseAll(bool onlyInterfaces = false)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            RAGE.Game.Ui.SetPauseMenuActive(false);

            CEF.ActionBox.Close(true);

            if (pData != null)
                Sync.Phone.CallChangeState(pData, Phone.PhoneStateTypes.Off);

            CEF.HUD.Menu.Switch(false);
            CEF.Inventory.Close(true);
            CEF.Interaction.CloseMenu();
            CEF.Menu.Close();
            CEF.Death.Close();
            CEF.Animations.Close();
            CEF.Shop.Close(true, true);
            CEF.Gas.Close(true);

            CEF.Documents.Close();

            CEF.BlipsMenu.Close(true);
            CEF.ATM.Close();
            CEF.Bank.Close(true);

            CEF.Estate.Close(true);
            CEF.EstateAgency.Close(true);

            CEF.GarageMenu.Close();
            CEF.HouseMenu.Close(true);
            CEF.BusinessMenu.Close(true);

            CEF.FractionMenu.Close();
            CEF.PoliceTabletPC.Close();

            Data.Minigames.ShootingRange.Finish();

            Data.Minigames.Casino.Casino.Close();

            Data.NPC.CurrentNPC?.SwitchDialogue(false);

            if (!onlyInterfaces)
            {
                Sync.PushVehicle.Off();
                Sync.Crouch.Off();
                Sync.Crawl.Off();

                Sync.Finger.Stop();
            }
        }

        private static void SetPlayerAsAdmin(int aLvl)
        {
            var flyBindIdx = Player.LocalPlayer.GetData<int>("ADMIN::BINDS::FLY");

            if (flyBindIdx >= 0)
                KeyBinds.Unbind(flyBindIdx);

            if (aLvl <= 0)
            {

            }
            else
            {
                Player.LocalPlayer.SetData("ADMIN::BINDS::FLY", KeyBinds.Bind(RAGE.Ui.VirtualKeys.F5, true, () => Data.Commands.Fly(null)));
            }
        }

        private static float FlyF { get; set; } = 2f;
        private static float FlyW { get; set; } = 2f;
        private static float FlyH { get; set; } = 2f;

        private static void FlyRender()
        {
            var pos = Player.LocalPlayer.GetCoords(false);
            var dir = Utils.RotationToDirection(RAGE.Game.Cam.GetGameplayCamRot(0));

            if (RAGE.Game.Pad.IsControlPressed(32, 32)) // W
            {
                if (FlyF < 8f)
                    FlyF *= 1.025f;

                pos.X += dir.X * FlyF;
                pos.Y += dir.Y * FlyF;
                pos.Z += dir.Z * FlyF;
            }
            else if (RAGE.Game.Pad.IsControlPressed(32, 33)) // S
            {
                if (FlyF < 8f)
                    FlyF *= 1.025f;

                pos.X -= dir.X * FlyF;
                pos.Y -= dir.Y * FlyF;
                pos.Z -= dir.Z * FlyF;
            }
            else
            {
                FlyF = 2f;
            }

            if (RAGE.Game.Pad.IsControlPressed(32, 34)) // A
            {
                if (FlyW < 8f)
                    FlyW *= 1.025f;

                pos.X += -dir.Y * FlyW;
                pos.Y += dir.X * FlyW;
            }
            else if (RAGE.Game.Pad.IsControlPressed(32, 35)) // D
            {
                if (FlyW < 8f)
                    FlyW *= 1.05f;

                pos.X -= -dir.Y * FlyW;
                pos.Y -= dir.X * FlyW;
            }
            else
            {
                FlyW = 2f;
            }

            if (RAGE.Game.Pad.IsControlPressed(32, 321)) // Space
            {
                if (FlyH < 8f)
                    FlyH *= 1.025f;

                pos.Z += FlyH;
            }
            else if (RAGE.Game.Pad.IsControlPressed(32, 326)) // LCtrl
            {
                if (FlyH < 8f)
                    FlyH *= 1.05f;

                pos.Z -= FlyH;
            }
            else
            {
                FlyH = 2f;
            }

            Player.LocalPlayer.SetCoordsNoOffset(pos.X, pos.Y, pos.Z, false, false, false);
        }
    }
}
