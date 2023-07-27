using System;
using BlaineRP.Server.Game.Management.Animations;
using BlaineRP.Server.Game.Management.Attachments;
using BlaineRP.Server.Sync;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Fractions
{
    public partial class EMS
    {
        internal class RemoteEvents : Script
        {
            [RemoteProc("EMS::BedOcc")]
            private static bool EMSBedOccupy(Player player, int fractionTypeNum, ushort bedIdx)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                var pData = sRes.Data;

                if (!Enum.IsDefined(typeof(Game.Fractions.FractionType), fractionTypeNum))
                    return false;

                var fData = Game.Fractions.Fraction.Get((Game.Fractions.FractionType)fractionTypeNum) as Game.Fractions.EMS;

                if (fData == null)
                    return false;

                var bedInfo = fData.GetBedInfoById(bedIdx);

                if (bedInfo == null || bedInfo.Timer != null)
                    return false;

                if (player.Dimension != Properties.Settings.Static.MainDimension || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked || pData.IsAnyAnimOn())
                    return false;

                if (player.Health > 80)
                    return false;

                if (pData.HasAnyHandAttachedObject || pData.IsAttachedToEntity != null)
                    return false;

                pData.Player.AttachObject(0, AttachmentType.EmsHealingBedFakeAttach, -1, null);

                pData.PlayAnim(GeneralType.BedLie0);

                fData.SetBedAsOccupied(bedIdx, pData);

                return true;
            }

            [RemoteEvent("EMS::BedFree")]
            private static void EMSBedFree(Player player)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                var pData = sRes.Data;

                if (!pData.Player.DetachObject(AttachmentType.EmsHealingBedFakeAttach))
                    return;
            }
        }
    }
}