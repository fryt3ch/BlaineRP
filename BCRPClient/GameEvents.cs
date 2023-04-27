#define DEBUGGING
using RAGE; using RAGE.Elements; using System;
using System.Linq;

namespace BCRPClient
{
    public class GameEvents : Events.Script
    {
        public static int FPS => (int)Math.Floor(1f / RAGE.Game.Misc.GetFrameTime());

        public static bool PlayerFreezed = false;

        private static AsyncTask MainLoop;

        public delegate void UpdateHandler();
        public delegate void ScreenResolutionChangeHandler(int x, int y);

        public delegate void OnMouseClick(int x, int y, bool up, bool right);
        public delegate void OnMouseClickCef(int x, int y, bool up, bool right, float relX, float relY, Vector3 worldPos, int eHandle);

        public delegate void WaypointCreatedHandler(Vector3 position);
        public delegate void WaypointDeletedHandler();

        public static event UpdateHandler Update;
        public static event UpdateHandler Render;

        public static event OnMouseClick MouseClicked;
        public static event OnMouseClickCef MouseClickedCef;

        public static event WaypointCreatedHandler WaypointCreated;
        public static event WaypointDeletedHandler WaypointDeleted;

        public static event ScreenResolutionChangeHandler ScreenResolutionChange;

        public static Vector3 WaypointPosition { get; private set; }

        public static RAGE.Ui.Cursor.Vector2 ScreenResolution { get; private set; } = new RAGE.Ui.Cursor.Vector2(0f, 0f);

        private static int DisableAllControlsCounter { get; set; }
        private static int DisableMoveCounter { get; set; }

        public static DateTime? ExtraGameDate { get; set; }

