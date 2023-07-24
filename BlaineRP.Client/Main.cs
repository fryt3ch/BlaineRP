#define DEBUGGING
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Utils.Game;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using BlaineRP.Client.EntitiesData;
using Players = BlaineRP.Client.Sync.Players;

namespace BlaineRP.Client
{
    public class Main : Events.Script
    {
        public static int CurrentFps => (int)System.Math.Floor(1f / RAGE.Game.Misc.GetFrameTime());

        public delegate void UpdateHandler();
        public delegate void ScreenResolutionChangeHandler(int x, int y);

        public delegate void OnMouseClick(int x, int y, bool up, bool right);
        public delegate void OnMouseClickCef(int x, int y, bool up, bool right, float relX, float relY, Vector3 worldPos, int eHandle);

        public delegate void WaypointCreatedHandler(Vector3 position);
        public delegate void WaypointDeletedHandler();

        public static event UpdateHandler Update;
        public static event UpdateHandler Render;

        public static event OnMouseClick MouseClicked;
        public static event OnMouseClickCef MouseClickedWithRaycast;

        public static event WaypointCreatedHandler WaypointCreated;
        public static event WaypointDeletedHandler WaypointDeleted;

        public static event ScreenResolutionChangeHandler ScreenResolutionChange;

        private static Vector3 _waypointPosition;
        private static RAGE.Ui.Cursor.Vector2 _screenResolution = new RAGE.Ui.Cursor.Vector2(0f, 0f);

        private static int _disableAllControlsCounter;
        private static int _disableMoveCounter;

        public static Vector3 WaypointPosition => _waypointPosition;

        public static RAGE.Ui.Cursor.Vector2 ScreenResolution => _screenResolution;

        public static DateTime? ExtraGameDate { get; set; }

