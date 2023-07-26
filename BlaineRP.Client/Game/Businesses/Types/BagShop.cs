using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.NPCs;
using BlaineRP.Client.Game.UI.CEF.Phone.Apps;
using RAGE;

namespace BlaineRP.Client.Game.Businesses
{
    public class BagShop : Business
    {
        public BagShop(int id, Vector3 positionInfo, uint price, uint rent, float tax, Utils.Vector4 positionInteract) : base(id,
            positionInfo,
            BusinessTypes.BagShop,
            price,
            rent,
            tax
        )
        {
            Blip = new ExtraBlip(377, positionInteract.Position, Name, 1f, 3, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            (string Model, string Name) npcParams = SubId >= NpCs.Length ? NpCs[0] : NpCs[SubId];

            Seller = new NPC($"vendor_{id}",
                npcParams.Name,
                NPC.Types.Talkable,
                npcParams.Model,
                positionInteract.Position,
                positionInteract.RotationZ,
                Settings.App.Static.MainDimension
            )
            {
                SubName = "NPC_SUBNAME_VENDOR",
                Data = this,
                DefaultDialogueId = "seller_bags_preprocess",
            };

            GPS.AddPosition("clothes", "clothesother", $"clothes_{id}", $"{Name} #{SubId}", new RAGE.Ui.Cursor.Vector2(positionInteract.X, positionInteract.Y));
        }

        private static (string Model, string Name)[] NpCs { get; set; } = new (string, string)[]
        {
            ("a_m_o_ktown_01", "Чжан"),
        };
    }
}