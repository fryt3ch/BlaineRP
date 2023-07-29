using GTANetworkAPI;
using System;
using System.Linq;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.Management;
using BlaineRP.Server.Game.Management.Punishments;
using BlaineRP.Server.Game.World;
using BlaineRP.Server.Sync;

namespace BlaineRP.Server.Events.Players.Misc
{
    internal class Other : Script
    {
        [RemoteProc("SW::GRD")]
        private static object GetRetrievableData(Player player, uint key)
        {
            if (player?.Exists != true)
                return null;

            return Game.World.Service.GetRetrievableData<object>(key, null);
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
                var aData = pData.AttachedEntities.Where(x => x.Type == AttachmentType.Carry).FirstOrDefault();

                if (aData == null)
                    return;

                var target = Utils.GetEntityById(aData.EntityType, aData.Id);

                if (target?.Exists != true)
                    return;

                player.DetachEntity(target);
            }
            else
            {
                var atData = atPlayer.GetAttachmentData(pData.Player);

                if (atData != null && atData.Type == AttachmentType.Carry)
                {
                    atPlayer.DetachEntity(player);
                }
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

            if (vData.AttachedObjects.Where(x => x.Type == AttachmentType.VehicleTrunk).Any())
                return 2;

            vehicle.AttachEntity(player, AttachmentType.VehicleTrunk, null);

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

            if (atData == null || atData.Type != AttachmentType.VehicleTrunk)
                return;

            if (pData.IsKnocked || pData.IsCuffed)
                return;

            atVeh.DetachEntity(player);
        }

        [RemoteProc("Misc::GetPing")]
        public static int GetPing(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return int.MinValue;

            var pData = sRes.Data;

            return player.Ping;
        }
    }
}
