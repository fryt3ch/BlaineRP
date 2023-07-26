using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.NPCs;
using RAGE;

namespace BlaineRP.Client.Game.Businesses
{
    public class TattooShop : Business
    {
        public TattooShop(int id, Vector3 positionInfo, uint price, uint rent, float tax, Utils.Vector4 positionInteract) : base(id, positionInfo, BusinessTypes.TattooShop, price, rent, tax)
        {
            Blip = new ExtraBlip(75, positionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            Seller = new NPC($"tatseller_{id}", "", NPC.Types.Talkable, "u_m_y_tattoo_01", positionInteract.Position, positionInteract.RotationZ, Settings.App.Static.MainDimension)
            {
                SubName = "NPC_SUBNAME_SELLER_TATTOO",

                Data = this,

                DefaultDialogueId = "seller_clothes_greeting_0",
            };
        }
    }
}