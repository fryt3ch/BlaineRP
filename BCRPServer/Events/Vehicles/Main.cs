using BCRPServer.Sync;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static BCRPServer.Game.Items.Inventory;

namespace BCRPServer.Events.Vehicles
{
    class Main : Script
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

            if (seatId < 0 || seatId >= veh.MaxOccupants || (pData.VehicleSeat < 0 && vData.Locked) || veh.GetEntityInVehicleSeat(seatId) != null)
            {
                player.WarpOutOfVehicle();

                return;
            }

            var curWeapon = pData.ActiveWeapon;

            if (curWeapon != null)
            {
                if (!curWeapon.Value.WeaponItem.Data.CanUseInVehicle)
                    pData.InventoryAction(curWeapon.Value.Group, curWeapon.Value.Slot, 5);
            }

            player.TriggerEvent("Vehicles::Enter", vData.FuelLevel, vData.Mileage);

            pData.VehicleSeat = seatId;
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

            if (vData.ForcedSpeed != 0f && veh.GetEntityInVehicleSeat(0) != null)
                vData.ForcedSpeed = 0f;

            pData.VehicleSeat = -1;
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

            if (player.VehicleSeat != 0)
                return;

            ToggleEngine(pData, vData);

            if (veh.GetSharedData<bool?>("InGarage") != null)
            {
                if (pData.CurrentHouse == null)
                    return;

                veh.Teleport(pData.CurrentHouse.GarageOutside.Position, Utils.Dimensions.Main, pData.CurrentHouse.GarageOutside.RotationZ, true);

                veh.ResetSharedData("InGarage");

                player.TriggerEvent("House::Exit");
            }
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
                        Chat.SendLocal(Chat.Types.Do, player, Locale.Chat.Vehicle.EngineBroken);
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
                player.AttachObject(AttachSystem.Models.VehicleRemoteFob, AttachSystem.Types.VehKey, 1250);

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

            if (player.VehicleSeat != 0 || type < 0 || type > 2 || (!Utils.IsCar(player.Vehicle) && !Utils.IsBike(player.Vehicle)))
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

            if (player.VehicleSeat != 0 || (!Utils.IsCar(player.Vehicle) && !Utils.IsBike(player.Vehicle)))
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
                player.AttachObject(AttachSystem.Models.VehicleRemoteFob, AttachSystem.Types.VehKey, 1250);

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
                player.AttachObject(AttachSystem.Models.VehicleRemoteFob, AttachSystem.Types.VehKey, 1250);

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
        [RemoteEvent("Vehicles::Sync")]
        public static void Sync(Player player, float fuelDiff, float mileageDiff, byte dirtLevel)
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
                float curFuel = vData.FuelLevel;

                curFuel -= fuelDiff;

                if (curFuel < 0f)
                    curFuel = 0f;

                vData.FuelLevel = curFuel;

                if (curFuel == 0f)
                {
                    ToggleEngine(pData, vData, false);
                }
            }

            if (mileageDiff > 0 && mileageDiff < 1000f)
            {
                vData.Mileage += mileageDiff;
            }
        }
        #endregion

        #region Shuffle Seat
        [RemoteEvent("Vehicles::ShuffleSeat")]
        public static void ShuffleSeat(Player player, int seatId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            if (seatId < 0)
                return;

            var pData = sRes.Data;

            var veh = player.Vehicle;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (seatId >= veh.MaxOccupants)
                return;

            var currentSeat = pData.VehicleSeat;

            if (currentSeat == seatId || currentSeat <= 0 || veh.GetEntityInVehicleSeat(seatId) != null)
                return;

            if (seatId == 0)
            {
                player.SetIntoVehicle(veh, seatId);
            }
            else
                pData.VehicleSeat = seatId;
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

            if (player.VehicleSeat != 0)
                return;

            var veh = player.Vehicle;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            var tData = target.GetMainData();

            if (tData == null)
                return;

            if (target.Vehicle != player.Vehicle)
                return;

            if (Chat.SendLocal(Chat.Types.TryPlayer, player, Locale.Chat.Vehicle.Kick, target))
            {
                target.WarpOutOfVehicle();
            }
        }
        #endregion

        [RemoteEvent("Vehicles::TakePlate")]
        public static void TakePlate(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (player.Vehicle != null)
                return;

            if (vData.Numberplate == null)
                return;

            if (vData.IsOwner(pData) != VehicleData.OwningTypes.Owner)
            {
                player.Notify("Vehicle::NotAllowed");

                return;
            }

            if (!pData.CanUseInventory(true))
                return;

            if (!pData.TryGiveExistingItem(vData.Numberplate, 1, true, true))
                return;

            vData.Numberplate.Take(vData);

            vData.Numberplate = null;

            MySQL.VehicleNumberplateUpdate(vData.Info);
        }

        [RemoteEvent("Vehicles::SetupPlate")]
        public static void SetupPlate(Player player, Vehicle veh, uint npUid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (player.Vehicle != null)
                return;

            if (vData.Numberplate != null)
                return;

            if (vData.IsOwner(pData) != VehicleData.OwningTypes.Owner)
            {
                player.Notify("Vehicle::NotAllowed");

                return;
            }

            if (npUid == 0)
            {
                Dictionary<uint, string> nps = new Dictionary<uint, string>();

                for (int i = 0; i < pData.Items.Length; i++)
                {
                    if (pData.Items[i] is Game.Items.Numberplate cnp)
                        nps.TryAdd(cnp.UID, cnp.Tag);
                }

                if (nps.Count == 0)
                {
                    player.Notify("Inventory::NoItem");

                    return;
                }
                else if (nps.Count == 1)
                {
                    npUid = nps.Keys.First();
                }
                else
                {
                    player.TriggerEvent("Vehicles::NPChoose", nps);

                    return;
                }
            }

            var idx = -1;

            for (int i = 0; i < pData.Items.Length; i++)
            {
                if (pData.Items[i]?.UID == npUid)
                {
                    idx = i;

                    break;
                }
            }

            if (idx < 0)
                return;

            var np = pData.Items[idx] as Game.Items.Numberplate;

            if (np == null)
                return;

            pData.Items[idx] = null;

            player.TriggerEvent("Inventory::Update", (int)Groups.Items, idx, "null");

            MySQL.CharacterItemsUpdate(pData.Info);

            np.Setup(vData);

            vData.Numberplate = np;

            MySQL.VehicleNumberplateUpdate(vData.Info);

            player.Notify("NP::Set", np.Tag);
        }

        [RemoteEvent("Vehicles::Anchor")]
        private static void Anchor(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

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

        [RemoteEvent("Vehicles::ShowPass")]
        private static void ShowPassport(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (veh?.Exists != true)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            vData.Info.ShowPassport(player);
        }
    }
}