        public GameEvents()
        {
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = Settings.CultureInfo;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = Settings.CultureInfo;
            System.Globalization.CultureInfo.CurrentCulture = Settings.CultureInfo;

            RAGE.Ui.Console.Clear();
            RAGE.Ui.Console.Reset();

            RAGE.Chat.Activate(false);
            RAGE.Chat.Show(false);

            RAGE.Game.Graphics.RemoveParticleFxInRange(0f, 0f, 0f, float.MaxValue);

            Player.LocalPlayer.SetVisible(true, false);

            RAGE.Game.Ui.SetPauseMenuActive(false);

            RAGE.Game.Invoker.Invoke(0x95C0A5BBDC189AA1);

            RAGE.Game.Misc.SetFadeOutAfterDeath(false);
            RAGE.Game.Misc.SetFadeOutAfterArrest(false);

            RAGE.Game.Misc.SetFadeInAfterDeathArrest(false);
            RAGE.Game.Misc.SetFadeInAfterLoad(false);

            RAGE.Game.Graphics.TransitionFromBlurred(0);

            RAGE.Game.Misc.DisableAutomaticRespawn(true);
            RAGE.Game.Misc.IgnoreNextRestart(true);

            RAGE.Game.Invoker.Invoke(0xE6C0C80B8C867537, true); // SetEnableVehicleSlipstreaming

            if (RAGE.Game.Audio.IsStreamPlaying())
                RAGE.Game.Audio.StopStream();

            RAGE.Game.Audio.StopAudioScenes();

            Utils.DisableFlightMusic();

            LoadHUD();

            RAGE.Game.Ui.DisplayRadar(false);

            Render += HideHudComponents;

            GameEvents.DisableAllControls(true);

            Events.OnClick += (x, y, up, right) => MouseClicked?.Invoke(x, y, up, right);
            Events.OnClickWithRaycast += (x, y, up, right, relX, relY, worldPos, eHandle) => MouseClickedCef?.Invoke(x, y, up, right, relX, relY, worldPos, eHandle);

            MouseClickedCef += (x, y, up, right, relX, relY, worldPos, eHandle) =>
            {
                if (right)
                {
                    if (up)
                    {

                    }
                    else
                    {

                    }
                }
                else
                {
                    if (up)
                    {

                    }
                    else
                    {
                        if (Player.LocalPlayer.HasData("Temp::SEntity"))
                        {
                            var pPos = Player.LocalPlayer.Position;

                            pPos.Z += 1f;

                            var gEntity = Utils.GetEntityByRaycast(pPos, worldPos, Player.LocalPlayer.Handle, 31) as GameEntity;

                            if (gEntity == null)
                                return;

                            Player.LocalPlayer.SetData("Temp::SEntity", gEntity);

                            Events.CallLocal("Chat::ShowServerMessage", $"[EntitySelect] Selected: Type: {gEntity.Type}, Handle: {gEntity.Handle}, Id: {gEntity.Id}, RemoteId: {(gEntity.IsLocal ? -1 : gEntity.RemoteId)}");
                        }
                    }
                }
            };

            Events.OnPlayerSpawn += (RAGE.Events.CancelEventArgs cancel) =>
            {
                var pos = Player.LocalPlayer.Position;

                Additional.SkyCamera.WrongFadeCheck();

                Player.LocalPlayer.SetInfiniteAmmoClip(true);

                Player.LocalPlayer.SetConfigFlag(429, true);
                Player.LocalPlayer.SetConfigFlag(35, false);

                RAGE.Game.Invoker.Invoke(0xE861D0B05C7662B8, Player.LocalPlayer.Handle, false, 0); // SetPedCanLosePropsOnDamage

                Sync.AttachSystem.ReattachObjects(Player.LocalPlayer);

                Player.LocalPlayer.SetFlashLightEnabled(true);

                Additional.ExtraColshape.UpdateStreamed();

                Additional.ExtraColshape.Streamed.Where(x => x.IsInside && x.Name.StartsWith("REAS")).ToList().ForEach(x =>
                {
                    x.OnExit?.Invoke(null);
                    x.OnEnter?.Invoke(null);
                });
            };

            Events.OnPlayerQuit += async (Player player) =>
            {
                if (player == null || player.Handle == Player.LocalPlayer.Handle || player.Dimension != Player.LocalPlayer.Dimension)
                    return;

                var pData = Sync.Players.GetData(player);

                if (pData == null)
                    return;

                var pos = player.Position;

                if (Player.LocalPlayer.Position.DistanceTo(pos) > 25f)
                    return;

                var curTime = Sync.World.ServerTime;

                var text = new TextLabel(pos, string.Format(Locale.General.Players.PlayerQuitText, curTime.ToString("dd.MM.yy"), curTime.ToString("HH:mm::ss"), pData.CID, player.RemoteId), new RGBA(255, 255, 255, 255), 10f, 0, true, player.Dimension) { Font = 4, LOS = false };

                pos.Z -= 1f;

                var marker = new Marker(42, pos, 1f, new Vector3(90f, 0f, 0f), new Vector3(0f, 0f, 0f), new RGBA(255, 165, 0, 125), true, player.Dimension);

                await RAGE.Game.Invoker.WaitAsync(60_000);

                marker?.Destroy();
                text?.Destroy();
            };

            Events.OnEntityStreamIn += async (entity) =>
            {
                var gEntity = entity as GameEntity;

                if (gEntity == null || !await gEntity.WaitIsLoaded())
                    return;

                await Sync.AttachSystem.OnEntityStreamIn(entity);

                if (entity is Vehicle veh)
                {
                    await Sync.Vehicles.OnVehicleStreamIn(veh);
                }
                else if (entity is Player player)
                {
                    await Sync.Players.OnPlayerStreamIn(player);
                }
                else if (entity is Ped ped)
                {
                    await Data.NPC.OnPedStreamIn(ped);
                }
                else if (entity is MapObject obj)
                {
                    await Sync.World.OnMapObjectStreamIn(obj);
                }

                CEF.Audio.OnEntityStreamIn(gEntity);

                entity.GetData<Action<Entity>>("ECA_SI")?.Invoke(entity);
            };

            Events.OnEntityStreamOut += async (entity) =>
            {
                await Sync.AttachSystem.OnEntityStreamOut(entity);

                if (entity is Vehicle veh)
                {
                    await Sync.Vehicles.OnVehicleStreamOut(veh);
                }
                else if (entity is Player player)
                {
                    await Sync.Players.OnPlayerStreamOut(player);
                }
                else if (entity is Ped ped)
                {
                    await Data.NPC.OnPedStreamOut(ped);
                }
                else if (entity is MapObject obj)
                {
                    await Sync.World.OnMapObjectStreamOut(obj);
                }

                if (entity is GameEntity gEntity)
                {
                    CEF.Audio.OnEntityStreamOut(gEntity);
                }

                entity.GetData<Action<Entity>>("ECA_SO")?.Invoke(entity);
            };

            MainLoop = new AsyncTask(() => Update?.Invoke(), 0, true);
            MainLoop.Run();

            Events.Tick += (_) => Render.Invoke();

            Render += PauseMenuWatcher;

            if (Settings.DISABLE_IDLE_CAM)
            {
                (new AsyncTask(() =>
                {
                    // InvalidateVehicleIdleCam
                    RAGE.Game.Invoker.Invoke(0x9E4CFFF989258472);
                    // InvalidateIdleCam
                    RAGE.Game.Invoker.Invoke(0xF4F2C0D4EE209E20);
                }, Settings.DISABLE_IDLE_CAM_TIMEOUT, true, 0)).Run();
            }

            (new AsyncTask(() =>
            {
                int x = 0, y = 0;

                RAGE.Game.Graphics.GetActiveScreenResolution(ref x, ref y);

                if (x == ScreenResolution.X && y == ScreenResolution.Y)
                    return;

                ScreenResolution.X = x; ScreenResolution.Y = y;

                ScreenResolutionChange?.Invoke(x, y);
            }, Settings.SCREEN_RESOLUTION_CHANGE_CHECK_TIMEOUT, true, 0)).Run();

            Vector3 lastWaypointPos = null;

            (new AsyncTask(() =>
            {
                var time = ExtraGameDate ?? Sync.World.ServerTime;

                RAGE.Game.Clock.SetClockDate(time.Day, time.Month, time.Year);
                RAGE.Game.Clock.SetClockTime(time.Hour, time.Minute, time.Second);

                var pos = Utils.GetWaypointPosition();

                if (pos != null)
                {
                    if (lastWaypointPos == null || lastWaypointPos.X != pos.X || lastWaypointPos.Y != pos.Y)
                    {
                        lastWaypointPos = pos;

                        WaypointCreated?.Invoke(pos);
                    }
                }
                else if (lastWaypointPos != null)
                {
                    lastWaypointPos = null;

                    WaypointDeleted?.Invoke();
                }

            }, 1000, true, 0)).Run();

            WaypointCreated += (Vector3 position) =>
            {
                //Utils.ConsoleOutput(position);

                WaypointPosition = position;

                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (pData.AdminLevel > -1 && Settings.Other.AutoTeleportMarker)
                    Data.Commands.TeleportMarker();
            };

            WaypointDeleted += () =>
            {
                //Utils.ConsoleOutput("DELETED");

                WaypointPosition = null;
            };

            /*            var dict = new Dictionary<int, bool>();

                        KeyBinds.NewBind(RAGE.Ui.VirtualKeys.L, true, () =>
                        {
                            var ent = RAGE.Game.Object.GetClosestObjectOfType(Player.LocalPlayer.Position.X, Player.LocalPlayer.Position.Y, Player.LocalPlayer.Position.Z, 10f, RAGE.Util.Joaat.Hash("brp_p_light_3_1"), false, true, true);

                            Utils.ConsoleOutput(ent);

                            if (ent <= 0)
                                return;

                            var state = true;

                            if (dict.ContainsKey(ent))
                            {
                                state = !dict[ent];

                                dict[ent] = state;
                            }
                            else
                                dict.Add(ent, state);

                            RAGE.Game.Entity.SetEntityLights(ent, state);
                        });

                        KeyBinds.NewBind(RAGE.Ui.VirtualKeys.B, true, () =>
                        {
                            var ent = RAGE.Game.Object.GetClosestObjectOfType(Player.LocalPlayer.Position.X, Player.LocalPlayer.Position.Y, Player.LocalPlayer.Position.Z, 10f, RAGE.Util.Joaat.Hash("brp_p_light_3_1"), false, true, true);

                            Utils.ConsoleOutput(ent);

                            if (ent <= 0)
                                return;

                            RAGE.Game.Invoker.Invoke(0x5F048334B4A4E774, ent, true, 255, 0, 0);
                        });*/
        }

