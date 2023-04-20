using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Events.Players
{
    internal class Misc : Script
    {
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
                var aData = pData.AttachedEntities.Where(x => x.Type == Sync.AttachSystem.Types.Carry && x.EntityType == EntityType.Player && x.Id >= 0).FirstOrDefault();

                if (aData == null)
                    return;

                var target = Utils.FindReadyPlayerOnline((uint)aData.Id);

                if (target?.Player?.Exists != true)
                    return;

                player.DetachEntity(target.Player);
            }
            else
            {
                atPlayer.DetachEntity(player);
            }
        }

        [RemoteEvent("Players::GoToTrunk")]
        public static void GoToTrunk(Player player, Vehicle vehicle)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen || pData.HasAnyHandAttachedObject || pData.IsAttachedToEntity != null || pData.IsAnyAnimOn())
                return;

            var vData = vehicle.GetMainData();

            if (vData == null)
                return;

            vehicle.AttachEntity(player, Sync.AttachSystem.Types.VehicleTrunk);
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

            if (pData.Player.Dimension != Utils.Dimensions.Demorgan)
                return;

            Utils.Demorgan.SetToDemorgan(pData, true);
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

            if (player.Dimension != Utils.Dimensions.Main || player.Position.DistanceTo(estAgency.Positions[posId]) > 5f)
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

            if (player.Dimension != Utils.Dimensions.Main || player.Position.DistanceTo(estAgency.Positions[posId]) > 5f)
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
    }
}
