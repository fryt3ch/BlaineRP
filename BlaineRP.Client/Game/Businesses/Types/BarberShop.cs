using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.NPCs;
using RAGE;

namespace BlaineRP.Client.Game.Businesses
{
    public class BarberShop : Business
    {
        public BarberShop(int id, Vector3 positionInfo, uint price, uint rent, float tax, Utils.Vector4 positionInteract) : base(id, positionInfo, BusinessTypes.BarberShop, price, rent, tax)
        {
            Blip = new ExtraBlip(71, positionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            Seller = new NPC($"seller_{id}", "", NPC.Types.Talkable, "s_f_y_clubbar_01", positionInteract.Position, positionInteract.RotationZ, Settings.App.Static.MainDimension)
            {
                SubName = "NPC_SUBNAME_SELLER_BARBER",

                Data = this,

                DefaultDialogueId = "seller_clothes_greeting_0",
            };
        }
    }
}