        #region Renders
        private static void HideHudComponents()
        {
            // Disable Other Hud Components
            RAGE.Game.Ui.HideHudComponentThisFrame(1);
            RAGE.Game.Ui.HideHudComponentThisFrame(2);
            RAGE.Game.Ui.HideHudComponentThisFrame(3);
            RAGE.Game.Ui.HideHudComponentThisFrame(4);
            RAGE.Game.Ui.HideHudComponentThisFrame(8);
            RAGE.Game.Ui.HideHudComponentThisFrame(13);

            // Disable Vehicle and Area Name
            RAGE.Game.Ui.HideHudComponentThisFrame(6);
            RAGE.Game.Ui.HideHudComponentThisFrame(7);
            RAGE.Game.Ui.HideHudComponentThisFrame(9);

            // Disable Z
            RAGE.Game.Pad.DisableControlAction(0, 48, true);

            // Disable F (enter vehicle)
            RAGE.Game.Pad.DisableControlAction(0, 23, true);

            // Disable F (detach parachute)
            //RAGE.Game.Pad.DisableControlAction(32, 145, true);

            // Disable G (plane landing gear)
            RAGE.Game.Pad.DisableControlAction(32, 113, true);
        }
        #endregion

        #region Stuff
        public static void OnReady()
        {
            Player.LocalPlayer.SetMaxHealth(Settings.MAX_PLAYER_HEALTH);
            Player.LocalPlayer.SetHealth(Settings.MAX_PLAYER_HEALTH);
            Player.LocalPlayer.SetArmour(0);
            Player.LocalPlayer.RemoveAllWeapons(true);

            Additional.AntiCheat.Enable();

            KeyBinds.LoadMain();

            CEF.Notification.ShowHint(string.Format(Locale.Notifications.Hints.AuthCursor, KeyBinds.Get(KeyBinds.Types.Cursor).GetKeyString()), false);

            Sync.World.Preload();
        }

