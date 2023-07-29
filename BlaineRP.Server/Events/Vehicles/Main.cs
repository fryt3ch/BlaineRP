using BlaineRP.Server.Sync;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Vehicles;
using BlaineRP.Server.Game.EntitiesData.Vehicles.Static;
using BlaineRP.Server.Game.Inventory;
using BlaineRP.Server.Game.Misc;
using BlaineRP.Server.Game.Quests;

namespace BlaineRP.Server.Events.Vehicles
{
    class Main : Script
    {
        #region Player Enter Vehicle
        [ServerEvent(Event.PlayerEnterVehicle)]
        private static void PlayerEntered(Player player, GTANetworkAPI.Vehicle veh, sbyte seatId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (pData.IsFrozen || ((pData.IsCuffed || pData.IsKnocked) && seatId < 1))
            {
                player.WarpOutOfVehicle();

                return;
            }

            if (seatId < 0 || seatId >= veh.MaxOccupants || (pData.VehicleSeat < 0 && vData.Locked) || veh.GetEntityInVehicleSeat(seatId) != null)
            {
                player.WarpOutOfVehicle();

                return;
            }

            Game.Items.Weapon activeWeapon;
            GroupTypes activeWeaponGroup;
            int activeWeaponSlot;

            if (pData.TryGetActiveWeapon(out activeWeapon, out activeWeaponGroup, out activeWeaponSlot))
            {
                if (!activeWeapon.Data.CanUseInVehicle)
                    pData.InventoryAction(activeWeaponGroup, activeWeaponSlot, 5);
            }

            player.TriggerEvent("Vehicles::Enter", vData.FuelLevel, vData.Mileage);

            pData.VehicleSeat = seatId;

            if (seatId == 0)
            {
                if (vData.OwnerType == OwnerTypes.PlayerRentJob)
                {
                    if (vData.OwnerID == 0)
                    {
                        if (pData.RentedJobVehicle == null)
                        {
                            if (vData.Job is Game.Jobs.Job jobData && jobData is Game.Jobs.IVehicleRelated jobDataVeh)
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
                else if (vData.OwnerType == OwnerTypes.PlayerDrivingSchool)
                {
                    if (vData.OwnerID == 0 && pData.Info.Quests.GetValueOrDefault(QuestType.DRSCHOOL0) is Quest quest && quest.Step == 0)
                    {
                        LicenseType licType;

                        for (int i = 0; i < DrivingSchool.All.Count; i++)
                        {
                            var x = DrivingSchool.All[i];

                            if (x.Vehicles.TryGetValue(vData.Info, out licType))
                            {
                                var rLicType = int.Parse(quest.CurrentData);

                                if (rLicType != (int)licType)
                                {
                                    player.Notify("DriveS::NPTT");

                                    player.WarpOutOfVehicle();

                                    return;
                                }

                                vData.OwnerID = pData.CID;

                                quest.UpdateStep(pData.Info, 1, 0, $"{vData.Vehicle.Id}&{i + 1}&{rLicType}");

                                break;
                            }
                        }
                    }
                    else
                    {
                        player.Notify("DriveS::NPTT");

                        player.WarpOutOfVehicle();
                    }
                }
            }

            if (vData.OwnerType == OwnerTypes.PlayerRent || vData.OwnerType == OwnerTypes.PlayerRentJob)
            {
                if (vData.OwnerID == pData.CID)
                    vData.CancelDeletionTask();
            }
        }
        #endregion

        #region Player Exit Vehicle
        [ServerEvent(Event.PlayerExitVehicle)]
        private static void PlayerExited(Player player, GTANetworkAPI.Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            Sync.Vehicles.OnPlayerLeaveVehicle(pData, vData);
        }
        #endregion

        [ServerEvent(Event.VehicleDeath)]
        private static void VehicleDeath(GTANetworkAPI.Vehicle veh)
        {
            if (veh?.Exists != true)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            vData.IsDead = true;

            if (vData.EngineOn)
                vData.EngineOn = false;

            if (vData.OwnerType == OwnerTypes.PlayerRentJob || vData.OwnerType == OwnerTypes.PlayerRent || vData.OwnerType == OwnerTypes.PlayerDrivingSchool)
            {
                vData.Delete(false);
            }

            //Console.WriteLine($"{vData.VID} died - {veh.Health}");
        }

        [RemoteEvent("votc")]
        private static void VehicleTrailerChange(Player player, GTANetworkAPI.Vehicle veh, GTANetworkAPI.Vehicle trailer)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (veh.Controller != player)
                return;

            if (trailer == null)
            {
                var atVeh = vData.IsAttachedTo as GTANetworkAPI.Vehicle;

                if (atVeh?.Exists != true)
                    return;

                var atData = atVeh.GetAttachmentData(veh);

                if (atData == null || (atData.Type != AttachmentType.VehicleTrailerObjBoat))
                    return;

                atVeh.DetachEntity(veh);

                Console.WriteLine("trailer detached");
            }
            else
            {
                var tData = trailer.GetMainData();

                if (tData == null)
                    return;

                var atData = vData.IsAttachedTo;

                if (atData != null)
                    return;

                if (tData.Data.Type == VehicleTypes.Boat)
                {
                    if (tData.AttachedObjects.Where(x => x.Type == AttachmentType.TrailerObjOnBoat).Any())
                    {
                        if (!tData.CanManipulate(pData, true))
                            return;

                        if (trailer.AttachEntity(veh, AttachmentType.VehicleTrailerObjBoat, null))
                        {
                            Console.WriteLine("trailer attached");
                        }
                    }
                }
                else
                {
                    return;
                }
            }
        }
    }
}
