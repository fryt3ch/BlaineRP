using System.Collections.Generic;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Helpers.Colshapes.Types
{
    public class Cuboid : Polygon
    {
        public Cuboid(Vector3 Position, float Width, float Depth, float Height, float Heading, bool IsVisible, Utils.Colour Colour, uint Dimension = 7, Colshape Colshape = null) :
            base(ColshapeTypes.Cuboid, GetBaseVertices(Position, Width, Depth, Height), Height, Heading, IsVisible, Colour, Dimension, Colshape)
        {
            this.Width = Width;
            this.Depth = Depth;
        }

        public override string ShortData => $"Type: {Type}, CenterPos: {RAGE.Util.Json.Serialize(Position)}, Width: {Width}, Depth: {Depth}, Height: {Height}, Heading: {Heading}";

        public float Width { get; set; }

        public float Depth { get; set; }

        public static List<Vector3> GetBaseVertices(Vector3 centerPos, float width, float depth, float height)
        {
            float zCoord = centerPos.Z - height / 2;

            float width2 = width / 2f;
            float depth2 = depth / 2f;

            return new List<Vector3>()
            {
                new Vector3(centerPos.X - width2, centerPos.Y - depth2, zCoord),
                new Vector3(centerPos.X + width2, centerPos.Y - depth2, zCoord),
                new Vector3(centerPos.X + width2, centerPos.Y + depth2, zCoord),
                new Vector3(centerPos.X - width2, centerPos.Y + depth2, zCoord),
            };
        }

        public void SetWidth(float value)
        {
            Width = value;

            value /= 2f;

            float depth2 = Depth / 2f;

            Vertices[0].X = Position.X - value;
            Vertices[1].X = Position.X + value;
            Vertices[2].X = Position.X + value;
            Vertices[3].X = Position.X - value;

            Vertices[0].Y = Position.Y - depth2;
            Vertices[1].Y = Position.Y - depth2;
            Vertices[2].Y = Position.Y + depth2;
            Vertices[3].Y = Position.Y + depth2;

            UpdatePolygonCenterAndMaxRange();

            float heading = Heading;

            Heading = 0f;

            SetHeading(heading);
        }

        public void SetDepth(float value)
        {
            Depth = value;

            value /= 2f;

            float width2 = Width / 2f;

            Vertices[0].Y = Position.Y - value;
            Vertices[1].Y = Position.Y - value;
            Vertices[2].Y = Position.Y + value;
            Vertices[3].Y = Position.Y + value;

            Vertices[0].X = Position.X - width2;
            Vertices[1].X = Position.X + width2;
            Vertices[2].X = Position.X + width2;
            Vertices[3].X = Position.X - width2;

            UpdatePolygonCenterAndMaxRange();

            float heading = Heading;

            Heading = 0f;

            SetHeading(heading);
        }
    }
}