using System.Collections.Generic;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.UI.CEF.Phone.Apps;

namespace BlaineRP.Client.Game.Misc
{
    public partial class ATM
    {
        public static Dictionary<int, ATM> All = new Dictionary<int, ATM>();

        public ATM(int Id, Utils.Vector4 PositionParams)
        {
            this.Id = Id;

            All.Add(Id, this);

            Colshape = new Sphere(PositionParams.Position, PositionParams.RotationZ, false, new Utils.Colour(255, 0, 0, 255), Settings.App.Static.MainDimension, null)
            {
                Data = this,
                InteractionType = InteractionTypes.ATM,
                ActionType = ActionTypes.ATM,
                Name = $"atm_{Id}",
            };

            Blip = new ExtraBlip(108, PositionParams.Position, Locale.Property.AtmNameDef, 0.4f, 25, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            GPS.AddPosition("money", "atms", $"atm_{Id}", $"atm& #{Id + 1}", new RAGE.Ui.Cursor.Vector2(PositionParams.Position.X, PositionParams.Position.Y));
        }

        public int Id { get; set; }

        public ExtraColshape Colshape { get; set; }

        public ExtraBlip Blip { get; set; }
    }
}