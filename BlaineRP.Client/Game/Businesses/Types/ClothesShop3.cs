using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.NPCs;
using BlaineRP.Client.Game.UI.CEF.Phone.Apps;
using RAGE;

namespace BlaineRP.Client.Game.Businesses
{
    public class ClothesShop3 : Business
    {
        public ClothesShop3(int id, Vector3 positionInfo, uint price, uint rent, float tax, Utils.Vector4 positionInteract) : base(id,
            positionInfo,
            BusinessTypes.ClothesShop3,
            price,
            rent,
            tax
        )
        {
            Blip = new ExtraBlip(439, positionInteract.Position, Name, 1f, 5, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            Seller = new NPC($"seller_{id}", "", NPC.Types.Talkable, "s_f_m_shop_high", positionInteract.Position, positionInteract.RotationZ, Settings.App.Static.MainDimension)
            {
                SubName = "NPC_SUBNAME_SELLER",
                Data = this,
                DefaultDialogueId = "seller_clothes_greeting_0",
            };

            GPS.AddPosition("clothes", "clothes3", $"clothes_{id}", $"clothess& #{SubId}", new RAGE.Ui.Cursor.Vector2(positionInteract.X, positionInteract.Y));
        }
    }
}