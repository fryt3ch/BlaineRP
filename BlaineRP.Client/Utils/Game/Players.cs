using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Players;
using BlaineRP.Client.Game.Fractions;
using RAGE.Elements;

namespace BlaineRP.Client.Utils.Game
{
    internal static class Players
    {
        public static string GetPlayerName(Player player, bool familiarOnly = true, bool dontMask = true, bool includeId = false)
        {
            var pData = PlayerData.GetData(player);

            if (pData == null)
            {
                string defNameMale = Locale.Get("NPC_NOTFAM_MALE");

                return includeId ? defNameMale + $" ({player?.RemoteId ?? ushort.MaxValue})" : defNameMale;
            }

            string name = familiarOnly
                ? player.IsFamilliar() && (dontMask || !pData.IsMasked) ? player.Name : Locale.Get(pData.Sex ? "NPC_NOTFAM_MALE" : "NPC_NOTFAM_FEMALE")
                : player.Name;

            if (includeId)
                return name + $" ({player.RemoteId})";
            else
                return name;
        }

        public static bool IsPlayerFamiliar(Player player, bool fractionToo = true)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);
            var tData = PlayerData.GetData(player);

            if (pData == null || tData == null)
                return false;

            if (pData.CID == tData.CID)
                return true;

            if (fractionToo)
                return pData.Familiars.Contains(tData.CID) || pData.Fraction == tData.Fraction && pData.Fraction != FractionTypes.None;
            else
                return pData.Familiars.Contains(tData.CID);
        }
    }
}