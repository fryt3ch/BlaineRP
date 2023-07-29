using System.Linq;
using BlaineRP.Server.Game.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Management.Punishments
{
    public class RemoteEvents : Script
    {
        [RemoteEvent("Player::UnpunishMe")]
        private static void UnpunishMe(Player player, uint punishmentId)
        {
            (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            PlayerData pData = sRes.Data;

            Punishment punishment = pData.Punishments.Where(x => x.Id == punishmentId).FirstOrDefault();

            if (punishment == null)
                return;

            if (punishment.IsActive())
                return;

            if (punishment.Type == PunishmentType.Mute)
            {
                pData.IsMuted = false;

                player.TriggerEvent("Player::Punish", punishment.Id, (int)punishment.Type, ushort.MaxValue, -2, null);

                punishment.AmnestyInfo = new Punishment.Amnesty();

                MySQL.UpdatePunishmentAmnesty(punishment);
            }
            else if (punishment.Type == PunishmentType.Warn)
            {
                player.TriggerEvent("Player::Punish", punishment.Id, (int)punishment.Type, ushort.MaxValue, -2, null);

                punishment.AmnestyInfo = new Punishment.Amnesty();

                MySQL.UpdatePunishmentAmnesty(punishment);
            }
            else if (punishment.Type == PunishmentType.FractionMute)
            {
                player.TriggerEvent("Player::Punish", punishment.Id, (int)punishment.Type, ushort.MaxValue, -2, null);

                punishment.AmnestyInfo = new Punishment.Amnesty();

                MySQL.UpdatePunishmentAmnesty(punishment);
            }
        }

        [RemoteEvent("Player::NRPP::TPME")]
        public static void NonRpPrisonTeleportMe(Player player)
        {
            (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            PlayerData pData = sRes.Data;

            if (pData.IsFrozen)
                return;

            if (pData.Player.Dimension != Properties.Settings.Static.DemorganDimension)
                return;

            Utils.Demorgan.SetToDemorgan(pData, true);
        }

        [RemoteEvent("Player::COPAR::TPME")]
        public static void PoliceArrestTeleportMe(Player player)
        {
            (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            PlayerData pData = sRes.Data;

            if (pData.IsFrozen)
                return;

            Punishment punishment = pData.Punishments.Where(x => x.Type == PunishmentType.Arrest && x.IsActive()).FirstOrDefault();

            if (punishment == null)
                return;

            string[] dataS = punishment.AdditionalData.Split('_');

            var fData = Fractions.Fraction.Get((Fractions.FractionType)int.Parse(dataS[1])) as Fractions.Police;

            if (fData == null)
                return;

            fData.SetPlayerToPrison(pData, true);
        }
    }
}