        public static void DisableAllControls(bool state)
        {
            if (state)
            {
                DisableAllControlsCounter++;

                Render -= DisableAllControlsRender;
                Render += DisableAllControlsRender;
            }
            else
            {
                if (DisableAllControlsCounter > 0)
                {
                    DisableAllControlsCounter--;

                    if (DisableAllControlsCounter > 0)
                        return;
                }

                Render -= DisableAllControlsRender;
            }
        }

        public static void DisableMove(bool state)
        {
            if (state)
            {
                DisableMoveCounter++;

                Render -= DisableMoveRender;
                Render += DisableMoveRender;
            }
            else
            {
                if (DisableMoveCounter > 0)
                {
                    DisableMoveCounter--;

                    if (DisableMoveCounter > 0)
                        return;
                }

                Render -= DisableMoveRender;
            }
        }

        private static void DisableAllControlsRender()
        {
            RAGE.Game.Pad.DisableAllControlActions(0);
            RAGE.Game.Pad.DisableAllControlActions(1);
            RAGE.Game.Pad.DisableAllControlActions(2);
        }

        private static void DisableMoveRender()
        {
            RAGE.Game.Pad.DisableAllControlActions(0);
            RAGE.Game.Pad.DisableAllControlActions(2);

            RAGE.Game.Pad.EnableControlAction(0, 1, true);
            RAGE.Game.Pad.EnableControlAction(0, 2, true);
        }

