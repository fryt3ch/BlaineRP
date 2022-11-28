﻿using BCRPClient.CEF;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace BCRPClient.Sync
{
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

        private static GameEvents.UpdateHandler WoundedHandler;
        private static GameEvents.UpdateHandler HungryHandler;

        private static List<Player> KnockedPlayers { get; set; }

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
        #endregion

        public static Dictionary<SkillTypes, int> MaxSkills = new Dictionary<SkillTypes, int>()
        {
            { SkillTypes.Strength, 100 },
            { SkillTypes.Shooting, 100 },
            { SkillTypes.Cooking, 100 },
            { SkillTypes.Fishing, 100 },
        };

        public class PlayerData
        {
            public Player Player { get; private set; }

            public PlayerData(Player Player) => this.Player = Player;

            #region Player Data
            public uint CID => (uint)Player.GetSharedData<int>("CID", 0);

            public int Cash => Player.GetSharedData<int>("Cash", 0);

            public int BankBalance => Player.GetSharedData<int>("BankBalance", 0);

            public bool Sex => Player.GetSharedData<bool>("Sex", true);

            public FractionTypes Fraction => (FractionTypes)Player.GetSharedData<int>("Fraction", -1);

            public int Satiety => Player.GetSharedData<int>("Satiety", 0);

            public int Mood => Player.GetSharedData<int>("Mood", 0);

            public bool Masked => Player.GetSharedData<bool>("Masked", false);

            public bool IsKnocked => Player.GetSharedData<bool>("Knocked", false);

            public bool CrouchOn => Player.GetSharedData<bool>("Crouch::On", false);

            public bool CrawlOn => Player.GetSharedData<bool>("Crawl::On", false);

            public float VoiceRange => Player.GetSharedData<float>("VoiceRange", 0f);

            public bool IsMuted => VoiceRange < 0f;

            public bool IsInvalid => Player.GetSharedData<bool>("IsInvalid", false);

            public bool IsJailed => Player.GetSharedData<bool>("IsJailed", false);

            public bool IsFrozen => Player.GetSharedData<bool>("IsFrozen", false);

            public string Hat => Player.GetSharedData<string>("Hat", null);

            public bool IsWounded => Player.GetSharedData<bool>("IsWounded", false);

            public bool BeltOn => Player.GetSharedData<bool>("Belt::On", false);

            public int VehicleSeat => Player.GetSharedData<int>("VehicleSeat", -1);

            public bool PhoneOn => Player.GetSharedData<bool>("Phone::On", false);

            public int AdminLevel => Player.GetSharedData<int>("AdminLevel", -1);

            public Sync.Animations.GeneralTypes GeneralAnim => (Sync.Animations.GeneralTypes)Player.GetSharedData<int>("Anim::General", -1);

            public Sync.Animations.FastTypes FastAnim => (Sync.Animations.FastTypes)Player.GetSharedData<int>("Anim::Fast", -1);

            public Sync.Animations.OtherTypes OtherAnim => (Sync.Animations.OtherTypes)Player.GetSharedData<int>("Anim::Other", -1);

            public Sync.Animations.WalkstyleTypes Walkstyle => (Sync.Animations.WalkstyleTypes)Player.GetSharedData<int>("Walkstyle", -1);

            public Sync.Animations.EmotionTypes Emotion => (Sync.Animations.EmotionTypes)Player.GetSharedData<int>("Emotion", -1);

            public bool IsInvisible => Player.GetSharedData<bool>("IsInvisible", false);

            public bool IsInvincible => Player.GetSharedData<bool>("IsInvincible", false);

            public int MuteTime { get => Player.GetData<int>("MuteTime"); set => Player.SetData("MuteTime", value); }

            public int JailTime { get => Player.GetData<int>("JailTime"); set => Player.SetData("JailTime", value); }

            public List<(uint VID, Data.Vehicles.Vehicle Data)> OwnedVehicles { get => Player.LocalPlayer.GetData<List<(uint VID, Data.Vehicles.Vehicle Data)>>("OwnedVehicles"); set => Player.LocalPlayer.SetData("OwnedVehicles", value); }

            public List<Data.Locations.Business> OwnedBusinesses { get => Player.LocalPlayer.GetData<List<Data.Locations.Business>>("OwnedBusinesses"); set => Player.LocalPlayer.SetData("OwnedBusinesses", value); }

            public List<Data.Locations.House> OwnedHouses { get => Player.LocalPlayer.GetData<List<Data.Locations.House>>("OwnedHouses"); set => Player.LocalPlayer.SetData("OwnedHouses", value); }

            public List<int> OwnedApartments { get; set; }

            public List<int> OwnedGarages { get; set; }

            public List<uint> Familiars { get => Player.LocalPlayer.GetData<List<uint>>("Familiars"); set => Player.LocalPlayer.SetData("Familiars", value); }

            public Dictionary<SkillTypes, int> Skills { get => Player.LocalPlayer.GetData<Dictionary<SkillTypes, int>>("Skills"); set => Player.LocalPlayer.SetData("Skills", value); }

            public List<LicenseTypes> Licenses { get => Player.LocalPlayer.GetData<List<LicenseTypes>>("Licenses"); set => Player.LocalPlayer.SetData("Licenses", value); }

            public Entity IsAttachedTo { get => Player.GetData<Entity>("IsAttachedTo::Entity"); set => Player.SetData("IsAttachedTo::Entity", value); }

            public Data.Customization.HairOverlay HairOverlay
            {
                get => Player.GetData<Data.Customization.HairOverlay>("HairOverlay");

                set
                {
                    Player.SetData("HairOverlay", value);

                    Player.ClearFacialDecorations();

                    if (value == null)
                        return;

                    Player.SetFacialDecoration(value.Collection, value.Overlay);
                }
            }
            #endregion

            public void Reset()
            {
                if (Player == null)
                    return;

                Player.ClearTasksImmediately();

                Player.SetNoCollisionEntity(Player.LocalPlayer.Handle, false);

                Sync.Microphone.RemoveTalker(Player);
                Sync.Microphone.RemoveListener(Player, false);

                KnockedPlayers.Remove(Player);

                Player.ResetData();
            }
        }

        public static PlayerData GetData(Player player)
        {
            if (player == null)
                return null;

            return player.HasData("SyncedData") ? player.GetData<PlayerData>("SyncedData") : null;
        }

        public static void SetData(Player player, PlayerData data)
        {
            if (player == null)
                return;

            player.SetData("SyncedData", data);
        }

        public Players()
        {
            CharacterLoaded = false;

            (new AsyncTask(() => WoundedHandler?.Invoke(), WoundedTime, true, 0)).Run();
            (new AsyncTask(() => HungryHandler?.Invoke(), HungryTime, true, 0)).Run();

            GameEvents.Render += RenderPlayers;

            KnockedPlayers = new List<Player>();

            Player.LocalPlayer.SetData("IsFrozen", false);

            (new AsyncTask(() =>
            {
                for (int i = 0; i < KnockedPlayers.Count; i++)
                {
                    var p = KnockedPlayers[i];

                    if (p == null || p.IsPlayingAnim("random@dealgonewrong", "idle_a", 3))
                        continue;

                    p.TaskPlayAnim("random@dealgonewrong", "idle_a", 1f, 1f, -1, 1, 0, false, false, false);
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
                {
                    await RAGE.Game.Invoker.WaitAsync(10);
                }

                Player.LocalPlayer.AutoVolume = false;
                Player.LocalPlayer.VoiceVolume = 0f;

                await CEF.Browser.Render(CEF.Browser.IntTypes.Inventory_Full, true, false);

                await CEF.Browser.Render(CEF.Browser.IntTypes.Chat, true, false);

                await CEF.Browser.Render(CEF.Browser.IntTypes.Interaction, true, false);

                await CEF.Browser.Render(CEF.Browser.IntTypes.NPC, true, false);

                CEF.Browser.Window.ExecuteJs("Hud.createSpeedometer", 500);

                var player = Player.LocalPlayer;

                PlayerData data = new PlayerData(Player.LocalPlayer);

                var settings = RAGE.Util.Json.Deserialize<(float, float, float, float, float, float)>((string)args[0]);

                BCRPClient.Settings.STREAM_DISTANCE = settings.Item1;
                BCRPClient.Settings.ENTITY_INTERACTION_MAX_DISTANCE = settings.Item2;
                BCRPClient.Settings.ENTITY_INTERACTION_MAX_DISTANCE_RENDER = settings.Item3;
                BCRPClient.Settings.MIN_CRUISE_CONTROL_SPEED = settings.Item4;
                BCRPClient.Settings.MAX_CRUISE_CONTROL_SPEED = settings.Item5;
                BCRPClient.Settings.MAX_INVENTORY_WEIGHT = settings.Item6;

                var sData = RAGE.Util.Json.Deserialize<JObject>((string)args[1]);

                data.Familiars = RAGE.Util.Json.Deserialize<List<uint>>((string)sData["Familiars"]);

                data.Licenses = RAGE.Util.Json.Deserialize<List<LicenseTypes>>((string)sData["Licenses"]);

                data.Skills = RAGE.Util.Json.Deserialize<Dictionary<SkillTypes, int>>((string)sData["Skills"]);

                data.OwnedVehicles = RAGE.Util.Json.Deserialize<List<string>>((string)sData["Vehicles"]).Select(x => { var data = x.Split('_'); return (Convert.ToUInt32(data[0]), Data.Vehicles.GetById(data[1])); }).ToList();
                data.OwnedBusinesses = RAGE.Util.Json.Deserialize<List<int>>((string)sData["Businesses"]).Select(x => Data.Locations.Business.All[x]).ToList();
                data.OwnedHouses = RAGE.Util.Json.Deserialize<List<uint>>((string)sData["Houses"]).Select(x => Data.Locations.House.All[x]).ToList();

                foreach (var x in data.Skills)
                    UpdateSkill(x.Key, x.Value);

                UpdateDrivingSkill(data.Licenses.Contains(LicenseTypes.B));
                UpdateBikeSkill(data.Licenses.Contains(LicenseTypes.A));
                UpdateFlyingSkill(data.Licenses.Contains(LicenseTypes.Fly));

                CEF.Inventory.Load((JArray)sData["Inventory"]);

                CEF.Menu.Load(data, (int)sData["TimePlayed"], (DateTime)sData["CreationDate"], (DateTime)sData["BirthDate"], RAGE.Util.Json.Deserialize<Dictionary<uint, (int, string, int , int)>>((string)sData["Gifts"]));

                CEF.Menu.SetOrganisation((string)sData["Org"]);

                foreach (var x in data.Skills)
                    CEF.Menu.UpdateSkill(x.Key, x.Value);

                while (data.CID == 0)
                {
                    await RAGE.Game.Invoker.WaitAsync(10);
                }

                CEF.Menu.SetCID(data.CID);

                InvokeHandler("Cash", data, data.Cash, null);
                InvokeHandler("BankBalance", data, data.BankBalance, null);

                InvokeHandler("Sex", data, data.Sex, null);

                InvokeHandler("Fraction", data, data.Fraction, null);

                InvokeHandler("Mood", data, data.Mood, null);
                InvokeHandler("Satiety", data, data.Satiety, null);

                InvokeHandler("Knocked", data, data.IsKnocked, null);

                InvokeHandler("Wounded", data, data.IsWounded, null);

                InvokeHandler("Anim::General", data, data.GeneralAnim, null);

                data.HairOverlay = Data.Customization.GetHairOverlay(data.Sex, player.GetSharedData<int>("Customization::HairOverlay"));

                SetData(Player.LocalPlayer, data);
            });

            Events.Add("Players::CharacterReady", async (object[] args) =>
            {
                if (CharacterLoaded)
                    return;

                PlayerData data = GetData(Player.LocalPlayer);

                while (data == null)
                {
                    await RAGE.Game.Invoker.WaitAsync(10);

                    data = GetData(Player.LocalPlayer);
                }

                (new AsyncTask(() =>
                {
                    Events.CallRemote("Player::UpdateTime");

                    CEF.Menu.TimePlayed += 1;
                }, 60000, true, 60000)).Run();

                CEF.HUD.Menu.SetCurrentTypes(HUD.Menu.Types.Menu, HUD.Menu.Types.Documents, HUD.Menu.Types.BlipsMenu);

                Settings.Load();
                KeyBinds.LoadAll();

                await CEF.Animations.Load();

                Events.CallRemote("Players::CharacterReady", data.IsInvalid, Settings.Other.CurrentEmotion, Settings.Other.CurrentWalkstyle);

                CharacterLoaded = true;

                CEF.Menu.UpdateSettingsData();
                CEF.Menu.UpdateKeyBindsData();

                Player.LocalPlayer.SetInvincible(false);

                GameEvents.Render -= GameEvents.DisableAllControls;

                CEF.HUD.ShowHUD(!Settings.Interface.HideHUD);

                Interaction.Enabled = true;
                Sync.World.EnabledItemsOnGround = true;

                CEF.Chat.Show(true);

                Additional.ExtraColshape.Activate();

                Additional.Discord.SetDefault();

                await RAGE.Game.Invoker.WaitAsync(500);

                foreach (var x in data.OwnedBusinesses)
                    x.ToggleOwnerBlip(true);

                foreach (var x in data.OwnedHouses)
                    x.ToggleOwnerBlip(true);

                //CEF.Menu.UpdateProperties(data);
            });
            #endregion

            #region New Player Stream
            Events.OnEntityStreamIn += async (Entity entity) =>
            {
                if (entity is Player player)
                {
                    if (!player.Exists || player.IsLocal)
                        return;

                    var loaded = await player.WaitIsLoaded();

                    if (!loaded)
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

                    InvokeHandler("IsInvisible", data, data.IsInvisible, null);

                    data.HairOverlay = Data.Customization.GetHairOverlay(data.Sex, player.GetSharedData<int>("Customization::HairOverlay", -1));

                    if (data.VehicleSeat != -1)
                    {
                        InvokeHandler("VehicleSeat", data, data.VehicleSeat, null);
                    }

                    if (data.VoiceRange > 0f)
                        Sync.Microphone.AddTalker(player);

                    if (data.CrouchOn)
                        Crouch.On(true, player);

                    if (data.PhoneOn)
                        Phone.On(true, player);

                    if (data.GeneralAnim != Animations.GeneralTypes.None)
                        Sync.Animations.Play(player, data.GeneralAnim);

                    if (data.OtherAnim != Animations.OtherTypes.None)
                        Sync.Animations.Play(player, data.OtherAnim);

                    SetData(player, data);
                }
            };

            Events.OnEntityStreamOut += (Entity entity) =>
            {
                if (entity is Player player)
                {
                    var data = GetData(player);

                    if (data == null)
                        return;

                    data.Reset();
                }
            };
            #endregion

            #region Local Player Events

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

                Player.LocalPlayer.ResetData("Smoke::Data::CTask");
            });

            Events.Add("Player::Smoke::Puff", (object[] args) =>
            {
                AsyncTask.RunSlim(async () =>
                {
                    if (!Player.LocalPlayer.HasData("Smoke::Data::Puffs"))
                        return;

                    await Utils.RequestPtfx("core");

                    var fxHandle = RAGE.Game.Graphics.StartParticleFxLoopedOnEntityBone("exp_grd_bzgas_smoke", Player.LocalPlayer.Handle, 0f, 0f, 0f, 0f, 0f, 0f, Player.LocalPlayer.GetBoneIndex(20279), 0.15f, false, false, false);

                    await RAGE.Game.Invoker.WaitAsync(1000);

                    RAGE.Game.Graphics.StopParticleFxLooped(fxHandle, false);
                }, 2000);

                AsyncTask.RunSlim(() =>
                {
                    if (!Player.LocalPlayer.HasData("Smoke::Data::Puffs"))
                        return;

                    Player.LocalPlayer.SetData("Smoke::Data::Puffs", Player.LocalPlayer.GetData<int>("Smoke::Data::Puffs") - 1);
                }, 3000);
            });

            Events.Add("Player::CloseAll", (object[] args) =>
            {
                CloseAll((bool)args[0]);
            });

            AddDataHandler("Anim::Fast", (pData, value, oldValue) =>
            {

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
                }

                CEF.Menu.UpdateProperties(data);
            });

            Events.Add("Players::Freeze", (object[] args) =>
            {
                var state = (bool)args[0];

                RAGE.Elements.Player.LocalPlayer.FreezePosition(state);

                Player.LocalPlayer.SetData("IsFrozen", state);

                if (state)
                {
                    CloseAll(false);

                    GameEvents.Render -= GameEvents.DisableAllControls;
                    GameEvents.Render += GameEvents.DisableAllControls;
                }
                else
                    GameEvents.Render -= GameEvents.DisableAllControls;

                if (args.Length > 1)
                    CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Notifications.DefHeader, string.Format(state ? Locale.Notifications.Players.Administrator.FreezedBy : Locale.Notifications.Players.Administrator.UnfreezedBy, (string)args[1]));
            });

            AddDataHandler("Cash", (pData, value, oldValue) =>
            {
                if (pData.Player.Handle != Player.LocalPlayer.Handle)
                    return;

                var cash = (int)value;

                CEF.HUD.SetCash(cash);
                CEF.Menu.SetCash(cash);

                var diff = cash - (oldValue == null ? cash : (int)oldValue);

                if (diff == 0)
                    return;

                string header = string.Format(diff <= 0 ? Locale.Notifications.Money.Cash.LossHeader : Locale.Notifications.Money.Cash.AddHeader, Math.Abs(diff));
                string content = string.Format(Locale.Notifications.Money.Cash.Balance, cash);

                CEF.Notification.Show(CEF.Notification.Types.Cash, header, content);
            });

            AddDataHandler("BankBalance", (pData, value, oldValue) =>
            {
                if (pData.Player.Handle != Player.LocalPlayer.Handle)
                    return;

                var bank = (int)value;

                CEF.HUD.SetBank(bank);
                CEF.Menu.SetBank(bank);

                CEF.ATM.UpdateMoney(bank);
                CEF.Bank.UpdateMoney(bank);

                var diff = bank - (oldValue == null ? bank : (int)oldValue);

                if (diff == 0)
                    return;

                string header = string.Format(diff <= 0 ? Locale.Notifications.Money.Bank.LossHeader : Locale.Notifications.Money.Bank.AddHeader, Math.Abs(diff));
                string content = string.Format(Locale.Notifications.Money.Bank.Balance, bank);

                CEF.Notification.Show(CEF.Notification.Types.Bank, header, content);
            });

            AddDataHandler("IsWounded", (pData, value, oldValue) =>
            {
                var player = pData.Player;

                var state = (bool)value;

                if (player.Handle == Player.LocalPlayer.Handle)
                {
                    if (state)
                    {
                        CEF.HUD.SwitchStatusIcon(HUD.StatusTypes.Wounded, true);

                        CEF.Notification.ShowHint(Locale.Notifications.Players.States.Wounded, false);

                        WoundedHandler -= WoundedUpdate;
                        WoundedHandler += WoundedUpdate;

                        RAGE.Game.Graphics.StartScreenEffect("DeathFailMPDark", 0, true);
                    }
                    else
                    {
                        RAGE.Game.Graphics.StopScreenEffect("DeathFailMPDark");

                        CEF.HUD.SwitchStatusIcon(HUD.StatusTypes.Wounded, false);

                        WoundedHandler -= WoundedUpdate;
                    }
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
                        HungryHandler -= HungryUpdate;
                        HungryHandler += HungryUpdate;
                    }
                }
                else
                {
                    HungryHandler -= HungryUpdate;

                    CEF.HUD.SwitchStatusIcon(HUD.StatusTypes.Mood, false);
                }

                CEF.Inventory.UpdateStates();
            });
            #endregion

            #region Streamed Players Data Change

            AddDataHandler("Emotion", (pData, value, oldValue) =>
            {
                var player = pData.Player;

                var emotion = (Animations.EmotionTypes)(int)value;

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

                var wStyle = (Animations.WalkstyleTypes)(int)value;

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

                var anim = (Sync.Animations.OtherTypes)(int)value;

                if (player.Handle == Player.LocalPlayer.Handle)
                {
                    if (anim == Animations.OtherTypes.None)
                    {
                        if (oldValue is int oldAnim)
                            CEF.Animations.ToggleAnim("a-" + ((Sync.Animations.OtherTypes)oldAnim).ToString(), false);

                        GameEvents.Render -= CEF.Animations.Render;
                        KeyBinds.Get(KeyBinds.Types.CancelAnimation).Disable();
                    }
                    else
                    {
                        CEF.Animations.ToggleAnim("a-" + anim.ToString(), true);
                    }
                }

                if (anim == Animations.OtherTypes.None)
                    Sync.Animations.Stop(player);
                else
                    Sync.Animations.Play(player, anim);
            });

            AddDataHandler("Anim::General", (pData, value, oldValue) =>
            {
                var player = pData.Player;

                var anim = (Sync.Animations.GeneralTypes)(int)value;

                if (anim == Animations.GeneralTypes.None)
                {
                    Sync.Animations.Stop(player);
                }
                else
                {
                    Sync.Animations.Play(player, anim);
                }
            });

            AddDataHandler("Customization::HairOverlay", (pData, value, oldValue) =>
            {
                pData.HairOverlay = Data.Customization.GetHairOverlay(pData.Sex, (int)value);
            });

            AddDataHandler("Belt::On", (pData, value, oldValue) =>
            {
                if (pData.Player.Handle != Player.LocalPlayer.Handle)
                    return;

                var state = (bool)value;

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

            AddDataHandler("Phone::On", (pData, value, oldValue) =>
            {
                var player = pData.Player;

                var state = (bool)value;

                if (state)
                {
                    Phone.On(true, player);
                }
                else
                {
                    Phone.Off(true, player);
                }
            });

            AddDataHandler("Crawl::On", (pData, value, oldValue) =>
            {
                var player = pData.Player;

                var state = (bool)value;

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

                var state = (bool)value;

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
                var state = (bool)value;

                if (player.Handle == Player.LocalPlayer.Handle)
                {
                    Settings.Special.DisabledPerson = state;
                }
            });

            AddDataHandler("IsInvisible", (pData, value, oldValue) =>
            {
                var player = pData.Player;

                player.SetNoCollisionEntity(Player.LocalPlayer.Handle, !(bool)value);
            });

            AddDataHandler("Hat", (pData, value, oldValue) =>
            {

            });

            AddDataHandler("Sex", (pData, value, oldValue) =>
            {
                if (pData.Player.Handle != Player.LocalPlayer.Handle)
                    return;

                var state = (bool)value;

                CEF.Menu.SetSex(state);
            });

            AddDataHandler("Masked", (pData, value, oldValue) =>
            {

            });

            AddDataHandler("Knocked", (pData, value, oldValue) =>
            {
                var player = pData.Player;

                var state = (bool)value;

                if (state)
                {
                    if (!KnockedPlayers.Contains(player))
                        KnockedPlayers.Add(player);
                }
                else
                    KnockedPlayers.Remove(player);

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

                        GameEvents.Update -= GameEvents.DisableMove;
                        GameEvents.Update += GameEvents.DisableMove;

                        RAGE.Game.Graphics.StartScreenEffect("DeathFailMPIn", 0, true);
                    }
                    else
                    {
                        GameEvents.Update -= GameEvents.DisableMove;

                        RAGE.Game.Graphics.StopScreenEffect("DeathFailMPIn");

                        Additional.Scaleform.Close();
                    }
                }
            });

            AddDataHandler("Fraction", (pData, value, oldValue) =>
            {
                var fraction = (FractionTypes)(int)value;

                if (pData.Player.Handle == Player.LocalPlayer.Handle)
                    CEF.Menu.SetFraction(fraction);
            });

            AddDataHandler("AdminLevel", (pData, value, oldValue) =>
            {

            });

            AddDataHandler("VehicleSeat", async (pData, value, oldValue) =>
            {
                var player = pData.Player;

                var seat = (int)value;

                if (seat >= 0)
                {
                    if (player.Vehicle?.Exists != true)
                        return;

                    player.SetIntoVehicle(player.Vehicle.Handle, seat - 1);

                    AsyncTask.RunSlim(() =>
                    {
                        Sync.Players.UpdateHat(player);

                        Phone.TurnVehiclePhone(player);
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
                            HUD.SwitchSpeedometer(false);
                    }
                }
                else
                {
                    Sync.Players.UpdateHat(player);

                    Phone.TurnVehiclePhone(player);
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

            #endregion
        }

        private static void RenderPlayers()
        {
            for (int i = 0; i < RAGE.Elements.Entities.Players.Streamed.Count; i++)
            {
                var player = RAGE.Elements.Entities.Players.Streamed[i];

                if (player == null)
                    continue;

                player.SetResetFlag(200, true);
            }
        }

        private static void WoundedUpdate()
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null || pData.IsInvincible)
                return;

            Player.LocalPlayer.SetRealHealth(Player.LocalPlayer.GetRealHealth() - WoundedReduceHP);
        }

        private static void HungryUpdate()
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
        }

        public static void UpdateHat(Player player)
        {
            if (player == null)
                return;

            var pData = GetData(player);

            if (pData == null)
                return;

            if (pData.Hat == null)
            {
                player.ClearProp(0);

                return;
            }

            var hData = pData.Hat.Split('|');

            if (hData.Length < 3)
                return;

            var data = (Data.Items.Hat.ItemData)Data.Items.GetData(hData[0], typeof(Data.Items.Hat));

            if (data == null)
                return;

            player.SetPropIndex(0, hData[2] == "1" ? data.ExtraData?.Drawable ?? data.Drawable : data.Drawable, data.Textures[int.Parse(hData[1])], true);
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

        public static void CloseAll(bool onlyInterfaces = false)
        {
            RAGE.Game.Ui.SetPauseMenuActive(false);

            CEF.HUD.Menu.Switch(false);
            CEF.Inventory.Close(true);
            CEF.Interaction.CloseMenu();
            CEF.Menu.Close();
            CEF.Death.Close();
            CEF.Animations.Close();
            CEF.ActionBox.Close(true);
            CEF.Shop.Close(true, true);
            CEF.Gas.Close(true);

            CEF.Documents.Close();

            CEF.BlipsMenu.Close(true);
            CEF.ATM.Close(true);
            CEF.Bank.Close(true);

            CEF.Estate.Close(true);
            CEF.Estate.Agency.Close(true);

            Data.NPC.CurrentNPC?.SwitchDialogue(false);

            Sync.Phone.Off();

            if (!onlyInterfaces)
            {
                Sync.PushVehicle.Off();
                Sync.Crouch.Off();
                Sync.Crawl.Off();

                Sync.Finger.Stop();
            }
        }
    }
}
