using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlaineRP.Server.Events.Vehicles
{
    internal class Operations : Script
    {
        private static TimeSpan VehicleLockAnimationTime { get; } = TimeSpan.FromMilliseconds(1_500);

        [RemoteProc("Vehicles::ET")]
        private static byte ToggleEngineRemote(Player player, Vehicle veh, byte state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsFrozen || pData.IsCuffed)
                return 0;

            var vData = veh.GetMainData();

            if (vData == null)
                return 0;

            if (player.VehicleSeat != 0)
                return 0;

            if (state == 2)
            {
                if (!vData.EngineOn)
                    return 1;

                Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Do, player, Language.Strings.Get("CHAT_VEHICLE_ENGINE_BROKEN_0"));

                vData.EngineOn = false;

                return 255;
            }

            if (!vData.CanManipulate(pData, true))
                return 2;

            if (vData.Info.LastData.GarageSlot >= 0)
            {
                if (pData.CurrentHouseBase is Game.Estates.House house)
                {
                    if (house.GarageOutside == null)
                        return 3;

                    veh.Teleport(house.GarageOutside.Position, Properties.Settings.Static.MainDimension, house.GarageOutside.RotationZ, true, Additional.AntiCheat.VehicleTeleportTypes.OnlyDriver);
                }
                else if (pData.CurrentGarage is Game.Estates.Garage garage)
                {
                    var ePos = garage.Root.GetNextVehicleExit();

                    veh.Teleport(ePos.Position, Properties.Settings.Static.MainDimension, ePos.RotationZ, true, Additional.AntiCheat.VehicleTeleportTypes.OnlyDriver);
                }
                else
                {
                    return 3;
                }
            }

            var newState = state == 1;

            if (vData.EngineOn == newState)
                return 4;

            if (newState)
            {
                if (vData.IsDead || vData.Vehicle.Health <= -4000f)
                {
                    Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Do, player, Language.Strings.Get("CHAT_VEHICLE_ENGINE_BROKEN_1"));

                    return 5;
                }

                if (vData.FuelLevel > 0f)
                {
                    vData.EngineOn = true;

                    Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_VEHICLE_ENGINE_ON"));

                    return 255;
                }
                else
                {
                    Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Do, player, Language.Strings.Get("CHAT_VEHICLE_ENGINE_BROKEN_1"));

                    return 6;
                }
            }
            else
            {
                vData.EngineOn = false;

                Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_VEHICLE_ENGINE_OFF"));

                return 255;
            }
        }

        [RemoteProc("Vehicles::TDL")]
        public static byte ToggleDoorsLock(Player player, Vehicle veh, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsFrozen || pData.IsCuffed)
                return 0;

            var vData = veh.GetMainData();

            if (vData == null)
                return 0;

            if (player.Vehicle != veh && !player.IsNearToEntity(veh, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                return 0;

            if (!vData.CanManipulate(pData, true))
                return 1;

            if (state == vData.Locked)
                return 2;

            if (player.Vehicle == null && pData.CanPlayAnimNow() && !pData.HasAnyActiveWeapon())
            {
                player.AttachObject(Sync.AttachSystem.Models.VehicleRemoteFob, Sync.AttachSystem.Types.VehKey, 1250, null);

                pData.PlayAnim(Sync.Animations.FastTypes.VehLocking, VehicleLockAnimationTime);
            }

            if (state)
            {
                vData.Locked = true;

                Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_VEHICLE_DOORS_LOCKED"));
            }
            else
            {
                vData.Locked = false;

                Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_VEHICLE_DOORS_UNLOCKED"));
            }

            return 255;
        }

        [RemoteProc("Vehicles::TIND")]
        public static byte ToggleIndicatorsState(Player player, Vehicle veh, byte state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsFrozen || pData.IsCuffed)
                return 0;

            var vData = veh.GetMainData();

            if (vData == null)
                return 0;

            if (player.Vehicle != veh || player.VehicleSeat != 0 || state < 0 || state > 3 || (vData.Data.Type != Game.Data.Vehicles.Vehicle.Types.Car && vData.Data.Type != Game.Data.Vehicles.Vehicle.Types.Motorcycle))
                return 0;

            var oldState = vData.IndicatorsState;

            if (oldState == state)
                return 1;

            vData.IndicatorsState = state;

            return 255;
        }

        [RemoteProc("Vehicles::TLI")]
        public static byte ToggleLights(Player player, Vehicle veh, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsFrozen || pData.IsCuffed)
                return 0;

            var vData = veh.GetMainData();

            if (vData == null)
                return 0;

            if (player.Vehicle != veh || player.VehicleSeat != 0 || (vData.Data.Type != Game.Data.Vehicles.Vehicle.Types.Car && vData.Data.Type != Game.Data.Vehicles.Vehicle.Types.Motorcycle))
                return 0;

            if (vData.LightsOn == state)
                return 1;

            if (state)
            {
                vData.LightsOn = true;

                Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_VEHICLE_LIGHTS_ON"));
            }
            else
            {
                vData.LightsOn = false;

                Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_VEHICLE_LIGHTS_OFF"));
            }

            return 255;
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

        [RemoteProc("Vehicles::TTL")]
        public static byte ToggleTrunk(Player player, Vehicle veh, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsFrozen || pData.IsCuffed)
                return 0;

            var vData = veh.GetMainData();

            if (vData == null)
                return 0;

            if (player.Vehicle != veh && !player.IsNearToEntity(veh, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                return 0;

            if (!vData.CanManipulate(pData, true))
                return 1;

            if (vData.TrunkLocked == state)
                return 2;

            if (player.Vehicle == null && pData.CanPlayAnimNow() && !pData.HasAnyActiveWeapon())
            {
                player.AttachObject(Sync.AttachSystem.Models.VehicleRemoteFob, Sync.AttachSystem.Types.VehKey, 1250, null);

                pData.PlayAnim(Sync.Animations.FastTypes.VehLocking, VehicleLockAnimationTime);
            }

            if (state)
            {
                vData.TrunkLocked = true;

                var cont = Game.Items.Container.Get(vData.TID);

                if (cont != null)
                {
                    cont.ClearAllWrongObservers();
                }

                Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_VEHICLE_TRUNK_LOCKED"));
            }
            else
            {
                vData.TrunkLocked = false;

                Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_VEHICLE_TRUNK_UNLOCKED"));
            }

            return 255;
        }

        [RemoteProc("Vehicles::THL")]
        public static byte ToggleHood(Player player, Vehicle veh, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsFrozen || pData.IsCuffed)
                return 0;

            var vData = veh.GetMainData();

            if (vData == null)
                return 0;

            if (player.Vehicle != veh && !player.IsNearToEntity(veh, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                return 0;

            if (!vData.CanManipulate(pData, true))
                return 1;

            if (vData.HoodLocked == state)
                return 2;

            if (player.Vehicle == null && pData.CanPlayAnimNow() && !pData.HasAnyActiveWeapon())
            {
                player.AttachObject(Sync.AttachSystem.Models.VehicleRemoteFob, Sync.AttachSystem.Types.VehKey, 1250, null);

                pData.PlayAnim(Sync.Animations.FastTypes.VehLocking, VehicleLockAnimationTime);
            }

            if (state)
            {
                vData.HoodLocked = true;

                Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_VEHICLE_HOOD_LOCKED"));
            }
            else
            {
                vData.HoodLocked = false;

                Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_VEHICLE_HOOD_UNLOCKED"));
            }

            return 255;
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

                        Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Do, player, Language.Strings.Get("CHAT_VEHICLE_FUEL_OUTOF"));
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
        private static byte SetPlaneLandingGearState(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return 0;

            var vData = player.Vehicle?.GetMainData();

            if (vData == null || vData.Data.Type != Game.Data.Vehicles.Vehicle.Types.Plane || pData.VehicleSeat != 0)
                return 0;

            if (state == vData.IsPlaneChassisOff)
                return 1;

            if (state)
            {
                vData.IsPlaneChassisOff = true;

                Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_VEHICLE_LGEAR_OFF"));
            }
            else
            {
                vData.IsPlaneChassisOff = false;

                Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_VEHICLE_LGEAR_ON"));
            }

            return 255;
        }
    }
}
