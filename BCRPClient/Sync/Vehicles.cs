using BCRPClient.CEF;using Newtonsoft.Json.Linq;using RAGE;using RAGE.Elements;using System;using System.Collections.Generic;using System.Linq;
using System.Reflection;

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

        public static DateTime LastRadioSent;

        private static DateTime LastVehicleExitedTime;
        #endregion

        private static AsyncTask CurrentDriverSyncTask { get; set; }

        public static List<Vehicle> ControlledVehicles = new List<Vehicle>();

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

            public RentedVehicle(ushort RemoteId, Data.Vehicles.Vehicle VehicleData)
            {
                this.RemoteId = RemoteId;
                this.VehicleData = VehicleData;

                this.TimeToDelete = Settings.RENTED_VEHICLE_TIME_TO_AUTODELETE;

                this.TimeLeftToDelete = TimeToDelete;
            }

            public void ShowTimeLeftNotification()
            {
                CEF.Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), string.Format(Locale.Notifications.Vehicles.RentedVehicleTimeLeft, $"\"{VehicleData.Name}\"", Sync.World.ServerTime.AddMilliseconds(TimeLeftToDelete).Subtract(Sync.World.ServerTime).GetBeautyString()));
            }

            public static void Check()
            {
                var curVeh = Player.LocalPlayer.Vehicle;

                for (int i = 0; i < All.Count; i++)
                {
                    var x = All[i];

                    if (curVeh == null || curVeh.RemoteId != x.RemoteId)
                    {
                        if (x.TimeLeftToDelete > 0)
                            x.TimeLeftToDelete -= 1000;

                        if (x.TimeLeftToDelete < 0)
                            x.TimeLeftToDelete = 0;

                        if (x.TimeLeftToDelete <= 0)
                        {
                            //Events.CallRemote("VRent::Cancel", x.RemoteId);
                        }
                        else
                        {
                            if (x.TimeLeftToDelete % 60_000 == 0 || x.TimeLeftToDelete <= 5_000 || (x.TimeLeftToDelete <= 30_000 && x.TimeLeftToDelete % 10_000 == 0))
                                x.ShowTimeLeftNotification();
                        }
                    }
                    else
                    {
                        if (x.TimeLeftToDelete != x.TimeToDelete)
                            x.TimeLeftToDelete = x.TimeToDelete;
                    }
                }
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

            public byte IndicatorsState => (byte)Vehicle.GetSharedData<int>("Inds", 0);

            public Sync.Radio.StationTypes Radio => (Sync.Radio.StationTypes)Vehicle.GetSharedData<int>("Radio", 0);

            public float ForcedSpeed => Vehicle.GetSharedData<float>("ForcedSpeed", 0f);

            public float FuelLevel { get => Vehicle.GetData<float?>("Fuel") ?? 0f; set => Vehicle.SetData("Fuel", value); }

            public float Mileage { get => Vehicle.GetData<float?>("Mileage") ?? 0f; set => Vehicle.SetData("Mileage", value); }

            public uint VID => Utils.ToUInt32(Vehicle.GetSharedData<object>("VID", 0));

            public uint TID => Utils.ToUInt32(Vehicle.GetSharedData<object>("TID", 0));

            public bool HasNeonMod => Vehicle.GetSharedData<bool>("Mods::Neon", false);

            public bool HasTurboTuning => Vehicle.GetSharedData<bool>("Mods::Turbo", false);

            public bool IsAnchored => Vehicle.GetSharedData<bool>("Anchor", false);

            public bool IsPlaneChassisOff => Vehicle.GetSharedData<bool>("IPCO", false);

            public Utils.Colour TyreSmokeColour => Vehicle.GetSharedData<JObject>("Mods::TSColour")?.ToObject<Utils.Colour>();

            public byte DirtLevel => (byte)Vehicle.GetSharedData<int>("DirtLevel", 0);

            public string FrozenPosition => Vehicle.GetSharedData<string>("IsFrozen");

            public float ColshapeLimitedMaxSpeed { get => Vehicle.GetData<float>("CLMS"); set { if (value <= 0f) Vehicle.ResetData("CLMS"); else Vehicle.SetData("CLMS", value); } }

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
            var pos = veh.GetCoords(false);

            #region Required Things For Normal Behaviour
            //veh.SetAutomaticallyAttaches(0, 0);

            RAGE.Game.Streaming.RequestCollisionAtCoord(pos.X, pos.Y, pos.Z);
            RAGE.Game.Streaming.RequestAdditionalCollisionAtCoord(pos.X, pos.Y, pos.Z);

            veh.SetLoadCollisionFlag(true, 0);
            veh.TrackVisibility();

            veh.SetUndriveable(false);

            //veh.SetAutomaticallyAttaches(0, 0);
            #endregion

            #region Default Settings
            veh.SetDisablePetrolTankDamage(true);

            veh.SetHandling("fWeaponDamageMult", 0.1f);
            #endregion

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

            data = new VehicleData(veh);

            InvokeHandler("Anchor", data, data.IsAnchored, null);

            InvokeHandler("IsFrozen", data, data.FrozenPosition, null);

            InvokeHandler("IsInvincible", data, data.IsInvincible, null);

            InvokeHandler("Mods::TSColour", data, veh.GetSharedData<JObject>("Mods::TSColour", null), null);
            InvokeHandler("Mods::Turbo", data, data.HasTurboTuning, null);

            InvokeHandler("Mods::Xenon", data, veh.GetSharedData<int>("Mods::Xenon", -2), null);

            InvokeHandler("Mods::CT", data, veh.GetSharedData<int>("Mods::CT", 0), null);

            InvokeHandler("Engine::On", data, data.EngineOn, null);

            InvokeHandler("Inds", data, data.IndicatorsState, null);

            InvokeHandler("Radio", data, (int)data.Radio, null);

            InvokeHandler("DirtLevel", data, data.DirtLevel, null);

            if (veh.HasLandingGear())
            {
                veh.ControlLandingGear(data.IsPlaneChassisOff ? 3 : 2);
            }

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
            RAGE.Game.Vehicle.DefaultEngineBehaviour = false;
            RAGE.Game.Vehicle.RepairOnExtraToggle = false;

            //Player.LocalPlayer.SetConfigFlag(184, true);
            #endregion

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
                            Utils.DrawText($"ID: {x.RemoteId} | VID: {data.VID} | TID: {(data.TID == 0 ? "null" : data.TID.ToString())}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                            Utils.DrawText($"EngineOn: {data.EngineOn} | Locked: {data.DoorsLocked} | TrunkLocked: {data.TrunkLocked}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                            Utils.DrawText($"Fuel: {data.FuelLevel.ToString("0.00")} | Mileage: {data.Mileage.ToString("0.00")}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                            Utils.DrawText($"EHP: {x.GetEngineHealth()} | BHP: {x.GetBodyHealth()} | IsInvincible: {data.IsInvincible}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                            Utils.DrawText($"Speed: {x.GetSpeedKm().ToString("0.00")} | ForcedSpeed: {(data.ForcedSpeed * 3.6f).ToString("0.00")}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                        }
                        else
                        {
                            Utils.DrawText($"ID: {x.RemoteId} | VID: {data.VID}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                            Utils.DrawText($"EngineHP: {x.GetEngineHealth()}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
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

                    InvokeHandler("Inds", data, data.IndicatorsState, null);

                    InvokeHandler("Radio", data, (int)data.Radio, null);

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

                    InvokeHandler("Engine::On", data, data.EngineOn, null);

                    Sync.Radio.SetCurrentRadioStationType(data.Radio);
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

            #region Events

            AddDataHandler("IPCO", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                var state = (bool?)value ?? false;

                if (!veh.HasLandingGear())
                    return;

                veh.ControlLandingGear(state ? 1 : 0);
            });

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

                var state = value as bool? ?? false;

                veh.SetEngineOn(state, true, true);
                veh.SetJetEngineOn(state);

                veh.SetLights(state ? (vData.LightsOn ? 2 : 1) : 1);

                if (Player.LocalPlayer.Vehicle?.Handle == veh.Handle)
                    HUD.SwitchEngineIcon(state);
            });

            AddDataHandler("Doors::Locked", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                var state = value as bool? ?? false;

                if (state)
                {
/*                    veh.SetDoorsLocked(2);
                    veh.SetDoorsLockedForAllPlayers(true);

                    veh.SetDoorsLockedForPlayer(Player.LocalPlayer.Handle, true);*/

                    RAGE.Game.Audio.PlaySoundFromEntity(-1, "Remote_Control_Close", veh.Handle, "PI_Menu_Sounds", true, 0);
                }
                else
                {
/*                    veh.SetDoorsLocked(1);
                    veh.SetDoorsLockedForAllPlayers(false);

                    veh.SetDoorsLockedForPlayer(Player.LocalPlayer.Handle, false);*/

                    RAGE.Game.Audio.PlaySoundFromEntity(-1, "Remote_Control_Open", veh.Handle, "PI_Menu_Sounds", true, 0);
                }

                if (Player.LocalPlayer.Vehicle != null && Player.LocalPlayer.Vehicle == veh)
                    HUD.SwitchDoorsIcon(state);
            });

            AddDataHandler("Inds", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                var state = Utils.ToByte(value);

                if (state == 0)
                {
                    veh.SetIndicatorLights(0, false);
                    veh.SetIndicatorLights(1, false);
                }
                else if (state == 1)
                {
                    veh.SetIndicatorLights(0, true);
                    veh.SetIndicatorLights(1, false);
                }
                else if (state == 2)
                {
                    veh.SetIndicatorLights(0, false);
                    veh.SetIndicatorLights(1, true);
                }
                else if (state == 3)
                {
                    veh.SetIndicatorLights(0, true);
                    veh.SetIndicatorLights(1, true);
                }
            });

            AddDataHandler("Lights::On", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                var state = value as bool? ?? false;

                if (state)
                {
                    veh.SetLights(2);
                }
                else
                {
                    veh.SetLights(1);
                }

                if (Player.LocalPlayer.Vehicle?.Handle == veh.Handle)
                    HUD.SwitchLightsIcon(state);
            });

            AddDataHandler("Radio", (vData, value, oldValue) =>
            {
                var veh = vData.Vehicle;

                var statinTypeNum = value == null ? Sync.Radio.StationTypes.Off : (Sync.Radio.StationTypes)(int)value;

                Sync.Radio.SetVehicleRadioStation(veh, statinTypeNum);
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
                vData.Vehicle.SetDirtLevel(Utils.ToSingle(value));
            });
            #endregion

            Events.Add("Vehicles::Garage::SlotsMenu", async (args) =>
            {
                if (Utils.IsAnyCefActive(true))
                    return;

                var freeSlots = ((JArray)args[0]).ToObject<List<int>>();

                freeSlots.Insert(0, int.MinValue + freeSlots[(new Random()).Next(0, freeSlots.Count)]);

                await CEF.ActionBox.ShowSelect
                (
                    "GarageVehiclePlaceSelect", Locale.Actions.GarageVehicleSlotSelectHeader, freeSlots.Select(x => ((decimal)x, x < 0 ? "Случайное место" : $"Место #{x + 1}")).ToArray(), null, null,

                    CEF.ActionBox.DefaultBindAction,

                    (rType, idD) =>
                    {
                        var id = (int)idD;

                        if (rType == CEF.ActionBox.ReplyTypes.OK)
                        {
                            var vehicle = BCRPClient.Interaction.CurrentEntity as Vehicle;

                            if (vehicle == null)
                                return;

                            CEF.ActionBox.Close(true);

                            if (id < 0)
                                id = int.MinValue + id;

                            Sync.Vehicles.Park(vehicle, id);
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

            Events.Add("Vehicles::WTS", (args) =>
            {
                var veh = RAGE.Elements.Entities.Vehicles.GetAtRemote((ushort)(int)args[0]);
                var seat = (int)args[1] - 1;

                var timeout = (int)args[2];

                Utils.CancelPendingTask("Vehicles::WTS");

                AsyncTask task = null;

                task = new AsyncTask(async () =>
                {
                    var time = Sync.World.ServerTime;

                    while (Utils.IsTaskStillPending("Vehicles::WTS", task) && Sync.World.ServerTime.Subtract(time).TotalMilliseconds <= timeout)
                    {
                        await RAGE.Game.Invoker.WaitAsync(50);

                        if (!Utils.IsTaskStillPending("Vehicles::WTS", task))
                            return;

                        if (Additional.SkyCamera.IsFadedOut || veh?.Exists != true)
                            continue;

                        if (Player.LocalPlayer.Vehicle == veh)
                        {
                            Utils.CancelPendingTask("Vehicles::WTS");

                            return;
                        }

                        if (veh.IsSeatFree(seat, 0))
                        {
                            Player.LocalPlayer.SetIntoVehicle(veh.Handle, seat);
                        }
                    }

                    Utils.CancelPendingTask("Vehicles::WTS");
                }, 25, false, 0);

                Utils.SetTaskAsPending("Vehicles::WTS", task);
            });

            Events.Add("Vehicles::JVRO", async (args) =>
            {
                var rentPrice = Utils.ToDecimal(args[0]);

                var vehicle = Player.LocalPlayer.Vehicle;

                if (vehicle?.Exists != true)
                    return;

                var vData = GetData(vehicle);

                if (vData == null)
                    return;

                await CEF.ActionBox.ShowMoney
                (
                    "JobVehicleRentMoney", Locale.Actions.JobVehicleRentTitle, string.Format(Locale.Actions.JobVehicleRentText, $"{vData.Data.Name} [{(vehicle.GetNumberplateText() ?? "null")}]", Utils.GetPriceString(rentPrice)),

                    () =>
                    {
                        CEF.ActionBox.DefaultBindAction.Invoke();

                        var checkAction = new Action(() =>
                        {
                            if (vehicle?.Exists != true || Player.LocalPlayer.Vehicle != vehicle || vehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                CEF.ActionBox.Close(true);
                        });

                        Player.LocalPlayer.SetData("ActionBox::Temp::JVRVA", checkAction);

                        GameEvents.Update -= checkAction.Invoke;
                        GameEvents.Update += checkAction.Invoke;
                    },

                    async (rType) =>
                    {
                        if (CEF.ActionBox.LastSent.IsSpam(500, false, true))
                            return;

                        if (rType == CEF.ActionBox.ReplyTypes.OK || rType == CEF.ActionBox.ReplyTypes.Cancel)
                        {
                            CEF.ActionBox.LastSent = Sync.World.ServerTime;

                            var res = (int)await Events.CallRemoteProc("Vehicles::JVRS", rType == CEF.ActionBox.ReplyTypes.OK);

                            if (res == byte.MaxValue)
                            {
                                Player.LocalPlayer.SetData("ActionBox::Temp::JVRVA::DLV", true);

                                CEF.ActionBox.Close(true);
                            }
                            else
                            {
                                if (res == 3)
                                    return;

                                CEF.ActionBox.Close(true);

                                if (res == 1)
                                {
                                    CEF.Notification.ShowError(Locale.Notifications.General.JobRentVehicleAlreadyRented0);
                                }
                                else if (res == 2)
                                {
                                    CEF.Notification.ShowError(Locale.Notifications.General.JobRentVehicleAlreadyRented1);
                                }
                            }
                        }
                        else if (rType == CEF.ActionBox.ReplyTypes.Additional1)
                        {
                            CEF.ActionBox.Close(true);
                        }
                    },

                    () =>
                    {
                        var checkAction = Player.LocalPlayer.GetData<Action>("ActionBox::Temp::JVRVA");

                        if (checkAction != null)
                        {
                            GameEvents.Update -= checkAction.Invoke;

                            Player.LocalPlayer.ResetData("ActionBox::Temp::JVRVA");
                        }

                        if (!Player.LocalPlayer.HasData("ActionBox::Temp::JVRVA::DLV"))
                        {
                            if (Player.LocalPlayer.Vehicle == vehicle)
                            {
                                Player.LocalPlayer.TaskLeaveAnyVehicle(0, 0);
                            }
                        }
                        else
                        {
                            Player.LocalPlayer.ResetData("ActionBox::Temp::JVRVA::DLV");
                        }
                    }
                );
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

            LastCruiseControlToggled = Sync.World.ServerTime;
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

            LastBeltToggled = Sync.World.ServerTime;

            Events.CallRemote("Players::ToggleBelt");
        }

        public static void BeltTick()
        {
            RAGE.Game.Pad.DisableControlAction(32, 75, true);

            if (RAGE.Game.Pad.IsDisabledControlJustPressed(32, 75))
                if (Sync.World.ServerTime.Subtract(LastSeatBeltShowed).TotalMilliseconds > 500)
                {
                    Notification.Show(Notification.Types.Information, Locale.Notifications.Vehicles.SeatBelt.Header, Locale.Notifications.Vehicles.SeatBelt.TakeOffToLeave);

                    LastSeatBeltShowed = Sync.World.ServerTime;
                }

            if (Player.LocalPlayer.Vehicle?.Exists != true)
            {
                ToggleBelt(true);

                GameEvents.Update -= BeltTick;
            }
        }
        #endregion

        public static async void Lock(bool? forceвState = null, Vehicle vehicle = null)
        {
            if (LastDoorsLockToggled.IsSpam(500, false, true))
                return;

            LastDoorsLockToggled = Sync.World.ServerTime;

            if (vehicle == null)
            {
                vehicle = Player.LocalPlayer.Vehicle;

                if (vehicle?.Exists != true || vehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                    vehicle = BCRPClient.Interaction.CurrentEntity as Vehicle ?? Utils.GetClosestVehicle(Player.LocalPlayer.Position, Settings.ENTITY_INTERACTION_MAX_DISTANCE);
            }

            if (vehicle?.Exists != true)
                return;

            var vData = GetData(vehicle);

            if (vData == null)
                return;

            var state = vData.DoorsLocked;

            if (forceвState != null)
            {
                if (forceвState == state)
                {
                    CEF.Notification.ShowInfo(state ? Locale.Get("VEHICLE_DOORS_LOCKED_E_1") : Locale.Get("VEHICLE_DOORS_UNLOCKED_E_1"));

                    return;
                }
                else
                {
                    state = (bool)forceвState;
                }
            }
            else
            {
                state = !state;
            }

            var res = (int)await Events.CallRemoteProc("Vehicles::TDL", vehicle, state);

            if (res == 255)
            {
                CEF.Notification.ShowSuccess(state ? Locale.Get("VEHICLE_DOORS_LOCKED_S_0") : Locale.Get("VEHICLE_DOORS_UNLOCKED_S_0"));
            }
            else if (res == 0)
            {
                CEF.Notification.ShowErrorDefault();
            }
        }

        public static async void ToggleEngine(Vehicle veh, bool? forcedState)
        {
            if (veh?.Exists != true)
                return;

            var vData = Sync.Vehicles.GetData(veh);

            if (vData == null)
                return;

            if (veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                return;

            var state = vData.EngineOn;

            if (forcedState != null)
            {
                if (forcedState == state)
                {
                    CEF.Notification.ShowInfo(state ? Locale.Get("VEHICLE_ENGINE_ON_E_1") : Locale.Get("VEHICLE_ENGINE_OFF_E_1"));

                    return;
                }
                else
                {
                    state = (bool)forcedState;
                }
            }
            else
            {
                state = !state;
            }

            if (LastEngineToggled.IsSpam(1000, false, true))
                return;

            LastEngineToggled = Sync.World.ServerTime;

            var res = (int)await Events.CallRemoteProc("Vehicles::ET", veh, (byte)(state ? 1 : 0));

            if (res == 255)
            {
                CEF.Notification.ShowSuccess(state ? Locale.Get("VEHICLE_ENGINE_ON_S_0") : Locale.Get("VEHICLE_ENGINE_OFF_S_0"));
            }
            else if (res == 5)
            {
                CEF.Notification.ShowError(Locale.Get("VEHICLE_ENGINE_BROKEN_S_0"));
            }
            else if (res == 6)
            {
                CEF.Notification.ShowError(Locale.Get("VEHICLE_FUEL_OUTOF_S_0"));
            }
        }

        public static async void ToggleIndicator(Vehicle veh, byte type) // 0 - right, 1 - left, 2 - both
        {
            if (veh?.Exists != true)
                return;

            var vData = GetData(veh);

            if (vData == null)
                return;

            if (LastIndicatorToggled.IsSpam(500, false, false))
                return;

            int vehClass = veh.GetClass();

            // if cycle, boat, helicopter, plane
            if (vehClass == 13 || vehClass == 14 || vehClass == 15 || vehClass == 16)
                return;

            if (veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                return;

            LastIndicatorToggled = Sync.World.ServerTime;

            var state = vData.IndicatorsState;

            if (type == 0)
            {
                if (state == 1)
                    state = 0;
                else
                    state = 1;
            }
            else if (type == 1)
            {
                if (state == 2)
                    state = 0;
                else
                    state = 2;
            }
            else
            {
                if (state == 3)
                    state = 0;
                else
                    state = 3;
            }

            var res = (int)await Events.CallRemoteProc("Vehicles::TIND", veh, state);
        }

        public static async void ToggleLights(Vehicle veh)
        {
            if (veh?.Exists != true)
                return;

            var vData = GetData(veh);

            if (vData == null)
                return;

            if (veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                return;

            if (!veh.GetIsEngineRunning())
                return;

            if (LastLightsToggled.IsSpam(500, false, false))
                return;

            var vehClass = veh.GetClass();

            // if cycle, boat, helicopter, plane
            if (vehClass == 13 || vehClass == 14 || vehClass == 15 || vehClass == 16)
                return;

            LastLightsToggled = Sync.World.ServerTime;

            var state = vData.LightsOn;

            state = !state;

            var res = (int)await Events.CallRemoteProc("Vehicles::TLI", veh, state);
        }

        public static async void ToggleTrunkLock(bool? forcedState = null, Vehicle vehicle = null)
        {
            if (LastDoorsLockToggled.IsSpam(1000, false, true))
                return;

            LastDoorsLockToggled = Sync.World.ServerTime;

            if (vehicle == null)
            {
                vehicle = Player.LocalPlayer.Vehicle;

                if (vehicle?.Exists != true || vehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                    vehicle = BCRPClient.Interaction.CurrentEntity as Vehicle ?? Utils.GetClosestVehicle(Player.LocalPlayer.Position, Settings.ENTITY_INTERACTION_MAX_DISTANCE);
            }

            if (vehicle?.Exists != true)
                return;

            var vData = GetData(vehicle);

            if (vData == null)
                return;

            var state = vData.TrunkLocked;

            if (forcedState != null)
            {
                if (forcedState == state)
                {
                    CEF.Notification.ShowInfo(state ? Locale.Get("VEHICLE_TRUNK_LOCKED_E_1") : Locale.Get("VEHICLE_TRUNK_UNLOCKED_E_1"));

                    return;
                }
                else
                {
                    state = (bool)forcedState;
                }
            }
            else
            {
                state = !state;
            }

            var res = (int)await Events.CallRemoteProc("Vehicles::TTL", vehicle, state);

            if (res == 255)
            {
                CEF.Notification.ShowSuccess(state ? Locale.Get("VEHICLE_TRUNK_LOCKED_S_0") : Locale.Get("VEHICLE_TRUNK_UNLOCKED_S_0"));
            }
            else if (res == 0)
            {
                CEF.Notification.ShowErrorDefault();
            }
        }

        public static async void ToggleHoodLock(bool? forcedState = null, Vehicle vehicle = null)
        {
            if (LastDoorsLockToggled.IsSpam(500, false, true))
                return;

            LastDoorsLockToggled = Sync.World.ServerTime;

            if (vehicle == null)
            {
                vehicle = Player.LocalPlayer.Vehicle;

                if (vehicle?.Exists != true || vehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                    vehicle = BCRPClient.Interaction.CurrentEntity as Vehicle ?? Utils.GetClosestVehicle(Player.LocalPlayer.Position, Settings.ENTITY_INTERACTION_MAX_DISTANCE);
            }

            if (vehicle?.Exists != true)
                return;

            var vData = GetData(vehicle);

            if (vData == null)
                return;

            var state = vData.HoodLocked;

            if (forcedState != null)
            {
                if (forcedState == state)
                {
                    CEF.Notification.ShowInfo(state ? Locale.Get("VEHICLE_HOOD_LOCKED_E_1") : Locale.Get("VEHICLE_HOOD_UNLOCKED_E_1"));

                    return;
                }
                else
                {
                    state = (bool)forcedState;
                }
            }
            else
            {
                state = !state;
            }

            var res = (int)await Events.CallRemoteProc("Vehicles::THL", vehicle, state);

            if (res == 255)
            {
                CEF.Notification.ShowSuccess(state ? Locale.Get("VEHICLE_HOOD_LOCKED_S_0") : Locale.Get("VEHICLE_HOOD_UNLOCKED_S_0"));
            }
            else if (res == 0)
            {
                CEF.Notification.ShowErrorDefault();
            }
        }

        #endregion

        public static async void ToggleLandingGearState(Vehicle vehicle)
        {
            if (vehicle == null || vehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                return;

            if (!vehicle.HasLandingGear())
                return;

            var vType = Data.Vehicles.GetByModel(vehicle.Model);

            if (vType?.Type != Data.Vehicles.Vehicle.Types.Plane || vType.ID == "duster")
                return;

            if (vehicle.IsLocal)
            {
                var curGearState = vehicle.GetLandingGearState();

                vehicle.ControlLandingGear(curGearState == 1 || curGearState == 3 ? 0 : 1);
            }
            else
            {
                var vData = Sync.Vehicles.GetData(vehicle);

                if (vData == null)
                    return;

                if (LastCruiseControlToggled.IsSpam(1000, false, true))
                    return;

                LastCruiseControlToggled = Sync.World.ServerTime;

                var state = vData.IsPlaneChassisOff;

                state = !state;

                var res = (int)await Events.CallRemoteProc("Vehicles::SPSOS", state);

                if (res == 255)
                {
                    CEF.Notification.ShowSuccess(state ? Locale.Get("VEHICLE_LGEAR_OFF_S_0") : Locale.Get("VEHICLE_LGEAR_ON_S_0"));
                }
                else if (res == 0)
                {
                    CEF.Notification.ShowErrorDefault();
                }
            }
        }

        #region Vehicle Menu Methods
        #region Shuffle Seat
        public static async void SeatTo(int seatId, Vehicle vehicle)
        {
            if (vehicle == null)
                return;

            if (Vector3.Distance(Player.LocalPlayer.Position, vehicle.Position) > Settings.ENTITY_INTERACTION_MAX_DISTANCE)
                return;

            var data = Players.GetData(Player.LocalPlayer);
            var vData = GetData(vehicle);

            if (data == null || vData == null)
                return;

            // to trunk
            if (seatId == int.MaxValue)
            {
                if (vehicle.DoesHaveDoor(5) > 0)
                {
                    var res = (int)await Events.CallRemoteProc("Players::GoToTrunk", vehicle);

                    if (res == 255)
                    {
                        return;
                    }
                    else if (res == 0)
                    {
                        CEF.Notification.ShowErrorDefault();
                    }
                    else if (res == 1)
                    {
                        CEF.Notification.ShowError(Locale.Get("VEHICLE_TRUNK_LOCKED_E_0"));
                    }
                    else if (res == 2)
                    {
                        CEF.Notification.ShowError(Locale.Get("VEHICLE_TRUNK_E_2"));
                    }
                }
                else
                {
                    CEF.Notification.ShowError(Locale.Get("VEHICLE_TRUNK_E_1"));
                }

                return;
            }

            var maxSeats = RAGE.Game.Vehicle.GetVehicleModelNumberOfSeats(vehicle.Model);

            if (maxSeats <= 0)
            {
                CEF.Notification.ShowErrorDefault();

                return;
            }

            if (seatId >= maxSeats)
                seatId = maxSeats - 1;

            if (vehicle.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle)
            {
                CEF.Notification.ShowError(Locale.Get("VEHICLE_SEAT_E_3"));

                return;
            }

            if (!vehicle.IsSeatFree(seatId - 1, 0))
            {
                CEF.Notification.ShowError(Locale.Get("VEHICLE_SEAT_E_2"));

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

            if (CEF.PhoneApps.CameraApp.IsActive)
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

            if (!Utils.CanDoSomething(true, Utils.Actions.Knocked, Utils.Actions.Frozen, Utils.Actions.Cuffed, Utils.Actions.PushingVehicle, Utils.Actions.OtherAnimation, Utils.Actions.Animation, Utils.Actions.Scenario, Utils.Actions.FastAnimation, Utils.Actions.InVehicle, Utils.Actions.Shooting, Utils.Actions.Reloading, Utils.Actions.Climbing, Utils.Actions.Falling, Utils.Actions.Ragdoll, Utils.Actions.Jumping, Utils.Actions.NotOnFoot, Utils.Actions.IsSwimming, Utils.Actions.IsAttachedTo))
                return;

            if (veh.IsDead(0))
            {
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
            Player.LocalPlayer.SetData("TEV::T", Sync.World.ServerTime);

            Utils.JsEval("mp.players.local.taskEnterVehicle", veh.Handle, -1, seatId - 1, 1.5f, 1, 0);

            GameEvents.Render -= EnterVehicleRender;
            GameEvents.Render += EnterVehicleRender;
        }

        private static void EnterVehicleRender()
        {
            var tStatus = Player.LocalPlayer.GetScriptTaskStatus(2500551826);

            var seatId = Player.LocalPlayer.GetData<int>("TEV::S");
            var veh = Player.LocalPlayer.GetData<Vehicle>("TEV::V");
            var timePassed = Sync.World.ServerTime.Subtract(Player.LocalPlayer.GetData<DateTime>("TEV::T")).TotalMilliseconds;

            if (tStatus == 7 || veh?.Exists != true || veh.IsDead(0) || Player.LocalPlayer.Position.DistanceTo(veh.Position) > Settings.ENTITY_INTERACTION_MAX_DISTANCE || timePassed > 7500 || (timePassed > 1_000 && Utils.AnyOnFootMovingControlJustPressed()) || (!veh.IsOnAllWheels() && SetIntoVehicle(veh, seatId)))
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
                LastVehicleExitedTime = Sync.World.ServerTime;

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
                    Events.CallRemote("Garage::SlotsMenu", vehicle, gRoot.Id);
                }
                else
                {
                    Events.CallRemote("Garage::Vehicle", slot, vehicle, gRoot.Id);
                }
            }
            else
            {
                CEF.Notification.ShowError(Locale.Notifications.House.NotNearGarage);

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

            CurrentDriverSyncTask = new AsyncTask(async () =>
            {
                while (true)
                {
                    veh = Player.LocalPlayer.Vehicle;

                    if (veh?.Exists != true || veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                        break;

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

                        if (!veh.GetIsEngineRunning())
                        {
                            var res = (int)await Events.CallRemoteProc("Vehicles::ET", (byte)2);

                            if (res == 255)
                            {

                            }
                        }

                        //RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Error, "Fuel: " + data.FuelLevel);
                        //RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Error, "Mileage: " + data.Mileage);
                        //RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Error, "DirtLevel: " + data.DirtLevel);

                        lastPos = curPos;
                    }

                    SetColshapeVehicleMaxSpeed(veh, Player.LocalPlayer.HasData("ColshapeVehicleSpeedLimited") ? Player.LocalPlayer.GetData<float>("ColshapeVehicleSpeedLimited") : float.MinValue);

                    await RAGE.Game.Invoker.WaitAsync(1_500);
                }

                CurrentDriverSyncTask?.Cancel();

                CurrentDriverSyncTask = null;
            }, 0, false, 0);

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

                var fSpeed = vData.ForcedSpeed;

                if (fSpeed != 0f)
                {
                    var lms = Player.LocalPlayer.Vehicle == vData.Vehicle ? Player.LocalPlayer.HasData("ColshapeVehicleSpeedLimited") ? Player.LocalPlayer.GetData<float>("ColshapeVehicleSpeedLimited") : float.MinValue : float.MinValue;

                    if (lms >= 0f)
                    {
                        if (fSpeed > 0f)
                            veh.SetForwardSpeed(fSpeed > lms ? lms : fSpeed);
                        else
                            veh.SetForwardSpeed(-fSpeed > lms ? -lms : fSpeed);
                    }
                    else
                    {
                        veh.SetForwardSpeed(fSpeed);
                    }
                }
            }
        }

        public static void ShowContainer(Vehicle veh)
        {
            var vData = GetData(veh);

            if (vData == null)
                return;

            if (Player.LocalPlayer.Vehicle != null)
                return;

            if (vData.TID == 0)
            {
                CEF.Notification.ShowError(Locale.Get("VEHICLE_TRUNK_E_0"));

                return;
            }

            CEF.Inventory.Show(Inventory.Types.Container, vData.TID);
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
                CEF.Notification.ShowError(Locale.Notifications.Vehicles.NoPlate);

                return;
            }

            Events.CallRemote("Vehicles::TakePlate", veh);
        }

        public static async void SetupPlate(Vehicle veh)
        {
            var vData = GetData(veh);

            if (vData == null)
                return;

            if (Player.LocalPlayer.Vehicle != null)
                return;

            var plateText = veh.GetNumberplateText();

            if (plateText != null && plateText.Length > 0)
            {
                CEF.Notification.ShowError(Locale.Notifications.Vehicles.PlateExists);

                return;
            }

            var allNumberplates = new List<(decimal, string)>();

            for (int i = 0; i < CEF.Inventory.ItemsParams.Length; i++)
            {
                if (CEF.Inventory.ItemsParams[i] == null)
                    continue;

                if (Data.Items.GetType(CEF.Inventory.ItemsParams[i].Id, false) == typeof(Data.Items.Numberplate))
                {
                    var name = ((string)(((object[])CEF.Inventory.ItemsData[i][0])[1])).Split(' ');

                    allNumberplates.Add((i, name[name.Length - 1]));
                }
            }

            if (allNumberplates.Count == 0)
            {
                CEF.Notification.Show("Inventory::NoItem");
            }
            else if (allNumberplates.Count == 1)
            {
                Events.CallRemote("Vehicles::SetupPlate", veh, allNumberplates[0].Item1);
            }
            else
            {
                await CEF.ActionBox.ShowSelect
                (
                    "NumberplateSelect", Locale.Actions.NumberplateSelectHeader, allNumberplates.ToArray(), null, null,

                    CEF.ActionBox.DefaultBindAction,

                    (rType, id) =>
                    {
                        if (rType == CEF.ActionBox.ReplyTypes.OK)
                        {
                            Events.CallRemote("Vehicles::SetupPlate", veh, id);
                        }

                        CEF.ActionBox.Close(true);
                    },

                    null
                );
            }

            //Events.CallRemote("Vehicles::SetupPlate", veh, 0);
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
                    if (vData.Data.Type != Data.Vehicles.Vehicle.Types.Plane)
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
                blip.SetRoute(true);

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

            LastCruiseControlToggled = Sync.World.ServerTime;
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
                CEF.Notification.ShowError(Locale.Notifications.Commands.Teleport.NoWaypoint);

                return;
            }

            Sync.Offers.Request(driver, Offers.Types.WaypointShare, new { X = wpPos.X, Y = wpPos.Y, });
        }

        public static async void BoatFromTrailerToWater(Vehicle veh)
        {
            var vData = GetData(veh);

            if (vData == null)
                return;

            if (vData.Data.Type != Data.Vehicles.Vehicle.Types.Boat)
                return;

            if (vData.IsAttachedToLocalTrailer is Vehicle trVeh)
            {
                await CEF.ActionBox.ShowSelect
                (
                    "BoatFromTrailerSelect", "Снять лодку с прицепа", new (decimal Id, string Text)[] { (0, "На воду"), (1, "На землю") }, null, null,

                    CEF.ActionBox.DefaultBindAction,

                    (rType, id) =>
                    {
                        if (rType == ActionBox.ReplyTypes.OK)
                        {
                            if (id == 0)
                            {
                                if (!veh.IsInWater() && !trVeh.IsInWater())
                                {
                                    var wPos = Utils.FindEntityWaterIntersectionCoord(veh, new Vector3(0f, 0f, 2.5f), 10f, 1.5f, -7.5f, 45f, 0.5f, 31);

                                    if (wPos != null)
                                    {
                                        Events.CallRemote("Vehicles::BTOW", veh, wPos.X, wPos.Y, wPos.Z);
                                    }
                                    else
                                    {
                                        CEF.Notification.ShowError(Locale.Notifications.Vehicles.BoatTrailerNotNearWater);

                                        return;
                                    }
                                }
                                else
                                {
                                    Events.CallRemote("Vehicles::BTOW", veh, float.MaxValue, float.MaxValue, float.MaxValue);
                                }
                            }
                            else if (id == 1)
                            {
                                Events.CallRemote("Vehicles::BTOW", veh, float.MaxValue, float.MaxValue, float.MaxValue);
                            }

                            CEF.ActionBox.Close(true);
                        }
                        else
                        {
                            CEF.ActionBox.Close(true);
                        }
                    },

                    null
                );
            }
            else
            {
                Events.CallRemote("Vehicles::BTOT", veh);

                return;
            }
        }

        public static void SetColshapeVehicleMaxSpeed(Vehicle veh, float maxSpeed)
        {
            veh.SetMaxSpeed(maxSpeed);

            var vData = GetData(veh);

            if (vData == null)
                return;
        }

        public static async void LookHood(Vehicle veh)
        {
            if (veh?.Exists != true || veh.IsLocal)
                return;

            var vData = GetData(veh);

            if (vData == null)
                return;

            if (!(bool)await Events.CallRemoteProc("Vehicles::HVIL", veh))
                return;

            vData = GetData(veh);

            if (vData == null)
                return;

            var vDataData = vData.Data;

            CEF.Estate.ShowVehicleInfo(vDataData.ID, vData.VID, veh.GetMod(11), vData.HasTurboTuning, true);
        }

        public static void FixVehicle(Vehicle vehicle)
        {
            if (!vehicle.IsDamaged() && vehicle.GetEngineHealth() >= 1000f && vehicle.GetBodyHealth() >= 1000f)
            {
                CEF.Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Notifications.Vehicles.VehicleIsNotDamagedFixError);
            }
            else
            {
                Events.CallRemote("Vehicles::Fix", vehicle);
            }
        }
    }
}
