using BlaineRP.Client.Game.Misc;
using BlaineRP.Client.Game.Wrappers.Colshapes.Enums;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Wrappers.Colshapes.Types
{
    public class Sphere : ExtraColshape
    {
        public override string ShortData => $"Type: {Type}, Pos: {RAGE.Util.Json.Serialize(Position)}, Radius: {Radius}";

        public float Radius { get; set; }

        public Sphere(Vector3 Position, float Radius, bool IsVisible, Utils.Colour Colour, uint Dimension, Colshape Colshape = null) : base(ColshapeTypes.Sphere, IsVisible, Colour, Dimension, Colshape)
        {
            this.Radius = Radius;

            this.Position = Position;

            if (IsStreamed())
                Streamed.Add(this);
        }

        public override void Draw()
        {
            Graphics.DrawSphere(Position, Radius, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha / 255f);

            if (Settings.User.Other.DebugLabels)
            {
                float screenX = 0f, screenY = 0f;

                if (!Graphics.GetScreenCoordFromWorldCoord(Position, ref screenX, ref screenY))
                    return;

                Graphics.DrawText($"Name: {Name} | Type: {Type} | ID: {Colshape.Id} | IsLocal: {Colshape?.IsLocal == true}", screenX, screenY, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                Graphics.DrawText($"Radius: {Radius}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                Graphics.DrawText($"ActionType: {ActionType} | InteractionType: {InteractionType} | Data: {Data}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
            }
        }

        public override bool IsStreamed()
        {
            if (!base.IsStreamed())
                return false;

            return Vector3.Distance(Player.LocalPlayer.Position, Position) <= Radius + Settings.App.Profile.Current.Game.StreamDistance;
        }

        public override bool IsPointInside(Vector3 point) => Vector3.Distance(point, Position) <= Radius;
    }
}