#define DEBUGGING

using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net.NetworkInformation;

namespace BCRPClient
{
    public class GameEvents : Events.Script
    {
        private static int _FPS { get; set; }

        public static int FPS { get; private set; }

        public static bool PlayerFreezed = false;

        private static AsyncTask MainLoop;

        public delegate void UpdateHandler();
        public delegate void ScreenResolutionChangeHandler(int x, int y);

        public delegate void WaypointCreatedHandler(Vector3 position);
        public delegate void WaypointDeletedHandler();

        public static event UpdateHandler Update;
        public static event UpdateHandler Render;

        public static event WaypointCreatedHandler WaypointCreated;
        public static event WaypointDeletedHandler WaypointDeleted;

        public static event ScreenResolutionChangeHandler ScreenResolutionChange;

        public static Vector3 WaypointPosition = null;

        public static (int X, int Y) ScreenResolution = (0, 0);

        private static int DisableAllControlsCounter { get; set; }
        private static int DisableMoveCounter { get; set; }

        public GameEvents()
        {
            RAGE.Ui.Console.Clear();
            RAGE.Chat.Activate(false);
            RAGE.Chat.Show(false);

            FpsCounterStart();

            Player.LocalPlayer.SetVisible(true, false);

            RAGE.Game.Ui.SetPauseMenuActive(false);

            RAGE.Game.Invoker.Invoke(0x95C0A5BBDC189AA1);

            RAGE.Game.Misc.SetFadeOutAfterDeath(false);
            RAGE.Game.Misc.SetFadeOutAfterArrest(false);

            RAGE.Game.Misc.SetFadeInAfterDeathArrest(true);
            RAGE.Game.Misc.SetFadeInAfterLoad(true);

            RAGE.Game.Graphics.TransitionFromBlurred(0);

            LoadHUD();

            RAGE.Game.Ui.DisplayRadar(false);

            Render += HideHudComponents;

            GameEvents.DisableAllControls(true);

            Events.OnPlayerSpawn += (RAGE.Events.CancelEventArgs cancel) =>
            {
                var pos = Player.LocalPlayer.Position;

                Additional.SkyCamera.WrongFadeCheck();

                // SetPedCanLosePropsOnDamage
                RAGE.Game.Invoker.Invoke(0xE861D0B05C7662B8, Player.LocalPlayer.Handle, false, 0);

                Sync.AttachSystem.ReattachObjects(Player.LocalPlayer, true);

                Additional.ExtraColshape.UpdateStreamed();
            };

            Events.OnPlayerQuit += async (Player player) =>
            {
                var pData = Sync.Players.GetData(player);

                if (pData == null)
                    return;

                var pos = player.Position;

                if (Player.LocalPlayer.Position.DistanceTo(pos) > 25f)
                    return;

                var curTime = Utils.GetServerTime();

                var text = new TextLabel(pos, string.Format(Locale.General.Players.PlayerQuitText, curTime.ToString("dd.MM.yy"), curTime.ToString("HH:mm::ss"), pData.CID, player.RemoteId), new RGBA(255, 255, 255, 255), 10f, 0, true, player.Dimension) { Font = 4, LOS = false };

                pos.Z -= 1f;

                var marker = new Marker(42, pos, 1f, new Vector3(90f, 0f, 0f), new Vector3(0f, 0f, 0f), new RGBA(255, 165, 0, 125), true, player.Dimension);

                await RAGE.Game.Invoker.WaitAsync(60_000);

                marker?.Destroy();
                text?.Destroy();
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

                        RAGE.Input.Bind(RAGE.Ui.VirtualKeys.L, true, () =>
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

                        RAGE.Input.Bind(RAGE.Ui.VirtualKeys.B, true, () =>
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

            CEF.Notification.ShowHint(string.Format(Locale.Notifications.Hints.AuthCursor, KeyBinds.Get(KeyBinds.Types.Cursor).GetKeyString()), false, 5000);

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

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            // BeginScaleformMovieMethodOnFrontendHeader
            RAGE.Game.Invoker.Invoke(0xB9449845F73F5E9C, "SET_HEADING_DETAILS");

            // ScaleformMovieMethodAddParamTextureNameString
            RAGE.Game.Invoker.Invoke(0xBA7148484BD90365, $"{Player.LocalPlayer.Name} ({Player.LocalPlayer.RemoteId})");
            RAGE.Game.Invoker.Invoke(0xBA7148484BD90365, $"CID #{pData.CID}");
            RAGE.Game.Invoker.Invoke(0xBA7148484BD90365, string.Format(Locale.PauseMenu.Money, pData.Cash, pData.BankBalance));

            // ScaleformMovieMethodAddParamBool
            RAGE.Game.Invoker.Invoke(0xC58424BA936EB458, false);

            // EndScaleformMovieMethod
            RAGE.Game.Invoker.Invoke(0xC6796A8FFA375E53);
        }

        private static async void FpsCounterStart()
        {
            while (true)
            {
                _FPS = 0;

                Render += FpsCounterRender;

                await RAGE.Game.Invoker.WaitAsync(1000);

                Render -= FpsCounterRender;

                FPS = _FPS;
            }
        }

        private static void FpsCounterRender() => _FPS++;
        #endregion
    }
}
