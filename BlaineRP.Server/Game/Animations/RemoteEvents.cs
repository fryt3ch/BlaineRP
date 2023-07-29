using System;
using BlaineRP.Server.Game.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Animations
{
    public class RemoteEvents : Script
    {
        [RemoteEvent("Players::PFA")]
        public static void PlayFastAnim(Player player, int anim)
        {
            (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            PlayerData pData = sRes.Data;

            if (!Enum.IsDefined(typeof(FastType), anim))
                return;

            var aType = (FastType)anim;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen || pData.HasAnyHandAttachedObject || pData.IsAnyAnimOn())
                return;

            if (aType == FastType.Whistle)
                pData.PlayAnim(aType, Properties.Settings.Static.WhistleAnimationTime);
        }

        [RemoteEvent("Players::SFTA")]
        public static void StopFastTimeoutedAnim(Player player)
        {
            (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            PlayerData pData = sRes.Data;

            pData.StopFastAnim();
        }

        [RemoteEvent("Players::SetWalkstyle")]
        public static void SetWalkstyle(Player player, int walkstyle)
        {
            (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            PlayerData pData = sRes.Data;

            if (!Enum.IsDefined(typeof(WalkstyleType), walkstyle))
                return;

            pData.SetWalkstyle((WalkstyleType)walkstyle);
        }

        [RemoteEvent("Players::SetEmotion")]
        public static void SetEmotion(Player player, int emotion)
        {
            (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            PlayerData pData = sRes.Data;

            if (!Enum.IsDefined(typeof(EmotionType), emotion))
                return;

            pData.SetEmotion((EmotionType)emotion);
        }

        [RemoteEvent("Players::SetAnim")]
        public static void SetAnim(Player player, int anim)
        {
            (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            PlayerData pData = sRes.Data;

            if (!Enum.IsDefined(typeof(OtherType), anim))
                return;

            var aType = (OtherType)anim;

            if (pData.OtherAnim == aType)
                return;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen || pData.HasAnyHandAttachedObject || pData.GeneralAnim != GeneralType.None || pData.FastAnim != FastType.None)
                return;

            pData.PlayAnim((OtherType)anim);
        }
    }
}