        private static void LoadHUD()
        {
            // Enable Red HUD and Map Title
            RAGE.Game.Ui.SetHudColour(143, Settings.HUD_COLOUR.R, Settings.HUD_COLOUR.G, Settings.HUD_COLOUR.B, Settings.HUD_COLOUR.A);
            RAGE.Game.Ui.SetHudColour(116, Settings.HUD_COLOUR.R, Settings.HUD_COLOUR.G, Settings.HUD_COLOUR.B, Settings.HUD_COLOUR.A);

            // Waypoint
            RAGE.Game.Ui.SetHudColour(142, Settings.HUD_COLOUR.R, Settings.HUD_COLOUR.G, Settings.HUD_COLOUR.B, Settings.HUD_COLOUR.A);

            RAGE.Game.Gxt.Add("PM_PAUSE_HDR", Settings.HUD_MAIN_TEXT);

            RAGE.Game.Ui.SetMinimapComponent(15, true, -1); // Fort Zancudo
            RAGE.Game.Ui.SetMinimapComponent(6, true, -1); // Vespucci Beach lifeguard building
            RAGE.Game.Ui.SetMinimapComponent(8, true, -1); // Paleto Bay fire station building
            RAGE.Game.Ui.SetMinimapComponent(9, true, -1); // Land Act Dam
            RAGE.Game.Ui.SetMinimapComponent(10, true, -1); // Paleto Forest cable car station
            RAGE.Game.Ui.SetMinimapComponent(11, true, -1); // Galileo Observatory
            RAGE.Game.Ui.SetMinimapComponent(12, true, -1); // Engine Rebuils building (Strawberry)
            RAGE.Game.Ui.SetMinimapComponent(13, true, -1); // Mansion pool (Richman)
            RAGE.Game.Ui.SetMinimapComponent(7, true, -1); // Beam Me Up (Grand Senora Desert)
            RAGE.Game.Ui.SetMinimapComponent(14, true, -1); // Beam Me Up 2 (Grand Senora Desert)
        }


        private static void PauseMenuWatcher()
        {
            if (!RAGE.Game.Ui.IsPauseMenuActive())
                return;

            //RAGE.Game.Invoker.Invoke(0x77F16B447824DA6C, 0); // open map instantly

            //RAGE.Game.Pad.DisableControlAction(32, 217, true);

            RAGE.Game.Invoker.Invoke(0xB9449845F73F5E9C, "SET_HEADING_DETAILS"); // BeginScaleformMovieMethodOnFrontendHeader

            // ScaleformMovieMethodAddParamTextureNameString
            RAGE.Game.Invoker.Invoke(0xBA7148484BD90365, $"{Player.LocalPlayer.Name} ({Player.LocalPlayer.RemoteId})");

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
            {
                RAGE.Game.Invoker.Invoke(0xBA7148484BD90365, "NOT LOGGED IN");
                RAGE.Game.Invoker.Invoke(0xBA7148484BD90365, "PLZ LOGIN :)");
            }
            else
            {
                RAGE.Game.Invoker.Invoke(0xBA7148484BD90365, $"CID #{pData.CID}");
                RAGE.Game.Invoker.Invoke(0xBA7148484BD90365, string.Format(Locale.PauseMenu.Money, Utils.GetPriceString(pData.Cash), Utils.GetPriceString(pData.BankBalance)));
            }

            RAGE.Game.Invoker.Invoke(0xC58424BA936EB458, false); // ScaleformMovieMethodAddParamBool

            RAGE.Game.Invoker.Invoke(0xC6796A8FFA375E53); // EndScaleformMovieMethod
        }
        #endregion
    }
}
