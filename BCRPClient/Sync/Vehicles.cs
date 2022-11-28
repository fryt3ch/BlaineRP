using BCRPClient.CEF;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;

namespace BCRPClient.Sync
{
    public class Vehicles : Events.Script
    {
        #region Last Times
        private static DateTime LastBeltToggled;
        private static DateTime LastDoorsLockToggled;
        private static DateTime LastEngineToggled;
        private static DateTime LastIndicatorToggled;
        private static DateTime LastLightsToggled;
        private static DateTime LastCruiseControlToggled;
        private static DateTime LastSeatBeltShowed;

        private static DateTime LastRadioSent;
        private static DateTime LastSyncSent;
        #endregion

        public static List<Vehicle> ControlledVehicles;

        private static GameEvents.UpdateHandler RadioUpdate;

        private static Dictionary<string, Action<VehicleData, object, object>> DataActions = new Dictionary<string, Action<VehicleData, object, object>>();

        private static void InvokeHandler(string dataKey, VehicleData vData, object value, object oldValue = null) => DataActions.GetValueOrDefault(dataKey)?.Invoke(vData, value, oldValue);

        private static void AddDataHandler(string dataKey, Action<VehicleData, object, object> action)
        {
            Events.AddDataHandler(dataKey, (Entity entity, object value, object oldValue) =>
            {
                if (entity is Vehicle vehicle)
                {
                    var data = Sync.Vehicles.GetData(vehicle);

                    if (data == null)
                        return;

                    action.Invoke(data, value, oldValue);
                }
            });

            DataActions.Add(dataKey, action);
        }

        public static VehicleData GetData(Vehicle vehicle)
        {
            if (vehicle == null)
                return null;

            return vehicle.HasData("SyncedData") ? vehicle.GetData<VehicleData>("SyncedData") : null;
        }

        public static void SetData(Vehicle vehicle, VehicleData data)
        {
            if (vehicle == null)
                return;

            vehicle.SetData("SyncedData", data);
        }

        public class VehicleData
        {
            public VehicleData(Vehicle Vehicle)
            {
                this.Vehicle = Vehicle;

                this.Data = BCRPClient.Data.Vehicles.GetByModel(Vehicle.Model);
            }

            public Vehicle Vehicle { get; set; }

            #region Vehicle Data
            public bool IsInvincible => Vehicle.GetSharedData<bool>("IsInvincible", false);

            public bool EngineOn => Vehicle.GetSharedData<bool>("Engine::On", false);

            public bool DoorsLocked => Vehicle.GetSharedData<bool>("Doors::Locked", false);

            public bool TrunkLocked => Vehicle.GetSharedData<bool>("Trunk::Locked", false);

            public bool HoodLocked => Vehicle.GetSharedData<bool>("Hood::Locked", false);

            public bool LightsOn => Vehicle.GetSharedData<bool>("Lights::On", false);

            public bool LeftIndicatorOn => Vehicle.GetSharedData<bool>("Indicators::LeftOn", false);

            public bool RightIndicatorOn => Vehicle.GetSharedData<bool>("Indicators::RightOn", false);

            public int Radio => Vehicle.GetSharedData<int>("Radio", 255);

            public float ForcedSpeed => Vehicle.GetSharedData<float>("ForcedSpeed", 0f);

            public float FuelLevel => Vehicle.GetSharedData<float>("Fuel::Level", 0f);

            public float Mileage => Vehicle.GetSharedData<float>("Mileage", 0f);

            public uint VID => (uint)Vehicle.GetSharedData<int>("VID", 0);

            public uint? TID { get => Vehicle.GetData<uint?>("ContainerID"); set => Vehicle.SetData("ContainerID", value); }

            public float LastAllowedHealth { get => Vehicle.GetData<float>("LastAllowedHealth"); set => Vehicle.SetData("LastAllowedHealth", value); }

            public float LastHealth { get => Vehicle.GetData<float>("LastHealth"); set => Vehicle.SetData("LastHealth", value); }

            public Data.Vehicles.Vehicle Data { get; set; }
            #endregion

            public void Reset()
            {
                if (Vehicle == null)
                    return;

                ControlledVehicles.Remove(Vehicle);

                Vehicle.ResetData();
            }
        }

