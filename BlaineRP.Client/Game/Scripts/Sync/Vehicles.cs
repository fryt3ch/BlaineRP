using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Components;
using BlaineRP.Client.Game.EntitiesData.Enums;
using BlaineRP.Client.Game.Estates;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Items;
using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Game.Management.Misc;
using BlaineRP.Client.Game.Management.Radio.Enums;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Scripts.Sync
{
    [Script(int.MaxValue)]
    public class Vehicles
    {
        private static DateTime _lastBeltToggled;
        private static DateTime _lastDoorsLockToggled;
        private static DateTime _lastEngineToggled;
        private static DateTime _lastIndicatorToggled;
        private static DateTime _lastLightsToggled;
        private static DateTime _lastCruiseControlToggled;
        private static DateTime _lastSeatBeltShowed;

        public static DateTime LastRadioSent;

        private static DateTime _lastVehicleExitedTime;

        private static AsyncTask _currentDriverSyncTask;

        public static readonly List<Vehicle> ControlledVehicles = new List<Vehicle>();

        private static readonly Dictionary<string, Action<VehicleData, object, object>> _dataActions = new Dictionary<string, Action<VehicleData, object, object>>();

        public Vehicles()
        {
            RAGE.Game.Vehicle.DefaultEngineBehaviour = false;
            RAGE.Game.Vehicle.RepairOnExtraToggle = false;

            //Player.LocalPlayer.SetConfigFlag(184, true);

            Main.Update += ControlledTick;

            Main.Render += () =>
            {
                float screenX = 0f, screenY = 0f;

                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                foreach (Vehicle x in Utils.Game.Misc.GetVehiclesOnScreen(5))
                {
                    var data = VehicleData.GetData(x);

                    if (data == null)
                        continue;

                    Vector3 pos = x.GetRealPosition();

                    if (Vector3.Distance(pos, Player.LocalPlayer.Position) > 10f)
                        continue;

                    if (!Graphics.GetScreenCoordFromWorldCoord(pos, ref screenX, ref screenY))
                        continue;

                    if (Settings.User.Other.DebugLabels)
                    {
                        if (pData.AdminLevel > -1)
                        {
                            Graphics.DrawText($"ID: {x.RemoteId} | VID: {data.VID} | TID: {(data.TID == 0 ? "null" : data.TID.ToString())}",
                                screenX,
                                screenY += NameTags.Interval / 2f,
                                255,
                                255,
                                255,
                                255,
                                0.4f,
                                RAGE.Game.Font.ChaletComprimeCologne,
                                true
                            );
                            Graphics.DrawText($"EngineOn: {data.EngineOn} | Locked: {data.DoorsLocked} | TrunkLocked: {data.TrunkLocked}",
                                screenX,
                                screenY += NameTags.Interval / 2f,
                                255,
                                255,
                                255,
                                255,
                                0.4f,
                                RAGE.Game.Font.ChaletComprimeCologne,
                                true
                            );
                            Graphics.DrawText($"Fuel: {data.FuelLevel.ToString("0.00")} | Mileage: {data.Mileage.ToString("0.00")}",
                                screenX,
                                screenY += NameTags.Interval / 2f,
                                255,
                                255,
                                255,
                                255,
                                0.4f,
                                RAGE.Game.Font.ChaletComprimeCologne,
                                true
                            );
                            Graphics.DrawText($"EHP: {x.GetEngineHealth()} | BHP: {x.GetBodyHealth()} | IsInvincible: {data.IsInvincible}",
                                screenX,
                                screenY += NameTags.Interval / 2f,
                                255,
                                255,
                                255,
                                255,
                                0.4f,
                                RAGE.Game.Font.ChaletComprimeCologne,
                                true
                            );
                            Graphics.DrawText($"Speed: {x.GetSpeedKm().ToString("0.00")} | ForcedSpeed: {(data.ForcedSpeed * 3.6f).ToString("0.00")}",
                                screenX,
                                screenY += NameTags.Interval / 2f,
                                255,
                                255,
                                255,
                                255,
                                0.4f,
                                RAGE.Game.Font.ChaletComprimeCologne,
                                true
                            );
                        }
                        else
                        {
                            Graphics.DrawText($"ID: {x.RemoteId} | VID: {data.VID}",
                                screenX,
                                screenY += NameTags.Interval / 2f,
                                255,
                                255,
                                255,
                                255,
                                0.4f,
                                RAGE.Game.Font.ChaletComprimeCologne,
                                true
                            );
                            Graphics.DrawText($"EngineHP: {x.GetEngineHealth()}",
                                screenX,
                                screenY += NameTags.Interval / 2f,
                                255,
                                255,
                                255,
                                255,
                                0.4f,
                                RAGE.Game.Font.ChaletComprimeCologne,
                                true
                            );
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

                    while ((data = VehicleData.GetData(veh)) == null)
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
                        veh.SetDoorShut(5, false);
                    else
                        veh.SetDoorOpen(5, false, false);

                    if (data.HoodLocked)
                        veh.SetDoorShut(4, false);
                    else
                        veh.SetDoorOpen(4, false, false);
                }
            };

            #region Leave/Enter Vehicle Events

            Events.OnPlayerLeaveVehicle += (Vehicle vehicle, int seatId) =>
            {
                HUD.SwitchSpeedometer(false);

                if (vehicle?.Exists != true)
                    return;

                var data = VehicleData.GetData(vehicle);

                if (data == null)
                    return;

                InvokeHandler("Engine::On", data, data.EngineOn, null);

                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (RentedVehicle.All.Where(x => x.RemoteId == vehicle.RemoteId).FirstOrDefault() is RentedVehicle rVehData)
                    rVehData.ShowTimeLeftNotification();
            };

            Events.OnPlayerEnterVehicle += async (Vehicle vehicle, int seatId) =>
            {
                if (vehicle?.Exists != true)
                    return;

                Main.Render -= InVehicleRender;
                Main.Render += InVehicleRender;

                NPCs.NPC.CurrentNPC?.SwitchDialogue(false);

                var data = VehicleData.GetData(vehicle);

                if (!vehicle.IsLocal)
                {
                    while (data == null)
                    {
                        await RAGE.Game.Invoker.WaitAsync(25);

                        data = VehicleData.GetData(vehicle);

                        if (Player.LocalPlayer.Vehicle == null || Player.LocalPlayer.Vehicle.Handle != vehicle?.Handle)
                            return;
                    }

                    InvokeHandler("Engine::On", data, data.EngineOn, null);

                    Management.Radio.Core.SetCurrentRadioStationType(data.Radio);
                }
                else
                {
                    vehicle.SetEngineOn(true, true, true);
                    vehicle.SetJetEngineOn(true);

                    vehicle.SetLights(true);

                    if (seatId == -1 || seatId == 0)
                        HUD.SwitchSpeedometer(true);
                    else
                        HUD.SwitchSpeedometer(false);
                }

                await RAGE.Game.Invoker.WaitAsync(250);

                Players.UpdateHat(Player.LocalPlayer);
            };

            Events.OnPlayerStartEnterVehicle += (Vehicle vehicle, int seatId, Events.CancelEventArgs cancel) =>
            {
                if (vehicle?.Exists != true)
                    return;

                if (!vehicle.IsLocal)
                {
                    var data = VehicleData.GetData(vehicle);

                    if (data == null)
                    {
                        cancel.Cancel = true;

                        return;
                    }
                }
            };

            #endregion

            #region Events

            AddDataHandler("IPCO",
                (vData, value, oldValue) =>
                {
                    Vehicle veh = vData.Vehicle;

                    bool state = (bool?)value ?? false;

                    if (!veh.HasLandingGear())
                        return;

                    veh.ControlLandingGear(state ? 1 : 0);
                }
            );

            AddDataHandler("IsInvincible",
                (vData, value, oldValue) =>
                {
                    Vehicle veh = vData.Vehicle;

                    bool state = (bool?)value ?? false;

                    veh.SetInvincible(state);
                    veh.SetCanBeDamaged(!state);

                    veh.SetWheelsCanBreak(!state);
                }
            );

            AddDataHandler("ForcedSpeed",
                (vData, value, oldValue) =>
                {
                    Vehicle veh = vData.Vehicle;

                    float fSpeed = (float?)value ?? 0f;

                    if (veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                        return;

                    if (fSpeed >= 8.3f)
                    {
                        ToggleAutoPilot(false);

                        Main.Update -= CruiseControlTick;
                        Main.Update += CruiseControlTick;

                        Notification.Show(Notification.Types.Information, Locale.Notifications.Vehicles.Additional.HeaderCruise, Locale.Notifications.Vehicles.Additional.On);
                    }
                    else if (oldValue != null && (float)oldValue >= 8.3f)
                    {
                        Main.Update -= CruiseControlTick;

                        Notification.Show(Notification.Types.Information, Locale.Notifications.Vehicles.Additional.HeaderCruise, Locale.Notifications.Vehicles.Additional.Off);
                    }
                }
            );

            AddDataHandler("Engine::On",
                (vData, value, oldValue) =>
                {
                    Vehicle veh = vData.Vehicle;

                    bool state = value as bool? ?? false;

                    veh.SetEngineOn(state, true, true);
                    veh.SetJetEngineOn(state);

                    veh.SetLights(state ? vData.LightsOn ? 2 : 1 : 1);

                    if (Player.LocalPlayer.Vehicle?.Handle == veh.Handle)
                        HUD.SwitchEngineIcon(state);
                }
            );

            AddDataHandler("Doors::Locked",
                (vData, value, oldValue) =>
                {
                    Vehicle veh = vData.Vehicle;

                    bool state = value as bool? ?? false;

                    if (state)
                        /*                    veh.SetDoorsLocked(2);
                                            veh.SetDoorsLockedForAllPlayers(true);
    
                                            veh.SetDoorsLockedForPlayer(Player.LocalPlayer.Handle, true);*/
                            RAGE.Game.Audio.PlaySoundFromEntity(-1, "Remote_Control_Close", veh.Handle, "PI_Menu_Sounds", true, 0);
                        else
                            /*                    veh.SetDoorsLocked(1);
                                                veh.SetDoorsLockedForAllPlayers(false);
        
                                                veh.SetDoorsLockedForPlayer(Player.LocalPlayer.Handle, false);*/
                                RAGE.Game.Audio.PlaySoundFromEntity(-1, "Remote_Control_Open", veh.Handle, "PI_Menu_Sounds", true, 0);

                            if (Player.LocalPlayer.Vehicle != null && Player.LocalPlayer.Vehicle == veh)
                                HUD.SwitchDoorsIcon(state);
                            }
                            );

                            AddDataHandler("Inds",
                                (vData, value, oldValue) =>
                                {
                                    Vehicle veh = vData.Vehicle;

                                    var state = Utils.Convert.ToByte(value);

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
                                }
                            );

                            AddDataHandler("Lights::On",
                                (vData, value, oldValue) =>
                                {
                                    Vehicle veh = vData.Vehicle;

                                    bool state = value as bool? ?? false;

                                    if (state)
                                        veh.SetLights(2);
                                    else
                                        veh.SetLights(1);

                                    if (Player.LocalPlayer.Vehicle?.Handle == veh.Handle)
                                        HUD.SwitchLightsIcon(state);
                                }
                            );

                            AddDataHandler("Radio",
                                (vData, value, oldValue) =>
                                {
                                    Vehicle veh = vData.Vehicle;

                                    RadioStationTypes statinTypeNum = value == null ? RadioStationTypes.Off : (RadioStationTypes)(int)value;

                                    Management.Radio.Core.SetVehicleRadioStation(veh, statinTypeNum);
                                }
                            );

                            AddDataHandler("Trunk::Locked",
                                (vData, value, oldValue) =>
                                {
                                    Vehicle veh = vData.Vehicle;

                                    var state = (bool)value;

                                    if (state)
                                        veh.SetDoorShut(5, false);
                                    else
                                        veh.SetDoorOpen(5, false, false);
                                }
                            );

                            AddDataHandler("Hood::Locked",
                                (vData, value, oldValue) =>
                                {
                                    Vehicle veh = vData.Vehicle;

                                    var state = (bool)value;

                                    if (state)
                                        veh.SetDoorShut(4, false);
                                    else
                                        veh.SetDoorOpen(4, false, false);
                                }
                            );

                            AddDataHandler("Mods::TSColour",
                                (vData, value, oldValue) =>
                                {
                                    Vehicle veh = vData.Vehicle;

                                    Colour colour = ((JObject)value)?.ToObject<Colour>();

                                    if (colour == null)
                                    {
                                        veh.ToggleMod(20, false);
                                    }
                                    else
                                    {
                                        veh.ToggleMod(20, true);

                                        veh.SetTyreSmokeColor(colour.Red, colour.Green, colour.Blue);
                                    }
                                }
                            );

                            AddDataHandler("Mods::Turbo",
                                (vData, value, oldValue) =>
                                {
                                    Vehicle veh = vData.Vehicle;

                                    bool state = value is bool valueBool ? valueBool : false;

                                    veh.ToggleMod(18, state);
                                }
                            );

                            AddDataHandler("Mods::Xenon",
                                (vData, value, oldValue) =>
                                {
                                    Vehicle veh = vData.Vehicle;

                                    var colour = (int?)value;
                                    veh.SetXenonColour(colour < -1 ? null : colour);
                                }
                            );

                            AddDataHandler("Mods::CT",
                                (vData, value, oldValue) =>
                                {
                                    Vehicle veh = vData.Vehicle;

                                    var colour = (int)value;
                                    veh.SetColourType(colour);
                                }
                            );

                            AddDataHandler("Anchor",
                                (vData, value, oldValue) =>
                                {
                                    Vehicle veh = vData.Vehicle;

                                    bool state = (bool?)value ?? false;

                                    RAGE.Game.Invoker.Invoke(0xE3EBAAE484798530, veh.Handle, state);

                                    veh.SetBoatAnchor(state);
                                }
                            );

                            AddDataHandler("IsFrozen",
                                (vData, value, oldValue) =>
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
                                }
                            );

                            AddDataHandler("DirtLevel",
                                (vData, value, oldValue) =>
                                {
                                    vData.Vehicle.SetDirtLevel(Utils.Convert.ToSingle(value));
                                }
                            );

                            #endregion

                            Events.Add("Vehicles::Garage::SlotsMenu",
                                async (args) =>
                                {
                                    if (Utils.Misc.IsAnyCefActive(true))
                                        return;

                                    List<int> freeSlots = ((JArray)args[0]).ToObject<List<int>>();

                                    freeSlots.Insert(0, int.MinValue + freeSlots[new Random().Next(0, freeSlots.Count)]);

                                    await ActionBox.ShowSelect("GarageVehiclePlaceSelect",
                                        Locale.Actions.GarageVehicleSlotSelectHeader,
                                        freeSlots.Select(x => ((decimal)x, x < 0 ? "Случайное место" : $"Место #{x + 1}")).ToArray(),
                                        null,
                                        null,
                                        ActionBox.DefaultBindAction,
                                        (rType, idD) =>
                                        {
                                            var id = (int)idD;

                                            if (rType == ActionBox.ReplyTypes.OK)
                                            {
                                                var vehicle = Management.Interaction.CurrentEntity as Vehicle;

                                                if (vehicle == null)
                                                    return;

                                                ActionBox.Close(true);

                                                if (id < 0)
                                                    id = int.MinValue + id;

                                                Park(vehicle, id);
                                            }
                                            else if (rType == ActionBox.ReplyTypes.Cancel)
                                            {
                                                ActionBox.Close(true);
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        },
                                        null
                                    );
                                }
                            );

                            Events.Add("Vehicles::Fuel",
                                (args) =>
                                {
                                    Vehicle veh = Player.LocalPlayer.Vehicle;

                                    if (veh == null)
                                        return;

                                    var vData = VehicleData.GetData(veh);

                                    if (vData == null)
                                        return;

                                    vData.FuelLevel = (float)args[0];
                                }
                            );

                            Events.Add("Vehicles::Mileage",
                                (args) =>
                                {
                                    Vehicle veh = Player.LocalPlayer.Vehicle;

                                    if (veh == null)
                                        return;

                                    var vData = VehicleData.GetData(veh);

                                    if (vData == null)
                                        return;

                                    vData.Mileage = (float)args[0];
                                }
                            );

                            Events.Add("Vehicles::Enter",
                                async (args) =>
                                {
                                    Vehicle veh = Player.LocalPlayer.Vehicle;

                                    if (veh == null)
                                        return;

                                    veh.SetData("Fuel", (float)args[0]);
                                    veh.SetData("Mileage", (float)args[1]);
                                }
                            );

                            Events.Add("Vehicles::Fix",
                                (args) =>
                                {
                                    var veh = (Vehicle)args[0];

                                    veh.SetData("LastHealth", 1000f);

                                    veh.SetBodyHealth(1000f);

                                    veh.SetEngineHealth(1000f);

                                    veh.SetFixed();
                                    veh.SetDeformationFixed();
                                }
                            );

                            Events.Add("Vehicles::FixV",
                                (args) =>
                                {
                                    var veh = (Vehicle)args[0];

                                    veh.SetBodyHealth(1000f);

                                    veh.SetFixed();
                                    veh.SetDeformationFixed();
                                }
                            );

                            Events.Add("Vehicles::WTS",
                                (args) =>
                                {
                                    Vehicle veh = Entities.Vehicles.GetAtRemote((ushort)(int)args[0]);
                                    int seat = (int)args[1] - 1;

                                    var timeout = (int)args[2];

                                    AsyncTask.Methods.CancelPendingTask("Vehicles::WTS");

                                    AsyncTask task = null;

                                    task = new AsyncTask(async () =>
                                        {
                                            DateTime time = World.Core.ServerTime;

                                            while (AsyncTask.Methods.IsTaskStillPending("Vehicles::WTS", task) && World.Core.ServerTime.Subtract(time).TotalMilliseconds <= timeout)
                                            {
                                                await RAGE.Game.Invoker.WaitAsync(50);

                                                if (!AsyncTask.Methods.IsTaskStillPending("Vehicles::WTS", task))
                                                    return;

                                                if (SkyCamera.IsFadedOut || veh?.Exists != true)
                                                    continue;

                                                if (Player.LocalPlayer.Vehicle == veh)
                                                {
                                                    AsyncTask.Methods.CancelPendingTask("Vehicles::WTS");

                                                    return;
                                                }

                                                if (veh.IsSeatFree(seat, 0))
                                                    Player.LocalPlayer.SetIntoVehicle(veh.Handle, seat);
                                            }

                                            AsyncTask.Methods.CancelPendingTask("Vehicles::WTS");
                                        },
                                        25,
                                        false,
                                        0
                                    );

                                    AsyncTask.Methods.SetAsPending(task, "Vehicles::WTS");
                                }
                            );

                            Events.Add("Vehicles::JVRO",
                                async (args) =>
                                {
                                    var rentPrice = Utils.Convert.ToDecimal(args[0]);

                                    Vehicle vehicle = Player.LocalPlayer.Vehicle;

                                    if (vehicle?.Exists != true)
                                        return;

                                    var vData = VehicleData.GetData(vehicle);

                                    if (vData == null)
                                        return;

                                    await ActionBox.ShowMoney("JobVehicleRentMoney",
                                        Locale.Actions.JobVehicleRentTitle,
                                        string.Format(Locale.Actions.JobVehicleRentText,
                                            $"{vData.Data.Name} [{vehicle.GetNumberplateText() ?? "null"}]",
                                            Locale.Get("GEN_MONEY_0", rentPrice)
                                        ),
                                        () =>
                                        {
                                            ActionBox.DefaultBindAction.Invoke();

                                            var checkAction = new Action(() =>
                                                {
                                                    if (vehicle?.Exists != true ||
                                                        Player.LocalPlayer.Vehicle != vehicle ||
                                                        vehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                                        ActionBox.Close(true);
                                                }
                                            );

                                            Player.LocalPlayer.SetData("ActionBox::Temp::JVRVA", checkAction);

                                            Main.Update -= checkAction.Invoke;
                                            Main.Update += checkAction.Invoke;
                                        },
                                        async (rType) =>
                                        {
                                            if (ActionBox.LastSent.IsSpam(500, false, true))
                                                return;

                                            if (rType == ActionBox.ReplyTypes.OK || rType == ActionBox.ReplyTypes.Cancel)
                                            {
                                                ActionBox.LastSent = World.Core.ServerTime;

                                                var res = (int)await Events.CallRemoteProc("Vehicles::JVRS", rType == ActionBox.ReplyTypes.OK);

                                                if (res == byte.MaxValue)
                                                {
                                                    Player.LocalPlayer.SetData("ActionBox::Temp::JVRVA::DLV", true);

                                                    ActionBox.Close(true);
                                                }
                                                else
                                                {
                                                    if (res == 3)
                                                        return;

                                                    ActionBox.Close(true);

                                                    if (res == 1)
                                                        Notification.ShowError(Locale.Notifications.General.JobRentVehicleAlreadyRented0);
                                                    else if (res == 2)
                                                        Notification.ShowError(Locale.Notifications.General.JobRentVehicleAlreadyRented1);
                                                }
                                            }
                                            else if (rType == ActionBox.ReplyTypes.Additional1)
                                            {
                                                ActionBox.Close(true);
                                            }
                                        },
                                        () =>
                                        {
                                            Action checkAction = Player.LocalPlayer.GetData<Action>("ActionBox::Temp::JVRVA");

                                            if (checkAction != null)
                                            {
                                                Main.Update -= checkAction.Invoke;

                                                Player.LocalPlayer.ResetData("ActionBox::Temp::JVRVA");
                                            }

                                            if (!Player.LocalPlayer.HasData("ActionBox::Temp::JVRVA::DLV"))
                                            {
                                                if (Player.LocalPlayer.Vehicle == vehicle)
                                                    Player.LocalPlayer.TaskLeaveAnyVehicle(0, 0);
                                            }
                                            else
                                            {
                                                Player.LocalPlayer.ResetData("ActionBox::Temp::JVRVA::DLV");
                                            }
                                        }
                                    );
                                }
                            );
                            }

                            private static void InvokeHandler(string dataKey, VehicleData vData, object value, object oldValue = null)
                            {
                                _dataActions.GetValueOrDefault(dataKey)?.Invoke(vData, value, oldValue);
                            }

                            private static void AddDataHandler(string dataKey, Action<VehicleData, object, object> action)
                            {
                                Events.AddDataHandler(dataKey,
                                    (Entity entity, object value, object oldValue) =>
                                    {
                                        if (entity is Vehicle vehicle)
                                        {
                                            var data = VehicleData.GetData(vehicle);

                                            if (data == null)
                                                return;

                                            action.Invoke(data, value, oldValue);
                                        }
                                    }
                                );

                                _dataActions.Add(dataKey, action);
                            }

                            public static async System.Threading.Tasks.Task OnVehicleStreamIn(Vehicle veh)
                            {
                                Vector3 pos = veh.GetCoords(false);

                                RAGE.Game.Streaming.RequestCollisionAtCoord(pos.X, pos.Y, pos.Z);
                                RAGE.Game.Streaming.RequestAdditionalCollisionAtCoord(pos.X, pos.Y, pos.Z);

                                veh.SetLoadCollisionFlag(true, 0);
                                veh.TrackVisibility();

                                veh.SetUndriveable(false);

                                //veh.SetAutomaticallyAttaches(0, 0);

                                veh.SetDisablePetrolTankDamage(true);

                                veh.SetHandling("fWeaponDamageMult", 0.1f);

                                if (veh.IsLocal)
                                    /*                if (veh.GetData<Vehicle>("TrailerSync::Owner") is Vehicle trVeh && GetData(trVeh)?.IsFrozen == true)
                                                        veh.FreezePosition(true);*/
                                        return;

                                    var data = VehicleData.GetData(veh);

                                    if (data != null)
                                        data.Reset();

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
                                        veh.ControlLandingGear(data.IsPlaneChassisOff ? 3 : 2);

                                    if (data.TrunkLocked)
                                        veh.SetDoorShut(5, false);
                                    else
                                        veh.SetDoorOpen(5, false, false);

                                    if (data.HoodLocked)
                                        veh.SetDoorShut(4, false);
                                    else
                                        veh.SetDoorOpen(4, false, false);

                                    VehicleData.SetData(veh, data);
                                    }

                                    public static async System.Threading.Tasks.Task OnVehicleStreamOut(Vehicle veh)
                                    {
                                        var data = VehicleData.GetData(veh);

                                        if (data == null)
                                            return;

                                        data.Reset();
                                    }

                                    public static async void ToggleLandingGearState(Vehicle vehicle)
                                    {
                                        if (vehicle == null || vehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                            return;

                                        if (!vehicle.HasLandingGear())
                                            return;

                                        Data.Vehicles.Vehicle vType = Data.Vehicles.Core.GetByModel(vehicle.Model);

                                        if (vType?.Type != Data.Vehicles.VehicleTypes.Plane || vType.ID == "duster")
                                            return;

                                        if (vehicle.IsLocal)
                                        {
                                            int curGearState = vehicle.GetLandingGearState();

                                            vehicle.ControlLandingGear(curGearState == 1 || curGearState == 3 ? 0 : 1);
                                        }
                                        else
                                        {
                                            var vData = VehicleData.GetData(vehicle);

                                            if (vData == null)
                                                return;

                                            if (_lastCruiseControlToggled.IsSpam(1000, false, true))
                                                return;

                                            _lastCruiseControlToggled = World.Core.ServerTime;

                                            bool state = vData.IsPlaneChassisOff;

                                            state = !state;

                                            var res = (int)await Events.CallRemoteProc("Vehicles::SPSOS", state);

                                            if (res == 255)
                                                Notification.ShowSuccess(state ? Locale.Get("VEHICLE_LGEAR_OFF_S_0") : Locale.Get("VEHICLE_LGEAR_ON_S_0"));
                                            else if (res == 0)
                                                Notification.ShowErrorDefault();
                                        }
                                    }

                                    public static void StartDriverSync()
                                    {
                                        _currentDriverSyncTask?.Cancel();

                                        Vehicle veh = Player.LocalPlayer.Vehicle;

                                        if (veh?.Exists != true)
                                            return;

                                        var data = VehicleData.GetData(veh);

                                        if (data == null)
                                            return;

                                        Vector3 lastPos = veh.GetCoords(false);

                                        _currentDriverSyncTask = new AsyncTask(async () =>
                                            {
                                                while (true)
                                                {
                                                    veh = Player.LocalPlayer.Vehicle;

                                                    if (veh?.Exists != true || veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                                        break;

                                                    if (data.EngineOn)
                                                    {
                                                        Vector3 curPos = veh.GetCoords(false);

                                                        double dist = System.Math.Round(System.Math.Abs(Vector3.Distance(lastPos, curPos)), 2);
                                                        double fuelDiff = 0.001f * dist;
                                                        var dirtLevel = (byte)System.Math.Round(veh.GetDirtLevel());

                                                        if (dirtLevel > 15)
                                                            dirtLevel = 15;

                                                        if (fuelDiff > 0 || dist > 0)
                                                            Events.CallRemote("Vehicles::Sync", fuelDiff, dist, dirtLevel);

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

                                                    SetColshapeVehicleMaxSpeed(veh,
                                                        Player.LocalPlayer.HasData("ColshapeVehicleSpeedLimited")
                                                            ? Player.LocalPlayer.GetData<float>("ColshapeVehicleSpeedLimited")
                                                            : float.MinValue
                                                    );

                                                    await RAGE.Game.Invoker.WaitAsync(1_500);
                                                }

                                                _currentDriverSyncTask?.Cancel();

                                                _currentDriverSyncTask = null;
                                            },
                                            0,
                                            false,
                                            0
                                        );

                                        _currentDriverSyncTask.Run();
                                    }

                                    private static void ControlledTick()
                                    {
                                        for (var i = 0; i < ControlledVehicles.Count; i++)
                                        {
                                            Vehicle veh = ControlledVehicles[i];

                                            if (veh == null)
                                                continue;

                                            var vData = VehicleData.GetData(veh);

                                            float fSpeed = vData.ForcedSpeed;

                                            if (fSpeed != 0f)
                                            {
                                                float lms = Player.LocalPlayer.Vehicle == vData.Vehicle
                                                    ? Player.LocalPlayer.HasData("ColshapeVehicleSpeedLimited")
                                                        ? Player.LocalPlayer.GetData<float>("ColshapeVehicleSpeedLimited")
                                                        : float.MinValue
                                                    : float.MinValue;

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
                                        var vData = VehicleData.GetData(veh);

                                        if (vData == null)
                                            return;

                                        if (Player.LocalPlayer.Vehicle != null)
                                            return;

                                        if (vData.TID == 0)
                                        {
                                            Notification.ShowError(Locale.Get("VEHICLE_TRUNK_E_0"));

                                            return;
                                        }

                                        Inventory.Show(Inventory.Types.Container, vData.TID);
                                    }

                                    public static void TakePlate(Vehicle veh)
                                    {
                                        var vData = VehicleData.GetData(veh);

                                        if (vData == null)
                                            return;

                                        if (Player.LocalPlayer.Vehicle != null)
                                            return;

                                        string plateText = veh.GetNumberplateText();

                                        if (plateText == null || plateText.Length == 0)
                                        {
                                            Notification.ShowError(Locale.Notifications.Vehicles.NoPlate);

                                            return;
                                        }

                                        Events.CallRemote("Vehicles::TakePlate", veh);
                                    }

                                    public static async void SetupPlate(Vehicle veh)
                                    {
                                        var vData = VehicleData.GetData(veh);

                                        if (vData == null)
                                            return;

                                        if (Player.LocalPlayer.Vehicle != null)
                                            return;

                                        string plateText = veh.GetNumberplateText();

                                        if (plateText != null && plateText.Length > 0)
                                        {
                                            Notification.ShowError(Locale.Notifications.Vehicles.PlateExists);

                                            return;
                                        }

                                        var allNumberplates = new List<(decimal, string)>();

                                        for (var i = 0; i < Inventory.ItemsParams.Length; i++)
                                        {
                                            if (Inventory.ItemsParams[i] == null)
                                                continue;

                                            if (Items.Core.GetType(Inventory.ItemsParams[i].Id, false) == typeof(Numberplate))
                                            {
                                                string[] name = ((string)((object[])Inventory.ItemsData[i][0])[1]).Split(' ');

                                                allNumberplates.Add((i, name[name.Length - 1]));
                                            }
                                        }

                                        if (allNumberplates.Count == 0)
                                            Notification.Show("Inventory::NoItem");
                                        else if (allNumberplates.Count == 1)
                                            Events.CallRemote("Vehicles::SetupPlate", veh, allNumberplates[0].Item1);
                                        else
                                            await ActionBox.ShowSelect("NumberplateSelect",
                                                Locale.Actions.NumberplateSelectHeader,
                                                allNumberplates.ToArray(),
                                                null,
                                                null,
                                                ActionBox.DefaultBindAction,
                                                (rType, id) =>
                                                {
                                                    if (rType == ActionBox.ReplyTypes.OK)
                                                        Events.CallRemote("Vehicles::SetupPlate", veh, id);

                                                    ActionBox.Close(true);
                                                },
                                                null
                                            );

                                        //Events.CallRemote("Vehicles::SetupPlate", veh, 0);
                                    }

                                    public static void ToggleAutoPilot(bool? forceStatus = null, bool stopVehicle = false)
                                    {
                                        var pData = PlayerData.GetData(Player.LocalPlayer);
                                        if (pData == null)
                                            return;

                                        Main.Render -= AutoPilotTick;

                                        Vehicle veh = pData.AutoPilot;

                                        if (forceStatus == false || veh != null)
                                        {
                                            Player.LocalPlayer.ClearTasks();

                                            if (stopVehicle)
                                                veh?.TaskTempAction(27, 10000);

                                            pData.AutoPilot = null;

                                            ExtraBlip blip = Player.LocalPlayer.GetData<ExtraBlip>("AutoPilot::Blip");

                                            if (blip != null)
                                            {
                                                int wBlip = Utils.Game.Misc.GetWaypointBlip();

                                                if (wBlip > 0)
                                                    RAGE.Game.Ui.SetBlipRoute(wBlip, true);

                                                blip.Destroy();
                                            }

                                            Player.LocalPlayer.ResetData("AutoPilot::Blip");

                                            if (veh != null)
                                                Notification.Show(Notification.Types.Information,
                                                    Locale.Notifications.Vehicles.Additional.HeaderAutoPilot,
                                                    Locale.Notifications.Vehicles.Additional.Off
                                                );
                                        }
                                        else
                                        {
                                            veh = Player.LocalPlayer.Vehicle;

                                            if (veh == null)
                                                return;

                                            var vData = VehicleData.GetData(veh);

                                            if (vData == null)
                                                return;

                                            if (veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle || !veh.GetIsEngineRunning() || vData.ForcedSpeed != 0f)
                                                return;

                                            if (!vData.Data.HasAutoPilot)
                                            {
                                                if (vData.Data.Type != Data.Vehicles.VehicleTypes.Plane)
                                                    Notification.Show(Notification.Types.Error,
                                                        Locale.Notifications.Vehicles.Additional.HeaderAutoPilot,
                                                        Locale.Notifications.Vehicles.Additional.Unsupported
                                                    );

                                                return;
                                            }

                                            Vector3 pos = Main.WaypointPosition;

                                            if (pos == null)
                                            {
                                                Notification.Show(Notification.Types.Error,
                                                    Locale.Notifications.Vehicles.Additional.HeaderAutoPilot,
                                                    Locale.Notifications.Commands.Teleport.NoWaypoint
                                                );

                                                return;
                                            }

                                            RAGE.Game.Ui.ClearGpsPlayerWaypoint();

                                            var blip = new ExtraBlip(162, pos, null, 1f, 2, 255, 0f, false, 0, 0f, Player.LocalPlayer.Dimension, ExtraBlip.Types.AutoPilot);

                                            blip.SetAsReachable(7.5f);
                                            blip.SetRoute(true);

                                            Player.LocalPlayer.SetData("AutoPilot::Blip", blip);

                                            Player.LocalPlayer.TaskVehicleDriveToCoord(veh.Handle, pos.X, pos.Y, pos.Z, 30f, 1, 1, 2883621, 30f, 1f);

                                            pData.AutoPilot = veh;

                                            Main.Render += AutoPilotTick;

                                            Notification.Show(Notification.Types.Information,
                                                Locale.Notifications.Vehicles.Additional.HeaderAutoPilot,
                                                Locale.Notifications.Vehicles.Additional.On
                                            );

                                            blip.SetAsReachable();
                                        }
                                    }

                                    private static void AutoPilotTick()
                                    {
                                        Vehicle veh = Player.LocalPlayer.Vehicle;

                                        if (veh?.Exists != true ||
                                            veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle ||
                                            !veh.GetIsEngineRunning() ||
                                            veh.GetSharedData<float>("ForcedSpeed", 1f) != 0f)
                                        {
                                            ToggleAutoPilot(false, false);

                                            return;
                                        }

                                        if (Player.LocalPlayer.GetScriptTaskStatus(0x93A5526E) != 1)
                                        {
                                            ToggleAutoPilot(false, true);

                                            return;
                                        }

                                        if (RAGE.Game.Pad.IsControlJustPressed(32, 133) ||
                                            RAGE.Game.Pad.IsControlJustPressed(32, 134) ||
                                            RAGE.Game.Pad.IsControlJustPressed(32, 130) ||
                                            RAGE.Game.Pad.IsControlJustPressed(32, 129) ||
                                            RAGE.Game.Pad.IsControlJustPressed(32, 76))
                                        {
                                            Notification.Show(Notification.Types.Information,
                                                Locale.Notifications.Vehicles.Additional.HeaderAutoPilot,
                                                Locale.Notifications.Vehicles.Additional.Invtervention
                                            );

                                            ToggleAutoPilot(false, false);

                                            return;
                                        }
                                    }

                                    public static void ToggleAnchor()
                                    {
                                        if (_lastCruiseControlToggled.IsSpam(1000, false, false))
                                            return;

                                        Vehicle veh = Player.LocalPlayer.Vehicle;

                                        var vData = VehicleData.GetData(veh);

                                        if (vData == null)
                                            return;

                                        if (veh == null || veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle || vData.ForcedSpeed != 0f)
                                            return;

                                        if (vData.Data.Type != Data.Vehicles.VehicleTypes.Boat)
                                            return;

                                        Events.CallRemote("Vehicles::Anchor", !vData.IsAnchored);

                                        _lastCruiseControlToggled = World.Core.ServerTime;
                                    }

                                    public static void SendCoordsToDriver()
                                    {
                                        Vehicle veh = Player.LocalPlayer.Vehicle;

                                        if (veh?.Exists != true)
                                            return;

                                        var vData = VehicleData.GetData(veh);

                                        if (vData == null)
                                            return;

                                        int pHandle = veh.GetPedInSeat(-1, 0);

                                        if (pHandle == Player.LocalPlayer.Handle)
                                            return;

                                        Player driver = Utils.Game.Misc.GetPlayerByHandle(pHandle, true);

                                        if (driver?.Exists != true)
                                            return;

                                        Vector3 wpPos = Main.WaypointPosition;

                                        if (wpPos == null)
                                        {
                                            Notification.ShowError(Locale.Notifications.Commands.Teleport.NoWaypoint);

                                            return;
                                        }

                                        Offers.Request(driver,
                                            OfferTypes.WaypointShare,
                                            new
                                            {
                                                X = wpPos.X,
                                                Y = wpPos.Y,
                                            }
                                        );
                                    }

                                    public static async void BoatFromTrailerToWater(Vehicle veh)
                                    {
                                        var vData = VehicleData.GetData(veh);

                                        if (vData == null)
                                            return;

                                        if (vData.Data.Type != Data.Vehicles.VehicleTypes.Boat)
                                            return;

                                        if (vData.IsAttachedToLocalTrailer is Vehicle trVeh)
                                        {
                                            await ActionBox.ShowSelect("BoatFromTrailerSelect",
                                                "Снять лодку с прицепа",
                                                new (decimal Id, string Text)[]
                                                {
                                                    (0, "На воду"),
                                                    (1, "На землю"),
                                                },
                                                null,
                                                null,
                                                ActionBox.DefaultBindAction,
                                                (rType, id) =>
                                                {
                                                    if (rType == ActionBox.ReplyTypes.OK)
                                                    {
                                                        if (id == 0)
                                                        {
                                                            if (!veh.IsInWater() && !trVeh.IsInWater())
                                                            {
                                                                Vector3 wPos = Raycast.FindEntityWaterIntersectionCoord(veh,
                                                                    new Vector3(0f, 0f, 2.5f),
                                                                    10f,
                                                                    1.5f,
                                                                    -7.5f,
                                                                    45f,
                                                                    0.5f,
                                                                    31
                                                                );

                                                                if (wPos != null)
                                                                {
                                                                    Events.CallRemote("Vehicles::BTOW", veh, wPos.X, wPos.Y, wPos.Z);
                                                                }
                                                                else
                                                                {
                                                                    Notification.ShowError(Locale.Notifications.Vehicles.BoatTrailerNotNearWater);

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

                                                        ActionBox.Close(true);
                                                    }
                                                    else
                                                    {
                                                        ActionBox.Close(true);
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

                                        var vData = VehicleData.GetData(veh);

                                        if (vData == null)
                                            return;
                                    }

                                    public static async void LookHood(Vehicle veh)
                                    {
                                        if (veh?.Exists != true || veh.IsLocal)
                                            return;

                                        var vData = VehicleData.GetData(veh);

                                        if (vData == null)
                                            return;

                                        if (!(bool)await Events.CallRemoteProc("Vehicles::HVIL", veh))
                                            return;

                                        vData = VehicleData.GetData(veh);

                                        if (vData == null)
                                            return;

                                        Data.Vehicles.Vehicle vDataData = vData.Data;

                                        Estate.ShowVehicleInfo(vDataData.ID, vData.VID, veh.GetMod(11), vData.HasTurboTuning, true);
                                    }

                                    public static void FixVehicle(Vehicle vehicle)
                                    {
                                        if (!vehicle.IsDamaged() && vehicle.GetEngineHealth() >= 1000f && vehicle.GetBodyHealth() >= 1000f)
                                            Notification.Show(Notification.Types.Information,
                                                Locale.Get("NOTIFICATION_HEADER_DEF"),
                                                Locale.Notifications.Vehicles.VehicleIsNotDamagedFixError
                                            );
                                        else
                                            Events.CallRemote("Vehicles::Fix", vehicle);
                                    }

                                    public class RentedVehicle
                                    {
                                        public RentedVehicle(ushort RemoteId, Game.Data.Vehicles.Vehicle VehicleData)
                                        {
                                            this.RemoteId = RemoteId;
                                            this.VehicleData = VehicleData;

                                            TimeToDelete = Settings.App.Static.RENTED_VEHICLE_TIME_TO_AUTODELETE;

                                            TimeLeftToDelete = TimeToDelete;
                                        }

                                        public static List<RentedVehicle> All { get; set; } = new List<RentedVehicle>();

                                        public ushort RemoteId { get; set; }

                                        public Game.Data.Vehicles.Vehicle VehicleData { get; set; }

                                        public int TimeToDelete { get; set; }

                                        public int TimeLeftToDelete { get; set; }

                                        public void ShowTimeLeftNotification()
                                        {
                                            Notification.Show(Notification.Types.Information,
                                                Locale.Get("NOTIFICATION_HEADER_DEF"),
                                                string.Format(Locale.Notifications.Vehicles.RentedVehicleTimeLeft,
                                                    $"\"{VehicleData.Name}\"",
                                                    World.Core.ServerTime.AddMilliseconds(TimeLeftToDelete).Subtract(World.Core.ServerTime).GetBeautyString()
                                                )
                                            );
                                        }

                                        public static void Check()
                                        {
                                            Vehicle curVeh = Player.LocalPlayer.Vehicle;

                                            for (var i = 0; i < All.Count; i++)
                                            {
                                                RentedVehicle x = All[i];

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
                                                        if (x.TimeLeftToDelete % 60_000 == 0 ||
                                                            x.TimeLeftToDelete <= 5_000 ||
                                                            x.TimeLeftToDelete <= 30_000 && x.TimeLeftToDelete % 10_000 == 0)
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

                                    #region Handlers

                                    #region Cruise Control

                                    public static void ToggleCruiseControl(bool ignoreIf = false)
                                    {
                                        var pData = PlayerData.GetData(Player.LocalPlayer);

                                        if (pData == null)
                                            return;

                                        Vehicle vehicle = Player.LocalPlayer.Vehicle;

                                        if (!ignoreIf)
                                        {
                                            if (vehicle?.Exists != true ||
                                                vehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle ||
                                                !vehicle.GetIsEngineRunning() ||
                                                pData.AutoPilot != null)
                                                return;

                                            if (_lastCruiseControlToggled.IsSpam(1000, false, false))
                                                return;

                                            var vData = VehicleData.GetData(vehicle);

                                            if (vData == null || vData.IsAnchored)
                                                return;

                                            if (!vData.Data.HasCruiseControl)
                                            {
                                                Notification.Show(Notification.Types.Error,
                                                    Locale.Notifications.Vehicles.Additional.HeaderCruise,
                                                    Locale.Notifications.Vehicles.Additional.Unsupported
                                                );

                                                return;
                                            }

                                            Vector3 spVect = vehicle.GetSpeedVector(true);

                                            if (spVect.Y < 0)
                                            {
                                                Notification.Show(Notification.Types.Error,
                                                    Locale.Notifications.Vehicles.Additional.HeaderCruise,
                                                    Locale.Notifications.Vehicles.Additional.Reverse
                                                );

                                                return;
                                            }

                                            float speed = vehicle.GetSpeed();

                                            if (speed < Settings.App.Static.MIN_CRUISE_CONTROL_SPEED)
                                            {
                                                Notification.Show(Notification.Types.Error,
                                                    Locale.Notifications.Vehicles.Additional.HeaderCruise,
                                                    string.Format(Locale.Notifications.Vehicles.Additional.MinSpeed,
                                                        System.Math.Floor(Settings.App.Static.MIN_CRUISE_CONTROL_SPEED * 3.6f)
                                                    )
                                                );

                                                return;
                                            }
                                            else if (speed > Settings.App.Static.MAX_CRUISE_CONTROL_SPEED)
                                            {
                                                Notification.Show(Notification.Types.Error,
                                                    Locale.Notifications.Vehicles.Additional.HeaderCruise,
                                                    string.Format(Locale.Notifications.Vehicles.Additional.MaxSpeed,
                                                        System.Math.Floor(Settings.App.Static.MAX_CRUISE_CONTROL_SPEED * 3.6f)
                                                    )
                                                );

                                                return;
                                            }
                                        }

                                        Events.CallRemote("Players::ToggleCruiseControl", vehicle?.GetSpeed() ?? 0f);

                                        _lastCruiseControlToggled = World.Core.ServerTime;
                                    }

                                    public static void CruiseControlTick()
                                    {
                                        Vehicle veh = Player.LocalPlayer.Vehicle;

                                        if (veh?.Exists != true || !veh.GetIsEngineRunning() || veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                        {
                                            ToggleCruiseControl(true);

                                            Main.Update -= CruiseControlTick;

                                            return;
                                        }

                                        Vector3 rotVect = veh.GetRotationVelocity();

                                        if (veh.GetHeightAboveGround() > 1f || System.Math.Abs(rotVect.Z) > 1.5f)
                                        {
                                            Notification.Show(Notification.Types.Information,
                                                Locale.Notifications.Vehicles.Additional.HeaderCruise,
                                                Locale.Notifications.Vehicles.Additional.Danger,
                                                2500
                                            );

                                            ToggleCruiseControl(true);

                                            Main.Update -= CruiseControlTick;

                                            return;
                                        }

                                        if (veh.HasCollidedWithAnything())
                                        {
                                            Notification.Show(Notification.Types.Information,
                                                Locale.Notifications.Vehicles.Additional.HeaderCruise,
                                                Locale.Notifications.Vehicles.Additional.Collision,
                                                2500
                                            );

                                            ToggleCruiseControl(true);

                                            Main.Update -= CruiseControlTick;

                                            return;
                                        }

                                        if (RAGE.Game.Pad.IsControlJustPressed(32, 130) ||
                                            RAGE.Game.Pad.IsControlJustPressed(32, 129) ||
                                            RAGE.Game.Pad.IsControlJustPressed(32, 76))
                                        {
                                            Notification.Show(Notification.Types.Information,
                                                Locale.Notifications.Vehicles.Additional.HeaderCruise,
                                                Locale.Notifications.Vehicles.Additional.Invtervention,
                                                2500
                                            );

                                            ToggleCruiseControl(true);

                                            Main.Update -= CruiseControlTick;

                                            return;
                                        }
                                    }

                                    #endregion

                                    #region Belt

                                    public static void ToggleBelt(bool ignoreIf = false)
                                    {
                                        if (!ignoreIf)
                                        {
                                            Vehicle vehicle = Player.LocalPlayer.Vehicle;

                                            if (vehicle?.Exists != true || Data.Vehicles.Core.GetByModel(vehicle.Model)?.Type != Data.Vehicles.VehicleTypes.Car)
                                                return;

                                            if (_lastBeltToggled.IsSpam(1000, false, false))
                                                return;
                                        }

                                        _lastBeltToggled = World.Core.ServerTime;

                                        Events.CallRemote("Players::ToggleBelt");
                                    }

                                    public static void BeltTick()
                                    {
                                        RAGE.Game.Pad.DisableControlAction(32, 75, true);

                                        if (RAGE.Game.Pad.IsDisabledControlJustPressed(32, 75))
                                            if (World.Core.ServerTime.Subtract(_lastSeatBeltShowed).TotalMilliseconds > 500)
                                            {
                                                Notification.Show(Notification.Types.Information,
                                                    Locale.Notifications.Vehicles.SeatBelt.Header,
                                                    Locale.Notifications.Vehicles.SeatBelt.TakeOffToLeave
                                                );

                                                _lastSeatBeltShowed = World.Core.ServerTime;
                                            }

                                        if (Player.LocalPlayer.Vehicle?.Exists != true)
                                        {
                                            ToggleBelt(true);

                                            Main.Update -= BeltTick;
                                        }
                                    }

                                    #endregion

                                    public static async void Lock(bool? forceвState = null, Vehicle vehicle = null)
                                    {
                                        if (_lastDoorsLockToggled.IsSpam(500, false, true))
                                            return;

                                        _lastDoorsLockToggled = World.Core.ServerTime;

                                        if (vehicle == null)
                                        {
                                            vehicle = Player.LocalPlayer.Vehicle;

                                            if (vehicle?.Exists != true || vehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                                vehicle = Management.Interaction.CurrentEntity as Vehicle ??
                                                          Utils.Game.Vehicles.GetClosestVehicle(Player.LocalPlayer.Position, Settings.App.Static.EntityInteractionMaxDistance);
                                        }

                                        if (vehicle?.Exists != true)
                                            return;

                                        var vData = VehicleData.GetData(vehicle);

                                        if (vData == null)
                                            return;

                                        bool state = vData.DoorsLocked;

                                        if (forceвState != null)
                                        {
                                            if (forceвState == state)
                                            {
                                                Notification.ShowInfo(state ? Locale.Get("VEHICLE_DOORS_LOCKED_E_1") : Locale.Get("VEHICLE_DOORS_UNLOCKED_E_1"));

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
                                            Notification.ShowSuccess(state ? Locale.Get("VEHICLE_DOORS_LOCKED_S_0") : Locale.Get("VEHICLE_DOORS_UNLOCKED_S_0"));
                                        else if (res == 0)
                                            Notification.ShowErrorDefault();
                                    }

                                    public static async void ToggleEngine(Vehicle veh, bool? forcedState)
                                    {
                                        if (veh?.Exists != true)
                                            return;

                                        var vData = VehicleData.GetData(veh);

                                        if (vData == null)
                                            return;

                                        if (veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                            return;

                                        bool state = vData.EngineOn;

                                        if (forcedState != null)
                                        {
                                            if (forcedState == state)
                                            {
                                                Notification.ShowInfo(state ? Locale.Get("VEHICLE_ENGINE_ON_E_1") : Locale.Get("VEHICLE_ENGINE_OFF_E_1"));

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

                                        if (_lastEngineToggled.IsSpam(1000, false, true))
                                            return;

                                        _lastEngineToggled = World.Core.ServerTime;

                                        var res = (int)await Events.CallRemoteProc("Vehicles::ET", veh, (byte)(state ? 1 : 0));

                                        if (res == 255)
                                            Notification.ShowSuccess(state ? Locale.Get("VEHICLE_ENGINE_ON_S_0") : Locale.Get("VEHICLE_ENGINE_OFF_S_0"));
                                        else if (res == 5)
                                            Notification.ShowError(Locale.Get("VEHICLE_ENGINE_BROKEN_S_0"));
                                        else if (res == 6)
                                            Notification.ShowError(Locale.Get("VEHICLE_FUEL_OUTOF_S_0"));
                                    }

                                    public static async void ToggleIndicator(Vehicle veh, byte type) // 0 - right, 1 - left, 2 - both
                                    {
                                        if (veh?.Exists != true)
                                            return;

                                        var vData = VehicleData.GetData(veh);

                                        if (vData == null)
                                            return;

                                        if (_lastIndicatorToggled.IsSpam(500, false, false))
                                            return;

                                        int vehClass = veh.GetClass();

                                        // if cycle, boat, helicopter, plane
                                        if (vehClass == 13 || vehClass == 14 || vehClass == 15 || vehClass == 16)
                                            return;

                                        if (veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                            return;

                                        _lastIndicatorToggled = World.Core.ServerTime;

                                        byte state = vData.IndicatorsState;

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

                                        var vData = VehicleData.GetData(veh);

                                        if (vData == null)
                                            return;

                                        if (veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                            return;

                                        if (!veh.GetIsEngineRunning())
                                            return;

                                        if (_lastLightsToggled.IsSpam(500, false, false))
                                            return;

                                        int vehClass = veh.GetClass();

                                        // if cycle, boat, helicopter, plane
                                        if (vehClass == 13 || vehClass == 14 || vehClass == 15 || vehClass == 16)
                                            return;

                                        _lastLightsToggled = World.Core.ServerTime;

                                        bool state = vData.LightsOn;

                                        state = !state;

                                        var res = (int)await Events.CallRemoteProc("Vehicles::TLI", veh, state);
                                    }

                                    public static async void ToggleTrunkLock(bool? forcedState = null, Vehicle vehicle = null)
                                    {
                                        if (_lastDoorsLockToggled.IsSpam(1000, false, true))
                                            return;

                                        _lastDoorsLockToggled = World.Core.ServerTime;

                                        if (vehicle == null)
                                        {
                                            vehicle = Player.LocalPlayer.Vehicle;

                                            if (vehicle?.Exists != true || vehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                                vehicle = Management.Interaction.CurrentEntity as Vehicle ??
                                                          Utils.Game.Vehicles.GetClosestVehicle(Player.LocalPlayer.Position, Settings.App.Static.EntityInteractionMaxDistance);
                                        }

                                        if (vehicle?.Exists != true)
                                            return;

                                        var vData = VehicleData.GetData(vehicle);

                                        if (vData == null)
                                            return;

                                        bool state = vData.TrunkLocked;

                                        if (forcedState != null)
                                        {
                                            if (forcedState == state)
                                            {
                                                Notification.ShowInfo(state ? Locale.Get("VEHICLE_TRUNK_LOCKED_E_1") : Locale.Get("VEHICLE_TRUNK_UNLOCKED_E_1"));

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
                                            Notification.ShowSuccess(state ? Locale.Get("VEHICLE_TRUNK_LOCKED_S_0") : Locale.Get("VEHICLE_TRUNK_UNLOCKED_S_0"));
                                        else if (res == 0)
                                            Notification.ShowErrorDefault();
                                    }

                                    public static async void ToggleHoodLock(bool? forcedState = null, Vehicle vehicle = null)
                                    {
                                        if (_lastDoorsLockToggled.IsSpam(500, false, true))
                                            return;

                                        _lastDoorsLockToggled = World.Core.ServerTime;

                                        if (vehicle == null)
                                        {
                                            vehicle = Player.LocalPlayer.Vehicle;

                                            if (vehicle?.Exists != true || vehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                                vehicle = Management.Interaction.CurrentEntity as Vehicle ??
                                                          Utils.Game.Vehicles.GetClosestVehicle(Player.LocalPlayer.Position, Settings.App.Static.EntityInteractionMaxDistance);
                                        }

                                        if (vehicle?.Exists != true)
                                            return;

                                        var vData = VehicleData.GetData(vehicle);

                                        if (vData == null)
                                            return;

                                        bool state = vData.HoodLocked;

                                        if (forcedState != null)
                                        {
                                            if (forcedState == state)
                                            {
                                                Notification.ShowInfo(state ? Locale.Get("VEHICLE_HOOD_LOCKED_E_1") : Locale.Get("VEHICLE_HOOD_UNLOCKED_E_1"));

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
                                            Notification.ShowSuccess(state ? Locale.Get("VEHICLE_HOOD_LOCKED_S_0") : Locale.Get("VEHICLE_HOOD_UNLOCKED_S_0"));
                                        else if (res == 0)
                                            Notification.ShowErrorDefault();
                                    }

                                    #endregion

                                    #region Vehicle Menu Methods

                                    #region Shuffle Seat

                                    public static async void SeatTo(int seatId, Vehicle vehicle)
                                    {
                                        if (vehicle == null)
                                            return;

                                        if (Vector3.Distance(Player.LocalPlayer.Position, vehicle.Position) > Settings.App.Static.EntityInteractionMaxDistance)
                                            return;

                                        var data = PlayerData.GetData(Player.LocalPlayer);
                                        var vData = VehicleData.GetData(vehicle);

                                        if (data == null || vData == null)
                                            return;

                                        // to trunk
                                        if (seatId == int.MaxValue)
                                        {
                                            if (vehicle.DoesHaveDoor(5) > 0)
                                            {
                                                var res = (int)await Events.CallRemoteProc("Players::GoToTrunk", vehicle);

                                                if (res == 255)
                                                    return;
                                                else if (res == 0)
                                                    Notification.ShowErrorDefault();
                                                else if (res == 1)
                                                    Notification.ShowError(Locale.Get("VEHICLE_TRUNK_LOCKED_E_0"));
                                                else if (res == 2)
                                                    Notification.ShowError(Locale.Get("VEHICLE_TRUNK_E_2"));
                                            }
                                            else
                                            {
                                                Notification.ShowError(Locale.Get("VEHICLE_TRUNK_E_1"));
                                            }

                                            return;
                                        }

                                        int maxSeats = RAGE.Game.Vehicle.GetVehicleModelNumberOfSeats(vehicle.Model);

                                        if (maxSeats <= 0)
                                        {
                                            Notification.ShowErrorDefault();

                                            return;
                                        }

                                        if (seatId >= maxSeats)
                                            seatId = maxSeats - 1;

                                        if (vehicle.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle)
                                        {
                                            Notification.ShowError(Locale.Get("VEHICLE_SEAT_E_3"));

                                            return;
                                        }

                                        if (!vehicle.IsSeatFree(seatId - 1, 0))
                                        {
                                            Notification.ShowError(Locale.Get("VEHICLE_SEAT_E_2"));

                                            return;
                                        }

                                        if (Player.LocalPlayer.Vehicle == vehicle)
                                        {
                                            if (data.BeltOn)
                                            {
                                                Notification.Show(Notification.Types.Information,
                                                    Locale.Notifications.Vehicles.SeatBelt.Header,
                                                    Locale.Notifications.Vehicles.SeatBelt.TakeOffToSeat
                                                );

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
                                        var pData = PlayerData.GetData(Player.LocalPlayer);

                                        if (pData == null)
                                            return;

                                        if (Player.LocalPlayer.IsInAnyVehicle(false) || _lastVehicleExitedTime.IsSpam(1000, false, false))
                                            return;

                                        if (UI.CEF.Phone.Apps.Camera.IsActive)
                                            return;

                                        if (Player.LocalPlayer.GetScriptTaskStatus(2500551826) != 7)
                                        {
                                            Player.LocalPlayer.ClearTasks();

                                            return;
                                        }

                                        if (veh == null)
                                        {
                                            veh = Utils.Game.Vehicles.GetClosestVehicleToSeatIn(Player.LocalPlayer.Position,
                                                Settings.App.Static.EntityInteractionMaxDistance,
                                                seatId
                                            );

                                            if (veh == null)
                                                return;
                                        }

                                        if (PlayerActions.IsAnyActionActive(true,
                                                PlayerActions.Types.Knocked,
                                                PlayerActions.Types.Frozen,
                                                PlayerActions.Types.Cuffed,
                                                PlayerActions.Types.PushingVehicle,
                                                PlayerActions.Types.OtherAnimation,
                                                PlayerActions.Types.Animation,
                                                PlayerActions.Types.Scenario,
                                                PlayerActions.Types.FastAnimation,
                                                PlayerActions.Types.InVehicle,
                                                PlayerActions.Types.Shooting,
                                                PlayerActions.Types.Reloading,
                                                PlayerActions.Types.Climbing,
                                                PlayerActions.Types.Falling,
                                                PlayerActions.Types.Ragdoll,
                                                PlayerActions.Types.Jumping,
                                                PlayerActions.Types.NotOnFoot,
                                                PlayerActions.Types.IsSwimming,
                                                PlayerActions.Types.IsAttachedTo
                                            ))
                                            return;

                                        if (veh.IsDead(0))
                                            return;

                                        if (seatId < 0)
                                        {
                                            seatId = veh.GetFirstFreeSeatId(0);

                                            if (seatId < 0)
                                                return;
                                        }

                                        Player.LocalPlayer.SetData("TEV::V", veh);
                                        Player.LocalPlayer.SetData("TEV::S", seatId);
                                        Player.LocalPlayer.SetData("TEV::T", World.Core.ServerTime);
                                        Invoker.JsEval("mp.players.local.taskEnterVehicle", veh.Handle, -1, seatId - 1, 1.5f, 1, 0);

                                        Main.Render -= EnterVehicleRender;
                                        Main.Render += EnterVehicleRender;
                                    }

                                    private static void EnterVehicleRender()
                                    {
                                        int tStatus = Player.LocalPlayer.GetScriptTaskStatus(2500551826);

                                        int seatId = Player.LocalPlayer.GetData<int>("TEV::S");
                                        Vehicle veh = Player.LocalPlayer.GetData<Vehicle>("TEV::V");
                                        double timePassed = World.Core.ServerTime.Subtract(Player.LocalPlayer.GetData<DateTime>("TEV::T")).TotalMilliseconds;

                                        if (tStatus == 7 ||
                                            veh?.Exists != true ||
                                            veh.IsDead(0) ||
                                            Player.LocalPlayer.Position.DistanceTo(veh.Position) > Settings.App.Static.EntityInteractionMaxDistance ||
                                            timePassed > 7500 ||
                                            timePassed > 1_000 && Utils.Game.Misc.AnyOnFootMovingControlJustPressed() ||
                                            !veh.IsOnAllWheels() && SetIntoVehicle(veh, seatId))
                                        {
                                            if (tStatus != 7)
                                                Player.LocalPlayer.ClearTasks();

                                            Player.LocalPlayer.ResetData("TEV::V");
                                            Player.LocalPlayer.ResetData("TEV::S");
                                            Player.LocalPlayer.ResetData("TEV::T");

                                            Main.Render -= EnterVehicleRender;
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
                                            _lastVehicleExitedTime = World.Core.ServerTime;

                                            Main.Render -= InVehicleRender;
                                        }
                                    }

                                    #endregion

                                    #region Kick Passenger

                                    public static void KickPassenger(Player player)
                                    {
                                        Vehicle veh = Player.LocalPlayer.Vehicle;

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
                                            House house = Player.LocalPlayer.GetData<House>("CurrentHouse");

                                            if (house?.GarageType == null)
                                                return;

                                            if (slot < 0)
                                                Events.CallRemote("House::Garage::SlotsMenu", vehicle, house.Id);
                                            else
                                                Events.CallRemote("House::Garage::Vehicle", slot, vehicle, house.Id);
                                        }
                                        else if (Player.LocalPlayer.HasData("CurrentGarageRoot"))
                                        {
                                            GarageRoot gRoot = Player.LocalPlayer.GetData<GarageRoot>("CurrentGarageRoot");

                                            if (gRoot == null)
                                                return;

                                            if (slot < 0)
                                                Events.CallRemote("Garage::SlotsMenu", vehicle, gRoot.Id);
                                            else
                                                Events.CallRemote("Garage::Vehicle", slot, vehicle, gRoot.Id);
                                        }
                                        else
                                        {
                                            Notification.ShowError(Locale.Notifications.House.NotNearGarage);

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
                                            int handle = veh.GetPedInSeat(i, 0);

                                            if (handle <= 0)
                                                continue;

                                            Player player = Utils.Game.Misc.GetPlayerByHandle(handle, true);

                                            if (player == null)
                                                continue;

                                            players.Add(player);
                                        }

                                        return players;
                                    }

                                    #endregion
                                    }
                                    }