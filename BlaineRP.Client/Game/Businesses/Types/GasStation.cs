using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.UI.CEF.Phone.Apps;
using RAGE;

namespace BlaineRP.Client.Game.Businesses
{
    public class GasStation : Business
    {
        public GasStation(int id, Vector3 positionInfo, uint price, uint rent, float tax, Utils.Vector4 positionGas, Utils.Vector4 positionInteract) : base(id,
            positionInfo,
            BusinessTypes.GasStation,
            price,
            rent,
            tax
        )
        {
            Blip = new ExtraBlip(361, positionGas.Position, Name, 0.75f, 47, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            var cs = new Cylinder(new Vector3(positionGas.X, positionGas.Y, positionGas.Z - 1f),
                positionGas.RotationZ,
                2.5f,
                false,
                new Utils.Colour(255, 0, 0, 125),
                Settings.App.Static.MainDimension,
                null
            )
            {
                Data = Id,
                ActionType = ActionTypes.GasStation,
                ApproveType = ApproveTypes.None,
            };

            //this.Seller = new NPC($"seller_{Id}", NamePed, NPC.Types.Seller, ModelPed, PositionPed, HeadingPed, Settings.App.Static.MainDimension, "seller_clothes_greeting_0");

            //this.Seller.Data = this;

            GPS.AddPosition("bizother", "gas", $"bizother_{id}", $"{Name} #{SubId}", new RAGE.Ui.Cursor.Vector2(positionGas.X, positionGas.Y));
        }
    }
}