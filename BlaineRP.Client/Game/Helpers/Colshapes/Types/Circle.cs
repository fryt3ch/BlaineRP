using BlaineRP.Client.Extensions.RAGE;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Helpers.Colshapes.Types
{
    public class Circle : ExtraColshape
    {
        public Circle(Vector3 Position, float Radius, bool IsVisible, Utils.Colour Colour, uint Dimension, Colshape Colshape = null) : base(ColshapeTypes.Circle,
            IsVisible,
            Colour,
            Dimension,
            Colshape
        )
        {
            this.Radius = Radius;

            this.Position = Position;

            if (IsStreamed())
                Streamed.Add(this);
        }

        public override string ShortData => $"Type: {Type}, Pos: {RAGE.Util.Json.Serialize(Position)}, Radius: {Radius}";

        public float Radius { get; set; }

        public override void Draw()
        {
            float diameter = Radius * 2;

            RAGE.Game.Graphics.DrawMarker(1,
                Position.X,
                Position.Y,
                Position.Z,
                0f,
                0f,
                0f,
                1f,
                1f,
                1f,
                diameter,
                diameter,
                10f,
                Colour.Red,
                Colour.Green,
                Colour.Blue,
                Colour.Alpha,
                false,
                false,
                2,
                false,
                null,
                null,
                false
            );

            if (Settings.User.Other.DebugLabels)
            {
                float screenX = 0f, screenY = 0f;

                if (!Graphics.GetScreenCoordFromWorldCoord(Position, ref screenX, ref screenY))
                    return;

                Graphics.DrawText($"Name: {Name} | Type: {Type} | ID: {Colshape.Id} | IsLocal: {Colshape?.IsLocal == true}",
                    screenX,
                    screenY,
                    255,
                    255,
                    255,
                    255,
                    0.4f,
                    RAGE.Game.Font.ChaletComprimeCologne,
                    true
                );
                Graphics.DrawText($"Radius: {Radius}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                Graphics.DrawText($"ActionType: {ActionType} | InteractionType: {InteractionType} | Data: {Data}",
                    screenX,
                    screenY += NameTags.Interval / 2,
                    255,
                    255,
                    255,
                    255,
                    0.4f,
                    RAGE.Game.Font.ChaletComprimeCologne,
                    true
                );
            }
        }

        public override bool IsStreamed()
        {
            if (!base.IsStreamed())
                return false;

            return Position.DistanceIgnoreZ(Player.LocalPlayer.Position) <= Radius + Settings.App.Profile.Current.Game.StreamDistance;
        }

        public override bool IsPointInside(Vector3 point)
        {
            return point.DistanceIgnoreZ(Position) <= Radius;
        }
    }
}