        public Vehicles()
        {
            #region Default Settings
            LastBeltToggled = DateTime.Now;
            LastDoorsLockToggled = DateTime.Now;
            LastEngineToggled = DateTime.Now;
            LastIndicatorToggled = DateTime.Now;
            LastLightsToggled = DateTime.Now;
            LastCruiseControlToggled = DateTime.Now;
            LastSeatBeltShowed = DateTime.Now;

            LastRadioSent = DateTime.Now;
            LastSyncSent = DateTime.Now;

            RAGE.Game.Vehicle.DefaultEngineBehaviour = false;
            Player.LocalPlayer.SetConfigFlag(429, true);
            Player.LocalPlayer.SetConfigFlag(35, false);
            //Player.LocalPlayer.SetConfigFlag(184, true);
            #endregion

            (new AsyncTask(() => RadioUpdate?.Invoke(), 1000, true)).Run();

            ControlledVehicles = new List<Vehicle>();

            GameEvents.Update += ControlledTick;

            GameEvents.Render += () =>
            {
                float screenX = 0f, screenY = 0f;

                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                foreach (var x in Utils.GetVehiclesOnScreen(5))
                {
                    var data = GetData(x);

                    if (data == null)
                        continue;

                    var pos = x.GetRealPosition();

                    if (Vector3.Distance(pos, Player.LocalPlayer.Position) > 10f)
                        continue;

                    if (!Utils.GetScreenCoordFromWorldCoord(pos, ref screenX, ref screenY))
                        continue;

                    if (Settings.Other.DebugLabels)
                    {
                        if (pData.AdminLevel > -1)
                        {
                            Utils.DrawText($"ID: {x.RemoteId} | VID: {data.VID} | TID: {(data.TID == null ? "null" : data.TID.ToString())}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                            Utils.DrawText($"EngineOn: {data.EngineOn} | Locked: {data.DoorsLocked} | TrunkLocked: {data.TrunkLocked}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                            Utils.DrawText($"Fuel: {data.FuelLevel.ToString("0.00")} | Mileage: {data.Mileage.ToString("0.00")}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                            Utils.DrawText($"EngineHP: {x.GetEngineHealth()} | IsInvincible: {data.IsInvincible}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                            Utils.DrawText($"Speed: {x.GetSpeedKm().ToString("0.00")} | ForcedSpeed: {(data.ForcedSpeed * 3.6f).ToString("0.00")}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                        }
                        else
                        {
                            Utils.DrawText($"ID: {x.RemoteId} | VID: {data.VID}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                            Utils.DrawText($"EngineHP: {x.GetEngineHealth()}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                        }
                    }
                }
            };

            Events.OnEntityControllerChange += async (Entity entity, Player newController) =>
            {
                if (entity is Vehicle veh)
                {
                    if (newController?.Handle != Player.LocalPlayer.Handle)
                    {
                        ControlledVehicles.Remove(veh);

                        return;
                    }

                    if (ControlledVehicles.Contains(veh))
                        return;

                    VehicleData data = null;

                    while ((data = GetData(veh)) == null)
                    {
                        await RAGE.Game.Invoker.WaitAsync(25);

                        if (veh?.Exists != true)
                            return;
                    }

                    data.LastHealth = veh.GetEngineHealth();
                    data.LastAllowedHealth = data.LastHealth;

                    ControlledVehicles.Add(veh);

                    InvokeHandler("Engine::On", data, data.EngineOn, null);

                    InvokeHandler("Indicators::LeftOn", data, data.LeftIndicatorOn, null);
                    InvokeHandler("Indicators::RightOn", data, data.RightIndicatorOn, null);

                    InvokeHandler("Radio", data, data.Radio, null);

                    if (data.TrunkLocked)
                    {
                        veh.SetDoorShut(5, false);
                    }
                    else
                    {
                        veh.SetDoorOpen(5, false, false);
                    }

                    if (data.HoodLocked)
                    {
                        veh.SetDoorShut(4, false);
                    }
                    else
                    {
                        veh.SetDoorOpen(4, false, false);
                    }
                }
            };

            #region New Vehicle Stream
            #region Stream In
            Events.OnEntityStreamIn += async (Entity entity) =>
            {
                if (entity is Vehicle veh)
                {
                    if (!veh.Exists || veh.IsLocal)
                        return;

                    var loaded = await veh.WaitIsLoaded();

                    if (!loaded)
                        return;

                    var data = GetData(veh);

                    if (data != null)
                    {
                        data.Reset();
                    }

                    #region Required Things For Normal Behaviour
                    RAGE.Game.Streaming.RequestCollisionAtCoord(veh.Position.X, veh.Position.Y, veh.Position.Z);
                    RAGE.Game.Streaming.RequestAdditionalCollisionAtCoord(veh.Position.X, veh.Position.Y, veh.Position.Z);
                    veh.SetLoadCollisionFlag(true, 0);
                    veh.TrackVisibility();

                    veh.SetUndriveable(true);
                    #endregion

                    #region Default Settings
                    veh.SetWheelsCanBreak(true);
                    veh.SetDisablePetrolTankDamage(true);
                    #endregion

                    data = new VehicleData(veh);

                    InvokeHandler("IsInvincible", data, data.IsInvincible, null);

                    InvokeHandler("Mods::TSColour", data, veh.GetSharedData("Mods::TSColour"), null);
                    InvokeHandler("Mods::Turbo", data, veh.GetSharedData("Mods::Turbo"), null);
                    InvokeHandler("Mods::Xenon", data, veh.GetSharedData("Mods::Xenon"), null);

                    //InvokeHandler("Anchor", data, veh.GetSharedData("Anchor"), null);

                    data.TID = veh.GetSharedData<int?>("TID", null).ToUInt32();

                    InvokeHandler("Engine::On", data, data.EngineOn, null);

                    InvokeHandler("Indicators::LeftOn", data, data.LeftIndicatorOn, null);
                    InvokeHandler("Indicators::RightOn", data, data.RightIndicatorOn, null);

                    InvokeHandler("Radio", data, data.Radio, null);

                    if (data.TrunkLocked)
                    {
                        veh.SetDoorShut(5, false);
                    }
                    else
                    {
                        veh.SetDoorOpen(5, false, false);
                    }

                    if (data.HoodLocked)
                    {
                        veh.SetDoorShut(4, false);
                    }
                    else
                    {
                        veh.SetDoorOpen(4, false, false);
                    }

                    SetData(veh, data);
                }
            };
            #endregion

            #region Stream Out
            Events.OnEntityStreamOut += (Entity entity) =>
            {
                if (entity is Vehicle veh)
                {
                    var data = GetData(veh);

                    if (data == null)
                        return;

                    data.Reset();
                }
            };
            #endregion
            #endregion

            #region Leave/Enter Vehicle Events
            Events.OnPlayerLeaveVehicle += (Vehicle vehicle, int seatId) =>
            {
                HUD.SwitchSpeedometer(false);

                RadioUpdate -= RadioSync;

                if (vehicle?.Exists != true)
                    return;

                var data = GetData(vehicle);

                if (data == null)
                    return;

                InvokeHandler("Engine::On", data, data.EngineOn, null);
            };

            Events.OnPlayerEnterVehicle += async (Vehicle vehicle, int seatId) =>
            {
                if (vehicle?.Exists != true)
                    return;

                var data = GetData(vehicle);

                if (!vehicle.IsLocal)
                {
                    while (data == null)
                    {
                        await RAGE.Game.Invoker.WaitAsync(25);

                        data = GetData(vehicle);

                        if (Player.LocalPlayer.Vehicle == null || Player.LocalPlayer.Vehicle.Handle != vehicle?.Handle)
                            return;
                    }

                    RadioUpdate -= RadioSync;
                    RadioUpdate += RadioSync;

                    if (data.Radio == 255)
                        RAGE.Game.Audio.SetRadioToStationName("OFF");
                    else
                        RAGE.Game.Audio.SetRadioToStationIndex(data.Radio);
                }
                else
                {
                    vehicle.SetEngineOn(true, true, true);
                    vehicle.SetJetEngineOn(true);

                    vehicle.SetLights(true);

                    if (seatId == -1 || seatId == 0)
                    {
                        HUD.SwitchSpeedometer(true);
                    }
                    else
                        HUD.SwitchSpeedometer(false);
                }

                await RAGE.Game.Invoker.WaitAsync(250);

                Sync.Players.UpdateHat(Player.LocalPlayer);
            };

            Events.OnPlayerStartEnterVehicle += (Vehicle vehicle, int seatId, Events.CancelEventArgs cancel) =>
            {
                if (vehicle?.Exists != true)
                    return;

                if (!vehicle.IsLocal)
                {
                    var data = GetData(vehicle);

                    if (data == null)
                    {
                        cancel.Cancel = true;

                        return;
                    }
                }
            };
            #endregion

            #region Radio Sync
            static void RadioSync()
            {
                var veh = Player.LocalPlayer.Vehicle;

                if (veh?.Exists != true)
                    return;

                var data = GetData(veh);

                if (data == null)
                    return;

                var actualStation = data.Radio;

                if (veh.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle || veh.GetPedInSeat(0, 0) == Player.LocalPlayer.Handle)
                {
                    var currentStation = RAGE.Game.Audio.GetPlayerRadioStationIndex();

                    if (currentStation != actualStation && !LastRadioSent.IsSpam(1000, false, false))
                        RAGE.Events.CallRemote("Vehicles::SetRadio", currentStation);
                }
                else
                {
                    RAGE.Game.Audio.SetFrontendRadioActive(true);

                    if (actualStation == 255)
                    {
                        RAGE.Game.Audio.SetRadioToStationName("OFF");
                        veh.SetVehRadioStation("OFF");
                    }
                    else
                    {
                        RAGE.Game.Audio.SetRadioToStationIndex(actualStation);
                        veh.SetVehRadioStation(RAGE.Game.Audio.GetRadioStationName(actualStation));
                    }
                }
            }
            #endregion

            #region Events

            AddDataHandler("IsInvincible", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                bool state = (bool)value;

                veh.SetInvincible(state);
                veh.SetCanBeDamaged(!state);
            });

            AddDataHandler("ForcedSpeed", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                var fSpeed = (float)value;

                if (veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                    return;

                if ((fSpeed >= 8.3f))
                {
                    GameEvents.Update -= CruiseControlTick;
                    GameEvents.Update += CruiseControlTick;

                    Notification.Show(Notification.Types.Information, Locale.Notifications.Vehicles.CruiseControl.Header, Locale.Notifications.Vehicles.CruiseControl.On);
                }
                else if (oldValue != null && (float)oldValue >= 8.3f)
                {
                    GameEvents.Update -= CruiseControlTick;

                    Notification.Show(Notification.Types.Information, Locale.Notifications.Vehicles.CruiseControl.Header, Locale.Notifications.Vehicles.CruiseControl.Off);
                }
            });

            AddDataHandler("Engine::On", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                var state = (bool)value;

                veh.SetEngineOn(state, true, true);
                veh.SetJetEngineOn(state);

                veh.SetLights(state ? (vData.LightsOn ? 2 : 1) : 1);

                if (Player.LocalPlayer.Vehicle?.Handle == veh.Handle)
                    HUD.SwitchEngineIcon(state);
            });

            AddDataHandler("Doors::Locked", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                var state = (bool)value;

                RAGE.Game.Audio.PlaySoundFromEntity(1, state ? "Remote_Control_Close" : "Remote_Control_Open", veh.Handle, "PI_Menu_Sounds", true, 0);

                if (Player.LocalPlayer.Vehicle?.Handle == veh.Handle)
                    HUD.SwitchDoorsIcon(state);
            });

            AddDataHandler("Indicators::LeftOn", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                veh.SetIndicatorLights(1, (bool)value);
            });

            AddDataHandler("Indicators::RightOn", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                veh.SetIndicatorLights(0, (bool)value);
            });

            AddDataHandler("Lights::On", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                var state = (bool)value;

                veh.SetLights(state ? 2 : 1);

                if (Player.LocalPlayer.Vehicle?.Handle == veh.Handle)
                    HUD.SwitchLightsIcon(state);
            });

