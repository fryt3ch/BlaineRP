using BlaineRP.Client.Extensions.RAGE;
using BlaineRP.Client.Game.Misc;
using BlaineRP.Client.Game.Wrappers.Colshapes.Enums;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Wrappers.Colshapes.Types
{
    public class Cylinder : ExtraColshape
    {
        public override string ShortData => $"Type: {Type}, Pos: {RAGE.Util.Json.Serialize(Position)}, Radius: {Radius}, Height: {Height}";

        public float Radius { get; set; }
        public float Height { get; set; }

        public Cylinder(Vector3 Position, float Radius, float Height, bool IsVisible, Utils.Colour Colour, uint Dimension, Colshape Colshape = null) : base(ColshapeTypes.Cylinder, IsVisible, Colour, Dimension, Colshape)
        {
            this.Radius = Radius;
            this.Height = Height;

            this.Position = Position;

            if (IsStreamed())
                Streamed.Add(this);
        }

        public override void Draw()
        {
            var diameter = Radius * 2f;

            RAGE.Game.Graphics.DrawMarker(1, Position.X, Position.Y, Position.Z, 0f, 0f, 0f, 1f, 1f, 1f, diameter, diameter, Height, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha, false, false, 2, false, null, null, false);

            if (Settings.User.Other.DebugLabels)
            {
                float screenX = 0f, screenY = 0f;

                if (!Graphics.GetScreenCoordFromWorldCoord(Position, ref screenX, ref screenY))
                    return;

                Graphics.DrawText($"Name: {Name} | Type: {Type} | ID: {Colshape.Id} | IsLocal: {Colshape?.IsLocal == true}", screenX, screenY, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                Graphics.DrawText($"Radius: {Radius} | Height: {Height}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                Graphics.DrawText($"ActionType: {ActionType} | InteractionType: {InteractionType} | Data: {Data}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
            }
        }

        public override bool IsPointInside(Vector3 point)
        {
            if (point.Z < Position.Z || point.Z > Position.Z + Height)
                return false;

            return Position.DistanceIgnoreZ(point) <= Radius;
        }

        public override bool IsStreamed()
        {
            if (!base.IsStreamed())
                return false;

            return Vector3.Distance(Player.LocalPlayer.Position, Position) <= Height + Radius + Settings.App.Profile.Current.Game.StreamDistance;
        }
    }
}