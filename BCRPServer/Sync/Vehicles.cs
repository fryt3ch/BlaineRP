using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCRPServer.Sync
{
    class Vehicles : Script
    {
        #region Player Enter Vehicle
        [ServerEvent(Event.PlayerEnterVehicle)]
        private static async Task PlayerEntered(Player player, Vehicle veh, sbyte seatId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var vData = veh.GetMainData();

                if (!await vData.WaitAsync())
                    return;

                await NAPI.Task.RunAsync(async () =>
                {
                    if (player?.Exists != true || veh?.Exists != true)
                        return;

                    if (pData.VehicleSeat != -1)
                        return;

                    if (vData.Locked || vData.Passengers[seatId]?.Exists == true)
                    {
                        player.WarpOutOfVehicle();

                        return;
                    }

                    vData.AddPassenger(seatId, pData);

                    if (vData.IsInvincible)
                        vData.IsInvincible = false;

                    var curWeapon = pData.ActiveWeapon;

                    if (curWeapon != null)
                    {
                        if (!curWeapon.Value.WeaponItem.Data.CanUseInVehicle)
                            await pData.InventoryAction(curWeapon.Value.Group, curWeapon.Value.Slot, 5);
                    }
                });

                vData.Release();
            });

            pData.Release();
        }
        #endregion

        #region Player Exit Vehicle
        [ServerEvent(Event.PlayerExitVehicle)]
        private static async Task PlayerExited(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var vData = veh.GetMainData();

                if (!await vData.WaitAsync())
                    return;

                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true || veh?.Exists != true)
                        return;

                    vData.RemovePassenger(pData.VehicleSeat);

                    if (!vData.IsInvincible && !vData.Passengers.Where(x => x?.Exists == true).Any())
                        vData.IsInvincible = true;
                });

                vData.Release();
            });

            pData.Release();
        }
        #endregion

        #region Engine
        [RemoteEvent("Vehicles::ToggleEngineSync")]
        private static async Task ToggleEngineRemote(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            Vehicle veh = player.Vehicle;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var vData = veh.GetMainData();

                if (!await vData.WaitAsync())
                    return;

                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true || veh?.Exists != true || pData.VehicleSeat != 0)
                        return;

                    ToggleEngine(pData, vData);
                });

                vData.Release();
            });

            pData.Release();
        }

        public static void ToggleEngine(PlayerData pData, VehicleData vData, bool? forceStatus = null)
        {
            var player = pData.Player;

            if (forceStatus != null)
            {
                if (forceStatus == vData.EngineOn)
                    return;

                if (forceStatus == true)
                {
                    if (vData.FuelLevel <= 0f)
                        return;
                }
                else
                    vData.EngineOn = false;
            }
            else
            {
                if (!vData.EngineOn)
                {
                    if (vData.FuelLevel > 0f)
                    {
                        vData.EngineOn = true;

                        if (player?.Exists == true)
                        {
                            Chat.SendLocal(Chat.Type.Me, player, Locale.Chat.Vehicle.EngineOn);
                            player.Notify("Engine::On");
                        }
                    }
                    else if (player?.Exists == true)
                    {
                        Chat.SendLocal(Chat.Type.Do, player, Locale.Chat.Vehicle.OutOfFuel);
                        player.Notify("Engine::OutOfFuel");
                    }
                }
                else
                {
                    vData.EngineOn = false;

                    if (player?.Exists == true)
                    {
                        Chat.SendLocal(Chat.Type.Me, player, Locale.Chat.Vehicle.EngineOff);
                        player.Notify("Engine::Off");
                    }
                }
            }
        }
        #endregion

        #region Doors Lock
        [RemoteEvent("Vehicles::ToggleDoorsLockSync")]
        public static async Task ToggleDoorsLock(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var vData = veh.GetMainData();

                if (!await vData.WaitAsync())
                    return;

                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true || veh?.Exists != true || (player.Vehicle != veh && !player.AreEntitiesNearby(veh, Settings.ENTITY_INTERACTION_MAX_DISTANCE)))
                        return;

                    if (vData.IsOwner(pData) == null)
                        return;

                    bool newState = !vData.Locked;

                    if (player.Vehicle == null && !pData.AnyAnimActive() && pData.ActiveWeapon == null)
                    {
                        player.AttachObject("veh_key", AttachSystem.Types.VehKey, 1250);

                        pData.PlayAnim(Animations.FastTypes.VehLocking);
                    }

                    if (newState)
                    {
                        Chat.SendLocal(Chat.Type.Me, player, Locale.Chat.Vehicle.Locked);
                        player.Notify("Doors::Locked");
                    }
                    else
                    {
                        Chat.SendLocal(Chat.Type.Me, player, Locale.Chat.Vehicle.Unlocked);
                        player.Notify("Doors::Unlocked");
                    }

                    vData.Locked = newState;
                });

                vData.Release();
            });

            pData.Release();
        }
        #endregion

        #region Indicators + Lights
        [RemoteEvent("Vehicles::ToggleIndicator")]
        public static async Task ToggleIndicator(Player player, int type)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var veh = player.Vehicle;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var vData = veh.GetMainData();

                if (!await vData.WaitAsync())
                    return;

                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true || veh?.Exists != true || pData.VehicleSeat != 0 || type < 0 || type > 2 || (!Utils.IsCar(player.Vehicle) && !Utils.IsBike(player.Vehicle)))
                        return;

                    bool leftOn = vData.LeftIndicatorOn;
                    bool rightOn = vData.RightIndicatorOn;

                    if (type != 2)
                    {
                        if (type == 1)
                        {
                            if (rightOn)
                                vData.RightIndicatorOn = false;

                            if (!(rightOn && leftOn))
                                vData.LeftIndicatorOn = !leftOn;
                        }
                        else
                        {
                            if (leftOn)
                                vData.LeftIndicatorOn = false;

                            if (!(rightOn && leftOn))
                                vData.RightIndicatorOn = !rightOn;
                        }
                    }
                    else
                    {
                        if (leftOn && rightOn)
                        {
                            vData.LeftIndicatorOn = false;
                            vData.RightIndicatorOn = false;
                        }
                        else
                        {
                            vData.LeftIndicatorOn = true;
                            vData.RightIndicatorOn = true;
                        }
                    }
                });

                vData.Release();
            });

            pData.Release();
        }

        [RemoteEvent("Vehicles::ToggleLights")]
        public static async Task ToggleLights(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var veh = player.Vehicle;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var vData = veh.GetMainData();

                if (!await vData.WaitAsync())
                    return;

                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true || veh?.Exists != true || pData.VehicleSeat != 0 || (!Utils.IsCar(player.Vehicle) && !Utils.IsBike(player.Vehicle)))
                        return;

                    vData.LightsOn = !vData.LightsOn;
                });

                vData.Release();
            });

            pData.Release();
        }
        #endregion

        #region Radio
        [RemoteEvent("Vehicles::SetRadio")]
        public static async Task SetRadio(Player player, int ind)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var veh = player.Vehicle;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var vData = veh.GetMainData();

                if (!await vData.WaitAsync())
                    return;

                vData.Radio = ind;

                vData.Release();
            });

            pData.Release();
        }
        #endregion

        #region Trunk Lock
        [RemoteEvent("Vehicles::ToggleTrunkLockSync")]
        public static async Task ToggleTrunk(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var vData = veh.GetMainData();

                if (!await vData.WaitAsync())
                    return;

                await NAPI.Task.RunAsync(async () =>
                {
                    if (player?.Exists != true || veh?.Exists != true || (player.Vehicle != veh && !player.AreEntitiesNearby(veh, Settings.ENTITY_INTERACTION_MAX_DISTANCE)))
                        return;

                    if (vData.IsOwner(pData) == null)
                        return;

                    var doorsStates = vData.DoorsStates;
                    var newState = !vData.TrunkLocked;

                    if (doorsStates[5] != 2)
                    {
                        doorsStates[5] = newState ? 0 : 1;

                        vData.DoorsStates = doorsStates;
                    }

                    if (player.Vehicle == null && !pData.AnyAnimActive() && pData.ActiveWeapon == null)
                    {
                        player.AttachObject("veh_key", AttachSystem.Types.VehKey, 1250);

                        pData.PlayAnim(Animations.FastTypes.VehLocking);
                    }

                    if (newState)
                    {
                        Chat.SendLocal(Chat.Type.Me, player, Locale.Chat.Vehicle.TrunkOff);
                        player.Notify("Trunk::Locked");
                    }
                    else
                    {
                        Chat.SendLocal(Chat.Type.Me, player, Locale.Chat.Vehicle.TrunkOn);
                        player.Notify("Trunk::Unlocked");
                    }

                    vData.TrunkLocked = newState;

                    var tid = vData.TID;

                    // Clear All Trunk Observers If Closed
                    if (newState && tid != null)
                        await Task.Run(async () =>
                        {
                            var cont = Game.Items.Container.Get((uint)tid);

                            if (!await cont.WaitAsync())
                                return;

                            cont.ClearAllObservers();

                            cont.Release();
                        });
                });

                vData.Release();
            });

            pData.Release();
        }
        #endregion

        #region Hood Lock
        [RemoteEvent("Vehicles::ToggleHoodLockSync")]
        public static async Task ToggleHood(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var vData = veh.GetMainData();

                if (!await vData.WaitAsync())
                    return;

                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true || veh?.Exists != true || (player.Vehicle != veh && !player.AreEntitiesNearby(veh, Settings.ENTITY_INTERACTION_MAX_DISTANCE)))
                        return;

                    if (vData.IsOwner(pData) == null)
                        return;

                    var doorsStates = vData.DoorsStates;
                    var newState = !vData.HoodLocked;

                    if (doorsStates[4] != 2)
                    {
                        doorsStates[4] = newState ? 0 : 1;

                        vData.DoorsStates = doorsStates;
                    }

                    if (player.Vehicle == null && !pData.AnyAnimActive() && pData.ActiveWeapon == null)
                    {
                        player.AttachObject("veh_key", AttachSystem.Types.VehKey, 1250);

                        pData.PlayAnim(Animations.FastTypes.VehLocking);
                    }

                    if (newState)
                    {
                        Chat.SendLocal(Chat.Type.Me, player, Locale.Chat.Vehicle.HoodOff);
                        player.Notify("Hood::Locked");
                    }
                    else
                    {
                        Chat.SendLocal(Chat.Type.Me, player, Locale.Chat.Vehicle.HoodOn);
                        player.Notify("Hood::Unlocked");
                    }

                    vData.HoodLocked = newState;
                });

                vData.Release();
            });

            pData.Release();
        }
        #endregion

        #region Fuel Level + Mileage
        [RemoteEvent("Vehicles::UpdateFuelLevel")]
        public static async Task UpdateFuelLevel(Player player, float diff)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var veh = player.Vehicle;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var vData = veh.GetMainData();

                if (!await vData.WaitAsync())
                    return;

                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true || veh?.Exists != true || pData.VehicleSeat != 0)
                        return;

                    if (!vData.EngineOn)
                        return;

                    float currentLevel = vData.FuelLevel;

                    if (diff <= 0f || diff > 1f)
                        return;

                    currentLevel -= diff;

                    if (currentLevel < 0f)
                        currentLevel = 0f;

                    vData.FuelLevel = currentLevel;

                    if (currentLevel == 0f)
                    {
                        ToggleEngine(pData, vData, false);
                    }
                });

                vData.Release();
            });

            pData.Release();
        }

        [RemoteEvent("Vehicles::UpdateMileage")]
        public static async Task UpdateMileage(Player player, float diff)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var veh = player.Vehicle;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var vData = veh.GetMainData();

                if (!await vData.WaitAsync())
                    return;

                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true || veh?.Exists != true || pData.VehicleSeat != 0)
                        return;

                    if (!vData.EngineOn)
                        return;

                    float currentLevel = vData.Mileage;

                    if (diff <= 0f || diff > 1000f)
                        return;

                    currentLevel += diff;

                    vData.Mileage = currentLevel;
                });

                vData.Release();
            });

            pData.Release();
        }
        #endregion

        #region Update States
        [RemoteEvent("Vehicles::UpdateDirtLevel")]
        public static async Task UpdateDirtLevel(Player player, float level)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var veh = player.Vehicle;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var vData = veh.GetMainData();

                if (!await vData.WaitAsync())
                    return;

                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true || veh?.Exists != true || player.VehicleSeat != 0)
                        return;

                    vData.DirtLevel = level;
                });

                vData.Release();
            });

            pData.Release();
        }

        [RemoteEvent("Vehicles::UpdateDoorsStates")]
        public static async Task UpdateDoorsStates(Player player, Newtonsoft.Json.Linq.JArray array)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var veh = player.Vehicle;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var vData = veh.GetMainData();

                if (!await vData.WaitAsync())
                    return;

                var res = array.ToObject<int[]>();

                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true || veh?.Exists != true || pData.VehicleSeat != 0)
                        return;

                    vData.DoorsStates = res;
                });

                vData.Release();
            });

            pData.Release();
        }
        #endregion

        #region Shuffle Seat
        [RemoteEvent("Vehicles::ShuffleSeat")]
        public static async Task ShuffleSeat(Player player, int seatId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var veh = player.Vehicle;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var vData = veh.GetMainData();

                if (!await vData.WaitAsync())
                    return;

                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true || veh?.Exists != true)
                        return;

                    var currentSeat = pData.VehicleSeat;

                    if (currentSeat == seatId || currentSeat == 0)
                        return;

                    var seats = vData.Passengers;

                    if (seats[seatId]?.Exists == true)
                        return;

                    if (veh.MaxPassengers + 1 < seatId + 1)
                        return;

                    vData.RemovePassenger(currentSeat);

                    player.SetIntoVehicle(veh, seatId);
                    vData.AddPassenger(seatId, pData);
                });

                vData.Release();
            });

            pData.Release();
        }

        [RemoteEvent("Vehicles::SeatToTrunk")]
        public static async Task SeatToTrunk(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            pData.Release();
        }
        #endregion

        #region Kick Player
        [RemoteEvent("Vehicles::KickPlayer")]
        public static async Task KickPlayer(Player player, Player target)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var veh = player.Vehicle;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var vData = veh.GetMainData();

                if (!await vData.WaitAsync())
                    return;

                var tData = target.GetMainData();

                if (!await tData.WaitAsync())
                {
                    vData.Release();

                    return;
                }    

                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true || target?.Exists != true || veh?.Exists != true || pData.VehicleSeat != 0)
                        return;

                    if (target.Vehicle != player.Vehicle)
                        return;

                    if (Sync.Chat.SendLocal(Chat.Type.TryPlayer, player, Locale.Chat.Vehicle.Kick, target))
                    {
                        vData.RemovePassenger(tData.VehicleSeat);
                        target.WarpOutOfVehicle();
                    }
                });

                tData.Release();

                vData.Release();
            });

            pData.Release();
        }
        #endregion

        [RemoteEvent("Vehicles::Park")]
        public static async Task Park(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var veh = player.Vehicle;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var vData = veh.GetMainData();

                if (!await vData.WaitAsync())
                    return;

                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true || veh?.Exists != true)
                        return;

                    if (vData.IsOwner(pData) == null)
                    {
                        player.Notify("Vehicle::NotAllowed");

                        return;
                    }
                });

                vData.Release();
            });

            pData.Release();
        }
    }
}