        public Main()
        {
            Events.OnClickWithRaycast += (x, y, up, right, relX, relY, worldPos, eHandle) => MouseClicked?.Invoke(x, y, up, right);
            Events.OnClickWithRaycast += (x, y, up, right, relX, relY, worldPos, eHandle) => MouseClickedWithRaycast?.Invoke(x, y, up, right, relX, relY, worldPos, eHandle);

            MouseClickedWithRaycast += (x, y, up, right, relX, relY, worldPos, eHandle) =>
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

                            var gEntity = Raycast.GetEntityByRaycast(pPos, worldPos, Player.LocalPlayer.Handle, 31) as GameEntity;

                            if (gEntity == null)
                                return;

                            Player.LocalPlayer.SetData("Temp::SEntity", gEntity);

                            Events.CallLocal("Chat::ShowServerMessage", $"[EntitySelect] Selected: Type: {gEntity.Type}, Handle: {gEntity.Handle}, Id: {gEntity.Id}, RemoteId: {(gEntity.IsLocal ? -1 : gEntity.RemoteId)}");
                        }
                    }
                }
            };

            Events.OnPlayerReady += () =>
            {
                Sync.World.LoadServerDataColshape();

                var settingsProfile = Settings.App.Profile.LoadProfile(Sync.World.GetSharedData<JObject>("Settings"));

                Settings.App.Profile.SetCurrentProfile(settingsProfile);

                System.Globalization.CultureInfo.DefaultThreadCurrentCulture = Settings.App.Profile.Current.General.CultureInfo;
                System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = Settings.App.Profile.Current.General.CultureInfo;
                System.Globalization.CultureInfo.CurrentCulture = Settings.App.Profile.Current.General.CultureInfo;

                (new Utils.AsyncTask(() => Update?.Invoke(), 0, true)).Run();

                Events.Tick += (_) => Render.Invoke();

                Render += PauseMenuWatcher;

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

                RAGE.Game.Graphics.TransitionFromBlurred(0f);

                RAGE.Game.Graphics.StopAllScreenEffects();

                RAGE.Game.Misc.DisableAutomaticRespawn(true);
                RAGE.Game.Misc.IgnoreNextRestart(true);

                RAGE.Game.Invoker.Invoke(0xE6C0C80B8C867537, true); // SetEnableVehicleSlipstreaming

                RAGE.Game.Graphics.SetTvChannel(-1);
                RAGE.Game.Graphics.SetTvAudioFrontend(false);

                RAGE.Game.Ui.SetMapFullScreen(false);

                System.GC.Collect();

                RAGE.Game.Gxt.Add("BRP_AEBLPT", "~a~");
                Misc.ClearTvChannelPlaylist(-1);
                Misc.ClearTvChannelPlaylist(0);
                Misc.ClearTvChannelPlaylist(1);

                if (RAGE.Game.Audio.IsStreamPlaying())
                    RAGE.Game.Audio.StopStream();

                RAGE.Game.Audio.StopAudioScenes();

                RAGE.Game.Audio.SetAudioFlag("LoadMPData", true);
                Audio.DisableFlightMusic();

                LoadHUD();

                RAGE.Game.Ui.DisplayRadar(false);

                Render += HideHudComponents;

                DisableAllControls(true);

                Sync.World.Preload();

                #region SCRIPTS_TO_REPLACE



                #endregion

                Events.OnPlayerJoin += (player) =>
                {

                };

                Events.OnPlayerSpawn += (cancel) =>
                {
                    Additional.SkyCamera.WrongFadeCheck();

                    Player.LocalPlayer.SetInfiniteAmmoClip(true);

                    Player.LocalPlayer.SetConfigFlag(429, true);
                    Player.LocalPlayer.SetConfigFlag(35, false);

                    RAGE.Game.Invoker.Invoke(0xE861D0B05C7662B8, Player.LocalPlayer.Handle, false, 0); // SetPedCanLosePropsOnDamage

                    Sync.AttachSystem.ReattachObjects(Player.LocalPlayer);

                    Player.LocalPlayer.SetFlashLightEnabled(true);

                    Additional.ExtraColshape.UpdateStreamed();

                    Additional.ExtraBlip.RefreshAllBlips();

                    Sync.House.UpdateAllLights();
                };

                Events.OnPlayerQuit += async (player) =>
                {
                    if (player == null || player.Handle == Player.LocalPlayer.Handle || player.Dimension != Player.LocalPlayer.Dimension)
                        return;

                    var pData = PlayerData.GetData(player);

                    if (pData == null)
                        return;

                    var pos = player.Position;

                    if (Player.LocalPlayer.Position.DistanceTo(pos) > 25f)
                        return;

                    var curTime = Sync.World.ServerTime;

                    var text = new Additional.ExtraLabel(pos, Locale.Get("PLAYER_QUIT_TEXT", curTime.ToString("dd.MM.yy"), curTime.ToString("HH:mm::ss"), pData.CID, player.RemoteId), new RGBA(255, 255, 255, 255), 10f, 0, true, player.Dimension) { Font = 4, LOS = false };

                    pos.Z -= 1f;

                    var marker = new Marker(42, pos, 1f, new Vector3(90f, 0f, 0f), new Vector3(0f, 0f, 0f), new RGBA(255, 165, 0, 125), true, player.Dimension);

                    await RAGE.Game.Invoker.WaitAsync(60_000);

                    marker?.Destroy();
                    text?.Destroy();
                };

                Events.OnEntityStreamIn += async (entity) =>
                {
                    var gEntity = entity as GameEntity;

                    if (gEntity == null || !await Streaming.WaitIsLoaded(gEntity))
                        return;

                    await Sync.AttachSystem.OnEntityStreamIn(entity);

                    if (entity is Vehicle veh)
                    {
                        await Sync.Vehicles.OnVehicleStreamIn(veh);
                    }
                    else if (entity is Player player)
                    {
                        await Players.OnPlayerStreamIn(player);
                    }
                    else if (entity is Ped ped)
                    {
                        await Sync.Peds.OnPedStreamIn(ped);
                    }
                    else if (entity is MapObject obj)
                    {
                        await Sync.World.OnMapObjectStreamIn(obj);
                    }

                    CEF.Audio.OnEntityStreamIn(gEntity);

                    var customActions = entity.StreamInCustomActionsGet();

                    if (customActions != null)
                    {
                        foreach (var x in customActions)
                        {
                            x.Invoke(entity);
                        }
                    }
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
                        await Players.OnPlayerStreamOut(player);
                    }
                    else if (entity is Ped ped)
                    {
                        await Sync.Peds.OnPedStreamOut(ped);
                    }
                    else if (entity is MapObject obj)
                    {
                        await Sync.World.OnMapObjectStreamOut(obj);
                    }

                    if (entity is GameEntity gEntity)
                    {
                        CEF.Audio.OnEntityStreamOut(gEntity);
                    }

                    var customActions = entity.StreamOutCustomActionsGet();

                    if (customActions != null)
                    {
                        foreach (var x in customActions)
                        {
                            x.Invoke(entity);
                        }
                    }
                };

                if (Settings.App.Static.DisableIdleCamera)
                {
                    (new Utils.AsyncTask(() =>
                    {
                        RAGE.Game.Invoker.Invoke(0x9E4CFFF989258472);   // InvalidateVehicleIdleCam
                        RAGE.Game.Invoker.Invoke(0xF4F2C0D4EE209E20);   // InvalidateIdleCam
                    }, 25_000, true, 0)).Run();
                }

                (new Utils.AsyncTask(() =>
                {
                    int x = 0, y = 0;

                    RAGE.Game.Graphics.GetActiveScreenResolution(ref x, ref y);

                    if (x == ScreenResolution.X && y == ScreenResolution.Y)
                        return;

                    ScreenResolution.X = x; ScreenResolution.Y = y;

                    ScreenResolutionChange?.Invoke(x, y);
                }, 2_500, true, 0)).Run();

                Vector3 lastWaypointPos = null;

                (new Utils.AsyncTask(() =>
                {
                    var time = ExtraGameDate ?? Sync.World.ServerTime;

                    RAGE.Game.Clock.SetClockDate(time.Day, time.Month, time.Year);
                    RAGE.Game.Clock.SetClockTime(time.Hour, time.Minute, time.Second);

                    var pos = Misc.GetWaypointPosition();

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

                }, 1_000, true, 0)).Run();

                WaypointCreated += (Vector3 position) =>
                {
                    _waypointPosition = position;

                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (pData.AdminLevel > -1 && Settings.User.Other.AutoTeleportMarker)
                        Data.Commands.TeleportMarker();
                };

                WaypointDeleted += () =>
                {
                    _waypointPosition = null;
                };
            };
        }

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

        public static void OnReady()
        {
            Player.LocalPlayer.SetMaxHealth(100 + Settings.App.Static.PlayerMaxHealth);
            Player.LocalPlayer.SetRealHealth(Settings.App.Static.PlayerMaxHealth);
            Player.LocalPlayer.SetArmour(0);
            Player.LocalPlayer.RemoveAllWeapons(true);

            Additional.AntiCheat.Enable();

            KeyBinds.LoadMain();

            CEF.Notification.ShowHint(string.Format(Locale.Notifications.Hints.AuthCursor, KeyBinds.Get(KeyBinds.Types.Cursor).GetKeyString()), false);
        }

        public static void DisableAllControls(bool state)
        {
            if (state)
            {
                _disableAllControlsCounter++;

                Render -= DisableAllControlsRender;
                Render += DisableAllControlsRender;
            }
            else
            {
                if (_disableAllControlsCounter > 0)
                {
                    _disableAllControlsCounter--;

                    if (_disableAllControlsCounter > 0)
                        return;
                }

                Render -= DisableAllControlsRender;
            }
        }

        public static void DisableMove(bool state)
        {
            if (state)
            {
                _disableMoveCounter++;

                Render -= DisableMoveRender;
                Render += DisableMoveRender;
            }
            else
            {
                if (_disableMoveCounter > 0)
                {
                    _disableMoveCounter--;

                    if (_disableMoveCounter > 0)
                        return;
                }

                Render -= DisableMoveRender;
            }
        }

        public static void DisableAllControlsRender()
        {
            RAGE.Game.Pad.DisableAllControlActions(0);
            RAGE.Game.Pad.DisableAllControlActions(1);
            RAGE.Game.Pad.DisableAllControlActions(2);
        }

        public static void DisableMoveRender()
        {
            RAGE.Game.Pad.DisableAllControlActions(0);
            RAGE.Game.Pad.DisableAllControlActions(2);

            RAGE.Game.Pad.EnableControlAction(0, 0, true);
            RAGE.Game.Pad.EnableControlAction(0, 1, true);
            RAGE.Game.Pad.EnableControlAction(0, 2, true);
        }

        private static void LoadHUD()
        {
            // Enable Red HUD and Map Title
            RAGE.Game.Ui.SetHudColour(143, Settings.App.Static.HudColour.R, Settings.App.Static.HudColour.G, Settings.App.Static.HudColour.B, Settings.App.Static.HudColour.A);
            RAGE.Game.Ui.SetHudColour(116, Settings.App.Static.HudColour.R, Settings.App.Static.HudColour.G, Settings.App.Static.HudColour.B, Settings.App.Static.HudColour.A);

            // Waypoint
            RAGE.Game.Ui.SetHudColour(142, Settings.App.Static.HudColour.R, Settings.App.Static.HudColour.G, Settings.App.Static.HudColour.B, Settings.App.Static.HudColour.A);

            RAGE.Game.Gxt.Add("PM_PAUSE_HDR", Locale.Get("GEN_PAUSEMENU_HUDM_T"));

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

            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
            {
                RAGE.Game.Invoker.Invoke(0xBA7148484BD90365, "NOT LOGGED IN");
                RAGE.Game.Invoker.Invoke(0xBA7148484BD90365, "PLZ LOGIN :)");
            }
            else
            {
                RAGE.Game.Invoker.Invoke(0xBA7148484BD90365, $"CID #{pData.CID}");
                RAGE.Game.Invoker.Invoke(0xBA7148484BD90365, Locale.Get("GEN_PAUSEMENU_MONEY_T", Locale.Get("GEN_MONEY_0", pData.Cash), Locale.Get("GEN_MONEY_0", pData.BankBalance)));
            }

            RAGE.Game.Invoker.Invoke(0xC58424BA936EB458, false); // ScaleformMovieMethodAddParamBool

            RAGE.Game.Invoker.Invoke(0xC6796A8FFA375E53); // EndScaleformMovieMethod
        }

        public static void CloseGameNow(string message)
        {
            var exception = new Exception(message);

            while (true)
            {
                throw exception;
            }
        }
    }
}
