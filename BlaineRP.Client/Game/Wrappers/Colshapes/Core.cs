using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Additional;
using BlaineRP.Client.Extensions.RAGE;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Game.Local;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Wrappers.Colshapes
{
    [Script(int.MaxValue)]
    public class Core
    {
        public static AsyncTask PolygonCreationTask { get; set; }

        public static Vector3 TempPosition { get; set; }

        public static Polygon TempPolygon { get; set; }

        public static bool CancelLastColshape { get; set; }

        public static string OverrideInteractionText { get; set; }

        public Core()
        {
            ExtraColshape.InteractionColshapesAllowed = true;

            Main.Render -= ExtraColshape.Render;
            Main.Render += ExtraColshape.Render;

            Events.Add("ExtraColshape::New", (object[] args) =>
            {
                var cs = (RAGE.Elements.Colshape)args[0];

                if (cs == null)
                    return;

                var tNum = cs.GetSharedData<int?>("Type", null);

                if (tNum == null)
                    return;

                var type = (ExtraColshape.Types)tNum;

                ExtraColshape data = null;

                var pos = RAGE.Util.Json.Deserialize<Vector3>(cs.GetSharedData<string>("Position"));
                var isVisible = cs.GetSharedData<bool>("IsVisible");
                var dim = RAGE.Util.Json.Deserialize<uint>(cs.GetSharedData<string>("Dimension"));
                var colour = RAGE.Util.Json.Deserialize<Utils.Colour>(cs.GetSharedData<string>("Colour"));

                if (type == ExtraColshape.Types.Circle)
                {
                    var radius = cs.GetSharedData<float>("Radius");

                    data = new Circle(pos, radius, isVisible, colour, dim, cs);
                }
                else if (type == ExtraColshape.Types.Sphere)
                {
                    var radius = cs.GetSharedData<float>("Radius");

                    data = new Sphere(pos, radius, isVisible, colour, dim, cs);
                }
                else if (type == ExtraColshape.Types.Cylinder)
                {
                    var radius = cs.GetSharedData<float>("Radius");
                    var height = cs.GetSharedData<float>("Height");

                    data = new Cylinder(pos, radius, height, isVisible, colour, dim, cs);
                }
                else if (type == ExtraColshape.Types.Polygon)
                {
                    var vertices = RAGE.Util.Json.Deserialize<List<Vector3>>(cs.GetSharedData<string>("Vertices"));
                    var height = cs.GetSharedData<float>("Height");
                    var heading = cs.GetSharedData<float>("Heading");

                    data = new Polygon(vertices, height, heading, isVisible, colour, dim, cs);
                }

                if (data == null)
                    return;

                var cData = RAGE.Util.Json.Deserialize<(System.Type, string)>(cs.GetSharedData<string>("Data"));

                var aType = (ExtraColshape.ActionTypes)cs.GetSharedData<int>("ActionType");
                var iType = (ExtraColshape.InteractionTypes)cs.GetSharedData<int>("InteractionType");

                data.Data = Newtonsoft.Json.JsonConvert.DeserializeObject(cData.Item2, cData.Item1);
                data.ActionType = aType;
                data.InteractionType = iType;
            });

            Events.Add("ExtraColshape::Del", (object[] args) =>
            {
                var data = ExtraColshape.GetByRemoteId((int)args[0]);

                if (data == null)
                    return;

                data.Destroy();
            });

            Events.AddDataHandler("Data", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                var cData = RAGE.Util.Json.Deserialize<(System.Type, string)>((string)value);

                data.Data = Newtonsoft.Json.JsonConvert.DeserializeObject(cData.Item2, cData.Item1);
            });

            Events.AddDataHandler("ActionType", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                data.ActionType = (ExtraColshape.ActionTypes)(int)value;
            });

            Events.AddDataHandler("InteractionType", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                data.InteractionType = (ExtraColshape.InteractionTypes)(int)value;
            });

            Events.AddDataHandler("IsVisible", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                data.IsVisible = (bool)value;
            });

            Events.AddDataHandler("Position", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                data.Position = RAGE.Util.Json.Deserialize<Vector3>((string)value);
            });

            Events.AddDataHandler("Dimension", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                data.Dimension = RAGE.Util.Json.Deserialize<uint>((string)value);
            });

            Events.AddDataHandler("Height", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                if (data is Polygon)
                {
                    (data as Polygon).Height = (float)value;
                }
                else if (data is Cylinder)
                {
                    (data as Cylinder).Height = (float)value;
                }
            });

            Events.AddDataHandler("Heading", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                if (!(data is Polygon))
                    return;

                (data as Polygon).Heading = (float)value;
            });

            Events.AddDataHandler("Radius", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                if (data is Sphere)
                {
                    (data as Sphere).Radius = (float)value;
                }
                else if (data is Circle)
                {
                    (data as Circle).Radius = (float)value;
                }
            });

            Events.AddDataHandler("Colour", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                data.Colour = RAGE.Util.Json.Deserialize<Utils.Colour>((string)value);
            });

            Events.OnPlayerEnterColshape += (Colshape cs, Events.CancelEventArgs cancel) =>
            {
                if (cancel != null)
                    cancel.Cancel = true;

                var data = ExtraColshape.Get(cs);

                if (data == null)
                    return;

                if (data.ActionType != ExtraColshape.ActionTypes.None)
                {
                    var action = ExtraColshape.GetEnterAction(data.ActionType);

                    action?.Invoke(data);
                }

                if (CancelLastColshape)
                {
                    CancelLastColshape = false;

                    data.IsInside = false;

                    return;
                }

                if (data.IsInteraction)
                {
                    if (!ExtraColshape.InteractionColshapesAllowed)
                        return;

                    var func = ExtraColshape.GetInteractionAction(data.InteractionType);

                    HUD.InteractionAction = func;

                    var interactionText = OverrideInteractionText;

                    if (interactionText != null)
                    {
                        OverrideInteractionText = null;
                    }
                    else
                    {
                        interactionText = Locale.Interaction.Names.GetValueOrDefault(data.InteractionType) ?? Locale.Interaction.Names.GetValueOrDefault(Additional.ExtraColshape.InteractionTypes.Interact) ?? "null";
                    }

                    HUD.SwitchInteractionText(true, interactionText);
                }

                data.OnEnter?.Invoke(null);
            };

            Events.OnPlayerExitColshape += (Colshape cs, Events.CancelEventArgs cancel) =>
            {
                if (cancel != null)
                    cancel.Cancel = true;

                var data = ExtraColshape.Get(cs);

                if (data == null)
                    return;

                if (cs.GetData<bool>("PendingDeletion"))
                {
                    ExtraColshape.All.Remove(data);
                }

                if (data.IsInteraction)
                {
                    HUD.InteractionAction = null;

                    HUD.SwitchInteractionText(false, string.Empty);
                }

                if (data.ActionType != ExtraColshape.ActionTypes.None)
                {
                    var action = ExtraColshape.GetExitAction(data.ActionType);

                    action?.Invoke(data);
                }

                data.OnExit?.Invoke(null);
            };
        }
    }

    public class Sphere : ExtraColshape
    {
        public override string ShortData => $"Type: {Type}, Pos: {RAGE.Util.Json.Serialize(Position)}, Radius: {Radius}";

        public float Radius { get; set; }

        public Sphere(Vector3 Position, float Radius, bool IsVisible, Utils.Colour Colour, uint Dimension, Colshape Colshape = null) : base(Types.Sphere, IsVisible, Colour, Dimension, Colshape)
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

    public class Circle : ExtraColshape
    {
        public override string ShortData => $"Type: {Type}, Pos: {RAGE.Util.Json.Serialize(Position)}, Radius: {Radius}";

        public float Radius { get; set; }

        public Circle(Vector3 Position, float Radius, bool IsVisible, Utils.Colour Colour, uint Dimension, Colshape Colshape = null) : base(Types.Circle, IsVisible, Colour, Dimension, Colshape)
        {
            this.Radius = Radius;

            this.Position = Position;

            if (IsStreamed())
                Streamed.Add(this);
        }

        public override void Draw()
        {
            var diameter = Radius * 2;

            RAGE.Game.Graphics.DrawMarker(1, Position.X, Position.Y, Position.Z, 0f, 0f, 0f, 1f, 1f, 1f, diameter, diameter, 10f, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha, false, false, 2, false, null, null, false);

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

            return Position.DistanceIgnoreZ(Player.LocalPlayer.Position) <= Radius + Settings.App.Profile.Current.Game.StreamDistance;
        }

        public override bool IsPointInside(Vector3 point) => point.DistanceIgnoreZ(Position) <= Radius;
    }

    public class Cylinder : ExtraColshape
    {
        public override string ShortData => $"Type: {Type}, Pos: {RAGE.Util.Json.Serialize(Position)}, Radius: {Radius}, Height: {Height}";

        public float Radius { get; set; }
        public float Height { get; set; }

        public Cylinder(Vector3 Position, float Radius, float Height, bool IsVisible, Utils.Colour Colour, uint Dimension, Colshape Colshape = null) : base(Types.Cylinder, IsVisible, Colour, Dimension, Colshape)
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

    public class Cuboid : Polygon
    {
        public override string ShortData => $"Type: {Type}, CenterPos: {RAGE.Util.Json.Serialize(Position)}, Width: {Width}, Depth: {Depth}, Height: {Height}, Heading: {Heading}";

        public float Width { get; set; }

        public float Depth { get; set; }

        public Cuboid(Vector3 Position, float Width, float Depth, float Height, float Heading, bool IsVisible, Utils.Colour Colour, uint Dimension = 7, Colshape Colshape = null) : base(Types.Cuboid, GetBaseVertices(Position, Width, Depth, Height), Height, Heading, IsVisible, Colour, Dimension, Colshape)
        {
            this.Width = Width;
            this.Depth = Depth;
        }

        public static List<Vector3> GetBaseVertices(Vector3 centerPos, float width, float depth, float height)
        {
            var zCoord = centerPos.Z - height / 2;

            var width2 = width / 2f;
            var depth2 = depth / 2f;

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

            var depth2 = Depth / 2f;

            Vertices[0].X = Position.X - value;
            Vertices[1].X = Position.X + value;
            Vertices[2].X = Position.X + value;
            Vertices[3].X = Position.X - value;

            Vertices[0].Y = Position.Y - depth2;
            Vertices[1].Y = Position.Y - depth2;
            Vertices[2].Y = Position.Y + depth2;
            Vertices[3].Y = Position.Y + depth2;

            UpdatePolygonCenterAndMaxRange();

            var heading = Heading;

            Heading = 0f;

            SetHeading(heading);
        }

        public void SetDepth(float value)
        {
            Depth = value;

            value /= 2f;

            var width2 = Width / 2f;

            Vertices[0].Y = Position.Y - value;
            Vertices[1].Y = Position.Y - value;
            Vertices[2].Y = Position.Y + value;
            Vertices[3].Y = Position.Y + value;

            Vertices[0].X = Position.X - width2;
            Vertices[1].X = Position.X + width2;
            Vertices[2].X = Position.X + width2;
            Vertices[3].X = Position.X - width2;

            UpdatePolygonCenterAndMaxRange();

            var heading = Heading;

            Heading = 0f;

            SetHeading(heading);
        }
    }

    public class Polygon : ExtraColshape
    {
        public override string ShortData => $"Type: {Type}, Vertices: {RAGE.Util.Json.Serialize(Vertices)}, Height: {Height}, Heading: {Heading}";

        public float MaxRange { get; set; }

        public float Height { get; set; }

        public float Heading { get; set; }

        public List<Vector3> Vertices { get; set; }

        public bool Is3D => Height > 0;

        protected Polygon(Types Type, List<Vector3> Vertices, float Height, float Heading, bool IsVisible, Utils.Colour Colour, uint Dimension, Colshape Colshape = null) : base(Type, IsVisible, Colour, Dimension, Colshape)
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

        public Polygon(List<Vector3> Vertices, float Height, float Heading, bool IsVisible, Utils.Colour Colour, uint Dimension, Colshape Colshape = null) : this(Types.Polygon, Vertices, Height, Heading, IsVisible, Colour, Dimension, Colshape)
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
            if (Type == Types.Cuboid)
                return;

            if (!Is3D)
                vertice.Z = Vertices[0].Z;

            Vertices.Add(vertice);

            UpdatePolygonCenterAndMaxRange();
        }

        public void InsertVertice(int idx, Vector3 vertice)
        {
            if (Type == Types.Cuboid)
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
            if (Type == Types.Cuboid)
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
