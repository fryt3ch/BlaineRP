using BCRPClient.CEF;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using static BCRPClient.Sync.AttachSystem;

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
            Player Player = null;

            public PlayerData(Player Player) => this.Player = Player;

            #region Player Data
            public int CID
            {
                get => Player.GetData<int>("CID");

                set
                {
                    Player.SetData("CID", value);

                    if (Player.Handle == Player.LocalPlayer.Handle)
                    {
                        CEF.Menu.SetCID(value);
                    }
                }
            }

            public int Cash
            {
                get => Player.GetData<int>("Cash");

                set
                {
                    Player.SetData("Cash", value);

                    if (Player.Handle == Player.LocalPlayer.Handle)
                    {
                        CEF.HUD.SetCash(value);
                        CEF.Menu.SetCash(value);
                    }
                }
            }

            public int BankBalance
            {
                get => Player.GetData<int>("BankBalance");

                set
                {
                    Player.SetData("BankBalance", value);

                    if (Player.Handle == Player.LocalPlayer.Handle)
                    {
                        CEF.HUD.SetBank(value);
                        CEF.Menu.SetBank(value);
                    }
                }
            }

            public bool Sex
            {
                get => Player.GetData<bool>("Sex");

                set
                {
                    Player.SetData("Sex", value);

                    if (Player.Handle == Player.LocalPlayer.Handle)
                    {
                        CEF.Menu.SetSex(value);
                    }
                }
            }
            public FractionTypes Fraction
            {
                get => Player.GetData<FractionTypes>("Fraction");

                set
                {
                    Player.SetData("Fraction", value);

                    if (Player.Handle == Player.LocalPlayer.Handle)
                    {
                        CEF.Menu.SetFraction(value);
                    }
                }
            }

            public List<int> Familiars { get; set; }

            public Dictionary<SkillTypes, int> Skills { get; set; }

            public List<LicenseTypes> Licenses { get; set; }

            public int Satiety
            {
                get => Player.GetData<int>("Satiety");

                set
                {
                    Player.SetData("Satiety", value);

                    if (Player.Handle == Player.LocalPlayer.Handle)
                    {
                        CEF.Inventory.UpdateStates();

                        if (value <= 25)
                        {
                            CEF.HUD.SwitchStatusIcon(HUD.StatusTypes.Food, true);

                            if (value % 5 == 0)
                                CEF.Notification.ShowHint(Locale.Notifications.Players.States.LowSatiety, false, 5000);

                            if (value == 0)
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
                    }
                }
            }

            public int Mood
            {
                get => Player.GetData<int>("Mood");

                set
                {
                    Player.SetData("Mood", value);

                    if (Player.Handle == Player.LocalPlayer.Handle)
                    {
                        CEF.Inventory.UpdateStates();

                        if (value <= 25)
                        {
                            CEF.HUD.SwitchStatusIcon(HUD.StatusTypes.Mood, true);

                            if (value % 5 == 0)
                                CEF.Notification.ShowHint(Locale.Notifications.Players.States.LowMood, false, 5000);
                        }
                        else
                            CEF.HUD.SwitchStatusIcon(HUD.StatusTypes.Mood, false);
                    }
                }
            }

            public bool Masked
            {
                get => Player.GetData<bool>("Masked");

                set
                {
                    Player.SetData("Masked", value);
                }
            }

            public bool Knocked
            {
                get => Player.GetData<bool>("Knocked");
                
                set
                {
                    Player.SetData("Knocked", value);

                    if (value)
                    {
                        if (!KnockedPlayers.Contains(Player))
                            KnockedPlayers.Add(Player);
                    }
                    else
                        KnockedPlayers.Remove(Player);
                }
            }

            public bool CrouchOn
            {
                get => Player.GetData<bool>("Crouch::On");

                set
                {
                    Player.SetData("Crouch::On", value);
                }
            }

            public bool CrawlOn
            {
                get => Player.GetData<bool>("Crawl::On");

                set
                {
                    Player.SetData("Crawl::On", value);

                    if (Player.Handle == Player.LocalPlayer.Handle)
                    {
                        if (value)
                        {
                            Crawl.On(true);
                        }
                        else
                        {
                            Crawl.Off(true);
                        }
                    }
                }
            }

            public float VoiceRange
            {
                get => Player.GetData<float>("VoiceRange");

                set => Player.SetData("VoiceRange", value);
            }

            public bool IsMuted { get => VoiceRange < 0f; }

            public bool IsInvalid
            {
                get => Player.GetData<bool>("IsInvalid");

                set
                {
                    Player.SetData("IsInvalid", value);

                    if (Player.Handle == Player.LocalPlayer.Handle)
                    {
                        Settings.Special.DisabledPerson = value;
                    }
                }
            }

            public bool IsInvincible
            {
                get => Player.GetData<bool>("IsInvincible");

                set
                {
                    Player.SetData("IsInvincible", value);

                    if (Player.Handle != Player.LocalPlayer.Handle)
                    {
                        Player.SetInvincible(value);
                        Player.SetCanBeDamaged(!value);
                    }
                }
            }

            public bool IsJailed { get => Player.GetData<bool>("IsJailed"); set => Player.SetData("IsJailed", value); }
            public bool IsFrozen { get => Player.GetData<bool>("IsFrozen"); set => Player.SetData("IsFrozen", value); }
            public string Hat { get => Player.GetData<string>("Hat"); set => Player.SetData("Hat", value); }

            public bool IsWounded
            {
                get => Player.GetData<bool>("IsWounded");

                set
                {
                    Player.SetData("IsWounded", value);

                    if (Player.Handle == Player.LocalPlayer.Handle)
                    {
                        if (value)
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
                }
            }

            public bool BeltOn { get => Player.GetData<bool>("Belt::On"); set => Player.SetData("Belt::On", value); }
            public bool IsFingerPointing { get => Player.GetData<bool>("IsFingerPointing"); set => Player.SetData("IsFingerPointing", value); }

            public int VehicleSeat
            {
                get => Player.GetData<int>("VehicleSeat");

                set
                {
                    Player.SetData("VehicleSeat", value);

                    if (value >= 0)
                    {
                        if (Player.Vehicle?.Exists != true)
                            return;

                        Player.SetIntoVehicle(Player.Vehicle.Handle, value - 1);

                        (new AsyncTask(() =>
                        {
                            Sync.Players.UpdateHat(Player);

                            Phone.TurnVehiclePhone(Player);
                        }, 250, false, 0)).Run();

                        if (value == 0)
                        {
                            if (Player.Handle == Player.LocalPlayer.Handle)
                                Vehicles.StartDriverSync();
                        }
                    }
                    else
                    {
                        Sync.Players.UpdateHat(Player);

                        Phone.TurnVehiclePhone(Player);
                    }
                }
            }

            public bool PhoneOn { get => Player.GetData<bool>("Phone::On"); set => Player.SetData("Phone::On", value); }

            public Entity IsAttachedTo { get => Player.GetData<Entity>("IsAttachedTo::Entity"); set => Player.SetData("IsAttachedTo::Entity", value); }

            public int AdminLevel { get => Player.GetData<int>("AdminLevel"); set => Player.SetData("AdminLevel", value); }

            public Sync.Animations.GeneralTypes GeneralAnim { get => Player.GetData<Sync.Animations.GeneralTypes>("Anim::General"); set => Player.SetData("Anim::General", value); }
            public Sync.Animations.FastTypes FastAnim { get => Player.GetData<Sync.Animations.FastTypes>("Anim::Fast"); set => Player.SetData("Anim::Fast", value); }
            public Sync.Animations.OtherTypes OtherAnim { get => Player.GetData<Sync.Animations.OtherTypes>("Anim::Other"); set => Player.SetData("Anim::Other", value); }

            public Sync.Animations.WalkstyleTypes Walkstyle { get => Player.GetData<Sync.Animations.WalkstyleTypes>("Walkstyle"); set => Player.SetData("Walkstyle", value); }
            public Sync.Animations.EmotionTypes Emotion { get => Player.GetData<Sync.Animations.EmotionTypes>("Emotion"); set => Player.SetData("Emotion", value); }

            public int MuteTime { get => Player.GetData<int>("MuteTime"); set => Player.SetData("MuteTime", value); }
            public int JailTime { get => Player.GetData<int>("JailTime"); set => Player.SetData("JailTime", value); }

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

            public void Reset() => Player?.ResetData();
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

                Data.Locations.Business.LoadNames(RAGE.Util.Json.Deserialize<Dictionary<int, string>>((string)args[1]));

                data.Familiars = ((Newtonsoft.Json.Linq.JArray)args[2]).ToObject<List<int>>();
                data.Licenses = RAGE.Util.Json.Deserialize<List<LicenseTypes>>((string)args[3]);
                data.Skills = RAGE.Util.Json.Deserialize<Dictionary<SkillTypes, int>>((string)args[4]);

                CEF.Inventory.Load((Newtonsoft.Json.Linq.JArray)args[5]);
                CEF.Menu.Load(args[6], args[7], args[8]);

                foreach (var x in data.Skills)
                    CEF.Menu.UpdateSkill(x.Key, x.Value);

                data.CID = player.GetSharedData<int>("CID", -1);

                while (data.CID < 0)
                {
                    await RAGE.Game.Invoker.WaitAsync(10);

                    data.CID = player.GetSharedData<int>("CID", -1);
                }

                data.Cash = player.GetSharedData<int>("Cash");
                data.BankBalance = player.GetSharedData<int>("BankBalance");
                data.Sex = player.GetSharedData<bool>("Sex");
                data.Fraction = (FractionTypes)player.GetSharedData<int>("Fraction");

                data.Mood = player.GetSharedData<int>("Mood");
                data.Satiety = player.GetSharedData<int>("Satiety");

                data.Knocked = player.GetSharedData<bool>("Knocked");
                data.Masked = false;
                data.IsInvincible = player.GetSharedData<bool>("IsInvincible");

                data.CrouchOn = false;
                data.CrawlOn = false;
                data.VoiceRange = player.GetSharedData<float>("VoiceRange");
                data.IsWounded = player.GetSharedData<bool>("IsWounded");

                data.BeltOn = false;
                data.IsFingerPointing = false;

                data.PhoneOn = false;

                data.FastAnim = (Sync.Animations.FastTypes)player.GetSharedData<int>("Anim::Fast");
                data.GeneralAnim = (Sync.Animations.GeneralTypes)player.GetSharedData<int>("Anim::General");
                data.OtherAnim = (Sync.Animations.OtherTypes)player.GetSharedData<int>("Anim::Other");

                data.AdminLevel = player.GetSharedData<int>("AdminLevel");
                data.Hat = player.GetSharedData<string>("Hat");

                data.VehicleSeat = -1;

                data.HairOverlay = Data.Customization.GetHairOverlay(player.IsMale(), player.GetSharedData<int>("Customization::HairOverlay"));

                data.IsAttachedTo = null;

                SetData(Player.LocalPlayer, data);

                Sync.World.Preload();
            });

            Events.Add("Players::CharacterReady", async (object[] args) =>
            {
                if (CharacterLoaded)
                    return;

                PlayerData data = GetData(Player.LocalPlayer);

                while (data == null)
                {
                    await RAGE.Game.Invoker.WaitAsync(50);

                    data = GetData(Player.LocalPlayer);
                }

                (new AsyncTask(() =>
                {
                    Events.CallRemote("Players::UpdateTime");

                    CEF.Menu.TimePlayed += 1;
                }, 60000, true, 60000)).Run();

                CEF.HUD.Menu.SetCurrentTypes(HUD.Menu.Types.Menu, HUD.Menu.Types.Documents);

                Settings.Load();
                KeyBinds.LoadAll();

                await CEF.Animations.Load();

                Events.CallRemote("Players::CharacterReady", data.IsInvalid, Settings.Other.CurrentEmotion, Settings.Other.CurrentWalkstyle);

                //CEF.Browser.Window.ExecuteJs("Menu.drawColorPicker", Settings.Aim.Color.ToHEX());

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
            });
            #endregion

            #region New Player Stream
            Events.OnEntityStreamIn += (Entity entity) =>
            {
                if (entity.Type != RAGE.Elements.Type.Player)
                    return;

                Player player = entity as Player;

                player.AutoVolume = false;
                player.Voice3d = false;
                player.VoiceVolume = 0f;

                if (player?.Exists != true || player.IsLocal)
                    return;

                PlayerData data = new PlayerData(player);

                data.CID = player.GetSharedData<int>("CID", -1);

                if (data.CID < 0)
                    return;

                data.Sex = player.IsMale();
                data.Fraction = (FractionTypes)player.GetSharedData<int>("Fraction", -1);
                //data.Familiars = new List<int>() { data.CID };

                data.Knocked = player.GetSharedData<bool>("Knocked");
                data.Masked = player.GetSharedData<bool>("Masked");
                data.Hat = player.GetSharedData<string>("Hat", null);

                data.CrouchOn = player.GetSharedData<bool>("Crouch::On");
                data.CrawlOn = player.GetSharedData<bool>("Crawl::On");
                data.VoiceRange = player.GetSharedData<float>("VoiceRange");
                data.IsInvalid = player.GetSharedData<bool>("IsInvalid");

                //data.BeltOn = false;
                data.IsFingerPointing = player.GetSharedData<bool>("IsFingerPointing");

                data.VehicleSeat = player.GetSharedData<int>("VehicleSeat", -1);

                data.PhoneOn = player.GetSharedData<bool>("Phone::On");

                data.AdminLevel = player.GetSharedData<int>("AdminLevel", -1);

                data.GeneralAnim = (Sync.Animations.GeneralTypes)player.GetSharedData<int>("Anim::General", -1);
                data.OtherAnim = (Sync.Animations.OtherTypes)player.GetSharedData<int>("Anim::Other", -1);

                data.HairOverlay = Data.Customization.GetHairOverlay(player.IsMale(), player.GetSharedData<int>("Customization::HairOverlay", -1));

                if (player.GetAlpha() != 255)
                    player.SetNoCollisionEntity(Player.LocalPlayer.Handle, false);

                SetData(player, data);

                (new AsyncTask(() =>
                {
                    if (player?.Exists != true)
                        return true;

                    if (player.Handle != 0)
                    {
                        if (data.VehicleSeat != -1 && player.Vehicle != null)
                        {
                            data.VehicleSeat = data.VehicleSeat;
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

                        return true;
                    }

                    return false;
                }, 10, true, 500)).Run();
            };

            Events.OnEntityStreamOut += (Entity entity) =>
            {
                if (entity.Type != RAGE.Elements.Type.Player)
                    return;

                Player player = (Player)entity;

                var data = GetData(player);

                if (data == null)
                    return;

                Sync.Microphone.RemoveTalker(player);
                Sync.Microphone.RemoveListener(player, false);

                if (data.CrouchOn)
                    Crouch.Off(true, player);

                if (data.PhoneOn)
                    Phone.Off(true, player);

                if (data.GeneralAnim != Animations.GeneralTypes.None)
                    Animations.Stop(player);

                data.Reset();

                player.SetNoCollisionEntity(Player.LocalPlayer.Handle, true);

                KnockedPlayers.Remove(player);
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

                    Utils.RequestPtfx("core");

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

            Events.Add("Players::CloseAll", (object[] args) =>
            {
                RAGE.Game.Ui.SetPauseMenuActive(false);

                CEF.HUD.Menu.Switch(false);
                CEF.Inventory.Close(true);
                CEF.Interaction.CloseMenu();
                CEF.Menu.Close();
                CEF.Death.Close();
                CEF.Animations.Close();
                CEF.ActionBox.Close(true);

                Data.NPC.CurrentNPC?.SwitchDialogue(false);

                Sync.Phone.Off();
                Sync.PushVehicle.Off();
                Sync.Crouch.Off();
                Sync.Crawl.Off();

                Sync.Finger.Stop();
            });

            Events.AddDataHandler("Anim::Fast", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.FastAnim = (Animations.FastTypes)(int)value;
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
            });

            Events.Add("Player::Licenses::Update", (object[] args) =>
            {
                var data = Sync.Players.GetData(Player.LocalPlayer);

                if (data == null)
                    return;

                bool add = (bool)args[0];
                int cid = (int)args[1];

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

            Events.Add("Player::Familiars::Update", (object[] args) =>
            {
                var data = Sync.Players.GetData(Player.LocalPlayer);

                if (data == null)
                    return;

                bool add = (bool)args[0];
                int cid = (int)args[1];

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

            Events.Add("Players::Freeze", (object[] args) =>
            {
                var state = (bool)args[0];

                RAGE.Elements.Player.LocalPlayer.FreezePosition(state);

                Player.LocalPlayer.SetData("IsFrozen", state);

                if (state)
                {
                    Events.CallLocal("Players::CloseAll");

                    GameEvents.Render -= GameEvents.DisableAllControls;
                    GameEvents.Render += GameEvents.DisableAllControls;
                }
                else
                    GameEvents.Render -= GameEvents.DisableAllControls;

                if (args.Length > 1)
                    CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Notifications.DefHeader, string.Format(state ? Locale.Notifications.Players.Administrator.FreezedBy : Locale.Notifications.Players.Administrator.UnfreezedBy, (string)args[1]));
            });

            Events.AddDataHandler("Cash", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.Cash = (int)value;
                var diff = pData.Cash - (oldValue == null ? pData.Cash : (int)oldValue);

                if (diff == 0)
                    return;

                string header = string.Format(diff <= 0 ? Locale.Notifications.Money.Cash.LossHeader : Locale.Notifications.Money.Cash.AddHeader, Math.Abs(diff));
                string content = string.Format(Locale.Notifications.Money.Cash.Balance, pData.Cash);

                CEF.Notification.Show(CEF.Notification.Types.Cash, header, content);
            });

            Events.AddDataHandler("BankBalance", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.BankBalance = (int)value;
                var diff = pData.BankBalance - (oldValue == null ? pData.BankBalance : (int)oldValue);

                if (diff == 0)
                    return;

                string header = string.Format(diff <= 0 ? Locale.Notifications.Money.Bank.LossHeader : Locale.Notifications.Money.Bank.AddHeader, Math.Abs(diff));
                string content = string.Format(Locale.Notifications.Money.Bank.Balance, pData.BankBalance);

                CEF.Notification.Show(CEF.Notification.Types.Bank, header, content);
            });

            Events.AddDataHandler("Belt::On", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.BeltOn = (bool)value;

                if ((bool)value)
                {
                    GameEvents.Update -= Sync.Vehicles.BeltTick;
                    GameEvents.Update += Sync.Vehicles.BeltTick;
                }
                else
                {
                    GameEvents.Update -= Sync.Vehicles.BeltTick;
                }

                player.SetConfigFlag(32, !(bool)value);
                HUD.SwitchBeltIcon((bool)value);
            });

            Events.AddDataHandler("IsWounded", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.IsWounded = (bool)value;
            });

            Events.AddDataHandler("Mood", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.Mood = (int)value;
            });

            Events.AddDataHandler("Satiety", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.Satiety = (int)value;
            });
            #endregion

            #region Streamed Players Data Change

            Events.AddDataHandler("Emotion", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.Emotion = (Animations.EmotionTypes)(int)value;

                if (player.Handle == Player.LocalPlayer.Handle)
                {
                    Settings.Other.CurrentEmotion = pData.Emotion;

                    CEF.Animations.ToggleAnim("e-" + Settings.Other.CurrentEmotion.ToString(), true);
                }

                Sync.Animations.Set(player, pData.Emotion);
            });

            Events.AddDataHandler("Walkstyle", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.Walkstyle = (Animations.WalkstyleTypes)(int)value;

                if (player.Handle == Player.LocalPlayer.Handle)
                {
                    Settings.Other.CurrentWalkstyle = pData.Walkstyle;

                    CEF.Animations.ToggleAnim("w-" + Settings.Other.CurrentWalkstyle.ToString(), true);
                }

                if (!pData.CrouchOn)
                    Sync.Animations.Set(player, pData.Walkstyle);
            });

            Events.AddDataHandler("Anim::Other", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.OtherAnim = (Sync.Animations.OtherTypes)(int)value;

                if (player.Handle == Player.LocalPlayer.Handle)
                {
                    if (pData.OtherAnim == Animations.OtherTypes.None)
                    {
                        if (oldValue != null)
                            CEF.Animations.ToggleAnim("a-" + ((Sync.Animations.OtherTypes)(int)oldValue).ToString(), false);

                        GameEvents.Render -= CEF.Animations.Render;
                        KeyBinds.Get(KeyBinds.Types.CancelAnimation).Disable();
                    }
                    else
                    {
                        CEF.Animations.ToggleAnim("a-" + pData.OtherAnim.ToString(), true);
                    }
                }

                if (pData.OtherAnim == Animations.OtherTypes.None)
                    Sync.Animations.Stop(player);
                else
                    Sync.Animations.Play(player, pData.OtherAnim);
            });

            Events.AddDataHandler("Anim::General", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.GeneralAnim = (Sync.Animations.GeneralTypes)(int)value;

                if ((Sync.Animations.GeneralTypes)(int)value == Animations.GeneralTypes.None)
                    Sync.Animations.Stop(player);
                else
                    Sync.Animations.Play(player, (Sync.Animations.GeneralTypes)(int)value);
            });

            Events.AddDataHandler("VehicleSeat", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.VehicleSeat = (int)value;
            });

            Events.AddDataHandler("Customization::HairOverlay", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.HairOverlay = Data.Customization.GetHairOverlay(player.IsMale(), (int)value);
            });

            Events.AddDataHandler("Belt::On", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                if (player.Handle != Player.LocalPlayer.Handle)
                    return;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.BeltOn = (bool)value;

                if ((bool)value)
                {
                    GameEvents.Update -= Sync.Vehicles.BeltTick;
                    GameEvents.Update += Sync.Vehicles.BeltTick;
                }
                else
                {
                    GameEvents.Update -= Sync.Vehicles.BeltTick;
                }

                player.SetConfigFlag(32, !(bool)value);
                HUD.SwitchBeltIcon((bool)value);
            });

            Events.AddDataHandler("Phone::On", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.PhoneOn = (bool)value;

                if ((bool)value)
                {
                    Phone.On(true, player);
                }
                else
                {
                    Phone.Off(true, player);
                }
            });

            Events.AddDataHandler("Crawl::On", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.CrawlOn = (bool)value;
            });

            Events.AddDataHandler("Crouch::On", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.CrouchOn = (bool)value;

                if (pData.CrouchOn)
                {
                    Crouch.On(true, player);
                }
                else
                {
                    Crouch.Off(true, player);
                }
            });

            Events.AddDataHandler("IsInvalid", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.IsInvalid = (bool)value;
            });

            Events.AddDataHandler("IsInvisible", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                player.SetNoCollisionEntity(Player.LocalPlayer.Handle, !(bool)value);
            });

            Events.AddDataHandler("IsInvincible", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.IsInvincible = (bool)value;
            });

            Events.AddDataHandler("Hat", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.Hat = (string)value;
            });

            Events.AddDataHandler("Sex", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.Sex = (bool)value;
            });

            Events.AddDataHandler("Masked", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.Masked = (bool)value;
            });

            Events.AddDataHandler("Knocked", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.Knocked = (bool)value;

                if ((bool)value)
                {
                    player.SetCanRagdoll(false);
                }
                else
                {
                    player.SetCanRagdoll(true);
                }

                if (player.Handle == Player.LocalPlayer.Handle)
                {
                    if ((bool)value)
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

            Events.AddDataHandler("Fraction", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.Fraction = (FractionTypes)((int)value);
            });

            Events.AddDataHandler("AdminLevel", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.AdminLevel = (int)value;
            });

            Events.AddDataHandler("VehicleSeat", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.VehicleSeat = (int)value;
            });

            Events.AddDataHandler("VoiceRange", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = GetData(player);

                if (pData == null)
                    return;

                pData.VoiceRange = (float)value;

                if (player.Handle == Player.LocalPlayer.Handle)
                {
                    // Voice Off
                    if (pData.VoiceRange > 0f)
                    {
                        Voice.Muted = false;

                        CEF.HUD.SwitchMicroIcon(true);

                        GameEvents.Update -= Sync.Microphone.OnTick;
                        GameEvents.Update += Sync.Microphone.OnTick;

                        Sync.Microphone.StartUpdateListeners();

                        Sync.Microphone.SetTalkingAnim(Player.LocalPlayer, true);
                    }
                    // Voice On / Muted
                    else if (pData.VoiceRange <= 0f)
                    {
                        Sync.Microphone.StopUpdateListeners();

                        Voice.Muted = true;

                        CEF.HUD.SwitchMicroIcon(false);

                        Sync.Microphone.SetTalkingAnim(Player.LocalPlayer, false);

                        GameEvents.Update -= Sync.Microphone.OnTick;

                        if (pData.VoiceRange < 0f)
                        {
                            CEF.HUD.SwitchMicroIcon(null);
                        }
                    }
                }
                else
                {
                    if (pData.VoiceRange > 0f)
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
            Player.LocalPlayer.SetRealHealth(Player.LocalPlayer.GetRealHealth() - WoundedReduceHP);
        }

        private static void HungryUpdate()
        {
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
    }
}
