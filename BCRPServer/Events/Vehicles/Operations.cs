using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Events.Vehicles
{
    internal class Operations : Script
    {
        [RemoteEvent("Vehicles::ET")]
        private static void ToggleEngineRemote(Player player, byte state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsFrozen || pData.IsCuffed)
                return;

            var veh = player.Vehicle;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (player.VehicleSeat != 0)
                return;

            if (state == 2)
            {
                if (!vData.EngineOn)
                    return;

                Sync.Chat.SendLocal(Sync.Chat.Types.Do, player, Language.Strings.Get("CHAT_VEHICLE_ENGINE_BROKEN_0"));

                vData.EngineOn = false;

                player.Notify("Engine::SF");

                return;
            }

            if (!vData.CanManipulate(pData, true))
                return;

            if (vData.Info.LastData.GarageSlot >= 0)
            {
                if (pData.CurrentHouse is Game.Estates.House house)
                {
                    if (house.GarageOutside == null)
                        return;

                    veh.Teleport(house.GarageOutside.Position, Settings.MAIN_DIMENSION, house.GarageOutside.RotationZ, true, Additional.AntiCheat.VehicleTeleportTypes.OnlyDriver);
                }
                else if (pData.CurrentGarage is Game.Estates.Garage garage)
                {
                    var ePos = garage.Root.GetNextVehicleExit();

                    veh.Teleport(ePos.Position, Settings.MAIN_DIMENSION, ePos.RotationZ, true, Additional.AntiCheat.VehicleTeleportTypes.OnlyDriver);
                }
                else
                {
                    return;
                }
            }

            var newState = state == 1;

            if (vData.EngineOn == newState)
                return;

            if (newState)
            {
                if (vData.IsDead || vData.Vehicle.Health <= -4000f)
                {
                    Sync.Chat.SendLocal(Sync.Chat.Types.Do, player, Language.Strings.Get("CHAT_VEHICLE_ENGINE_BROKEN_1"));

                    player.Notify("Engine::SF");

                    return;
                }

                if (vData.FuelLevel > 0f)
                {
                    vData.EngineOn = true;

                    Sync.Chat.SendLocal(Sync.Chat.Types.Me, player, Language.Strings.Get("CHAT_VEHICLE_ENGINE_ON"));

                    player.Notify("Engine::On");
                }
                else
                {
                    Sync.Chat.SendLocal(Sync.Chat.Types.Do, player, Language.Strings.Get("CHAT_VEHICLE_ENGINE_BROKEN_1"));

                    player.Notify("Engine::OutOfFuel");
                }
            }
            else
            {
                vData.EngineOn = false;

                Sync.Chat.SendLocal(Sync.Chat.Types.Me, player, Language.Strings.Get("CHAT_VEHICLE_ENGINE_OFF"));

                player.Notify("Engine::Off");
            }
        }

        [RemoteEvent("Vehicles::TDL")]
        public static void ToggleDoorsLock(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsFrozen || pData.IsCuffed)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (player.Vehicle != veh && !player.AreEntitiesNearby(veh, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            if (!vData.CanManipulate(pData, true))
                return;

            var newState = !vData.Locked;

            if (player.Vehicle == null && pData.CanPlayAnimNow() && pData.ActiveWeapon == null)
            {
                player.AttachObject(Sync.AttachSystem.Models.VehicleRemoteFob, Sync.AttachSystem.Types.VehKey, 1250, null);

                pData.PlayAnim(Sync.Animations.FastTypes.VehLocking);
            }

            if (newState)
            {
                Sync.Chat.SendLocal(Sync.Chat.Types.Me, player, Language.Strings.Get("CHAT_VEHICLE_DOORS_LOCKED"));

                player.Notify("Doors::Locked");
            }
            else
            {
                Sync.Chat.SendLocal(Sync.Chat.Types.Me, player, Language.Strings.Get("CHAT_VEHICLE_DOORS_UNLOCKED"));

                player.Notify("Doors::Unlocked");
            }

            vData.Locked = newState;
        }

        [RemoteEvent("Vehicles::TIND")]
        public static void ToggleIndicator(Player player, int type)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsFrozen || pData.IsCuffed)
                return;

            var veh = player.Vehicle;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (player.VehicleSeat != 0 || type < 0 || type > 2 || (vData.Data.Type != Game.Data.Vehicles.Vehicle.Types.Car && vData.Data.Type != Game.Data.Vehicles.Vehicle.Types.Motorcycle))
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

        [RemoteEvent("Vehicles::TLI")]
        public static void ToggleLights(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsFrozen || pData.IsCuffed)
                return;

            var veh = player.Vehicle;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (player.VehicleSeat != 0 || (vData.Data.Type != Game.Data.Vehicles.Vehicle.Types.Car && vData.Data.Type != Game.Data.Vehicles.Vehicle.Types.Motorcycle))
                return;

            vData.LightsOn = !vData.LightsOn;
        }

        [RemoteEvent("Vehicles::SetRadio")]
        public static void SetRadio(Player player, byte stationNum)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(VehicleData.StationTypes), stationNum))
                return;

            if (pData.IsKnocked || pData.IsFrozen || pData.IsCuffed)
                return;

            var veh = player.Vehicle;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            var vehSeat = pData.VehicleSeat;

            if (vehSeat != 0 && vehSeat != 1)
                return;

            var stationType = (VehicleData.StationTypes)stationNum;

            vData.Radio = stationType;
        }

        [RemoteEvent("Vehicles::TTL")]
        public static void ToggleTrunk(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsFrozen || pData.IsCuffed)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (player.Vehicle != veh && !player.AreEntitiesNearby(veh, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            if (!vData.CanManipulate(pData, true))
                return;

            var newState = !vData.TrunkLocked;

            if (player.Vehicle == null && pData.CanPlayAnimNow() && pData.ActiveWeapon == null)
            {
                player.AttachObject(Sync.AttachSystem.Models.VehicleRemoteFob, Sync.AttachSystem.Types.VehKey, 1250, null);

                pData.PlayAnim(Sync.Animations.FastTypes.VehLocking);
            }

            if (newState)
            {
                Sync.Chat.SendLocal(Sync.Chat.Types.Me, player, Language.Strings.Get("CHAT_VEHICLE_TRUNK_LOCKED"));
                player.Notify("Trunk::Locked");
            }
            else
            {
                Sync.Chat.SendLocal(Sync.Chat.Types.Me, player, Language.Strings.Get("CHAT_VEHICLE_TRUNK_UNLOCKED"));
                player.Notify("Trunk::Unlocked");
            }

            vData.TrunkLocked = newState;

            // Clear All Trunk Observers If Closed
            if (newState)
            {
                var cont = Game.Items.Container.Get(vData.TID);

                if (cont != null)
                {
                    cont.ClearAllWrongObservers();
                }
            }
        }

        [RemoteEvent("Vehicles::THL")]
        public static void ToggleHood(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsFrozen || pData.IsCuffed)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (player.Vehicle != veh && !player.AreEntitiesNearby(veh, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            if (!vData.CanManipulate(pData, true))
                return;

            var newState = !vData.HoodLocked;

            if (player.Vehicle == null && pData.CanPlayAnimNow() && pData.ActiveWeapon == null)
            {
                player.AttachObject(Sync.AttachSystem.Models.VehicleRemoteFob, Sync.AttachSystem.Types.VehKey, 1250, null);

                pData.PlayAnim(Sync.Animations.FastTypes.VehLocking);
            }

            if (newState)
            {
                Sync.Chat.SendLocal(Sync.Chat.Types.Me, player, Language.Strings.Get("CHAT_VEHICLE_HOOD_LOCKED"));

                player.Notify("Hood::Locked");
            }
            else
            {
                Sync.Chat.SendLocal(Sync.Chat.Types.Me, player, Language.Strings.Get("CHAT_VEHICLE_HOOD_UNLOCKED"));

                player.Notify("Hood::Unlocked");
            }

            vData.HoodLocked = newState;
        }

        [RemoteEvent("Vehicles::Sync")]
        public static void SyncStates(Player player, float fuelDiff, float mileageDiff, byte dirtLevel)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (player.VehicleSeat != 0)
                return;

            var veh = player.Vehicle;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (!vData.EngineOn)
                return;

            var curDirt = vData.DirtLevel;

            if (curDirt < dirtLevel)
            {
                vData.DirtLevel = dirtLevel;
            }

            if (fuelDiff > 0f && fuelDiff < 1f)
            {
                var curFuel = vData.FuelLevel;

                var newFuel = curFuel - fuelDiff;

                if (newFuel < 0f)
                    newFuel = 0f;

                if (curFuel != newFuel)
                {
                    vData.FuelLevel = newFuel;

                    if (newFuel == 0f)
                    {
                        vData.EngineOn = false;

                        Sync.Chat.SendLocal(Sync.Chat.Types.Do, player, Language.Strings.Get("CHAT_VEHICLE_FUEL_OUTOF"));

                        player.Notify("Engine::OutOfFuel");
                    }
                }
            }

            if (mileageDiff > 0 && mileageDiff < 1000f)
            {
                var curMileage = vData.Mileage;

                var newMileage = curMileage + mileageDiff;

                if (newMileage > float.MaxValue)
                    newMileage = float.MaxValue;

                if (curMileage != newMileage)
                {
                    vData.Mileage = newMileage;
                }
            }
        }

        [RemoteEvent("Vehicles::Anchor")]
        private static void Anchor(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            var veh = player.Vehicle;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (vData.Data.Type != Game.Data.Vehicles.Vehicle.Types.Boat || vData.ForcedSpeed != 0f)
                return;

            if (state == vData.IsAnchored)
                return;

            vData.IsAnchored = state;
        }

        [RemoteProc("Vehicles::SPSOS")]
        private static bool SetPlaneChassisOffState(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return false;

            var vData = player.Vehicle?.GetMainData();

            if (vData == null || vData.Data.Type != Game.Data.Vehicles.Vehicle.Types.Plane || pData.VehicleSeat != 0)
                return false;

            if (state == vData.IsPlaneChassisOff)
                return false;

            vData.IsPlaneChassisOff = state;

            return true;
        }
    }
}
