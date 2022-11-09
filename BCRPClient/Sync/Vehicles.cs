using BCRPClient.CEF;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

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
            Vehicle Vehicle = null;

            public VehicleData(Vehicle Vehicle) => this.Vehicle = Vehicle;

            #region Vehicle Data
            public bool IsInvincible
            {
                get => Vehicle.GetData<bool>("IsInvincible");

                set
                {
                    Vehicle.SetData("IsInvincible", value);

                    Vehicle.SetInvincible(value);
                    Vehicle.SetCanBeDamaged(!value);
                }
            }

            public bool EngineOn
            {
                get => Vehicle.GetData<bool>("Engine::On");

                set
                {
                    Vehicle.SetData("Engine::On", value);

                    Vehicle.SetEngineOn(value, true, true);
                    Vehicle.SetJetEngineOn(value);

                    Vehicle.SetLights(value ? (LightsOn ? 2 : 1) : 1);

                    if (Player.LocalPlayer.Vehicle?.Handle == Vehicle.Handle)
                        HUD.SwitchEngineIcon(value);
                }
            }

            public bool DoorsLocked
            {
                get => Vehicle.GetData<bool>("Doors::Locked");

                set
                {
                    Vehicle.SetData("Doors::Locked", value);

                    if (Player.LocalPlayer.Vehicle?.Handle == Vehicle.Handle)
                        HUD.SwitchDoorsIcon(value);
                }
            }

            public bool TrunkLocked { get => Vehicle.GetData<bool>("Trunk::Locked"); set => Vehicle.SetData("Trunk::Locked", value); }
            public bool HoodLocked { get => Vehicle.GetData<bool>("Hood::Locked"); set => Vehicle.SetData("Hood::Locked", value); }
            public bool LightsOn { get => Vehicle.GetData<bool>("Lights::On"); set { Vehicle.SetData("Lights::On", value); Vehicle.SetLights(value ? 2 : 1); } }
            public bool LeftIndicatorOn { get => Vehicle.GetData<bool>("Indicators::LeftOn"); set { Vehicle.SetData("Indicators::LeftOn", value); Vehicle.SetIndicatorLights(1, value); } }
            public bool RightIndicatorOn { get => Vehicle.GetData<bool>("Indicators::RightOn"); set { Vehicle.SetData("Indicators::RightOn", value); Vehicle.SetIndicatorLights(0, value); } }
            public int Radio { get => Vehicle.GetData<int>("Radio"); set => Vehicle.SetData("Radio", value); }
            public float ForcedSpeed { get => Vehicle.GetData<float>("ForcedSpeed"); set => Vehicle.SetData("ForcedSpeed", value); }
            public float FuelLevel { get => Vehicle.GetData<float>("Fuel::Level"); set => Vehicle.SetData("Fuel::Level", value); }
            public float Mileage { get => Vehicle.GetData<float>("Mileage"); set => Vehicle.SetData("Mileage", value); }
            public float DirtLevel { get => Vehicle.GetData<float>("Dirt::Level"); set { Vehicle.SetData("Dirt::Level", value); Vehicle.SetDirtLevel(value); } }
            public int[] DoorsStates { get => Vehicle.GetData<int[]>("Doors::States"); set => Vehicle.SetData("Doors::States", value); }

            public uint? TID { get => Vehicle.GetData<uint?>("ContainerID"); set => Vehicle.SetData("ContainerID", value); }
            public int VID { get => Vehicle.GetData<int>("VID"); set => Vehicle.SetData("VID", value); }

            public float LastAllowedHealth { get => Vehicle.GetData<float>("LastAllowedHealth"); set => Vehicle.SetData("LasAllowedHealth", value); }
            public float LastHealth { get => Vehicle.GetData<float>("LastHealth"); set => Vehicle.SetData("LastHealth", value); }
            #endregion

            public void Reset() => Vehicle?.ResetData();
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

            Events.OnEntityControllerChange += (Entity entity, Player newController) =>
            {
                if (entity.Type != RAGE.Elements.Type.Vehicle)
                    return;

                var veh = entity as Vehicle;

                var data = GetData(veh);

                if (newController != Player.LocalPlayer)
                {
                    ControlledVehicles.Remove(veh);

                    return;
                }

                if (data == null)
                    return;

                data.LastHealth = veh.GetEngineHealth();
                data.LastAllowedHealth = data.LastHealth;

                ControlledVehicles.Add(veh);
            };

            #region New Vehicle Stream
            #region Stream In
            Events.OnEntityStreamIn += (Entity entity) =>
            {
                if (entity.Type != RAGE.Elements.Type.Vehicle)
                    return;

                Vehicle veh = (Vehicle)entity;

                if (veh?.Exists != true || veh.IsLocal)
                    return;

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

                VehicleData data = new VehicleData(veh);
                SetData(veh, data);

                // Custom Sync
                data.VID = veh.GetSharedData<int>("VID", -999);
                data.TID = RAGE.Util.Json.Deserialize<uint?>(veh.GetSharedData<string>("TID", null));

                data.IsInvincible = veh.GetSharedData<bool>("IsInvincible");

                data.EngineOn = veh.GetSharedData<bool>("Engine::On");
                data.DoorsLocked = veh.GetSharedData<bool>("Doors::Locked");
                data.TrunkLocked = veh.GetSharedData<bool>("Trunk::Locked");
                data.HoodLocked = veh.GetSharedData<bool>("Hood::Locked");

                data.LeftIndicatorOn = veh.GetSharedData<bool>("Indicators::LeftOn");
                data.RightIndicatorOn = veh.GetSharedData<bool>("Indicators::RightOn");
                data.ForcedSpeed = veh.GetSharedData<float>("ForcedSpeed", 0f);
                data.LightsOn = veh.GetSharedData<bool>("Lights::On");
                data.Radio = veh.GetSharedData<int>("Radio", 255);
                data.FuelLevel = veh.GetSharedData<float>("Fuel::Level", 0f);
                data.Mileage = veh.GetSharedData<float>("Mileage", 0f);

                data.DirtLevel = veh.GetSharedData<float>("Dirt::Level", 0f);
                data.DoorsStates = (veh.GetSharedData<Newtonsoft.Json.Linq.JArray>("Doors::States")).ToObject<int[]>();

                if (data.Radio == 255)
                    veh.SetVehRadioStation("OFF");
                else
                    veh.SetVehRadioStation(RAGE.Game.Audio.GetRadioStationName(data.Radio));

                #region States Sync
                #region Tyres Burst

                #endregion

                #region Doors States
                for (int i = 0; i < 8; i++)
                {
                    if (data.DoorsStates[i] == 0)
                        veh.SetDoorShut(i, false);
                    else if (data.DoorsStates[i] == 1)
                        veh.SetDoorOpen(i, false, false);
                    else
                        veh.SetDoorBroken(i, true);
                }
                #endregion
                #endregion
            };
            #endregion

            #region Stream Out
            Events.OnEntityStreamOut += (Entity entity) =>
            {
                if (entity.Type != RAGE.Elements.Type.Vehicle)
                    return;

                Vehicle veh = (Vehicle)entity;

                var data = GetData(veh);

                if (data == null)
                    return;

                GetData(veh).Reset();

                ControlledVehicles.Remove(veh);
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

                data.EngineOn = data.EngineOn; // to make engine work

                //vehicle.SetCanBeDamaged(false);
            };

            Events.OnPlayerEnterVehicle += (Vehicle vehicle, int seatId) =>
            {
                if (vehicle?.Exists != true)
                    return;

                var data = GetData(vehicle);

                while (data == null)
                    data = GetData(vehicle);

                if (seatId == -1 || seatId == 0)
                {
                    HUD.SwitchSpeedometer(true);

                    if (seatId == -1)
                    {
                        StartDriverSync();
                    }
                }
                else
                    HUD.SwitchSpeedometer(false);

                data.EngineOn = data.EngineOn; // to make engine work

                RadioUpdate -= RadioSync;
                RadioUpdate += RadioSync;

                if (data.Radio == 255)
                    RAGE.Game.Audio.SetRadioToStationName("OFF");
                else
                    RAGE.Game.Audio.SetRadioToStationIndex(data.Radio);

/*                if (seatId == -1)
                    vehicle.SetCanBeDamaged(true);*/
            };

            Events.OnPlayerStartEnterVehicle += (Vehicle vehicle, int seatId, Events.CancelEventArgs cancel) =>
            {
                var data = GetData(vehicle);

                if (data == null)
                {
                    cancel.Cancel = true;

                    return;
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

            Events.AddDataHandler("IsInvincible", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Vehicle)
                    return;

                var veh = entity as Vehicle;

                var vData = GetData(veh);

                if (vData == null)
                    return;

                vData.IsInvincible = (bool)value;
            });

            Events.AddDataHandler("ForcedSpeed", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Vehicle)
                    return;

                var veh = entity as Vehicle;

                var vData = GetData(veh);

                if (vData == null)
                    return;

                vData.ForcedSpeed = (float)value;

                if (veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                    return;

                if ((float)value >= 8.3f)
                {
                    GameEvents.Update -= CruiseControlTick;
                    GameEvents.Update += CruiseControlTick;

                    Notification.Show(Notification.Types.Information, Locale.Notifications.Vehicles.CruiseControl.Header, Locale.Notifications.Vehicles.CruiseControl.On);
                }
                else if ((float)oldValue >= 8.3f)
                {
                    GameEvents.Update -= CruiseControlTick;

                    Notification.Show(Notification.Types.Information, Locale.Notifications.Vehicles.CruiseControl.Header, Locale.Notifications.Vehicles.CruiseControl.Off);
                }
            });

            #region FuelLevel + Mileage
            Events.AddDataHandler("Fuel::Level", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Vehicle)
                    return;

                var veh = entity as Vehicle;

                var vData = GetData(veh);

                if (vData == null)
                    return;

                vData.FuelLevel = (float)value;
            });

            Events.AddDataHandler("Mileage", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Vehicle)
                    return;

                var veh = entity as Vehicle;

                var vData = GetData(veh);

                if (vData == null)
                    return;

                vData.Mileage = (float)value;
            });

            Events.AddDataHandler("Engine::On", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Vehicle)
                    return;

                var veh = entity as Vehicle;

                var vData = GetData(veh);

                if (vData == null)
                    return;

                vData.EngineOn = (bool)value;
            });

            Events.AddDataHandler("Doors::Locked", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Vehicle)
                    return;

                var veh = entity as Vehicle;

                var vData = GetData(veh);

                if (vData == null)
                    return;

                vData.DoorsLocked = (bool)value;

                RAGE.Game.Audio.PlaySoundFromEntity(1, (bool)value ? "Remote_Control_Close" : "Remote_Control_Open", veh.Handle, "PI_Menu_Sounds", true, 0);
            });

            #endregion

            #region Indicators + Lights

            Events.AddDataHandler("Indicators::LeftOn", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Vehicle)
                    return;

                var veh = entity as Vehicle;

                var vData = GetData(veh);

                if (vData == null)
                    return;

                vData.LeftIndicatorOn = (bool)value;
            });

            Events.AddDataHandler("Indicators::RightOn", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Vehicle)
                    return;

                var veh = entity as Vehicle;

                var vData = GetData(veh);

                if (vData == null)
                    return;

                vData.RightIndicatorOn = (bool)value;
            });

            Events.AddDataHandler("Lights::On", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Vehicle)
                    return;

                var veh = entity as Vehicle;

                var vData = GetData(veh);

                if (vData == null)
                    return;

                vData.LightsOn = (bool)value;

                if (Player.LocalPlayer.Vehicle?.Handle == veh.Handle)
                    HUD.SwitchLightsIcon((bool)value);
            });

            #endregion

            #region Radio
            Events.AddDataHandler("Radio", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Vehicle)
                    return;

                var veh = entity as Vehicle;

                var vData = GetData(veh);

                if (vData == null)
                    return;

                vData.Radio = (int)value;

                if ((int)value == 255)
                    veh.SetVehRadioStation("OFF");
                else
                    veh.SetVehRadioStation(RAGE.Game.Audio.GetRadioStationName((int)value));
            });
            #endregion

            Events.AddDataHandler("Trunk::Locked", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Vehicle)
                    return;

                var veh = entity as Vehicle;

                var vData = GetData(veh);

                if (vData == null)
                    return;

                vData.TrunkLocked = (bool)value;

                if (vData.DoorsStates[5] != 2)
                {
                    var actualStates = vData.DoorsStates;

                    if (!(bool)value)
                    {
                        veh.SetDoorOpen(5, false, false);
                        actualStates[5] = 1;
                    }
                    else
                    {
                        veh.SetDoorShut(5, false);
                        actualStates[5] = 0;
                    }

                    vData.DoorsStates = actualStates;
                }
            });

            Events.AddDataHandler("Hood::Locked", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Vehicle)
                    return;

                var veh = entity as Vehicle;

                var vData = GetData(veh);

                if (vData == null)
                    return;

                vData.HoodLocked = (bool)value;

                if (vData.DoorsStates[4] != 2)
                {
                    var actualStates = vData.DoorsStates;

                    if (!(bool)value)
                    {
                        veh.SetDoorOpen(4, false, false);
                        actualStates[4] = 1;
                    }
                    else
                    {
                        veh.SetDoorShut(4, false);
                        actualStates[4] = 0;
                    }

                    vData.DoorsStates = actualStates;
                }
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
                    Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.CruiseControl.Header, string.Format(Locale.Notifications.Vehicles.CruiseControl.MinSpeed, Settings.MIN_CRUISE_CONTROL_SPEED));

                    return;
                }
                else if (speed > Settings.MAX_CRUISE_CONTROL_SPEED)
                {
                    Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.CruiseControl.Header, string.Format(Locale.Notifications.Vehicles.CruiseControl.MaxSpeed, Settings.MAX_CRUISE_CONTROL_SPEED));

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
                Events.CallRemote("Players::GoToTrunk", vehicle);

                return;
            }

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

            var data = GetData(veh);

            if (data == null)
                return;

            Vector3 lastPos = veh.Position;
            DateTime lastSyncedFuelMilleage = DateTime.Now;

            float fuelCounter = 0f;
            float mileageCounter = 0f;

            await RAGE.Game.Invoker.WaitAsync(2000);

            while (Player.LocalPlayer.Vehicle?.Exists == true && Player.LocalPlayer.Vehicle.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle)
            {
                float actualDirtLevel = veh.GetDirtLevel();
                float diff = (actualDirtLevel - data.DirtLevel);

                #region Dirt Level
                if (diff >= 0.5f)
                {
                    data.DirtLevel = actualDirtLevel;

                    Events.CallRemote("Vehicles::UpdateDirtLevel", actualDirtLevel);

                    //RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Info, "Dirt: " + actualDirtLevel.ToString());
                }
                #endregion

                #region Tyres Burst
/*                var actualStates = data.TyresBurst;

                for (int i = 0; i < 8; i++)
                    actualStates[i] = !veh.IsTyreBurst(i, false) ? 0 : !veh.IsTyreBurst(i, true) ? 1 : 2;

                actualStates[8] = !veh.IsTyreBurst(45, false) ? 0 : !veh.IsTyreBurst(45, true) ? 1 : 2;
                actualStates[9] = !veh.IsTyreBurst(47, false) ? 0 : !veh.IsTyreBurst(47, true) ? 1 : 2;

                if (!actualStates.SequenceEqual(data.TyresBurst))
                {
                    data.TyresBurst = actualStates;

                    Events.CallRemote("Vehicles::UpdateTyresBurst", actualStates);

                    //RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Error, "Tyres: " + string.Join(" ", actualStates));
                }*/
                #endregion

                #region Doors
                var actualStates = data.DoorsStates;

                for (int i = 0; i < 8; i++)
                {
                    if (veh.IsDoorDamaged(i))
                        actualStates[i] = 2;
                    else
                        actualStates[i] = veh.GetDoorAngleRatio(i) > 0.25f ? 1 : 0;
                }

                if (!actualStates.SequenceEqual(data.DoorsStates))
                {
                    data.DoorsStates = actualStates;

                    Events.CallRemote("Vehicles::UpdateDoorsStates", actualStates);

                    //RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Error, "Doors: " + string.Join(" ", actualStates));
                }
                #endregion

                #region Fuel Level + Mileage
                if (data.EngineOn)
                {
                    float dist = Math.Abs(Vector3.Distance(lastPos, veh.Position));
                    float fuelDiff = 0.001f * dist;

                    data.FuelLevel -= fuelDiff;
                    fuelCounter += fuelDiff;

                    data.Mileage += dist;
                    mileageCounter += dist;

                    if (data.FuelLevel < 0f)
                        data.FuelLevel = 0f;

                    if (DateTime.Now.Subtract(lastSyncedFuelMilleage).TotalMilliseconds > 2500)
                    {
                        if (fuelCounter != 0)
                            Events.CallRemote("Vehicles::UpdateFuelLevel", fuelCounter);

                        if (mileageCounter != 0)
                            Events.CallRemote("Vehicles::UpdateMileage", mileageCounter);

                        fuelCounter = 0;
                        mileageCounter = 0;

                        //RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Error, "Fuel: " + data.FuelLevel);
                        //RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Error, "Mileage: " + data.Mileage);

                        lastSyncedFuelMilleage = DateTime.Now;
                    }

                    lastPos = veh.Position;
                }
                #endregion

                await RAGE.Game.Invoker.WaitAsync(1000);
            }

            if (fuelCounter != 0)
                Events.CallRemote("Vehicles::UpdateFuelLevel", fuelCounter);

            if (mileageCounter != 0)
                Events.CallRemote("Vehicles::UpdateMileage", mileageCounter);
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
