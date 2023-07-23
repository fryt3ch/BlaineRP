using RAGE.Elements;

namespace BlaineRP.Client.Extensions.RAGE.Elements
{
    public static class BlipExtensions
    {
        public static void SetName(this Blip blip, string name)
        {
            if (blip == null || !blip.DoesExist())
                return;

            global::RAGE.Game.Ui.BeginTextCommandSetBlipName("BRP_AEBLPT");

            global::RAGE.Game.Ui.AddTextComponentSubstringPlayerName(name);

            global::RAGE.Game.Ui.EndTextCommandSetBlipName(blip.Handle);
        }
    }
}