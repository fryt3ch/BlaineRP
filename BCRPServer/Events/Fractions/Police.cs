using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Events.Fractions
{
    internal class Police : Script
    {
        [RemoteProc("Police::Cuff")]
        private static byte Cuff(Player player, Player target, byte stateN)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            var tData = target?.GetMainData();

            if (tData == null || tData == pData)
                return 0;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return 0;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (fData == null)
                return 0;

            var state = stateN == 0 ? false : true;

            if (stateN == 2)
            {
                state = !tData.IsCuffed;
            }
            else if (state == tData.IsCuffed)
            {
                return 2;
            }

            if (tData.IsKnocked || tData.IsFrozen)
                return 1;

            tData.IsCuffed = state;

            if (state)
            {
                if (tData.IsAttachedToEntity is Entity entity)
                {
                    entity.DetachEntity(tData.Player);
                }

                tData.Player.DetachAllEntities();
                tData.Player.DetachAllObjectsInHand();

                tData.Player.AttachObject(Sync.AttachSystem.Models.Cuffs, Sync.AttachSystem.Types.Cuffs, -1, null);

                tData.PlayAnim(Sync.Animations.GeneralTypes.CuffedStatic0);

                tData.Player.NotifyWithPlayer("Cuffs::0_0", player);
            }
            else
            {
                if (tData.IsAttachedToEntity is Entity entity)
                {
                    entity.DetachEntity(tData.Player);
                }

                if (tData.Player.DetachObject(Sync.AttachSystem.Types.Cuffs))
                {
                    tData.Player.NotifyWithPlayer("Cuffs::0_1", player);
                }
                else
                {
                    tData.Player.DetachObject(Sync.AttachSystem.Types.CableCuffs);

                    tData.Player.NotifyWithPlayer("Cuffs::1_1", player);
                }

                if (tData.GeneralAnim == Sync.Animations.GeneralTypes.CuffedStatic0)
                    tData.StopGeneralAnim();
            }

            return byte.MaxValue;
        }

        [RemoteProc("Police::Escort")]
        private static byte Escort(Player player, Player target, byte stateN)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            var tData = target?.GetMainData();

            if (tData == null || tData == pData)
                return 0;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return 0;

            if (!tData.IsCuffed)
                return 1;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (fData == null)
                return 0;

            var state = stateN == 0 ? false : true;

            var curAttachState = player.GetAttachmentData(target);

            if (curAttachState != null && curAttachState.Type == Sync.AttachSystem.Types.PoliceEscort)
            {
                if (stateN == 2 || !state)
                {
                    if (pData.GeneralAnim == Sync.Animations.GeneralTypes.PoliceEscort0)
                        pData.StopGeneralAnim();

                    player.DetachEntity(tData.Player);

                    // notify both
                }
                else
                {
                    return 2;
                }
            }
            else
            {
                if (tData.IsAttachedToEntity != null)
                    return 2;

                if (pData.AttachedEntities.Where(x => x.Type == Sync.AttachSystem.Types.PoliceEscort).Any())
                    return 3;

                if (stateN == 2 || state)
                {
                    pData.PlayAnim(Sync.Animations.GeneralTypes.PoliceEscort0);

                    player.AttachEntity(target, Sync.AttachSystem.Types.PoliceEscort);

                    // notify both
                }
                else
                {
                    return 2;
                }
            }

            return byte.MaxValue;
        }
    }
}
