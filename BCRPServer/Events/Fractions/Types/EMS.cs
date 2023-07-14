using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Events.Fractions
{
    internal class EMS : Script
    {
        [RemoteProc("EMS::BedOcc")]
        private static bool EMSBedOccupy(Player player, int fractionTypeNum, ushort bedIdx)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(Game.Fractions.Types), fractionTypeNum))
                return false;

            var fData = Game.Fractions.Fraction.Get((Game.Fractions.Types)fractionTypeNum) as Game.Fractions.EMS;

            if (fData == null)
                return false;

            var bedInfo = fData.GetBedInfoById(bedIdx);

            if (bedInfo == null || bedInfo.Timer != null)
                return false;

            if (player.Dimension != Settings.CurrentProfile.Game.MainDimension || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked || pData.IsAnyAnimOn())
                return false;

            if (player.Health > 80)
                return false;

            if (pData.HasAnyHandAttachedObject || pData.IsAttachedToEntity != null)
                return false;

            pData.Player.AttachObject(0, Sync.AttachSystem.Types.EmsHealingBedFakeAttach, -1, null);

            pData.PlayAnim(Sync.Animations.GeneralTypes.BedLie0);

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

            if (!pData.Player.DetachObject(Sync.AttachSystem.Types.EmsHealingBedFakeAttach))
                return;
        }
    }
}
