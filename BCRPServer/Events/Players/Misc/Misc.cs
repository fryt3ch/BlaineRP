using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Events.Players.Misc
{
    internal class Other : Script
    {
        [RemoteProc("SW::GRD")]
        private static object GetRetrievableData(Player player, uint key)
        {
            if (player?.Exists != true)
                return null;

            return Sync.World.GetRetrievableData<object>(key, null);
        }

        [RemoteEvent("Players::StopCarry")]
        public static void StopCarry(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var atPlayer = pData.IsAttachedToEntity as Player;

            if (atPlayer?.Exists != true)
            {
                var aData = pData.AttachedEntities.Where(x => x.Type == Sync.AttachSystem.Types.Carry).FirstOrDefault();

                if (aData == null)
                    return;

                var target = Utils.GetEntityById(aData.EntityType, aData.Id);

                if (target?.Exists != true)
                    return;

                player.DetachEntity(target);
            }
            else
            {
                atPlayer.DetachEntity(player);
            }
        }

        [RemoteProc("Players::GoToTrunk")]
        public static byte GoToTrunk(Player player, Vehicle vehicle)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen || pData.HasAnyHandAttachedObject || pData.IsAttachedToEntity != null || pData.IsAnyAnimOn())
                return 0;

            var vData = vehicle.GetMainData();

            if (vData == null)
                return 0;

            if (vData.TrunkLocked)
                return 1;

            if (vData.AttachedObjects.Where(x => x.Type == Sync.AttachSystem.Types.VehicleTrunk).Any())
                return 2;

            vehicle.AttachEntity(player, Sync.AttachSystem.Types.VehicleTrunk, null);

            return 255;
        }

        [RemoteEvent("Players::StopInTrunk")]
        public static void StopInTrunk(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var atVeh = pData.IsAttachedToEntity as Vehicle;

            if (atVeh?.Exists != true)
                return;

            var atData = atVeh.GetAttachmentData(player);

            if (atData == null || atData.Type != Sync.AttachSystem.Types.VehicleTrunk)
                return;

            if (pData.IsKnocked || pData.IsCuffed)
                return;

            atVeh.DetachEntity(player);
        }

        [RemoteEvent("Players::Smoke::Stop")]
        public static void StopSmoke(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            foreach (var x in pData.AttachedObjects)
            {
                if (Game.Items.Cigarette.AttachTypes.Contains(x.Type))
                {
                    player.DetachObject(x.Type);

                    pData.StopFastAnim();

                    break;
                }
            }
        }

        [RemoteEvent("Players::Smoke::Puff")]
        public static void SmokeDoPuff(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            foreach (var x in pData.AttachedObjects)
            {
                if (Game.Items.Cigarette.AttachTypes.Contains(x.Type))
                {
                    pData.PlayAnim(Sync.Animations.FastTypes.SmokePuffCig);

                    player.TriggerEvent("Player::Smoke::Puff");

                    break;
                }
            }
        }

        [RemoteEvent("Players::Smoke::State")]
        public static void SmokeSetState(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            Sync.AttachSystem.AttachmentObjectNet attachData = null;

            foreach (var x in pData.AttachedObjects)
            {
                if (Game.Items.Cigarette.AttachTypes.Contains(x.Type))
                {
                    pData.PlayAnim(Sync.Animations.FastTypes.SmokeTransitionCig);

                    attachData = x;

                    break;
                }
            }

            if (attachData == null)
                return;

            var oppositeType = Game.Items.Cigarette.DependentTypes[attachData.Type];

            NAPI.Task.Run(() =>
            {
                if (player?.Exists != true)
                    return;

                if (player.DetachObject(attachData.Type, false))
                    player.AttachObject(attachData.Model, oppositeType, -1, null);
            }, 500);
        }

        [RemoteEvent("Player::NRPP::TPME")]
        public static void NonRpPrisonTeleportMe(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsFrozen)
                return;

            if (pData.Player.Dimension != Settings.DEMORGAN_DIMENSION)
                return;

            Utils.Demorgan.SetToDemorgan(pData, true);
        }

        [RemoteEvent("Player::COPAR::TPME")]
        public static void PoliceArrestTeleportMe(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsFrozen)
                return;

            var punishment = pData.Punishments.Where(x => x.Type == Sync.Punishment.Types.Arrest && x.IsActive()).FirstOrDefault();

            if (punishment == null)
                return;

            var dataS = punishment.AdditionalData.Split('_');

            var fData = Game.Fractions.Fraction.Get((Game.Fractions.Types)int.Parse(dataS[1])) as Game.Fractions.Police;

            if (fData == null)
                return;

            fData.SetPlayerToPrison(pData, true);
        }

        [RemoteProc("EstAgency::GD")]
        public static string EstateAgencyGetData(Player player, int estAgencyId, int posId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (pData.IsFrozen)
                return null;

            var estAgency = Game.Misc.EstateAgency.Get(estAgencyId);

            if (estAgency == null || posId < 0 || posId >= estAgency.Positions.Length)
                return null;

            if (player.Dimension != Settings.MAIN_DIMENSION || player.Position.DistanceTo(estAgency.Positions[posId]) > 5f)
                return null;

            return $"{estAgency.HouseGPSPrice}";
        }

        [RemoteProc("EstAgency::GPS")]
        public static bool EstateAgencyBuyGps(Player player, int estAgencyId, int posId, byte gpsType)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.IsFrozen)
                return false;

            var estAgency = Game.Misc.EstateAgency.Get(estAgencyId);

            if (estAgency == null || posId < 0 || posId >= estAgency.Positions.Length)
                return false;

            if (player.Dimension != Settings.MAIN_DIMENSION || player.Position.DistanceTo(estAgency.Positions[posId]) > 5f)
                return false;

            if (gpsType == 0)
            {
                ulong newBalance;

                if (!pData.TryRemoveCash(estAgency.HouseGPSPrice, out newBalance, true, null))
                    return false;

                pData.SetCash(newBalance);

                return true;
            }
            else
            {
                return false;
            }
        }

        [RemoteProc("Elevator::TP")]
        public static bool ElevatorTeleport(Player player, uint elevatorIdFrom, uint elevatorIdTo)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen || pData.IsAttachedToEntity != null)
                return false;

            var elevatorFrom = Game.Misc.Elevator.Get(elevatorIdFrom);

            if (elevatorFrom == null)
                return false;

            if (!elevatorFrom.LinkedElevators.Contains(elevatorIdTo))
                return false;

            var elevatorTo = Game.Misc.Elevator.Get(elevatorIdTo);

            if (elevatorTo == null)
                return false;

            if (player.Dimension != elevatorFrom.Dimension || player.Position.DistanceTo(elevatorFrom.Position.Position) > 5f)
                return false;

            if (!elevatorTo.GetCheckFunctionResult(pData, true))
                return false;

            Game.Misc.Elevator.Teleport(pData, elevatorFrom, elevatorTo, true);

            return true;
        }

        [RemoteProc("Door::Lock")]
        public static bool DoorLock(Player player, uint doorId, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            var door = Sync.DoorSystem.GetDoorById(doorId);

            if (door == null)
                return false;

            if (door.Dimension != player.Dimension || player.Position.DistanceTo(door.Position) > 5f)
                return false;

            if (Sync.DoorSystem.Door.GetLockState(doorId) == state)
                return false;

            if (!door.GetCheckFunctionResult(pData))
                return false;

            Sync.DoorSystem.Door.SetLockState(doorId, state, true);

            return true;
        }
    }
}