            AddDataHandler("Radio", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                var idx = (int)value;

                if (idx == 255)
                    veh.SetVehRadioStation("OFF");
                else
                    veh.SetVehRadioStation(RAGE.Game.Audio.GetRadioStationName(idx));
            });

            AddDataHandler("Trunk::Locked", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                var state = (bool)value;

                if (state)
                {
                    veh.SetDoorShut(5, false);
                }
                else
                {
                    veh.SetDoorOpen(5, false, false);
                }
            });

            AddDataHandler("Hood::Locked", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                var state = (bool)value;

                if (state)
                {
                    veh.SetDoorShut(4, false);
                }
                else
                {
                    veh.SetDoorOpen(4, false, false);
                }
            });

            AddDataHandler("Mods::TSColour", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                var colour = ((JObject)value).ToObject<Utils.Colour>();

                veh.ToggleMod(20, colour.Alpha == 255);

                veh.SetTyreSmokeColor(colour.Red, colour.Green, colour.Blue);
            });

            AddDataHandler("Mods::Turbo", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                var state = (bool)value;

                veh.ToggleMod(18, state);
            });

            AddDataHandler("Mods::Xenon", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                var colour = (int)value;

                if (colour < -1)
                {
                    veh.ToggleMod(22, false);
                }
                else
                {
                    veh.ToggleMod(22, true);

                    RAGE.Game.Invoker.Invoke(0xE41033B25D003A07, veh.Handle, colour);
                }
            });

            AddDataHandler("Anchor", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                var state = (bool)value;

                RAGE.Game.Invoker.Invoke(0xE3EBAAE484798530, veh.Handle, state);

                veh.SetBoatAnchor(state);
            });

            Events.Add("Vehicle::Heading", (object[] args) =>
            {
                var veh = (Vehicle)args[0];

                var heading = (float)args[1];

                if (!ControlledVehicles.Contains(veh))
                    return;

                veh.SetHeading(heading);
            });
            #endregion
        }

        #region Handlers

        #region Cruise Control
        public static void ToggleCruiseControl(bool ignoreIf = false)
        {
            var vehicle = Player.LocalPlayer.Vehicle;

            if (!ignoreIf)
            {
                if (vehicle?.Exists != true || vehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                    return;

                if (LastCruiseControlToggled.IsSpam(1000, false, false))
                    return;

                if (!Utils.IsCar(vehicle))
                {
                    Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.CruiseControl.Header, Locale.Notifications.Vehicles.CruiseControl.Unsupported);

                    return;
                }

                var spVect = vehicle.GetSpeedVector(true);

                if (spVect.Y < 0)
                {
                    Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.CruiseControl.Header, Locale.Notifications.Vehicles.CruiseControl.Reverse);

                    return;
                }

                var speed = vehicle.GetSpeed();

                if (speed < Settings.MIN_CRUISE_CONTROL_SPEED)
                {
                    Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.CruiseControl.Header, string.Format(Locale.Notifications.Vehicles.CruiseControl.MinSpeed, Math.Floor(Settings.MIN_CRUISE_CONTROL_SPEED * 3.6f)));

                    return;
                }
                else if (speed > Settings.MAX_CRUISE_CONTROL_SPEED)
                {
                    Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.CruiseControl.Header, string.Format(Locale.Notifications.Vehicles.CruiseControl.MaxSpeed, Math.Floor(Settings.MAX_CRUISE_CONTROL_SPEED * 3.6f)));

                    return;
                }
            }

            Events.CallRemote("Players::ToggleCruiseControl", vehicle?.GetSpeed() ?? 0f);

            LastCruiseControlToggled = DateTime.Now;
        }

        public static void CruiseControlTick()
        {
            var veh = Player.LocalPlayer.Vehicle;

            if (veh?.Exists != true || !veh.GetIsEngineRunning() || veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
            {
                ToggleCruiseControl(true);

                GameEvents.Update -= CruiseControlTick;

                return;
            }

            var spVect = veh.GetSpeedVector(true);

            Utils.ConsoleOutputLimited(spVect, true, 1000);

            if (veh.GetHeightAboveGround() > 1f || Math.Abs(spVect.X) > 1f)
            {
                Notification.Show(Notification.Types.Information, Locale.Notifications.Vehicles.CruiseControl.Header, Locale.Notifications.Vehicles.CruiseControl.Danger, 2500);

                ToggleCruiseControl(true);

                GameEvents.Update -= CruiseControlTick;

                return;
            }

            if (veh.HasCollidedWithAnything())
            {
                Notification.Show(Notification.Types.Information, Locale.Notifications.Vehicles.CruiseControl.Header, Locale.Notifications.Vehicles.CruiseControl.Collision, 2500);

                ToggleCruiseControl(true);

                GameEvents.Update -= CruiseControlTick;

                return;
            }

            if (RAGE.Game.Pad.IsControlJustPressed(32, 130) || RAGE.Game.Pad.IsControlJustPressed(32, 129) || RAGE.Game.Pad.IsControlJustPressed(32, 76))
            {
                Notification.Show(Notification.Types.Information, Locale.Notifications.Vehicles.CruiseControl.Header, Locale.Notifications.Vehicles.CruiseControl.Invtervention, 2500);

                ToggleCruiseControl(true);

                GameEvents.Update -= CruiseControlTick;

                return;
            }
        }
        #endregion

        #region Belt
        public static void ToggleBelt(bool ignoreIf = false)
        {
            if (!ignoreIf)
            {
                if (Player.LocalPlayer.Vehicle?.Exists != true || !Utils.IsCar(Player.LocalPlayer.Vehicle))
                    return;

                if (LastBeltToggled.IsSpam(1000, false, false))
                    return;
            }

            LastBeltToggled = DateTime.Now;

            Events.CallRemote("Players::ToggleBelt");
        }

        public static void BeltTick()
        {
            RAGE.Game.Pad.DisableControlAction(32, 75, true);

            if (RAGE.Game.Pad.IsDisabledControlJustPressed(32, 75))
                if (DateTime.Now.Subtract(LastSeatBeltShowed).TotalMilliseconds > 500)
                {
                    Notification.Show(Notification.Types.Information, Locale.Notifications.Vehicles.SeatBelt.Header, Locale.Notifications.Vehicles.SeatBelt.TakeOffToLeave);

                    LastSeatBeltShowed = DateTime.Now;
                }

            if (Player.LocalPlayer.Vehicle?.Exists != true)
            {
                ToggleBelt(true);

                GameEvents.Update -= BeltTick;
            }
        }
        #endregion

        #region Lock
        public static void Lock(bool? forceStatus = null, Vehicle vehicle = null)
        {
            if (LastDoorsLockToggled.IsSpam(1000, false, false))
                return;

            LastDoorsLockToggled = DateTime.Now;

            if (vehicle == null)
            {
                vehicle = Player.LocalPlayer.Vehicle;

                if (vehicle?.Exists == true && vehicle.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle)
                {
                    Events.CallRemote("Vehicles::ToggleDoorsLockSync", vehicle);

                    return;
                }

                vehicle = Utils.GetClosestVehicle(Player.LocalPlayer.Position, Settings.ENTITY_INTERACTION_MAX_DISTANCE);
            }

            if (vehicle?.Exists != true)
                return;

            var data = GetData(vehicle);

            if (data == null)
                return;

            if (forceStatus != null)
            {
                bool curStatus = data.DoorsLocked;

                if (forceStatus == curStatus)
                {
                    Notification.Show(Notification.Types.Information, Locale.Notifications.DefHeader, curStatus ? Locale.Notifications.Vehicles.Doors.AlreadyLocked : Locale.Notifications.Vehicles.Doors.AlreadyUnlocked);

                    return;
                }
            }

            Events.CallRemote("Vehicles::ToggleDoorsLockSync", vehicle);
        }
        #endregion

        #region Engine
        public static void Engine(bool ignoreIf = false)
        {
            Vehicle veh = Player.LocalPlayer.Vehicle;

            if (veh?.Exists != true)
                return;

            if (ignoreIf)
            {
                if (LastEngineToggled.IsSpam(1000, false, false))
                    return;
            }

            if (veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                return;

            LastEngineToggled = DateTime.Now;

            Events.CallRemote("Vehicles::ToggleEngineSync");
        }
        #endregion

        #region Indicators
        public static void ToggleIndicator(int type) // 0 - right, 1 - left, 2 - both
        {
            var veh = Player.LocalPlayer.Vehicle;

            if (veh?.Exists != true)
                return;

            if (LastIndicatorToggled.IsSpam(500, false, false))
                return;

            int vehClass = veh.GetClass();

            // if cycle, boat, helicopter, plane
            if (vehClass == 13 || vehClass == 14 || vehClass == 15 || vehClass == 16)
                return;

            if (veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                return;

            LastIndicatorToggled = DateTime.Now;

            Events.CallRemote("Vehicles::ToggleIndicator", type);
        }
        #endregion

        #region Lights
        public static void ToggleLights()
        {
            var veh = Player.LocalPlayer.Vehicle;

            if (veh?.Exists != true || !GetData(veh).EngineOn)
                return;

            if (LastLightsToggled.IsSpam(500, false, false))
                return;

            int vehClass = veh.GetClass();

            // if cycle, boat, helicopter, plane
            if (vehClass == 13 || vehClass == 14 || vehClass == 15 || vehClass == 16)
                return;

            if (veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                return;

            LastLightsToggled = DateTime.Now;

            Events.CallRemote("Vehicles::ToggleLights");
        }
        #endregion

        #region Trunk Lock
        public static void ToggleTrunkLock(bool? forceStatus = null, Vehicle vehicle = null)
        {
            if (LastDoorsLockToggled.IsSpam(1000, false, false))
                return;

            LastDoorsLockToggled = DateTime.Now;

            if (vehicle == null)
            {
                vehicle = Player.LocalPlayer.Vehicle;

                if (vehicle?.Exists == true && vehicle.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle)
                {
                    Events.CallRemote("Vehicles::ToggleTrunkLockSync", vehicle);

                    return;
                }

                vehicle = Utils.GetClosestVehicle(Player.LocalPlayer.Position, Settings.ENTITY_INTERACTION_MAX_DISTANCE);
            }

            if (vehicle?.Exists != true)
                return;

            var data = GetData(vehicle);

            if (data == null)
                return;

            if (forceStatus != null)
            {
                bool curStatus = data.TrunkLocked;

                if (forceStatus == curStatus)
                {
                    Notification.Show(Notification.Types.Information, Locale.Notifications.DefHeader, curStatus ? Locale.Notifications.Vehicles.Trunk.AlreadyLocked : Locale.Notifications.Vehicles.Trunk.AlreadyUnlocked);

                    return;
                }
            }

            Events.CallRemote("Vehicles::ToggleTrunkLockSync", vehicle);
        }
        #endregion

        #region Hood Lock
        public static void ToggleHoodLock(bool? forceStatus = null, Vehicle vehicle = null)
        {
            if (LastDoorsLockToggled.IsSpam(1000, false, false))
                return;

            LastDoorsLockToggled = DateTime.Now;

            if (vehicle == null)
            {
                vehicle = Player.LocalPlayer.Vehicle;

                if (vehicle?.Exists == true && vehicle.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle)
                {
                    Events.CallRemote("Vehicles::ToggleHoodLockSync", vehicle);

                    return;
                }

                vehicle = Utils.GetClosestVehicle(Player.LocalPlayer.Position, Settings.ENTITY_INTERACTION_MAX_DISTANCE);
            }

            if (vehicle?.Exists != true)
                return;

            var data = GetData(vehicle);

            if (data == null)
                return;

            if (forceStatus != null)
            {
                bool curStatus = data.HoodLocked;

                if (forceStatus == curStatus)
                {
                    Notification.Show(Notification.Types.Information, Locale.Notifications.DefHeader, curStatus ? Locale.Notifications.Vehicles.Hood.AlreadyLocked : Locale.Notifications.Vehicles.Hood.AlreadyUnlocked);

                    return;
                }
            }

            Events.CallRemote("Vehicles::ToggleHoodLockSync", vehicle);
        }
        #endregion

        #endregion

        #region Vehicle Menu Methods
        #region Shuffle Seat
        public static void SeatTo(int seatId, Vehicle vehicle)
        {
            if (vehicle == null)
                return;

            if (Vector3.Distance(Player.LocalPlayer.Position, vehicle.Position) > Settings.ENTITY_INTERACTION_MAX_DISTANCE)
                return;

            var data = Players.GetData(Player.LocalPlayer);
            var vehData = GetData(vehicle);

            if (data == null || vehData == null)
                return;

            // to trunk
            if (seatId == int.MaxValue)
            {
                if (vehicle.DoesHaveDoor(5) > 0)
                {
                    Events.CallRemote("Players::GoToTrunk", vehicle);
                }
                else
                {
                    CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Vehicles.Trunk.NoPhysicalTrunk);
                }

                return;
            }

            var maxSeats = RAGE.Game.Vehicle.GetVehicleModelNumberOfSeats(vehicle.Model);

            if (maxSeats <= 0)
            {
                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Vehicles.Passengers.NotEnterable);

                return;
            }

            if (seatId >= maxSeats)
                seatId = maxSeats - 1;

            if (vehicle.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle)
            {
                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Vehicles.Passengers.IsDriver);

                return;
            }

            if (!vehicle.IsSeatFree(seatId - 1, 0))
            {
                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Vehicles.Passengers.SomeoneSeating);

                return;
            }

            if (Player.LocalPlayer.Vehicle == vehicle)
            {
                if (data.BeltOn)
                {
                    CEF.Notification.Show(Notification.Types.Information, Locale.Notifications.Vehicles.SeatBelt.Header, Locale.Notifications.Vehicles.SeatBelt.TakeOffToSeat);

                    return;
                }

                Events.CallRemote("Vehicles::ShuffleSeat", seatId);
            }
            else
            {
                Utils.JsEval("mp.players.local.taskEnterVehicle", vehicle.Handle, 5000, seatId - 1, 2f, 1, 0);
            }
        }
        #endregion

        #region Kick Passenger
        public static void KickPassenger(Player player)
        {
            var veh = Player.LocalPlayer.Vehicle;

            if (veh?.Exists != true || player.Vehicle != veh || veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                return;

            Events.CallRemote("Vehicles::KickPlayer", player);
        }

        public static void Park(Vehicle vehicle)
        {
            if (vehicle == null)
            {
                if (Player.LocalPlayer.Vehicle == null)
                    return;

                vehicle = Player.LocalPlayer.Vehicle;
            }

            if (Player.LocalPlayer.HasData("CurrentHouse"))
            {
                var house = Player.LocalPlayer.GetData<Data.Locations.House>("CurrentHouse");

                if (house.GarageType == null)
                    return;

                Events.CallRemote("House::Garage::Vehicle", true, vehicle, house.Id);

                RAGE.Game.Audio.PlaySoundFrontend(-1, "RAMP_UP", "TRUCK_RAMP_DOWN", true);
            }
            else
            {
                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.House.NotNearGarage);

                return;
            }
        }
        #endregion

        public static List<Player> GetPlayersInVehicle(Vehicle veh)
        {
            var players = new List<Player>();

            if (veh?.Exists != true)
                return players;

            for (int i = -1; i < veh.GetMaxNumberOfPassengers(); i++)
            {
                var handle = veh.GetPedInSeat(i, 0);

                if (handle <= 0)
                    continue;

                var player = Utils.GetPlayerByHandle(handle, true);

                if (player == null)
                    continue;

                players.Add(player);
            }

            return players;
        }
        #endregion

        public static async System.Threading.Tasks.Task StartDriverSync()
        {
            var veh = Player.LocalPlayer.Vehicle;

            if (veh?.Exists != true)
                return;

            Vector3 lastPos = veh.Position;

            var data = GetData(veh);

            if (data == null)
                return;

            while (Player.LocalPlayer.Vehicle?.Exists == true && Player.LocalPlayer.Vehicle.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle)
            {
                if (data.EngineOn)
                {
                    float dist = Math.Abs(Vector3.Distance(lastPos, veh.Position));
                    float fuelDiff = 0.001f * dist;

                    if (fuelDiff > 0)
                    {
                        Events.CallRemote("Vehicles::UpdateFuelLevel", fuelDiff);
                    }

                    if (dist > 0)
                    {
                        Events.CallRemote("Vehicles::UpdateMileage", dist);
                    }

                    //RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Error, "Fuel: " + data.FuelLevel);
                    //RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Error, "Mileage: " + data.Mileage);

                    lastPos = veh.Position;

                    await RAGE.Game.Invoker.WaitAsync(1500);
                }
                else
                {
                    await RAGE.Game.Invoker.WaitAsync(500);
                }
            }
        }

        private static void ControlledTick()
        {
            for (int i = 0; i < ControlledVehicles.Count; i++)
            {
                var veh = ControlledVehicles[i];

                if (veh == null)
                    continue;

                var vData = GetData(veh);

                if (vData.ForcedSpeed != 0f)
                    veh.SetForwardSpeed(vData.ForcedSpeed);
            }
        }
    }
}
