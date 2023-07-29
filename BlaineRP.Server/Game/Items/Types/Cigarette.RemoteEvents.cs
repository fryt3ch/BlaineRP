using BlaineRP.Server.Game.Animations;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Items
{
    public partial class Cigarette
    {
        internal class RemoteEvents : Script
        {
            [RemoteEvent("Players::Smoke::Stop")]
            public static void StopSmoke(Player player)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                foreach (AttachmentObjectNet x in pData.AttachedObjects)
                {
                    if (AttachTypes.Contains(x.Type))
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
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                foreach (AttachmentObjectNet x in pData.AttachedObjects)
                {
                    if (AttachTypes.Contains(x.Type))
                    {
                        pData.PlayAnim(FastType.SmokePuffCig, SmokePuffAnimationTime);

                        player.TriggerEvent("Player::Smoke::Puff");

                        break;
                    }
                }
            }

            [RemoteEvent("Players::Smoke::State")]
            public static void SmokeSetState(Player player)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                AttachmentObjectNet attachData = null;

                foreach (AttachmentObjectNet x in pData.AttachedObjects)
                {
                    if (AttachTypes.Contains(x.Type))
                    {
                        pData.PlayAnim(FastType.SmokeTransitionCig, SmokeTransitionAnimationTime);

                        attachData = x;

                        break;
                    }
                }

                if (attachData == null)
                    return;

                AttachmentType oppositeType = DependentTypes[attachData.Type];

                NAPI.Task.Run(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        if (player.DetachObject(attachData.Type, false))
                            player.AttachObject(attachData.Model, oppositeType, -1, null);
                    },
                    (int)SmokeTransitionTime.TotalMilliseconds
                );
            }
        }
    }
}