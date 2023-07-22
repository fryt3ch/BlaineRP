using GTANetworkAPI;
using System;
using System.Collections.Generic;

namespace BlaineRP.Server.Additional
{
    public abstract class ExtraColshape
    {
        /// <summary>Типы колшейпов</summary>
        public enum Types
        {
            /// <summary>Сферический (трехмерный)</summary>
            Sphere = 0,
            /// <summary>Круговой (двумерный)</summary>
            Circle,
            /// <summary>Цилиндрический (трехмерный)</summary>
            Cylinder,
            /// <summary>Многогранник (трехмерный/двумерный)</summary>
            /// <remarks>Размерность зависит от высоты (0 - двухмерный, > 0 - трехмерный</remarks>
            Polygon,
        }

        public enum InteractionTypes
        {
            None = -1,

            DoorLock,
            DoorUnlock,

            BusinessEnter,
            BusinessInfo,

            HouseEnter,
            HouseExit,

            Locker,
            Fridge,
            Wardrobe,

            Interact,
        }

        public enum ActionTypes
        {
            /// <summary>Никакой, в таком случае нужно в ручную прописывать действия через OnEnter/OnExit</summary>
            None = -1,

            /// <summary>Межкомнатная дверь в доме/квартире</summary>
            HouseDoorLock,

            /// <summary>Межкомнатная дверь в доме/квартире</summary>
            HouseDoorUnlock,

            HouseEnter,
            HouseExit,

            BusinessEnter,
            BusinessInfo,
        }

        /// <summary>Сущность-держатель колшейпа, не имеет функциональности</summary>
        public ColShape Colshape { get; set; }

        /// <summary>Тип колшейпа</summary>
        public Types Type { get => (Types)Colshape.GetSharedData<int>("Type"); set => Colshape.SetSharedData("Type", (int)value); }

        /// <summary>Видимый ли?</summary>
        /// <remarks>Если колшейп видимый, то его будут видеть все игроки, иначе - только администраторы, и то, при включенной настройке на стороне клиента</remarks>
        public bool IsVisible { get => Colshape.GetSharedData<bool>("IsVisible"); set => Colshape.SetSharedData("IsVisible", value); }

        /// <summary>Позиция</summary>
        public Vector3 Position { get => NAPI.Util.FromJson<Vector3>(Colshape.GetSharedData<string>("Position")); set { var str = NAPI.Util.ToJson(value); Colshape.SetSharedData("Position", str); } }

        /// <summary>Измерение</summary>
        /// <remarks>Если используется uint.MaxValue, то колшейп работает независимо от измерения игрока</remarks>
        public uint Dimension { get => NAPI.Util.FromJson<uint>(Colshape.GetSharedData<string>("Dimension")); set { var str = NAPI.Util.ToJson(value); Colshape.SetSharedData("Dimension", str); } }

        /// <summary>Цвет</summary>
        public Color Colour { get => Colshape.GetSharedData<string>("Colour").DeserializeFromJson<Color>(); set { Colshape.SetSharedData("Colour", value.SerializeToJson()); } }

        /// <summary>Тип действия при входе/выходе в колшейп</summary>
        public ActionTypes ActionType { get => (ActionTypes)Colshape.GetSharedData<int>("ActionType"); set => Colshape.SetSharedData("ActionType", (int)value); }

        /// <summary>Тип действия для взаимодействия</summary>
        public InteractionTypes InteractionType { get => (InteractionTypes)Colshape.GetSharedData<int>("InteractionType"); set => Colshape.SetSharedData("InteractionType", (int)value); }

        /// <summary>Данные колшейпа</summary>
        /// <remarks>При присваивании всегда сериализирует данные в JSON. При получении - возвращает строку (сериализованные данные)</remarks>
        public object Data { get { var t = Colshape.GetSharedData<string>("Data").DeserializeFromJson<(Type, string)>(); return Newtonsoft.Json.JsonConvert.DeserializeObject(t.Item2, t.Item1); } set { Colshape.SetSharedData("Data", (value == null ? typeof(object) : value.GetType(), value).SerializeToJson()); } }

        /// <summary>Метод для проверки, находится ли точка в колшейпе</summary>
        /// <param name="point">Точка</param>
        public abstract bool IsPointInside(Vector3 point);

        public void Delete()
        {
            if (Colshape != null)
            {
                NAPI.ClientEvent.TriggerClientEventForAll("ExtraColshape::Del", this.Colshape.Id);

                Colshape.ResetData();

                Colshape.Delete();
            }
        }

