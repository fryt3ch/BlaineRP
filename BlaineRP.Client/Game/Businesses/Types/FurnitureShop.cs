using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.NPCs;
using BlaineRP.Client.Game.UI.CEF.Phone.Apps;
using RAGE;

namespace BlaineRP.Client.Game.Businesses
{
    public class FurnitureShop : Business
    {
        public FurnitureShop(int id, Vector3 positionInfo, uint price, uint rent, float tax, Utils.Vector4 positionInteract) : base(id, positionInfo, BusinessTypes.FurnitureShop, price, rent, tax)
        {
            Blip = new ExtraBlip(779, positionInteract.Position, Name, 1f, 8, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            Seller = new NPC($"seller_{id}", "", NPC.Types.Talkable, "ig_natalia", positionInteract.Position, positionInteract.RotationZ, Settings.App.Static.MainDimension)
            {
                SubName = "NPC_SUBNAME_SELLER",

                Data = this,

                DefaultDialogueId = "seller_furn_g_0",
            };

            GPS.AddPosition("bizother", "furn", $"bizother_{id}", $"{Name} #{SubId}", new RAGE.Ui.Cursor.Vector2(positionInteract.X, positionInteract.Y));
        }
    }
}