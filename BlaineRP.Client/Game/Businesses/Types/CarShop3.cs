using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.NPCs;
using RAGE;

namespace BlaineRP.Client.Game.Businesses
{
    public class CarShop3 : Business
    {
        public CarShop3(int id, Vector3 positionInfo, uint price, uint rent, float tax, Utils.Vector4 positionInteract) : base(id, positionInfo, BusinessTypes.CarShop3, price, rent, tax)
        {
            Blip = new ExtraBlip(523, positionInteract.Position, Name, 1f, 5, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            Seller = new NPC($"seller_{id}", "", NPC.Types.Talkable, "csb_anita", positionInteract.Position, positionInteract.RotationZ, Settings.App.Static.MainDimension)
            {
                SubName = "NPC_SUBNAME_SELLER",

                Data = this,

                DefaultDialogueId = "seller_clothes_greeting_0",
            };
        }
    }
}