        private ExtraColshape(Types Type, bool IsVisible, Color Colour, uint Dimension = 0, InteractionTypes InteractionType = InteractionTypes.None, ActionTypes ActionType = ActionTypes.None)
        {
            this.Colshape = NAPI.ColShape.CreateSphereColShape(Utils.ZeroVector, 0f, Properties.Settings.Static.StuffDimension);

            this.Colshape.OnEntityEnterColShape += (ColShape colShape, Player client) => client?.KickSilent();
            this.Colshape.OnEntityExitColShape += (ColShape colShape, Player client) => client?.KickSilent();

            this.Colour = Colour;
            this.Dimension = Dimension;
            this.IsVisible = IsVisible;

            this.InteractionType = InteractionType;
            this.ActionType = ActionType;

            this.Type = Type;

            this.Data = null;

            Colshape.SetData("Data::Class", this);

            NAPI.Task.Run(() =>
            {
                NAPI.ClientEvent.TriggerClientEventForAll("ExtraColshape::New", this.Colshape.Handle);
            }, 1000);
        }

        public static ExtraColshape GetData(ColShape colshape) => colshape.HasData("Data::Class") ? colshape.GetData<ExtraColshape>("Data::Class") : null;

        public class Sphere : ExtraColshape
        {
            /// <summary>Радиус</summary>
            public float Radius { get => Colshape.GetSharedData<float>("Radius"); set => Colshape.SetSharedData("Radius", value); }

            public Sphere(Vector3 Position, float Radius, bool IsVisible, Color Colour, uint Dimension = 0) : base(Types.Sphere, IsVisible, Colour, Dimension)
            {
                this.Radius = Radius;

                this.Position = Position;
            }

            public override bool IsPointInside(Vector3 point) => Vector3.Distance(point, Position) <= Radius;
        }

        public class Circle : ExtraColshape
        {
            /// <summary>Радиус</summary>
            public float Radius { get => Colshape.GetSharedData<float>("Radius"); set => Colshape.SetSharedData("Radius", value); }

            public Circle(Vector3 Position, float Radius, bool IsVisible, Color Colour, uint Dimension = 0) : base(Types.Circle, IsVisible, Colour, Dimension)
            {
                this.Radius = Radius;

                this.Position = Position;
            }

            public override bool IsPointInside(Vector3 point) => point.DistanceIgnoreZ(Position) <= Radius;
        }

        public class Cylinder : ExtraColshape
        {
            /// <summary>Радиус</summary>
            public float Radius { get => Colshape.GetSharedData<float>("Radius"); set => Colshape.SetSharedData("Radius", value); }

            /// <summary>Высота</summary>
            public float Height { get => Colshape.GetSharedData<float>("Height"); set => Colshape.SetSharedData("Height", value); }

            public Cylinder(Vector3 Position, float Radius, float Height, bool IsVisible, Color Colour, uint Dimension = 0) : base(Types.Cylinder, IsVisible, Colour, Dimension)
            {
                this.Radius = Radius;
                this.Height = Height;

                this.Position = Position;
            }

            public override bool IsPointInside(Vector3 point)
            {
                if (Position.Z < point.Z || Position.Z > point.Z + Height)
                    return false;

                return Position.DistanceIgnoreZ(point) <= Radius;
            }
        }

        public class Polygon : ExtraColshape
        {
            /// <summary>Высота</summary>
            public float Height { get => Colshape.GetSharedData<float>("Height"); set => Colshape.SetSharedData("Height", value); }

            /// <summary>Угол</summary>
            public float Heading { get => Colshape.GetSharedData<float>("Heading"); set => Colshape.SetSharedData("Heading", value); }

            /// <summary>Вершины</summary>
            public List<Vector3> Vertices { get => NAPI.Util.FromJson<List<Vector3>>(Colshape.GetSharedData<string>("Vertices")); set => Colshape.SetSharedData("Vertices", NAPI.Util.ToJson(value)); }

            public bool Is3D { get => Height > 0; }

            public Polygon(List<Vector3> Vertices, float Height, float Heading, bool IsVisible, Color Colour, uint Dimension = 0) : base(Types.Polygon, IsVisible, Colour, Dimension)
            {
                this.Colshape.SetSharedData("Position", NAPI.Util.ToJson(GetCenterPosition(Vertices, Height)));

                this.Height = Height;

                this.Heading = Heading;

                ApplyHeading();
            }

