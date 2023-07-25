using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE;
using BlaineRP.Client.Game.Misc;
using BlaineRP.Client.Game.Wrappers.Colshapes.Enums;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Wrappers.Colshapes.Types
{
    public class Polygon : ExtraColshape
    {
        public override string ShortData => $"Type: {Type}, Vertices: {RAGE.Util.Json.Serialize(Vertices)}, Height: {Height}, Heading: {Heading}";

        public float MaxRange { get; set; }

        public float Height { get; set; }

        public float Heading { get; set; }

        public List<Vector3> Vertices { get; set; }

        public bool Is3D => Height > 0;

        protected Polygon(ColshapeTypes Type, List<Vector3> Vertices, float Height, float Heading, bool IsVisible, Utils.Colour Colour, uint Dimension, Colshape Colshape = null) : base(Type, IsVisible, Colour, Dimension, Colshape)
        {
            this.Height = Height;

            this.Heading = 0;

            this.Vertices = Vertices;

            this.Position = GetCenterPosition(Vertices, Height);

            UpdatePolygonCenterAndMaxRange();

            SetHeading(Heading);

            if (IsStreamed())
                Streamed.Add(this);
        }

        public Polygon(List<Vector3> Vertices, float Height, float Heading, bool IsVisible, Utils.Colour Colour, uint Dimension, Colshape Colshape = null) : this(ColshapeTypes.Polygon, Vertices, Height, Heading, IsVisible, Colour, Dimension, Colshape)
        {

        }

        protected void UpdatePolygonCenterAndMaxRange()
        {
            Vector3 centerPos = new Vector3(0, 0, 0);

            for (int i = 0; i < Vertices.Count; i++)
            {
                centerPos.X += Vertices[i].X;
                centerPos.Y += Vertices[i].Y;
                centerPos.Z += Vertices[i].Z;
            }

            centerPos.X /= Vertices.Count;
            centerPos.Y /= Vertices.Count;
            centerPos.Z /= Vertices.Count;

            centerPos.Z += Height / 2;

            Position = centerPos;
            MaxRange = Vertices.Max(x => x.DistanceTo(centerPos));
        }

        public void SetHeading(float heading) => Rotate(heading - Heading);

        public static Vector3 GetCenterPosition(List<Vector3> vertices, float height)
        {
            Vector3 centerPos = new Vector3(0, 0, 0);

            for (int i = 0; i < vertices.Count; i++)
            {
                centerPos.X += vertices[i].X;
                centerPos.Y += vertices[i].Y;
                centerPos.Z += vertices[i].Z;
            }

            centerPos.X /= vertices.Count;
            centerPos.Y /= vertices.Count;
            centerPos.Z /= vertices.Count;

            centerPos.Z += height / 2;

            return centerPos;
        }

        public void Rotate(float angle)
        {
            for (int i = 0; i < Vertices.Count; i++)
                Geometry.RotatePoint(Vertices[i], Position, angle);

            Heading += angle;
        }

        public override void SetPosition(Vector3 position)
        {
            float diffX = position.X - Position.X;
            float diffY = position.Y - Position.Y;
            float diffZ = position.Z - Position.Z;

            if (diffX == 0f && diffY == 0f && diffZ == 0f)
                return;

            for (int i = 0; i < Vertices.Count; i++)
            {
                var curVertice = Vertices[i];

                curVertice.X += diffX;
                curVertice.Y += diffY;
                curVertice.Z += diffZ;

                Vertices[i] = curVertice;
            }

            base.SetPosition(position);
        }

        public void SetHeight(float height)
        {
            if (height < 0f)
                height = 0;

            Height = height;

            UpdatePolygonCenterAndMaxRange();
        }

        public void AddVertice(Vector3 vertice)
        {
            if (Type == ColshapeTypes.Cuboid)
                return;

            if (!Is3D)
                vertice.Z = Vertices[0].Z;

            Vertices.Add(vertice);

            UpdatePolygonCenterAndMaxRange();
        }

        public void InsertVertice(int idx, Vector3 vertice)
        {
            if (Type == ColshapeTypes.Cuboid)
                return;

            if (!Is3D)
                vertice.Z = Vertices[0].Z;

            if (idx >= Vertices.Count)
                return;

            Vertices.Insert(idx, vertice);

            UpdatePolygonCenterAndMaxRange();
        }

        public void RemoveVertice(int verticeId)
        {
            if (Type == ColshapeTypes.Cuboid)
                return;

            if (verticeId < 0 || verticeId >= Vertices.Count)
                return;

            Vertices.RemoveAt(verticeId);

            if (Vertices.Count == 0)
            {
                Destroy();

                return;
            }

            UpdatePolygonCenterAndMaxRange();
        }

        public override bool IsStreamed()
        {
            if (!base.IsStreamed())
                return false;

            return Position.DistanceIgnoreZ(Player.LocalPlayer.Position) <= MaxRange + Settings.App.Profile.Current.Game.StreamDistance;
        }

        public override void Draw()
        {
            float screenX = 0f, screenY = 0f;

            var vertIdLimiter = Vertices.Count <= 50 ? 1 : 10;

            if (Vertices.Count == 1)
            {
                var vertice = Vertices[0];

                RAGE.Game.Graphics.DrawLine(vertice.X, vertice.Y, vertice.Z, vertice.X, vertice.Y, vertice.Z + Height, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);
            }
            else if (Settings.User.Other.HighPolygonsMode)
            {
                if (Height == 0)
                {
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        var currentVertice = Vertices[i];
                        var nextVertice = i == Vertices.Count - 1 ? Vertices[0] : Vertices[i + 1];

                        RAGE.Game.Graphics.DrawPoly(currentVertice.X, currentVertice.Y, currentVertice.Z, nextVertice.X, nextVertice.Y, nextVertice.Z, Position.X, Position.Y, Position.Z, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);

                        if (Settings.User.Other.DebugLabels && (i % vertIdLimiter == 0))
                        {
                            if (!Graphics.GetScreenCoordFromWorldCoord(currentVertice, ref screenX, ref screenY))
                                continue;

                            Graphics.DrawText(i.ToString(), screenX, screenY, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        var currentVertice = Vertices[i];
                        var nextVertice = i == Vertices.Count - 1 ? Vertices[0] : Vertices[i + 1];

                        RAGE.Game.Graphics.DrawPoly(currentVertice.X, currentVertice.Y, currentVertice.Z, currentVertice.X, currentVertice.Y, currentVertice.Z + Height, nextVertice.X, nextVertice.Y, nextVertice.Z, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);
                        RAGE.Game.Graphics.DrawPoly(currentVertice.X, currentVertice.Y, currentVertice.Z + Height, nextVertice.X, nextVertice.Y, nextVertice.Z + Height, nextVertice.X, nextVertice.Y, nextVertice.Z, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);
                        RAGE.Game.Graphics.DrawPoly(currentVertice.X, currentVertice.Y, currentVertice.Z + Height, nextVertice.X, nextVertice.Y, nextVertice.Z + Height, Position.X, Position.Y, Position.Z + Height / 2, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);
                        RAGE.Game.Graphics.DrawPoly(currentVertice.X, currentVertice.Y, currentVertice.Z, nextVertice.X, nextVertice.Y, nextVertice.Z, Position.X, Position.Y, Position.Z - Height / 2, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);

                        if (Settings.User.Other.DebugLabels && (i % vertIdLimiter == 0))
                        {
                            if (!Graphics.GetScreenCoordFromWorldCoord(currentVertice, ref screenX, ref screenY))
                                continue;

                            Graphics.DrawText(i.ToString(), screenX, screenY, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                        }
                    }
                }
            }
            else
            {
                if (Height == 0)
                {
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        var currentVertice = Vertices[i];
                        var nextVertice = i == Vertices.Count - 1 ? Vertices[0] : Vertices[i + 1];

                        RAGE.Game.Graphics.DrawLine(currentVertice.X, currentVertice.Y, currentVertice.Z, nextVertice.X, nextVertice.Y, nextVertice.Z, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);

                        if (Settings.User.Other.DebugLabels && (i % vertIdLimiter == 0))
                        {
                            if (!Graphics.GetScreenCoordFromWorldCoord(currentVertice, ref screenX, ref screenY))
                                continue;

                            Graphics.DrawText(i.ToString(), screenX, screenY, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        var currentVertice = Vertices[i];
                        var nextVertice = i == Vertices.Count - 1 ? Vertices[0] : Vertices[i + 1];

                        RAGE.Game.Graphics.DrawLine(currentVertice.X, currentVertice.Y, currentVertice.Z, nextVertice.X, nextVertice.Y, nextVertice.Z, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);
                        RAGE.Game.Graphics.DrawLine(currentVertice.X, currentVertice.Y, currentVertice.Z, currentVertice.X, currentVertice.Y, currentVertice.Z + Height, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);
                        RAGE.Game.Graphics.DrawLine(nextVertice.X, nextVertice.Y, nextVertice.Z, nextVertice.X, nextVertice.Y, nextVertice.Z + Height, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);
                        RAGE.Game.Graphics.DrawLine(currentVertice.X, currentVertice.Y, currentVertice.Z + Height, nextVertice.X, nextVertice.Y, nextVertice.Z + Height, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);

                        if (Settings.User.Other.DebugLabels && (i % vertIdLimiter == 0))
                        {
                            if (!Graphics.GetScreenCoordFromWorldCoord(currentVertice, ref screenX, ref screenY))
                                continue;

                            Graphics.DrawText(i.ToString(), screenX, screenY, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                        }
                    }
                }
            }

            if (Settings.User.Other.DebugLabels)
            {
                if (!Graphics.GetScreenCoordFromWorldCoord(Position, ref screenX, ref screenY))
                    return;

                Graphics.DrawText($"Name: {Name} | Type: {Type} | ID: {Colshape.Id} | IsLocal: {Colshape?.IsLocal == true}", screenX, screenY, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                Graphics.DrawText($"Vertices: {Vertices.Count} | Height: {Height}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                Graphics.DrawText($"ActionType: {ActionType} | InteractionType: {InteractionType} | Data: {Data}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
            }
        }

        public override bool IsPointInside(Vector3 point)
        {
            double angleSum = 0f;

            for (int i = 0; i < Vertices.Count; i++)
            {
                var p1 = new Vector3(Vertices[i].X - point.X, Vertices[i].Y - point.Y, Vertices[i].Z - point.Z);
                var p2 = new Vector3(Vertices[(i + 1) % Vertices.Count].X - point.X, Vertices[(i + 1) % Vertices.Count].Y - point.Y, Vertices[(i + 1) % Vertices.Count].Z - point.Z);

                var m1 = System.Math.Sqrt((p1.X * p1.X) + (p1.Y * p1.Y) + (p1.Z * p1.Z));
                var m2 = System.Math.Sqrt((p2.X * p2.X) + (p2.Y * p2.Y) + (p2.Z * p2.Z));

                if (m1 * m2 <= float.Epsilon)
                {
                    angleSum = System.Math.PI * 2;

                    break;
                }
                else
                    angleSum += System.Math.Acos((p1.X * p2.X + p1.Y * p2.Y + p1.Z * p2.Z) / (m1 * m2));
            }

            var polygonPoints2d = new List<RAGE.Ui.Cursor.Vector2>();

            if (Height == 0)
            {
                for (int i = 0; i < Vertices.Count; i++)
                    polygonPoints2d.Add(new RAGE.Ui.Cursor.Vector2(Vertices[i].X, Vertices[i].Y));
            }
            else
            {
                for (int i = 0; i < Vertices.Count; i++)
                {
                    if (point.Z >= Vertices[i].Z && point.Z <= (Vertices[i].Z + Height) || angleSum >= 5.8f)
                        polygonPoints2d.Add(new RAGE.Ui.Cursor.Vector2(Vertices[i].X, Vertices[i].Y));
                    else
                        return false;
                }
            }

            bool inside = false;

            for (int i = 0, j = polygonPoints2d.Count - 1; i < polygonPoints2d.Count; j = i++)
            {
                float xi = polygonPoints2d[i].X, yi = polygonPoints2d[i].Y;
                float xj = polygonPoints2d[j].X, yj = polygonPoints2d[j].Y;

                if (((yi > point.Y) != (yj > point.Y)) && (point.X < (xj - xi) * (point.Y - yi) / (yj - yi) + xi))
                    inside = !inside;
            }

            return inside;
        }
    }
}