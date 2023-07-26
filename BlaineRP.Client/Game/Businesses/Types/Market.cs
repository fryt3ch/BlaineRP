using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.NPCs;
using BlaineRP.Client.Game.UI.CEF.Phone.Apps;
using RAGE;

namespace BlaineRP.Client.Game.Businesses
{
    public class Market : Business
    {
        public Market(int id, Vector3 positionInfo, uint price, uint rent, float tax, Utils.Vector4 positionInteract) : base(id,
            positionInfo,
            BusinessTypes.Market,
            price,
            rent,
            tax
        )
        {
            Blip = new ExtraBlip(52, positionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            Seller = new NPC($"seller_{id}", "", NPC.Types.Talkable, "mp_m_shopkeep_01", positionInteract.Position, positionInteract.RotationZ, Settings.App.Static.MainDimension)
            {
                SubName = "NPC_SUBNAME_SELLER",
                Data = this,
                DefaultDialogueId = "seller_clothes_greeting_0",
            };

            GPS.AddPosition("bizother", "market", $"bizother_{id}", $"{Name} #{SubId}", new RAGE.Ui.Cursor.Vector2(positionInteract.X, positionInteract.Y));
        }
    }
}