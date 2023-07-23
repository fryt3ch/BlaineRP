using RAGE.Elements;

namespace BlaineRP.Client.Extensions.RAGE.Elements
{
    public static class MapObjectExtensions
    {
        public static void SetLightColour(this MapObject mObj, byte r, byte g, byte b) => global::RAGE.Game.Invoker.Invoke(0x5F048334B4A4E774, mObj.Handle, true, r, g, b);

        public static void SetLightColour(this MapObject mObj, Utils.Colour rgb) => mObj.SetLightColour(rgb.Red, rgb.Green, rgb.Blue);

        public static void SetTotallyInvincible(this MapObject obj, bool state)
        {
            if (state)
            {
                obj.SetDisableFragDamage(true);

                obj.SetCanBeDamaged(false);
                obj.SetInvincible(true);
            }
            else
            {
                obj.SetDisableFragDamage(false);

                obj.SetCanBeDamaged(true);
                obj.SetInvincible(false);
            }
        }
    }
}