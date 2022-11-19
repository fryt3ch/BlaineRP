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
        public static bool PlayerFreezed = false;

        private static AsyncTask MainLoop;

        public delegate void UpdateHandler();
        public delegate void ScreenResolutionChangeHandler(int x, int y);

        public static event UpdateHandler Update;
        public static event UpdateHandler Render;

        public static event ScreenResolutionChangeHandler ScreenResolutionChange;

        public static Vector3 WaypointPosition = null;

        public static (int X, int Y) ScreenResolution = (0, 0);

        public GameEvents()
        {
            RAGE.Ui.Console.Clear();
            RAGE.Chat.Activate(false);
            RAGE.Chat.Show(false);

            Player.LocalPlayer.SetVisible(true, false);

            RAGE.Game.Invoker.Invoke(0x95C0A5BBDC189AA1);

            RAGE.Game.Misc.SetFadeOutAfterDeath(false);
            RAGE.Game.Misc.SetFadeOutAfterArrest(false);

            RAGE.Game.Misc.SetFadeInAfterDeathArrest(true);
            RAGE.Game.Misc.SetFadeInAfterLoad(true);

            RAGE.Game.Graphics.TransitionFromBlurred(0);

            LoadHUD();

            RAGE.Game.Ui.DisplayRadar(false);

            Render += HideHudComponents;
            Render += DisableAllControls;

            Events.OnPlayerSpawn += (RAGE.Events.CancelEventArgs cancel) =>
            {
                var pos = Player.LocalPlayer.Position;

                Additional.SkyCamera.WrongFadeCheck();

                // SetPedCanLosePropsOnDamage
                RAGE.Game.Invoker.Invoke(0xE861D0B05C7662B8, Player.LocalPlayer.Handle, false, 0);

                Sync.AttachSystem.ReattachObjects(Player.LocalPlayer, true);

                Additional.ExtraColshape.UpdateStreamed();
            };

            Events.OnPlayerCreateWaypoint += (Vector3 position) =>
            {
                if (position == null)
                    return;

                for (int i = 0; i < RAGE.Elements.Entities.Blips.All.Count; i++)
                {
                    var blip = RAGE.Elements.Entities.Blips.All[i];

                    if (blip?.Exists != true || blip.Dimension != Player.LocalPlayer.Dimension)
                        continue;

                    var coords = blip.GetCoords();

                    if (coords.DistanceIgnoreZ(position) <= 5f)
                    {
                        position.Z = coords.Z;

                        break;
                    }
                }

                WaypointPosition = position;
                
                Utils.ConsoleOutput(WaypointPosition);

                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (pData.AdminLevel > -1 && Settings.Other.AutoTeleportMarker)
                    Data.Commands.TeleportMarker();

/*                if (Player.LocalPlayer.Vehicle != null && Player.LocalPlayer.Vehicle.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle)
                {
                    CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Notifications.Vehicles.GPS.Header, Locale.Notifications.Vehicles.GPS.RouteReady);
                }*/
            };

            Events.OnPlayerRemoveWaypoint += () =>
            {
                //WaypointPosition = null;

/*                if (Player.LocalPlayer.Vehicle != null && Player.LocalPlayer.Vehicle.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle)
                {
                    CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Notifications.Vehicles.GPS.Header, Locale.Notifications.Vehicles.GPS.RouteCancel);
                }*/
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
        }

        public static void SwitchCaioPerico(bool state)
        {
            RAGE.Game.Invoker.Invoke(0x9A9D1BA639675CF1, "HeistIsland", state); // load island, unload original map
            RAGE.Game.Invoker.Invoke(0x5E1460624D194A38, state); // switching minimap
        }

        public static void DisableAllControls()
        {
            RAGE.Game.Pad.DisableAllControlActions(0);
            RAGE.Game.Pad.DisableAllControlActions(1);
            RAGE.Game.Pad.DisableAllControlActions(2);
        }

        public static void DisableMove()
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

            RAGE.Game.Gxt.Add("PM_PAUSE_HDR", Settings.HUD_MAIN_TEXT);
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
        #endregion
    }
}
