using System;
using BlaineRP.Server.Game.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Casino.Games
{
    public partial class LuckyWheel
    {
        internal class RemoteEvents : Script
        {
            [RemoteEvent("Casino::LCWS")]
            private static void CasinoLuckyWheelSpin(Player player, int casinoId, int luckyWheelId)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                var casino = CasinoEntity.GetById(casinoId);

                if (casino == null)
                    return;

                LuckyWheel luckyWheel = casino.GetLuckyWheelById(luckyWheelId);

                if (luckyWheel == null)
                    return;

                if (player.Dimension != Properties.Settings.Static.MainDimension || luckyWheel.Position.DistanceTo(player.Position) > 5f)
                    return;

                uint freeLuckyWheelCdHash = NAPI.Util.GetHashKey("CASINO_LW_FREE_0");

                DateTime curTime = Utils.GetCurrentTime();

                if (pData.Info.HasCooldown(freeLuckyWheelCdHash, curTime, out _, out _, out _, 1d))
                {
                    player.NotifyError(Language.Strings.Get("NTFC_COOLDOWN_GEN_3"));

                    return;
                }

                if (!luckyWheel.IsAvailableNow())
                {
                    player.Notify("Casino::LCWAS");

                    return;
                }

                foreach (CasinoEntity x in CasinoEntity.All)
                {
                    foreach (LuckyWheel y in x.LuckyWheels)
                    {
                        if (y.CurrentCID > 0 && y.CurrentCID == pData.CID)
                            return;
                    }
                }

                luckyWheel.Spin(casinoId, luckyWheelId, pData);

                pData.Info.SetCooldown(freeLuckyWheelCdHash, curTime, SpinDefaultCooldown, true);
            }
        }
    }
}