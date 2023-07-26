using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.NPCs;
using RAGE;

namespace BlaineRP.Client.Game.Businesses
{
    public class MotoShop : Business
    {
        public MotoShop(int id, Vector3 positionInfo, uint price, uint rent, float tax, Utils.Vector4 positionInteract) : base(id, positionInfo, BusinessTypes.MotoShop, price, rent, tax)
        {
            Blip = new ExtraBlip(522, positionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            Seller = new NPC($"seller_{id}", "", NPC.Types.Talkable, "u_m_y_sbike", positionInteract.Position, positionInteract.RotationZ, Settings.App.Static.MainDimension)
            {
                SubName = "NPC_SUBNAME_SELLER",

                Data = this,

                DefaultDialogueId = "seller_clothes_greeting_0",
            };
        }
    }
}