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
        private static void PlayerEntered(Player player, Vehicle veh, sbyte seatId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (pData.VehicleSeat != -1 || seatId < 0 || seatId >= veh.MaxOccupants || vData.Locked || vData.Passengers[seatId]?.Exists == true)
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
                    pData.InventoryAction(curWeapon.Value.Group, curWeapon.Value.Slot, 5);
            }
        }
        #endregion

        #region Player Exit Vehicle
        [ServerEvent(Event.PlayerExitVehicle)]
        private static void PlayerExited(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            vData.RemovePassenger(pData);

            if (!vData.IsInvincible && !vData.Passengers.Where(x => x?.Exists == true).Any())
                vData.IsInvincible = true;
        }
        #endregion

        #region Engine
        [RemoteEvent("Vehicles::ToggleEngineSync")]
        private static void ToggleEngineRemote(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            Vehicle veh = player.Vehicle;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (pData.VehicleSeat != 0)
                return;

            ToggleEngine(pData, vData);
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

                        Chat.SendLocal(Chat.Types.Me, player, Locale.Chat.Vehicle.EngineOn);
                        player.Notify("Engine::On");
                    }
                    else
                    {
                        Chat.SendLocal(Chat.Types.Do, player, Locale.Chat.Vehicle.OutOfFuel);
                        player.Notify("Engine::OutOfFuel");
                    }
                }
                else
                {
                    vData.EngineOn = false;

                    Chat.SendLocal(Chat.Types.Me, player, Locale.Chat.Vehicle.EngineOff);
                    player.Notify("Engine::Off");
                }
            }
        }
        #endregion

        #region Doors Lock
        [RemoteEvent("Vehicles::ToggleDoorsLockSync")]
        public static void ToggleDoorsLock(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (player.Vehicle != veh && !player.AreEntitiesNearby(veh, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            if (vData.IsOwner(pData) == null)
                return;

            bool newState = !vData.Locked;

            if (player.Vehicle == null && !pData.CanPlayAnim() && pData.ActiveWeapon == null)
            {
                player.AttachObject(Sync.AttachSystem.Models.VehicleRemoteFob, AttachSystem.Types.VehKey, 1250);

                pData.PlayAnim(Animations.FastTypes.VehLocking);
            }

            if (newState)
            {
                Chat.SendLocal(Chat.Types.Me, player, Locale.Chat.Vehicle.Locked);
                player.Notify("Doors::Locked");
            }
            else
            {
                Chat.SendLocal(Chat.Types.Me, player, Locale.Chat.Vehicle.Unlocked);
                player.Notify("Doors::Unlocked");
            }

            vData.Locked = newState;
        }
        #endregion

        #region Indicators + Lights
        [RemoteEvent("Vehicles::ToggleIndicator")]
        public static void ToggleIndicator(Player player, int type)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var veh = player.Vehicle;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (pData.VehicleSeat != 0 || type < 0 || type > 2 || (!Utils.IsCar(player.Vehicle) && !Utils.IsBike(player.Vehicle)))
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
        }

        [RemoteEvent("Vehicles::ToggleLights")]
        public static void ToggleLights(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var veh = player.Vehicle;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (pData.VehicleSeat != 0 || (!Utils.IsCar(player.Vehicle) && !Utils.IsBike(player.Vehicle)))
                return;

            vData.LightsOn = !vData.LightsOn;
        }
        #endregion

        #region Radio
        [RemoteEvent("Vehicles::SetRadio")]
        public static void SetRadio(Player player, int ind)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var veh = player.Vehicle;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            vData.Radio = ind;
        }
        #endregion

        #region Trunk Lock
        [RemoteEvent("Vehicles::ToggleTrunkLockSync")]
        public static void ToggleTrunk(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (player.Vehicle != veh && !player.AreEntitiesNearby(veh, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            if (vData.IsOwner(pData) == null)
                return;

            var newState = !vData.TrunkLocked;

            if (player.Vehicle == null && !pData.CanPlayAnim() && pData.ActiveWeapon == null)
            {
                player.AttachObject(Sync.AttachSystem.Models.VehicleRemoteFob, AttachSystem.Types.VehKey, 1250);

                pData.PlayAnim(Animations.FastTypes.VehLocking);
            }

            if (newState)
            {
                Chat.SendLocal(Chat.Types.Me, player, Locale.Chat.Vehicle.TrunkOff);
                player.Notify("Trunk::Locked");
            }
            else
            {
                Chat.SendLocal(Chat.Types.Me, player, Locale.Chat.Vehicle.TrunkOn);
                player.Notify("Trunk::Unlocked");
            }

            vData.TrunkLocked = newState;

            var tid = vData.TID;

            // Clear All Trunk Observers If Closed
            if (newState && tid != null)
            {
                var cont = Game.Items.Container.Get((uint)tid);

                if (cont == null)
                    return;

                cont.ClearAllObservers();
            }
        }
        #endregion

        #region Hood Lock
        [RemoteEvent("Vehicles::ToggleHoodLockSync")]
        public static void ToggleHood(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (player.Vehicle != veh && !player.AreEntitiesNearby(veh, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            if (vData.IsOwner(pData) == null)
                return;

            var newState = !vData.HoodLocked;

            if (player.Vehicle == null && !pData.CanPlayAnim() && pData.ActiveWeapon == null)
            {
                player.AttachObject(Sync.AttachSystem.Models.VehicleRemoteFob, AttachSystem.Types.VehKey, 1250);

                pData.PlayAnim(Animations.FastTypes.VehLocking);
            }

            if (newState)
            {
                Chat.SendLocal(Chat.Types.Me, player, Locale.Chat.Vehicle.HoodOff);
                player.Notify("Hood::Locked");
            }
            else
            {
                Chat.SendLocal(Chat.Types.Me, player, Locale.Chat.Vehicle.HoodOn);
                player.Notify("Hood::Unlocked");
            }

            vData.HoodLocked = newState;
        }
        #endregion

        #region Fuel Level + Mileage
        [RemoteEvent("Vehicles::UpdateFuelLevel")]
        public static void UpdateFuelLevel(Player player, float diff)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var veh = player.Vehicle;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (pData.VehicleSeat != 0)
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
        }

        [RemoteEvent("Vehicles::UpdateMileage")]
        public static void UpdateMileage(Player player, float diff)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var veh = player.Vehicle;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (pData.VehicleSeat != 0)
                return;

            if (!vData.EngineOn)
                return;

            float currentLevel = vData.Mileage;

            if (diff <= 0f || diff > 1000f)
                return;

            currentLevel += diff;

            vData.Mileage = currentLevel;
        }
        #endregion

        #region Shuffle Seat
        [RemoteEvent("Vehicles::ShuffleSeat")]
        public static void ShuffleSeat(Player player, int seatId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var veh = player.Vehicle;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            var currentSeat = pData.VehicleSeat;

            if (currentSeat == seatId || currentSeat == 0)
                return;

            var seats = vData.Passengers;

            if (seats[seatId]?.Exists == true)
                return;

            if (veh.MaxPassengers + 1 < seatId + 1)
                return;

            vData.RemovePassenger(pData);

            player.SetIntoVehicle(veh, seatId);

            if (seatId != 0)
                vData.AddPassenger(seatId, pData);
        }
        #endregion

        #region Kick Player
        [RemoteEvent("Vehicles::KickPlayer")]
        public static void KickPlayer(Player player, Player target)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var veh = player.Vehicle;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            var tData = target.GetMainData();

            if (tData == null)
                return;

            if (pData.VehicleSeat != 0)
                return;

            if (target.Vehicle != player.Vehicle)
                return;

            if (Sync.Chat.SendLocal(Chat.Types.TryPlayer, player, Locale.Chat.Vehicle.Kick, target))
            {
                //vData.RemovePassenger(tData.VehicleSeat);

                target.WarpOutOfVehicle();
            }
        }
        #endregion

        [RemoteEvent("Vehicles::Park")]
        public static void Park(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var veh = player.Vehicle;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (vData.IsOwner(pData) == null)
            {
                player.Notify("Vehicle::NotAllowed");

                return;
            }
        }
    }
}
