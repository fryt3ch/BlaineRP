using BCRPClient.CEF;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private static DateTime LastVehicleExitedTime;
        #endregion

        private static AsyncTask CurrentDriverSyncTask { get; set; }

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

        public class RentedVehicle
        {
            public static List<RentedVehicle> All { get; set; } = new List<RentedVehicle>();

            public ushort RemoteId { get; set; }

            public Data.Vehicles.Vehicle VehicleData { get; set; }

            public int TimeToDelete { get; set; }

            public int TimeLeftToDelete { get; set; }

            public RentedVehicle(ushort RemoteId, Data.Vehicles.Vehicle VehicleData, int TimeToDelete)
            {
                this.RemoteId = RemoteId;
                this.VehicleData = VehicleData;

                this.TimeToDelete = TimeToDelete;

                this.TimeLeftToDelete = TimeToDelete;
            }

            public void ShowTimeLeftNotification()
            {
                CEF.Notification.Show(Notification.Types.Information, Locale.Notifications.DefHeader, string.Format(Locale.Notifications.Vehicles.RentedVehicleTimeLeft, $"\"{VehicleData.SubName}\"", DateTime.Now.AddMilliseconds(TimeLeftToDelete).Subtract(DateTime.Now).GetBeautyString()), 5000);
            }

            public static void Check()
            {
                var curVeh = Player.LocalPlayer.Vehicle;

                All.ForEach(x =>
                {
                    if (curVeh == null || curVeh.RemoteId != x.RemoteId)
                    {
                        if (x.TimeLeftToDelete > 0)
                            x.TimeLeftToDelete -= 1000;

                        if (x.TimeLeftToDelete < 0)
                            x.TimeLeftToDelete = 0;

                        if (x.TimeLeftToDelete <= 0)
                        {
                            Events.CallRemote("VRent::Cancel", x.RemoteId);
                        }
                        else
                        {
                            if ((x.TimeLeftToDelete <= 30_000 && x.TimeToDelete % 10_000 == 0) || x.TimeLeftToDelete % 60_000 == 0)
                                x.ShowTimeLeftNotification();
                        }
                    }
                    else
                    {
                        if (x.TimeLeftToDelete != x.TimeToDelete)
                            x.TimeLeftToDelete = x.TimeToDelete;
                    }
                });
            }
        }

        public static VehicleData GetData(Vehicle vehicle)
        {
            if (vehicle == null)
                return null;

            return vehicle.GetData<VehicleData>("SyncedData");
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

            public bool IsFrozen => FrozenPosition != null;

            public bool EngineOn => Vehicle.GetSharedData<bool>("Engine::On", false);

            public bool DoorsLocked => Vehicle.GetSharedData<bool>("Doors::Locked", false);

            public bool TrunkLocked => Vehicle.GetSharedData<bool>("Trunk::Locked", false);

            public bool HoodLocked => Vehicle.GetSharedData<bool>("Hood::Locked", false);

            public bool LightsOn => Vehicle.GetSharedData<bool>("Lights::On", false);

            public bool LeftIndicatorOn => Vehicle.GetSharedData<bool>("Indicators::LeftOn", false);

            public bool RightIndicatorOn => Vehicle.GetSharedData<bool>("Indicators::RightOn", false);

            public int Radio => Vehicle.GetSharedData<int>("Radio", 255);

            public float ForcedSpeed => Vehicle.GetSharedData<float>("ForcedSpeed", 0f);

            public float FuelLevel { get => Vehicle.GetData<float?>("Fuel") ?? 0f; set => Vehicle.SetData("Fuel", value); }

            public float Mileage { get => Vehicle.GetData<float?>("Mileage") ?? 0f; set => Vehicle.SetData("Mileage", value); }

            public uint VID => (uint)Vehicle.GetSharedData<int>("VID", 0);

            public uint? TID => Vehicle.GetSharedData<int?>("TID", null).ToUInt32();

            public bool HasNeonMod => Vehicle.GetSharedData<bool>("Mods::Neon", false);

            public bool HasTurboTuning => Vehicle.GetSharedData<bool>("Mods::Turbo", false);

            public bool IsAnchored => Vehicle.GetSharedData<bool>("Anchor", false);

            public Utils.Colour TyreSmokeColour => Vehicle.GetSharedData<JObject>("Mods::TSColour")?.ToObject<Utils.Colour>();

            public byte DirtLevel => (byte)Vehicle.GetSharedData<int>("DirtLevel", 0);

            public string FrozenPosition => Vehicle.GetSharedData<string>("IsFrozen");

            public Data.Vehicles.Vehicle Data { get; set; }

            public Sync.AttachSystem.AttachmentEntity IsAttachedToVehicle
            {
                get
                {
                    var streamed = RAGE.Elements.Entities.Vehicles.Streamed;

                    for (int i = 0; i < streamed.Count; i++)
                    {
                        var t = streamed[i].GetData<List<Sync.AttachSystem.AttachmentEntity>>(Sync.AttachSystem.AttachedEntitiesKey)?.Where(x => x.RemoteID == Vehicle.RemoteId).FirstOrDefault();

                        if (t != null)
                            return t;
                    }

                    return null;
                }
            }

            public Vehicle IsAttachedToLocalTrailer => Vehicle.GetData<List<Sync.AttachSystem.AttachmentObject>>(Sync.AttachSystem.AttachedObjectsKey)?.Where(x => x.Type == AttachSystem.Types.TrailerObjOnBoat).FirstOrDefault()?.Object as Vehicle;
            #endregion

            public void Reset()
            {
                if (Vehicle == null)
                    return;

                ControlledVehicles.Remove(Vehicle);

                Vehicle.ResetData();
            }
        }

        public static async System.Threading.Tasks.Task OnVehicleStreamIn(Vehicle veh)
        {
            if (veh.IsLocal)
            {
/*                if (veh.GetData<Vehicle>("TrailerSync::Owner") is Vehicle trVeh && GetData(trVeh)?.IsFrozen == true)
                    veh.FreezePosition(true);*/

                return;
            }

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
            veh.SetDisablePetrolTankDamage(true);
            #endregion

            data = new VehicleData(veh);

            InvokeHandler("Anchor", data, data.IsAnchored, null);

            InvokeHandler("IsFrozen", data, data.FrozenPosition, null);

            InvokeHandler("IsInvincible", data, data.IsInvincible, null);

            InvokeHandler("Mods::TSColour", data, veh.GetSharedData<JObject>("Mods::TSColour", null), null);
            InvokeHandler("Mods::Turbo", data, data.HasTurboTuning, null);

            InvokeHandler("Mods::Xenon", data, veh.GetSharedData<int>("Mods::Xenon", -2), null);

            InvokeHandler("Mods::CT", data, veh.GetSharedData<int>("Mods::CT", 0), null);

            //InvokeHandler("Anchor", data, veh.GetSharedData("Anchor"), null);

            InvokeHandler("Engine::On", data, data.EngineOn, null);

            InvokeHandler("Indicators::LeftOn", data, data.LeftIndicatorOn, null);
            InvokeHandler("Indicators::RightOn", data, data.RightIndicatorOn, null);

            InvokeHandler("Radio", data, data.Radio, null);

            InvokeHandler("DirtLevel", data, data.DirtLevel, null);

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

        public static async System.Threading.Tasks.Task OnVehicleStreamOut(Vehicle veh)
        {
            var data = GetData(veh);

            if (data == null)
                return;

            data.Reset();
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
                            Utils.DrawText($"EHP: {x.GetEngineHealth()} | BHP: {x.GetBodyHealth()} | IsInvincible: {data.IsInvincible}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
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
                    if (veh.IsLocal)
                        return;

                    if (newController?.Handle != Player.LocalPlayer.Handle)
                    {
                        ControlledVehicles.Remove(veh);

                        veh.ResetData("LastHealth");

                        return;
                    }

                    if (ControlledVehicles.Contains(veh))
                        return;

                    VehicleData data = null;

                    while ((data = GetData(veh)) == null)
                    {
                        await RAGE.Game.Invoker.WaitAsync(25);

                        if (veh?.Exists != true || veh.Controller?.Handle != Player.LocalPlayer.Handle)
                            return;
                    }

                    veh.SetData("LastHealth", veh.GetEngineHealth());

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

                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (RentedVehicle.All.Where(x => x.RemoteId == vehicle.RemoteId).FirstOrDefault() is RentedVehicle rVehData)
                {
                    rVehData.ShowTimeLeftNotification();
                }
            };

            Events.OnPlayerEnterVehicle += async (Vehicle vehicle, int seatId) =>
            {
                if (vehicle?.Exists != true)
                    return;

                GameEvents.Render -= InVehicleRender;
                GameEvents.Render += InVehicleRender;

                Data.NPC.CurrentNPC?.SwitchDialogue(false);

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

                var state = (bool?)value ?? false;

                veh.SetInvincible(state);
                veh.SetCanBeDamaged(!state);

                veh.SetWheelsCanBreak(!state);
            });

            AddDataHandler("ForcedSpeed", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                var fSpeed = (float?)value ?? 0f;

                if (veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                    return;

                if ((fSpeed >= 8.3f))
                {
                    ToggleAutoPilot(false);

                    GameEvents.Update -= CruiseControlTick;
                    GameEvents.Update += CruiseControlTick;

                    Notification.Show(Notification.Types.Information, Locale.Notifications.Vehicles.Additional.HeaderCruise, Locale.Notifications.Vehicles.Additional.On);
                }
                else if (oldValue != null && (float)oldValue >= 8.3f)
                {
                    GameEvents.Update -= CruiseControlTick;

                    Notification.Show(Notification.Types.Information, Locale.Notifications.Vehicles.Additional.HeaderCruise, Locale.Notifications.Vehicles.Additional.Off);
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

                var colour = ((JObject)value)?.ToObject<Utils.Colour>();

                if (colour == null)
                {
                    veh.ToggleMod(20, false);
                }
                else
                {
                    veh.ToggleMod(20, true);

                    veh.SetTyreSmokeColor(colour.Red, colour.Green, colour.Blue);
                }
            });

            AddDataHandler("Mods::Turbo", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                var state = value is bool valueBool ? valueBool : false;

                veh.ToggleMod(18, state);
            });

            AddDataHandler("Mods::Xenon", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                var colour = (int?)value;

                veh.SetXenonColour(colour < -1 ? null : colour);
            });

            AddDataHandler("Mods::CT", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                var colour = (int)value;

                veh.SetColourType(colour);
            });

            AddDataHandler("Anchor", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                var state = (bool?)value ?? false;

                RAGE.Game.Invoker.Invoke(0xE3EBAAE484798530, veh.Handle, state);

                veh.SetBoatAnchor(state);
            });

            AddDataHandler("IsFrozen", (vData, value, oldValue) =>
            {
                if (value is string valueStr)
                {
                    vData.Vehicle.FreezePosition(true);

                    vData.IsAttachedToLocalTrailer?.FreezePosition(true);
                }
                else
                {
                    vData.Vehicle.FreezePosition(false);

                    vData.IsAttachedToLocalTrailer?.FreezePosition(false);
                }
            });

            AddDataHandler("DirtLevel", (vData, value, oldValue) =>
            {
                vData.Vehicle.SetDirtLevel(Convert.ToSingle(value));
            });
            #endregion

            Events.Add("Vehicles::NPChoose", (args) =>
            {
                if (Utils.IsAnyCefActive(true))
                    return;

                var items = ((JObject)args[0]).ToObject<Dictionary<uint, string>>();

                int counter = 0;

                CEF.ActionBox.ShowSelect(ActionBox.Contexts.NumberplateSelect, Locale.Actions.NumberplateSelectHeader, items.Select(x => (counter++, $"[{x.Value}]")).ToArray(), items);
            });

            Events.Add("Vehicles::Garage::SlotsMenu", (args) =>
            {
                if (Utils.IsAnyCefActive(true))
                    return;

                var freeSlots = ((JArray)args[0]).ToObject<List<int>>();

                freeSlots.Insert(0, int.MinValue + freeSlots[(new Random()).Next(0, freeSlots.Count)]);

                CEF.ActionBox.ShowSelect(ActionBox.Contexts.GarageVehiclePlaceSelect, Locale.Actions.GarageVehicleSlotSelectHeader, freeSlots.Select(x => (x, x < 0 ? "Случайное место" : $"Место #{x + 1}")).ToArray());
            });

            Events.Add("Vehicles::Fuel", (args) =>
            {
                var veh = Player.LocalPlayer.Vehicle;

                if (veh == null)
                    return;

                var vData = Sync.Vehicles.GetData(veh);

                if (vData == null)
                    return;

                vData.FuelLevel = (float)args[0];
            });

            Events.Add("Vehicles::Mileage", (args) =>
            {
                var veh = Player.LocalPlayer.Vehicle;

                if (veh == null)
                    return;

                var vData = Sync.Vehicles.GetData(veh);

                if (vData == null)
                    return;

                vData.Mileage = (float)args[0];
            });

            Events.Add("Vehicles::Enter", async (args) =>
            {
                var veh = Player.LocalPlayer.Vehicle;

                if (veh == null)
                    return;

                veh.SetData("Fuel", (float)args[0]);
                veh.SetData("Mileage", (float)args[1]);
            });

            Events.Add("Vehicles::Fix", (args) =>
            {
                var veh = (Vehicle)args[0];

                veh.SetData("LastHealth", 1000f);

                veh.SetBodyHealth(1000f);

                veh.SetEngineHealth(1000f);

                veh.SetFixed();
                veh.SetDeformationFixed();
            });

            Events.Add("Vehicles::FixV", (args) =>
            {
                var veh = (Vehicle)args[0];

                veh.SetBodyHealth(1000f);

                veh.SetFixed();
                veh.SetDeformationFixed();
            });

            RAGE.Input.Bind(RAGE.Ui.VirtualKeys.F, true, () =>
            {
                if (Utils.CanShowCEF(true, true))
                    Sync.Vehicles.TryEnterVehicle(Interaction.CurrentEntity as Vehicle, -1);
            });
        }

        #region Handlers

        #region Cruise Control
        public static void ToggleCruiseControl(bool ignoreIf = false)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var vehicle = Player.LocalPlayer.Vehicle;

            if (!ignoreIf)
            {
                if (vehicle?.Exists != true || vehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle || !vehicle.GetIsEngineRunning() || pData.AutoPilot != null)
                    return;

                if (LastCruiseControlToggled.IsSpam(1000, false, false))
                    return;

                var vData = GetData(vehicle);

                if (vData == null || vData.IsAnchored)
                    return;

                if (!vData.Data.HasCruiseControl)
                {
                    Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.Additional.HeaderCruise, Locale.Notifications.Vehicles.Additional.Unsupported);

                    return;
                }

                var spVect = vehicle.GetSpeedVector(true);

                if (spVect.Y < 0)
                {
                    Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.Additional.HeaderCruise, Locale.Notifications.Vehicles.Additional.Reverse);

                    return;
                }

                var speed = vehicle.GetSpeed();

                if (speed < Settings.MIN_CRUISE_CONTROL_SPEED)
                {
                    Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.Additional.HeaderCruise, string.Format(Locale.Notifications.Vehicles.Additional.MinSpeed, Math.Floor(Settings.MIN_CRUISE_CONTROL_SPEED * 3.6f)));

                    return;
                }
                else if (speed > Settings.MAX_CRUISE_CONTROL_SPEED)
                {
                    Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.Additional.HeaderCruise, string.Format(Locale.Notifications.Vehicles.Additional.MaxSpeed, Math.Floor(Settings.MAX_CRUISE_CONTROL_SPEED * 3.6f)));

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

            var rotVect = veh.GetRotationVelocity();

            if (veh.GetHeightAboveGround() > 1f || Math.Abs(rotVect.Z) > 1.5f)
            {
                Notification.Show(Notification.Types.Information, Locale.Notifications.Vehicles.Additional.HeaderCruise, Locale.Notifications.Vehicles.Additional.Danger, 2500);

                ToggleCruiseControl(true);

                GameEvents.Update -= CruiseControlTick;

                return;
            }

            if (veh.HasCollidedWithAnything())
            {
                Notification.Show(Notification.Types.Information, Locale.Notifications.Vehicles.Additional.HeaderCruise, Locale.Notifications.Vehicles.Additional.Collision, 2500);

                ToggleCruiseControl(true);

                GameEvents.Update -= CruiseControlTick;

                return;
            }

            if (RAGE.Game.Pad.IsControlJustPressed(32, 130) || RAGE.Game.Pad.IsControlJustPressed(32, 129) || RAGE.Game.Pad.IsControlJustPressed(32, 76))
            {
                Notification.Show(Notification.Types.Information, Locale.Notifications.Vehicles.Additional.HeaderCruise, Locale.Notifications.Vehicles.Additional.Invtervention, 2500);

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
                TryEnterVehicle(vehicle, seatId);
            }
        }

        public static void TryEnterVehicle(Vehicle veh = null, int seatId = -1)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (Player.LocalPlayer.IsInAnyVehicle(false) || LastVehicleExitedTime.IsSpam(1000, false, false))
                return;

            if (Player.LocalPlayer.GetScriptTaskStatus(2500551826) != 7)
            {
                Player.LocalPlayer.ClearTasks();

                return;
            }

            if (veh == null)
            {
                veh = Utils.GetClosestVehicleToSeatIn(Player.LocalPlayer.Position, Settings.ENTITY_INTERACTION_MAX_DISTANCE, seatId);

                if (veh == null)
                    return;
            }

            if (seatId < 0)
            {
                seatId = veh.GetFirstFreeSeatId(0);

                if (seatId < 0)
                    return;
            }

            Player.LocalPlayer.SetData("TEV::V", veh);
            Player.LocalPlayer.SetData("TEV::S", seatId);
            Player.LocalPlayer.SetData("TEV::T", DateTime.Now);

            Utils.JsEval("mp.players.local.taskEnterVehicle", veh.Handle, -1, seatId - 1, 1.5f, 1, 0);

            GameEvents.Render -= EnterVehicleRender;
            GameEvents.Render += EnterVehicleRender;
        }

        private static void EnterVehicleRender()
        {
            var tStatus = Player.LocalPlayer.GetScriptTaskStatus(2500551826);

            var seatId = Player.LocalPlayer.GetData<int>("TEV::S");
            var veh = Player.LocalPlayer.GetData<Vehicle>("TEV::V");
            var timePassed = DateTime.Now.Subtract(Player.LocalPlayer.GetData<DateTime>("TEV::T")).TotalMilliseconds;

            if (tStatus == 7 || veh?.Exists != true || Player.LocalPlayer.Position.DistanceTo(veh.Position) > Settings.ENTITY_INTERACTION_MAX_DISTANCE || timePassed > 5000 || (timePassed > 500 && Utils.AnyOnFootMovingControlPressed()) || (!veh.IsOnAllWheels() && SetIntoVehicle(veh, seatId)))
            {
                if (tStatus != 7)
                    Player.LocalPlayer.ClearTasks();

                Player.LocalPlayer.ResetData("TEV::V");
                Player.LocalPlayer.ResetData("TEV::S");
                Player.LocalPlayer.ResetData("TEV::T");

                GameEvents.Render -= EnterVehicleRender;
            }
        }

        private static bool SetIntoVehicle(Vehicle veh, int seatId)
        {
            Player.LocalPlayer.SetIntoVehicle(veh.Handle, seatId - 1);

            return true;
        }

        private static void InVehicleRender()
        {
            if (!Player.LocalPlayer.IsInAnyVehicle(false))
            {
                LastVehicleExitedTime = DateTime.Now;

                GameEvents.Render -= InVehicleRender;
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

        public static void Park(Vehicle vehicle, int slot = -1)
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

                if (house?.GarageType == null)
                    return;

                if (slot < 0)
                {
                    Events.CallRemote("House::Garage::SlotsMenu", vehicle, house.Id);
                }
                else
                {
                    Events.CallRemote("House::Garage::Vehicle", slot, vehicle, house.Id);
                }
            }
            else if (Player.LocalPlayer.HasData("CurrentGarageRoot"))
            {
                var gRoot = Player.LocalPlayer.GetData<Data.Locations.GarageRoot>("CurrentGarageRoot");

                if (gRoot == null)
                    return;

                if (slot < 0)
                {
                    Events.CallRemote("Garage::SlotsMenu", vehicle, (int)gRoot.Type);
                }
                else
                {
                    Events.CallRemote("Garage::Vehicle", slot, vehicle, (int)gRoot.Type);
                }
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

        public static void StartDriverSync()
        {
            CurrentDriverSyncTask?.Cancel();

            var veh = Player.LocalPlayer.Vehicle;

            if (veh?.Exists != true)
                return;

            var data = GetData(veh);

            if (data == null)
                return;

            Vector3 lastPos = veh.GetCoords(false);

            CurrentDriverSyncTask = new AsyncTask(() =>
            {
                veh = Player.LocalPlayer.Vehicle;

                if (veh?.Exists != true || veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                    return true;

                if (data.EngineOn)
                {
                    var curPos = veh.GetCoords(false);

                    var dist = Math.Round(Math.Abs(Vector3.Distance(lastPos, curPos)), 2);
                    var fuelDiff = 0.001f * dist;
                    var dirtLevel = (byte)Math.Round(veh.GetDirtLevel());

                    if (dirtLevel > 15)
                        dirtLevel = 15;

                    if (fuelDiff > 0 || dist > 0)
                    {
                        Events.CallRemote("Vehicles::Sync", fuelDiff, dist, dirtLevel);
                    }

                    //RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Error, "Fuel: " + data.FuelLevel);
                    //RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Error, "Mileage: " + data.Mileage);
                    //RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Error, "DirtLevel: " + data.DirtLevel);

                    lastPos = curPos;
                }

                return false;
            }, 1500, true, 0);

            CurrentDriverSyncTask.Run();
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

        public static void ShowContainer(Vehicle veh)
        {
            var vData = GetData(veh);

            if (vData == null)
                return;

            if (Player.LocalPlayer.Vehicle != null)
                return;

            if (vData.TID == null)
            {
                CEF.Notification.Show(Notification.Types.Information, Locale.Notifications.Vehicles.Header, Locale.Notifications.Vehicles.Trunk.NoTrunk);

                return;
            }

            CEF.Inventory.Show(Inventory.Types.Container, (uint)vData.TID);
        }

        public static void TakePlate(Vehicle veh)
        {
            var vData = GetData(veh);

            if (vData == null)
                return;

            if (Player.LocalPlayer.Vehicle != null)
                return;

            var plateText = veh.GetNumberplateText();

            if (plateText == null || plateText.Length == 0)
            {
                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Vehicles.NoPlate);

                return;
            }

            Events.CallRemote("Vehicles::TakePlate", veh);
        }

        public static void SetupPlate(Vehicle veh)
        {
            var vData = GetData(veh);

            if (vData == null)
                return;

            if (Player.LocalPlayer.Vehicle != null)
                return;

            var plateText = veh.GetNumberplateText();

            if (plateText != null && plateText.Length > 0)
            {
                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Vehicles.PlateExists);

                return;
            }

            Events.CallRemote("Vehicles::SetupPlate", veh, 0);
        }

        public static void ToggleAutoPilot(bool? forceStatus = null, bool stopVehicle = false)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            GameEvents.Render -= AutoPilotTick;

            var veh = pData.AutoPilot;

            if (forceStatus == false || veh != null)
            {
                Player.LocalPlayer.ClearTasks();

                if (stopVehicle)
                    veh?.TaskTempAction(27, 10000);

                pData.AutoPilot = null;

                var blip = Player.LocalPlayer.GetData<Additional.ExtraBlip>("AutoPilot::Blip");

                if (blip != null)
                {
                    var wBlip = Utils.GetWaypointBlip();

                    if (wBlip > 0)
                        RAGE.Game.Ui.SetBlipRoute(wBlip, true);

                    blip.Destroy();
                }

                Player.LocalPlayer.ResetData("AutoPilot::Blip");

                if (veh != null)
                    CEF.Notification.Show(Notification.Types.Information, Locale.Notifications.Vehicles.Additional.HeaderAutoPilot, Locale.Notifications.Vehicles.Additional.Off);
            }
            else
            {
                veh = Player.LocalPlayer.Vehicle;

                if (veh == null)
                    return;

                var vData = GetData(veh);

                if (vData == null)
                    return;

                if (veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle || !veh.GetIsEngineRunning() || vData.ForcedSpeed != 0f)
                    return;

                if (!vData.Data.HasAutoPilot)
                {
                    CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.Additional.HeaderAutoPilot, Locale.Notifications.Vehicles.Additional.Unsupported);

                    return;
                }

                var pos = GameEvents.WaypointPosition;

                if (pos == null)
                {
                    CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.Additional.HeaderAutoPilot, Locale.Notifications.Commands.Teleport.NoWaypoint);

                    return;
                }

                RAGE.Game.Ui.ClearGpsPlayerWaypoint();

                var blip = new Additional.ExtraBlip(162, pos, null, 1f, 2, 255, 0f, false, 0, 0f, Player.LocalPlayer.Dimension, Additional.ExtraBlip.Types.AutoPilot);

                blip.SetAsReachable(7.5f);
                blip.ToggleRouting(true);

                Player.LocalPlayer.SetData("AutoPilot::Blip", blip);

                Player.LocalPlayer.TaskVehicleDriveToCoord(veh.Handle, pos.X, pos.Y, pos.Z, 30f, 1, 1, 2883621, 30f, 1f);

                pData.AutoPilot = veh;

                GameEvents.Render += AutoPilotTick;

                CEF.Notification.Show(Notification.Types.Information, Locale.Notifications.Vehicles.Additional.HeaderAutoPilot, Locale.Notifications.Vehicles.Additional.On);

                blip.SetAsReachable();
            }
        }

        private static void AutoPilotTick()
        {
            var veh = Player.LocalPlayer.Vehicle;

            if (veh?.Exists != true || veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle || !veh.GetIsEngineRunning() || veh.GetSharedData<float>("ForcedSpeed", 1f) != 0f)
            {
                ToggleAutoPilot(false, false);

                return;
            }
            
            if (Player.LocalPlayer.GetScriptTaskStatus(0x93A5526E) != 1)
            {
                ToggleAutoPilot(false, true);

                return;
            }

            if (RAGE.Game.Pad.IsControlJustPressed(32, 133) || RAGE.Game.Pad.IsControlJustPressed(32, 134) || RAGE.Game.Pad.IsControlJustPressed(32, 130) || RAGE.Game.Pad.IsControlJustPressed(32, 129) || RAGE.Game.Pad.IsControlJustPressed(32, 76))
            {
                Notification.Show(Notification.Types.Information, Locale.Notifications.Vehicles.Additional.HeaderAutoPilot, Locale.Notifications.Vehicles.Additional.Invtervention);

                ToggleAutoPilot(false, false);

                return;
            }
        }

        public static void ToggleAnchor()
        {
            if (LastCruiseControlToggled.IsSpam(1000, false, false))
                return;

            var veh = Player.LocalPlayer.Vehicle;

            var vData = GetData(veh);

            if (vData == null)
                return;

            if (veh == null || veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle || vData.ForcedSpeed != 0f)
                return;

            if (vData.Data.Type != Data.Vehicles.Vehicle.Types.Boat)
                return;

            Events.CallRemote("Vehicles::Anchor", !vData.IsAnchored);

            LastCruiseControlToggled = DateTime.Now;
        }

        public static void SendCoordsToDriver()
        {
            var veh = Player.LocalPlayer.Vehicle;

            if (veh?.Exists != true)
                return;

            var vData = GetData(veh);

            if (vData == null)
                return;

            var pHandle = veh.GetPedInSeat(-1, 0);

            if (pHandle == Player.LocalPlayer.Handle)
                return;

            var driver = Utils.GetPlayerByHandle(pHandle, true);

            if (driver?.Exists != true)
                return;

            var wpPos = GameEvents.WaypointPosition;

            if (wpPos == null)
            {
                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Commands.Teleport.NoWaypoint);

                return;
            }

            Sync.Offers.Request(driver, Offers.Types.WaypointShare, $"{wpPos.X}_{wpPos.Y}");
        }

/*        public static void ApplyTrailerSattings(Vehicle veh)
        {
            veh.SetCanBeVisiblyDamaged(false);
            veh.SetCanBreak(false);
            veh.SetDeformationFixed();
            veh.SetDisablePetrolTankDamage(true);
            veh.SetDisablePetrolTankFires(true);
            veh.SetInvincible(true);
        }*/
    }
}
