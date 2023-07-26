using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.UI.CEF.Phone.Apps;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Businesses
{
    public class TuningShop : Business
    {
        public ExtraColshape EnteranceColshape { get; set; }

        public TuningShop(int id, Vector3 positionInfo, uint price, uint rent, float tax, Utils.Vector4 positionInteract) : base(id, positionInfo, BusinessTypes.TuningShop, price, rent, tax)
        {
            Blip = new ExtraBlip(72, positionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            var tPos = new Vector3(positionInteract.Position.X, positionInteract.Position.Y, positionInteract.Position.Z - 0.5f);

            EnteranceColshape = new Cylinder(tPos, 2.5f, 2f, false, new Utils.Colour(255, 0, 0, 125), Settings.App.Static.MainDimension, null)
            {
                ApproveType = ApproveTypes.OnlyServerVehicleDriver,

                InteractionType = InteractionTypes.TuningEnter,
                ActionType = ActionTypes.TuningEnter,

                Data = this,
            };

            new Marker(44, new Vector3(tPos.X, tPos.Y, tPos.Z + 0.75f), 1f, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new RGBA(255, 255, 255), true, Settings.App.Static.MainDimension);

            GPS.AddPosition("bizother", "tuning", $"bizother_{id}", $"{Name} #{SubId}", new RAGE.Ui.Cursor.Vector2(positionInteract.X, positionInteract.Y));
        }
    }
}