using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.NPCs;
using RAGE;

namespace BlaineRP.Client.Game.Businesses
{
    public class AeroShop : Business
    {
        public AeroShop(int id, Vector3 positionInfo, uint price, uint rent, float tax, Utils.Vector4 positionInteract) : base(id, positionInfo, BusinessTypes.AeroShop, price, rent, tax)
        {
            Blip = new ExtraBlip(602, positionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            Seller = new NPC($"seller_{id}", "", NPC.Types.Talkable, "ig_jewelass", positionInteract.Position, positionInteract.RotationZ, Settings.App.Static.MainDimension)
            {
                SubName = "NPC_SUBNAME_SELLER",

                Data = this,

                DefaultDialogueId = "seller_clothes_greeting_0",
            };
        }
    }
}