            /// <summary>Метод для создания кубического многогранника</summary>
            /// <param name="Position">Центр куба</param>
            /// <param name="Width">Ширина</param>
            /// <param name="Depth">Глубина</param>
            /// <param name="Height">Высота</param>
            /// <param name="Heading">Угол</param>
            /// <param name="IsVisible">Видимый ли?</param>
            /// <param name="Colour">Цвет</param>
            /// <param name="Dimension">Измерение</param>
            public static Polygon CreateCuboid(Vector3 Position, float Width, float Depth, float Height, float Heading, bool IsVisible, Color Colour, uint Dimension = 0)
            {
                var vertices = new List<Vector3>()
                {
                    new Vector3(Position.X - Width / 2, Position.Y - Depth / 2, Position.Z - Height / 2),
                    new Vector3(Position.X + Width / 2, Position.Y - Depth / 2, Position.Z - Height / 2),
                    new Vector3(Position.X + Width / 2, Position.Y + Depth / 2, Position.Z - Height / 2),
                    new Vector3(Position.X - Width / 2, Position.Y + Depth / 2, Position.Z - Height / 2),
                };

                return new Polygon(vertices, Height, Heading, IsVisible, Colour, Dimension);
            }

            /// <summary>Метод для создания кубического многогранника</summary>
            /// <param name="position1">Точка 1</param>
            /// <param name="position2">Точка 2</param>
            /// <param name="heading">Угол</param>
            /// <param name="isVisible">Видимый ли?</param>
            /// <param name="colour">Цвет</param>
            /// <param name="dimension">Измерение</param>
            public static Polygon CreateCuboid(Vector3 position1, Vector3 position2, float heading, bool isVisible, Color colour, uint dimension = 4294967295)
            {
                var middlePos = new Vector3((position1.X + position2.X) / 2, (position1.Y + position2.Y) / 2, (position1.Z + position2.Z) / 2);

                var width = Math.Abs(position2.X - position1.X);
                var depth = Math.Abs(position2.Y - position1.Y);
                var height = Math.Abs(position2.Z - position1.Z);

                return CreateCuboid(middlePos, width, depth, height, heading, isVisible, colour, dimension);
            }

            public override bool IsPointInside(Vector3 point)
            {
                double angleSum = 0f;

                for (int i = 0; i < Vertices.Count; i++)
                {
                    var p1 = new Vector3(Vertices[i].X - Position.X, Vertices[i].Y - Position.Y, Vertices[i].Z - Position.Z);
                    var p2 = new Vector3(Vertices[(i + 1) % Vertices.Count].X - Position.X, Vertices[(i + 1) % Vertices.Count].Y - Position.Y, Vertices[(i + 1) % Vertices.Count].Z - Position.Z);

                    var m1 = Math.Sqrt((p1.X * p1.X) + (p1.Y * p1.Y) + (p1.Z * p1.Z));
                    var m2 = Math.Sqrt((p2.X * p2.X) + (p2.Y * p2.Y) + (p2.Z * p2.Z));

                    if (m1 * m2 <= float.Epsilon)
                    {
                        angleSum = Math.PI * 2;

                        break;
                    }
                    else
                        angleSum += Math.Acos((p1.X * p2.X + p1.Y * p2.Y + p1.Z * p2.Z) / (m1 * m2));
                }

                var polygonPoints2d = new List<Utils.Vector2>();

                if (Height == 0)
                {
                    for (int i = 0; i < Vertices.Count; i++)
                        polygonPoints2d.Add(new Utils.Vector2(Vertices[i].X, Vertices[i].Y));
                }
                else
                {
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        if (Position.Z >= Vertices[i].Z && Position.Z <= (Vertices[i].Z + Height) || angleSum >= 5.8f)
                            polygonPoints2d.Add(new Utils.Vector2(Vertices[i].X, Vertices[i].Y));
                        else
                            return false;
                    }
                }

                bool inside = false;

                for (int i = 0, j = polygonPoints2d.Count - 1; i < polygonPoints2d.Count; j = i++)
                {
                    float xi = polygonPoints2d[i].X, yi = polygonPoints2d[i].Y;
                    float xj = polygonPoints2d[j].X, yj = polygonPoints2d[j].Y;

                    if (((yi > Position.Y) != (yj > Position.Y)) && (Position.X < (xj - xi) * (Position.Y - yi) / (yj - yi) + xi))
                        inside = !inside;
                }

                return inside;
            }

            /// <summary>Метод для применения нового угла многогранника</summary>
            /// <remarks>Поворачивает все вершины относительно центра многогранника В указанный угол</remarks>
            public void ApplyHeading()
            {
                var vertices = Vertices;

                var heading = Utils.DegreesToRadians(Heading);

                var originPoint = Position;

                for (int i = 0; i < Vertices.Count; i++)
                {
                    vertices[i] = Utils.RotatePoint(vertices[i], originPoint, heading, true);
                }

                Vertices = vertices;
            }

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
        }
    }
}
