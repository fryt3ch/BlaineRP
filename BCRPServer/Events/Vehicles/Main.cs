using BCRPServer.Sync;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
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

            if (pData.IsFrozen || pData.IsCuffed || pData.IsKnocked)
            {
                player.WarpOutOfVehicle();

                return;
            }

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

            if (seatId == 0)
            {
                if (vData.OwnerType == VehicleData.OwnerTypes.PlayerRentJob)
                {
                    if (vData.OwnerID == 0)
                    {
                        if (pData.RentedJobVehicle == null)
                        {
                            if (vData.Job is Game.Jobs.Job jobData && jobData is Game.Jobs.IVehicles jobDataVeh)
                            {
                                player.TriggerEvent("Vehicles::JVRO", jobDataVeh.VehicleRentPrice);
                            }
                        }
                        else
                        {
                            player.Notify("Vehicles::RVAH");
                        }
                    }
                }
            }

            if (vData.OwnerType == VehicleData.OwnerTypes.PlayerRent || vData.OwnerType == VehicleData.OwnerTypes.PlayerRentJob)
            {
                if (vData.OwnerID == pData.CID)
                    vData.CancelDeletionTask();
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

            BCRPServer.Sync.Vehicles.OnPlayerLeaveVehicle(pData, vData);
        }
        #endregion

        [ServerEvent(Event.VehicleDeath)]
        private static void VehicleDeath(Vehicle veh)
        {
            if (veh?.Exists != true)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            vData.IsDead = true;

            if (vData.EngineOn)
                vData.EngineOn = false;

            if (vData.OwnerType == VehicleData.OwnerTypes.PlayerRentJob || vData.OwnerType == VehicleData.OwnerTypes.PlayerRent)
            {
                vData.Delete(false);
            }

            //Console.WriteLine($"{vData.VID} died - {veh.Health}");
        }

        [RemoteEvent("votc")]
        private static void VehicleTrailerChange(Player player, Vehicle veh, Vehicle trailer)
        {
            if (player?.Exists != true)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (veh.Controller != player)
                return;

            if (trailer == null)
            {
                var atVeh = vData.IsAttachedTo as Vehicle;

                if (atVeh?.Exists != true)
                    return;

                var atData = atVeh.GetAttachmentData(veh);

                if (atData == null || (atData.Type != AttachSystem.Types.VehicleTrailerObjBoat))
                    return;

                atVeh.DetachEntity(veh);

                //Console.WriteLine("trailer detached");
            }
            else
            {
                var tData = trailer.GetMainData();

                if (tData == null)
                    return;

                var atData = vData.IsAttachedTo;

                if (atData != null)
                    return;

                if (tData.Data.Type == Game.Data.Vehicles.Vehicle.Types.Boat)
                    trailer.AttachEntity(veh, AttachSystem.Types.VehicleTrailerObjBoat);

                //Console.WriteLine("trailer attached");
            }
        }

        #region Engine
        [RemoteEvent("Vehicles::ToggleEngineSync")]
        private static void ToggleEngineRemote(Player player, bool selfToggled)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsFrozen || pData.IsCuffed)
                return;

            Vehicle veh = player.Vehicle;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (player.VehicleSeat != 0)
                return;

            if (!vData.CanManipulate(pData, true))
                return;

            if (vData.Info.LastData.GarageSlot >= 0)
            {
                if (pData.CurrentHouse is Game.Estates.House house)
                {
                    if (house.GarageOutside == null)
                        return;

                    veh.Teleport(house.GarageOutside.Position, Utils.Dimensions.Main, house.GarageOutside.RotationZ, true, Additional.AntiCheat.VehicleTeleportTypes.OnlyDriver);
                }
                else if (pData.CurrentGarage is Game.Estates.Garage garage)
                {
                    var ePos = garage.Root.GetNextVehicleExit();

                    veh.Teleport(ePos.Position, Utils.Dimensions.Main, ePos.RotationZ, true, Additional.AntiCheat.VehicleTeleportTypes.OnlyDriver);
                }
                else
                {
                    return;
                }
            }

            if (selfToggled)
                ToggleEngine(pData, vData, null);
            else
                ToggleEngine(pData, vData, false);
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
                    if (vData.FuelLevel <= 0f || vData.IsDead)
                        return;
                }
                else
                    vData.EngineOn = false;
            }
            else
            {
                if (!vData.EngineOn)
                {
                    if (vData.IsDead || vData.Vehicle.Health <= -4000f)
                    {
                        Chat.SendLocal(Chat.Types.Do, player, Locale.Chat.Vehicle.EngineBroken);

                        return;
                    }

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

            if (pData.IsKnocked || pData.IsFrozen || pData.IsCuffed)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (player.Vehicle != veh && !player.AreEntitiesNearby(veh, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            if (!vData.CanManipulate(pData, true))
                return;

            bool newState = !vData.Locked;

            if (player.Vehicle == null && pData.CanPlayAnimNow() && pData.ActiveWeapon == null)
            {
                player.AttachObject(AttachSystem.Models.VehicleRemoteFob, AttachSystem.Types.VehKey, 1250, null);

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

        [RemoteEvent("Vehicles::ToggleLights")]
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
        #endregion

        #region Radio
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

            var stationType = (VehicleData.StationTypes)stationNum;

            vData.Radio = stationType;
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
                player.AttachObject(AttachSystem.Models.VehicleRemoteFob, AttachSystem.Types.VehKey, 1250, null);

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

            // Clear All Trunk Observers If Closed
            if (newState && vData.TID is uint tid)
            {
                var cont = Game.Items.Container.Get(tid);

                if (cont == null)
                    return;

                cont.ClearAllWrongObservers();
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
                player.AttachObject(AttachSystem.Models.VehicleRemoteFob, AttachSystem.Types.VehKey, 1250, null);

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
                var newMileage = vData.Mileage + mileageDiff;

                if (newMileage > float.MaxValue)
                    vData.Mileage = float.MaxValue;
                else
                    vData.Mileage = newMileage;
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

            if (pData.IsKnocked || pData.IsFrozen || pData.IsCuffed)
                return;

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

            if (pData.IsKnocked || pData.IsFrozen || pData.IsCuffed)
                return;

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

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (player.Vehicle != null)
                return;

            if (vData.Numberplate == null)
                return;

            if (!vData.IsFullOwner(pData))
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

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (player.Vehicle != null)
                return;

            if (vData.Numberplate != null)
                return;

            if (!vData.IsFullOwner(pData))
                return;

            if (npUid == 0)
            {
                var nps = new Dictionary<uint, string>();

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

            player.InventoryUpdate(Groups.Items, idx, Game.Items.Item.ToClientJson(null, Game.Items.Inventory.Groups.Items));

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

        [RemoteEvent("Vehicles::ShowPass")]
        private static void ShowPassport(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            if (veh?.Exists != true)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            vData.Info.ShowPassport(player);
        }

        [RemoteEvent("Vehicles::Fix")]
        private static void VehicleFix(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            if (veh?.Exists != true)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (player.Vehicle != null || !player.AreEntitiesNearby(veh, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            if (vData.IsDead)
            {
                player.Notify("Vehicle::RKDE");

                return;
            }

            Game.Items.VehicleRepairKit rKit = null;

            var minAmount = int.MaxValue;

            var idx = -1;

            for (int i = 0; i < pData.Items.Length; i++)
            {
                if (pData.Items[i] is Game.Items.VehicleRepairKit vrk)
                {
                    if (vrk.Amount < minAmount)
                    {
                        rKit = vrk;

                        idx = i;

                        minAmount = vrk.Amount;
                    }
                }
            }

            if (rKit == null)
            {
                player.Notify("Inventory::NoItem");

                return;
            }

            rKit.Apply(pData, vData);

            if (rKit.Amount == 1)
            {
                rKit.Delete();

                rKit = null;

                pData.Items[idx] = null;

                MySQL.CharacterItemsUpdate(pData.Info);
            }
            else
            {
                rKit.Amount -= 1;

                rKit.Update();
            }

            player.InventoryUpdate(Game.Items.Inventory.Groups.Items, idx, Game.Items.Item.ToClientJson(rKit, Game.Items.Inventory.Groups.Items));
        }

        [RemoteEvent("VRent::Cancel")]
        private static void VehicleRentCancel(Player player, ushort vId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var vData = VehicleData.All.Values.Where(x => x.Vehicle.Id == vId).FirstOrDefault();

            if (vData == null)
                return;

            if (player.Vehicle == vData.Vehicle)
                return;

            if (vData.OwnerID != pData.CID)
                return;

            if (vData.OwnerType != VehicleData.OwnerTypes.PlayerRent && vData.OwnerType != VehicleData.OwnerTypes.PlayerRentJob)
                return;

            vData.Delete(false);
        }

        [RemoteEvent("Vehicles::BTOW")]
        private static void VehicleBoatTrailerOnWater(Player player, Vehicle veh, float x, float y, float z)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            if (veh?.Exists != true)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (vData.Data.Type != Game.Data.Vehicles.Vehicle.Types.Boat)
                return;

            if (x == 56.77f)
            {
                vData.AttachBoatToTrailer();

                return;
            }

            if (vData.DetachBoatFromTrailer())
            {
                var waterPos = new Vector3(x, y, z);

                vData.Vehicle.Teleport(waterPos, null, null, false, Additional.AntiCheat.VehicleTeleportTypes.All);
            }
        }

        [RemoteProc("Vehicles::JVRS")]
        private static byte JobVehicleRentStart(Player player, bool useCash)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return 0;

            var vData = player.Vehicle?.GetMainData();

            if (vData == null || vData.OwnerType != VehicleData.OwnerTypes.PlayerRentJob)
                return 0;

            if (pData.VehicleSeat != 0)
                return 0;

            if (pData.HasJob(true))
                return 0;

            var jobData = vData.Job;
            var jobDataV = jobData as Game.Jobs.IVehicles;

            if (jobData == null || jobDataV == null)
                return 0;

            if (vData.OwnerID != 0)
                return 1;

            if (vData.OwnerID == pData.CID)
                return 0;

            if (pData.RentedJobVehicle != null)
                return 2;

            ulong newBalance;

            if (useCash)
            {
                if (!pData.TryRemoveCash(jobDataV.VehicleRentPrice, out newBalance, true))
                    return 3;

                pData.SetCash(newBalance);
            }
            else
            {
                if (!pData.HasBankAccount(true) || !pData.BankAccount.TryRemoveMoneyDebit(jobDataV.VehicleRentPrice, out newBalance, true))
                    return 3;

                pData.BankAccount.SetDebitBalance(newBalance, null);
            }

            vData.OwnerID = pData.CID;

            pData.AddRentedVehicle(vData, 600_000);

            jobData.SetPlayerJob(pData, vData);

            return byte.MaxValue;
        }

        [RemoteEvent("Vehicles::LOWNV")]
        private static void LocateOwnedVehicle(Player player, uint vid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            var vInfo = pData.OwnedVehicles.Where(x => x.VID == vid).FirstOrDefault();

            if (vInfo == null)
                return;

            BCRPServer.Sync.Vehicles.TryLocateOwnedVehicle(pData, vInfo);
        }

        [RemoteEvent("Vehicles::LRENV")]
        private static void LocateRentedVehicle(Player player, ushort rid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            var vData = VehicleData.All.Values.Where(x => x.Vehicle.Id == rid).FirstOrDefault();

            if (vData == null)
                return;

            if (vData.OwnerID != pData.CID)
                return;

            BCRPServer.Sync.Vehicles.TryLocateRentedVehicle(pData, vData);
        }

        [RemoteEvent("Vehicles::EVAOWNV")]
        private static void EvacuateOwnedVehicle(Player player, uint vid, bool toHouse, uint pId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            var vInfo = pData.OwnedVehicles.Where(x => x.VID == vid).FirstOrDefault();

            if (vInfo == null)
                return;

            if (vInfo.VehicleData != null && player.Vehicle == vInfo.VehicleData.Vehicle)
                return;

            if (vInfo.LastData.GarageSlot >= 0)
            {
                player.Notify("Vehicle::OIG");

                return;
            }

            if (!pData.HasBankAccount(true))
                return;

            ulong newBalance;

            if (!pData.BankAccount.TryRemoveMoneyDebit(Settings.VEHICLE_EVACUATION_COST, out newBalance, true))
                return;

            if (toHouse)
            {
                var house = pData.OwnedHouses.Where(x => x.Id == pId).FirstOrDefault();

                if (house == null)
                    return;

                var garageData = house.GarageData;

                if (garageData == null)
                    return;

                var garageVehs = house.GetVehiclesInGarage()?.ToList();

                if (garageVehs == null)
                    return;

                var freeSlots = Enumerable.Range(0, garageData.MaxVehicles).ToList();

                if (garageVehs.Count == freeSlots.Count)
                {
                    player.Notify("Garage::NVP");

                    return;
                }

                foreach (var x in garageVehs)
                {
                    freeSlots.Remove(x.VehicleData.LastData.GarageSlot);
                }

                var garageSlot = freeSlots.First();

                house.SetVehicleToGarageOnlyData(vInfo, garageSlot);
            }
            else
            {
                var garage = pData.OwnedGarages.Where(x => x.Id == pId).FirstOrDefault();

                if (garage == null)
                    return;

                var freeSlots = Enumerable.Range(0, garage.StyleData.MaxVehicles).ToList();

                var garageVehs = garage.GetVehiclesInGarage().ToList();

                if (garageVehs.Count == freeSlots.Count)
                {
                    player.Notify("Garage::NVP");

                    return;
                }

                foreach (var x in garageVehs)
                {
                    freeSlots.Remove(x.VehicleData.LastData.GarageSlot);
                }

                var garageSlot = freeSlots.First();

                garage.SetVehicleToGarageOnlyData(vInfo, garageSlot);
            }

            pData.BankAccount.SetDebitBalance(newBalance, null);

            if (vInfo.VehicleData != null)
            {
                vInfo.VehicleData.AttachBoatToTrailer();

                vInfo.VehicleData.Respawn(true);

                vInfo.VehicleData.SetFreezePosition(vInfo.LastData.Position, vInfo.LastData.Heading);

                vInfo.VehicleData.IsInvincible = true;
            }
            else
            {
                vInfo.Spawn();
            }
        }
    }
}
