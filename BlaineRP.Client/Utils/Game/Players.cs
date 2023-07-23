using BlaineRP.Client.Extensions.RAGE.Elements;
using RAGE.Elements;

namespace BlaineRP.Client.Utils.Game
{
    internal static class Players
    {

        public static string GetPlayerName(Player player, bool familiarOnly = true, bool dontMask = true, bool includeId = false)
        {
            var pData = Sync.Players.GetData(player);

            if (pData == null)
            {
                var defNameMale = Locale.Get("NPC_NOTFAM_MALE");

                return includeId ? defNameMale + $" ({player?.RemoteId ?? ushort.MaxValue})" : defNameMale;
            }

            var name = familiarOnly ? player.IsFamilliar() && (dontMask || !pData.IsMasked) ? player.Name : Locale.Get(pData.Sex ? "NPC_NOTFAM_MALE" : "NPC_NOTFAM_FEMALE") : player.Name;

            if (includeId)
                return name + $" ({player.RemoteId})";
            else
                return name;
        }

        public static bool IsPlayerFamiliar(Player player, bool fractionToo = true)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);
            var tData = Sync.Players.GetData(player);

            if (pData == null || tData == null)
                return false;

            if (pData.CID == tData.CID)
                return true;

            if (fractionToo)
            {
                return pData.Familiars.Contains(tData.CID) || pData.Fraction == tData.Fraction && pData.Fraction != Data.Fractions.Types.None;
            }
            else
            {
                return pData.Familiars.Contains(tData.CID);
            }
        }

    }
}
