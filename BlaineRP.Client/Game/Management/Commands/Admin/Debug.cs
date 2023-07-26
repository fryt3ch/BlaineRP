using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.Businesses;
using BlaineRP.Client.Game.Estates;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;
using Interaction = BlaineRP.Client.Game.Management.Interaction;
using NPC = BlaineRP.Client.Game.NPCs.NPC;

namespace BlaineRP.Client.Game.Management.Commands
{
    partial class Core
    {
        [Command("colshape_delete", true, "Удалить колшейп", "cs_del")]
        public static void ColshapeDelete(uint id)
        {
            var cs = ExtraColshape.GetById((int)id);

            cs?.Destroy();
        }

        [Command("poly_stop", true, "Закончить создание полигона")]
        public static void PolygonStop()
        {
            if (Helpers.Colshapes.Core.PolygonCreationTask != null)
            {
                Helpers.Colshapes.Core.PolygonCreationTask.Cancel();
                Helpers.Colshapes.Core.PolygonCreationTask = null;

                Helpers.Colshapes.Core.TempPolygon = null;
            }
        }

        [Command("poly_start", true, "Начать создание полигона")]
        public static void PolygonStart(float height = 0, float step = 1f)
        {
            if (step <= 0f)
                return;

            var newVertice = Player.LocalPlayer.Position;
            newVertice.Z -= 0.5f;

            if (Helpers.Colshapes.Core.PolygonCreationTask != null)
            {
                Helpers.Colshapes.Core.PolygonCreationTask.Cancel();

                Helpers.Colshapes.Core.TempPolygon?.Destroy();
            }

            Helpers.Colshapes.Core.TempPolygon = new Polygon(new List<Vector3>() { newVertice }, height, 0f, false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension, null);

            Helpers.Colshapes.Core.PolygonCreationTask = new Utils.AsyncTask(() =>
            {
                if (Helpers.Colshapes.Core.TempPolygon == null)
                    return true;

                var newVertice = Player.LocalPlayer.Position;
                newVertice.Z -= 1f;

                var vertices = Helpers.Colshapes.Core.TempPolygon.Vertices;

                if (vertices[vertices.Count - 1].DistanceTo(newVertice) < step)
                    return false;

                Helpers.Colshapes.Core.TempPolygon.AddVertice(newVertice);

                //Events.CallLocal("Chat::ShowServerMessage", $"[TColshapes::Polygon_{(height > 0 ? "3D" : "2D")}] New pos: {newVertice}");

                return false;
            }, 100, true, 0);

            Helpers.Colshapes.Core.PolygonCreationTask.Run();
        }

        [Command("poly_rotate", true, "Повернуть полигон", "poly_rt")]
        public static void PolygonRotate(uint id, float angle)
        {
            var col = ExtraColshape.GetById((int)id);

            if (!(col is Polygon))
                return;

            (col as Polygon).Rotate(angle);
        }

        [Command("poly_angle", true, "Задать поворот полигона", "poly_ang")]
        public static void PolygonAngle(uint id, float angle)
        {
            var col = ExtraColshape.GetById((int)id);

            if (!(col is Polygon))
                return;

            (col as Polygon).SetHeading(angle);
        }

        [Command("colshape_new_circle", true, "Создать КШ Круг", "cs_n_crl")]
        public static void ColshapeNewCircle(float radius)
        {
            if (radius <= 0)
                return;

            var newVertice = Player.LocalPlayer.Position;
            newVertice.Z -= 1f;

            new Circle(newVertice, radius, false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension, null);

            Events.CallLocal("Chat::ShowServerMessage", $"[TColshapes::Circle_2D] Pos: {newVertice} | Radius: {radius}");
        }

        [Command("colshape_new_tube", true, "Создать КШ Цилиндр", "cs_n_tube")]
        public static void ColshapeNewTube(float radius, float height)
        {
            if (radius <= 0 || height <= 0)
                return;

            var newVertice = Player.LocalPlayer.Position;
            newVertice.Z -= 1f;

            new Cylinder(newVertice, radius, height, false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension, null);

            Events.CallLocal("Chat::ShowServerMessage", $"[TColshapes::Tube_3D] Pos: {newVertice} | Radius: {radius} | Height: {height}");
        }

        [Command("colshape_new_sphere", true, "Создать КШ Сфера", "cs_n_sph")]
        public static void ColshapeNewSphere(float radius)
        {
            if (radius <= 0)
                return;

            var newVertice = Player.LocalPlayer.Position;
            newVertice.Z -= 1f;

            new Sphere(newVertice, radius, false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension, null);

            Events.CallLocal("Chat::ShowServerMessage", $"[TColshapes::Sphere_3D] Pos: {newVertice} | Radius: {radius}");
        }

        [Command("colshape_new_cuboid", true, "Создать КШ Параллелепипед", "cs_n_c3d")]
        public static void ColshapeNewCuboid(float sX = 5f, float sY = 5f, float height = 5f)
        {
            if (sX < 0 || sY < 0 || height < 0)
                return;

            var newVertice = Player.LocalPlayer.Position;
            newVertice.Z -= 1f;

            if (sX != 0 && sY != 0)
            {
                new Cuboid(newVertice, sX, sY, height, 0, false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension);

                Events.CallLocal("Chat::ShowServerMessage", $"[TColshapes::Cuboid_3D] Pos: {newVertice} | Width: {sX} | Depth: {sY} | Height: {height}");
            }
        }

        [Command("cuboid_setwidth", true, "Добавить вершину к полигону", "c3d_width")]
        public static void CuboidSetWidth(uint id, float width)
        {
            var poly = ExtraColshape.GetById((int)id) as Cuboid;

            if (poly == null)
                return;

            poly.SetWidth(width);
        }

        [Command("cuboid_setdepth", true, "Добавить вершину к полигону", "c3d_depth")]
        public static void CuboidSetDepth(uint id, float depth)
        {
            var poly = ExtraColshape.GetById((int)id) as Cuboid;

            if (poly == null)
                return;

            poly.SetDepth(depth);
        }

        [Command("poly_addvertice", true, "Добавить вершину к полигону", "poly_addv")]
        public static void PolyAddVertice(uint id, int idx = -1)
        {
            var poly = ExtraColshape.GetById((int)id) as Polygon;

            if (poly == null)
                return;

            var newVertice = Player.LocalPlayer.Position;
            newVertice.Z -= 1f;

            if (idx < 0)
                poly.AddVertice(newVertice);
            else
                poly.InsertVertice(idx, newVertice);
        }

        [Command("poly_removevertice", true, "Удалить вершину у полигона", "poly_rmv")]
        public static void PolyRemoveVertice(uint id, uint vertId)
        {
            var poly = ExtraColshape.GetById((int)id) as Polygon;

            if (poly == null)
                return;

            poly.RemoveVertice((int)vertId);
        }

        [Command("cs_setheight", true, "Задать высоту полигону", "cs_sheight")]
        public static void CsSetHeight(uint id, float height)
        {
            if (height < 0f)
                return;

            var cs = ExtraColshape.GetById((int)id);

            if (cs == null)
                return;

            if (cs is Polygon poly)
                poly.SetHeight(height);
            else if (cs is Cylinder cyl)
                cyl.Height = height;
        }

        [Command("cs_setradius", true, "Задать высоту полигону", "cs_srad")]
        public static void CsSetRadius(uint id, float radius)
        {
            if (radius < 0f)
                return;

            var cs = ExtraColshape.GetById((int)id);

            if (cs == null)
                return;

            if (cs is Cylinder cyl)
                cyl.Radius = radius;
            else if (cs is Sphere sph)
                sph.Radius = radius;
        }

        [Command("colshape_save_server", true, "Задать высоту полигону", "poly_sheight")]
        public static void ColshapeSaveServer(uint id)
        {
            var poly = ExtraColshape.GetById((int)id);

            if (poly == null)
                return;
            Utils.Misc.DebugServerSaveText(poly.ShortData);
        }

        [Command("highpolymode", true, "Сменить режим вида полигонов", "hpolymode")]
        public static void HighPolyMode(bool? state = null)
        {
            if (state == null)
                Settings.User.Other.HighPolygonsMode = !Settings.User.Other.HighPolygonsMode;
            else
                Settings.User.Other.HighPolygonsMode = (bool)state;

            Notification.Show(Notification.Types.Success, Locale.Notifications.Commands.Header, string.Format(Settings.User.Other.HighPolygonsMode ? Locale.Notifications.Commands.Enabled : Locale.Notifications.Commands.Disabled, "HighPolyMode"));
        }

        [Command("colshapes_visible", true, "Сменить видимость колшейпов", "cs_vis")]
        public static void ColshapesVisible(bool? state = null)
        {
            if (state == null)
                Settings.User.Other.ColshapesVisible = !Settings.User.Other.ColshapesVisible;
            else
                Settings.User.Other.ColshapesVisible = (bool)state;

            Notification.Show(Notification.Types.Success, Locale.Notifications.Commands.Header, string.Format(Settings.User.Other.ColshapesVisible ? Locale.Notifications.Commands.Enabled : Locale.Notifications.Commands.Disabled, "ColshapesVisible"));
        }

        [Command("5sound", true, "Проиграть звук из GTA5", "gta5sound")]
        public static void Gta5Sound(string name, string setName)
        {
            RAGE.Game.Audio.PlaySoundFrontend(-1, name, setName, true);
        }

        [Command("newplacemarker", true, "Получить текущую позицию", "npmarker")]
        public static void NewPlaceMarker()
        {
            var pos = Player.LocalPlayer.Position;

            float z = pos.Z;

            RAGE.Game.Misc.GetGroundZFor3dCoord(pos.X, pos.Y, pos.Z, ref z, false);

            pos.Z = z;

            float minDist = 1000f;

            Marker marker = null;

            foreach (var x in RAGE.Elements.Entities.Markers.Streamed.ToList())
                if (Vector3.Distance(Player.LocalPlayer.Position, x.Position) < minDist)
                {
                    minDist = Vector3.Distance(Player.LocalPlayer.Position, x.Position);

                    marker = x;
                }

            if (marker == null)
                return;

            marker.Position = pos;
        }

        [Command("loadipl", true, "Загрузить IPL", "lipl")]
        public static void LoadIpl(string ipl, float x = 0, float y = 0, float z = 0)
        {
            var interior = RAGE.Game.Interior.GetInteriorAtCoords(x, y, z);

            RAGE.Game.Streaming.RequestIpl(ipl);

            RAGE.Game.Interior.RefreshInterior(interior);
        }

        [Command("unloadipl", true, "Выгрузить IPL", "unipl")]
        public static void RemoveIpl(string ipl, float x = 0, float y = 0, float z = 0)
        {
            var interior = RAGE.Game.Interior.GetInteriorAtCoords(x, y, z);

            RAGE.Game.Streaming.RemoveIpl(ipl);

            RAGE.Game.Interior.RefreshInterior(interior);
        }

        [Command("objpos", true, "Получить текущую позицию объекта типа", "objectpos")]
        public static void ObjectPosition(string model, float radius = 1f)
        {
            var pos = Player.LocalPlayer.Position;

            var handle = RAGE.Game.Object.GetClosestObjectOfType(pos.X, pos.Y, pos.Z, radius, RAGE.Util.Joaat.Hash(model), false, true, true);

            if (handle <= 0)
                return;

            Events.CallLocal("Chat::ShowServerMessage", $"Model: {model} | Pos: {RAGE.Game.Entity.GetEntityCoords(handle, true)} | Rot: {RAGE.Game.Entity.GetEntityRotation(handle, 5)}");
        }

        [Command("btw", true, "Установить сытость игроку")]
        public static void BoatToWater()
        {
            if (Interaction.CurrentEntity is Vehicle veh)
                Scripts.Sync.Vehicles.BoatFromTrailerToWater(veh);
        }

        [Command("attachtool_start", true, "Установить сытость игроку")]
        public static async void AttachToolStart(string model, int boneId, float offX = 0f, float offY = 0f, float offZ = 0f, float rotX = 0f, float rotY = 0f, float rotZ = 0f, bool fixedRot = true)
        {
            AttachToolStop();

            var modelNum = RAGE.Util.Joaat.Hash(model);

            await Streaming.RequestModel(modelNum);

            var gameEntity = new RAGE.Elements.MapObject(RAGE.Game.Object.CreateObject(modelNum, Player.LocalPlayer.Position.X, Player.LocalPlayer.Position.Y, Player.LocalPlayer.Position.Z, false, false, false));

            RAGE.Game.Entity.AttachEntityToEntity(gameEntity.Handle, Player.LocalPlayer.Handle, Player.LocalPlayer.GetBoneIndex(boneId), offX, offY, offZ, rotX, rotY, rotZ, false, false, false, false, 2, fixedRot);

            Player.LocalPlayer.SetData("Temp::ATTOOL::Sens", 0.1f);
            Player.LocalPlayer.SetData("Temp::ATTOOL::PosOff", new Vector3(offX, offY, offZ));
            Player.LocalPlayer.SetData("Temp::ATTOOL::Rot", new Vector3(rotX, rotY, rotZ));
            Player.LocalPlayer.SetData("Temp::ATTOOL::GE", gameEntity);
            Player.LocalPlayer.SetData("Temp::ATTOOL::FR", fixedRot);

            Player.LocalPlayer.SetData("Temp::ATTOOL::XYZ", 0);

            var binds = new List<int>()
            {
                Input.Core.Bind(RAGE.Ui.VirtualKeys.X, true, () =>
                {
                    Player.LocalPlayer.SetData("Temp::ATTOOL::XYZ", 0);

                    Events.CallLocal("Chat::ShowServerMessage", $"[AttachTool] Using X axis now!");
                }),
                Input.Core.Bind(RAGE.Ui.VirtualKeys.Y, true, () =>
                {
                    Player.LocalPlayer.SetData("Temp::ATTOOL::XYZ", 1);

                    Events.CallLocal("Chat::ShowServerMessage", $"[AttachTool] Using Y axis now!");
                }),
                Input.Core.Bind(RAGE.Ui.VirtualKeys.Z, true, () =>
                {
                    Player.LocalPlayer.SetData("Temp::ATTOOL::XYZ", 2);

                    Events.CallLocal("Chat::ShowServerMessage", $"[AttachTool] Using Z axis now!");
                }),

                Input.Core.Bind(RAGE.Ui.VirtualKeys.Left, true, () =>
                {
                    if (Utils.Misc.IsAnyCefActive(true))
                        return;

                    var sens = Player.LocalPlayer.GetData<float>("Temp::ATTOOL::Sens");

                    var xyz = Player.LocalPlayer.GetData<int>("Temp::ATTOOL::XYZ");

                    var rot = Player.LocalPlayer.GetData<Vector3>("Temp::ATTOOL::Rot");
                    var pos = Player.LocalPlayer.GetData<Vector3>("Temp::ATTOOL::PosOff");

                    var fr = Player.LocalPlayer.GetData<bool>("Temp::ATTOOL::FR");

                    if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Menu))
                    {
                        if (xyz == 0)
                            rot.X -= sens;
                        else if (xyz == 1)
                            rot.Y -= sens;
                        else
                            rot.Z -= sens;
                    }
                    else
                    {
                        if (xyz == 0)
                            pos.X -= sens;
                        else if (xyz == 1)
                            pos.Y -= sens;
                        else
                            pos.Z -= sens;
                    }

                    var ge = Player.LocalPlayer.GetData<GameEntity>("Temp::ATTOOL::GE");

                    RAGE.Game.Entity.DetachEntity(ge.Handle, false, false);

                    RAGE.Game.Entity.AttachEntityToEntity(gameEntity.Handle, Player.LocalPlayer.Handle, Player.LocalPlayer.GetBoneIndex(boneId), pos.X, pos.Y, pos.Z, rot.X, rot.Y, rot.Z, false, false, false, false, 2, fr);

                    Events.CallLocal("Chat::ShowServerMessage", $"[AttachTool] Pos: {pos.X}, {pos.Y}, {pos.Z} | Rot: {rot.X}, {rot.Y}, {rot.Z}");
                }),

                Input.Core.Bind(RAGE.Ui.VirtualKeys.Right, true, () =>
                {
                    if (Utils.Misc.IsAnyCefActive(true))
                        return;

                    var sens = Player.LocalPlayer.GetData<float>("Temp::ATTOOL::Sens");

                    var xyz = Player.LocalPlayer.GetData<int>("Temp::ATTOOL::XYZ");

                    var rot = Player.LocalPlayer.GetData<Vector3>("Temp::ATTOOL::Rot");
                    var pos = Player.LocalPlayer.GetData<Vector3>("Temp::ATTOOL::PosOff");

                    var fr = Player.LocalPlayer.GetData<bool>("Temp::ATTOOL::FR");

                    if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Menu))
                    {
                        if (xyz == 0)
                            rot.X += sens;
                        else if (xyz == 1)
                            rot.Y += sens;
                        else
                            rot.Z += sens;
                    }
                    else
                    {
                        if (xyz == 0)
                            pos.X += sens;
                        else if (xyz == 1)
                            pos.Y += sens;
                        else
                            pos.Z += sens;
                    }

                    var ge = Player.LocalPlayer.GetData<GameEntity>("Temp::ATTOOL::GE");

                    RAGE.Game.Entity.DetachEntity(ge.Handle, false, false);

                    RAGE.Game.Entity.AttachEntityToEntity(gameEntity.Handle, Player.LocalPlayer.Handle, Player.LocalPlayer.GetBoneIndex(boneId), pos.X, pos.Y, pos.Z, rot.X, rot.Y, rot.Z, false, false, false, false, 2, fr);

                    Events.CallLocal("Chat::ShowServerMessage", $"[AttachTool] Pos: {pos.X}, {pos.Y}, {pos.Z} | Rot: {rot.X}, {rot.Y}, {rot.Z}");
                }),
            };

            Player.LocalPlayer.SetData("Temp::ATTOOL::Binds", binds);
        }

        [Command("attachtool_sense", true, "Установить сытость игроку")]
        public static void AttachToolSense(float value)
        {
            if (!Player.LocalPlayer.HasData("Temp::ATTOOL::Sens"))
                return;

            Player.LocalPlayer.SetData("Temp::ATTOOL::Sens", value);

            Events.CallLocal("Chat::ShowServerMessage", $"[AttachTool] Sense - {value}!");
        }

        [Command("attachtool_stop", true, "Установить сытость игроку")]
        public static void AttachToolStop()
        {
            if (!Player.LocalPlayer.HasData("Temp::ATTOOL::Sens"))
                return;

            Player.LocalPlayer.GetData<GameEntity>("Temp::ATTOOL::GE")?.Destroy();

            Player.LocalPlayer.GetData<List<int>>("Temp::ATTOOL::Binds").ForEach((x) => Input.Core.Unbind(x));

            Player.LocalPlayer.ResetData("Temp::ATTOOL::Sens");
            Player.LocalPlayer.ResetData("Temp::ATTOOL::PosOff");
            Player.LocalPlayer.ResetData("Temp::ATTOOL::Rot");
            Player.LocalPlayer.ResetData("Temp::ATTOOL::GE");
            Player.LocalPlayer.ResetData("Temp::ATTOOL::Binds");
            Player.LocalPlayer.ResetData("Temp::ATTOOL::FR");
        }

        [Command("prop_spawn", true, "Установить сытость игроку")]
        public static async void SpawnProp(string model, float? posX = null, float? posY = null, float? posZ = null, float rotX = 0f, float rotY = 0f, float rotZ = 0f, bool onGround = false)
        {
            var propsList = Player.LocalPlayer.GetData<List<GameEntity>>("Temp::SPCL");

            if (propsList == null)
            {
                propsList = new List<GameEntity>();

                Player.LocalPlayer.SetData("Temp::SPCL", propsList);
            }

            var hash = RAGE.Util.Joaat.Hash(model);

            if (!await Streaming.RequestModel(hash))
                return;

            if (posX == null || posY == null || posZ == null)
            {
                posX = Player.LocalPlayer.Position.X;
                posY = Player.LocalPlayer.Position.Y;
                posZ = Player.LocalPlayer.Position.Z;
            }

            var gEntity = new MapObject(RAGE.Game.Object.CreateObject(hash, (float)posX, (float)posY, (float)posZ, false, false, false));

            RAGE.Game.Entity.FreezeEntityPosition(gEntity.Handle, true);

            RAGE.Game.Entity.SetEntityRotation(gEntity.Handle, rotX, rotY, rotZ, 2, true);

            if (onGround)
                gEntity.PlaceOnGroundProperly();

            propsList.Add(gEntity);
        }

        [Command("prop_del", true, "Установить сытость игроку")]
        public static void DelProp(int? handle = null)
        {
            var gEntity = handle is int handleInt ? Utils.Game.Misc.GetMapObjectByHandle(handleInt, false) : Player.LocalPlayer.GetData<GameEntity>("Temp::SEntity");

            if (gEntity == null)
                return;

            var propsList = Player.LocalPlayer.GetData<List<GameEntity>>("Temp::SPCL");

            if (propsList != null)
            {
                propsList.Remove(gEntity);

                if (propsList.Count == 0)
                    Player.LocalPlayer.ResetData("Temp::SPCL");
            }

            gEntity.Destroy();
        }

        [Command("entity_select_start", true, "Установить сытость игроку")]
        public static void EntitySelectorStart()
        {
            Player.LocalPlayer.SetData("Temp::SEntity", (GameEntity)null);
        }

        [Command("entity_select_stop", true, "Установить сытость игроку")]
        public static void EntitySelectorStop()
        {
            Player.LocalPlayer.ResetData("Temp::SEntity");
        }

        [Command("entity_select_edit_start", true, "Установить сытость игроку")]
        public static void EntitySelectorEditStart()
        {
            var gEntity = Player.LocalPlayer.GetData<GameEntity>("Temp::SEntity");

            if (gEntity?.Exists != true)
                return;

            if (MapEditor.IsActive)
                MapEditor.Close();

            MapEditor.Show
            (
                gEntity, "DebugEntityEdit", new MapEditor.Mode(true, true, true, true, true, true),

                () =>
                {

                },

                () => MapEditor.Render(),

                () =>
                {

                },

                (pos, rot) =>
                {

                }
            );
        }

        [Command("entity_select_edit_stop", true, "Установить сытость игроку")]
        public static void EntitySelectorEditStop()
        {
            if (!MapEditor.IsActive)
                return;

            MapEditor.Close();
        }

        [Command("colshape_edit_start", true, "Установить сытость игроку")]
        public static void ColshapeEditStart(int id)
        {
            var colshape = ExtraColshape.GetById(id);

            if (colshape == null)
                return;

            if (MapEditor.IsActive)
                MapEditor.Close();

            MapEditor.Show
            (
                colshape, "DebugColshapeEdit", new MapEditor.Mode(true, true, true, false, true, false),

                () =>
                {

                },

                () => MapEditor.RenderColshape(),

                () =>
                {

                },

                (pos, rot) =>
                {
                    if (pos == null)
                        pos = new Vector3(0f, 0f, 0f);

                    Events.CallLocal("Chat::ShowServerMessage", $"[TColshapes] Pos: {RAGE.Util.Json.Serialize(pos)}, Heading: {rot?.Z ?? 0f}");
                }
            );
        }

        [Command("colshape_edit_stop", true, "Установить сытость игроку")]
        public static void ColshapeEditStop()
        {
            if (!MapEditor.IsActive)
                return;

            MapEditor.Close();
        }

        [Command("farm_pos_save", true, "Установить сытость игроку")]
        public static void FarmPosSave()
        {
            var farm = Farm.All[38] as Farm;

            var t = new Dictionary<int, Dictionary<int, List<List<float>>>>();

            var fT = new Dictionary<int, List<List<float>>>();

            t.Add(farm.Id, fT);

            for (int i = 5; i < farm.CropFields.Count; i++)
            {
                var fiT = new List<List<float>>();

                fT.Add(i, fiT);

                for (byte j = 0; j < farm.CropFields[i].CropsData.Count; j++)
                {
                    var ficT = new List<float>();

                    fiT.Add(ficT);

                    for (byte k = 0; k < farm.CropFields[i].CropsData[j].Count; k++)
                    {
                        var coords = farm.CropFields[i].GetCropPosition3D(j, k);

                        var z = 0f;

                        RAGE.Game.Misc.GetGroundZFor3dCoord(coords.X, coords.Y, coords.Z + 10f, ref z, true);

                        ficT.Add(z);
                    }
                }
            }
            Utils.Misc.DebugServerSaveText(RAGE.Util.Json.Serialize(t));
        }


        [Command("pos_s", true, "Получить текущую позицию", "position_save")]
        public static void PositionSave(bool onGround = false, string info = "")
        {
            var pos = Player.LocalPlayer.Position;

            if (onGround)
                pos.Z -= 1f;
            Utils.Misc.DebugServerSaveText($"POS_S ({pos.X}f, {pos.Y}f, {pos.Z}f, {Player.LocalPlayer.GetHeading()}f) - {info}");
        }

        [Command("posv_s", true, "Получить текущую позицию", "position_save")]
        public static void PositionVehicleSave(string info = "")
        {
            var pos = Player.LocalPlayer.Vehicle?.Position;

            if (pos == null)
                return;
            Utils.Misc.DebugServerSaveText($"POSV_S ({pos.X}f, {pos.Y}f, {pos.Z}f, {Player.LocalPlayer.Vehicle.GetHeading()}f) - {info}");
        }

        [Command("debug_blip", true, "Получить текущую позицию")]
        public static void DebugBlip(uint model = 162, byte color = 1, string name = "DEBUG")
        {
            var pos = Player.LocalPlayer.Position;

            var blip = new ExtraBlip(model, pos, name, 1f, color, 255, 0f, true, 0, 0f, Player.LocalPlayer.Dimension, ExtraBlip.Types.Default);
        }

        [Command("tp_house", true, "Получить текущую позицию")]
        public static async void TpHouse(uint id)
        {
            var npc = NPC.GetData($"house_h_{id}");

            if (npc == null)
                return;

            Player.LocalPlayer.Position = npc.Ped.Position;

            await RAGE.Game.Invoker.WaitAsync(2000);

            Player.LocalPlayer.SetHeading(npc.Ped.GetHeading());
        }

        [Command("tp_garage", true, "Получить текущую позицию")]
        public static async void TpGarage(uint id)
        {
            var veh = RAGE.Elements.Entities.Vehicles.All.Where(x => x.GetData<uint>("HOUSE_ID") == id).FirstOrDefault();

            if (veh == null)
                return;

            var pos = veh.Position;

            pos.Z += 100f;

            Player.LocalPlayer.Position = pos;

            await RAGE.Game.Invoker.WaitAsync(3000);

            Player.LocalPlayer.SetIntoVehicle(veh.Handle, -1);
        }

        [Command("new_house", true, "Получить текущую позицию")]
        public static void NewHouse()
        {
            var last = House.All.Last();

            var house = new House(last.Key + 1, Player.LocalPlayer.Position, 2, 2, new Vector3(0f, 0f, 0f), 0, 0, 0);

            Player.LocalPlayer.SetData($"House::{house.Id}::RotZ", Player.LocalPlayer.GetHeading());

            house.ToggleOwnerBlip(true);
        }

        [Command("del_house", true, "")]
        public static void DelHouse(uint id)
        {
            House house;

            if (House.All.Remove(id, out house))
            {
                house.ToggleOwnerBlip(false);

                house.Colshape?.Destroy();
                house?.InfoText?.Destroy();
            }
        }

        [Command("garage_house", true, "")]
        public static void GarageHouse(uint id)
        {
            var house = House.All.GetValueOrDefault(id);

            if (house == null)
                return;

            house.GaragePosition = Player.LocalPlayer.Vehicle?.Position ?? Player.LocalPlayer.Position;

            Player.LocalPlayer.SetData($"House::{house.Id}::GRotZ", Player.LocalPlayer.Vehicle?.GetHeading() ?? Player.LocalPlayer.GetHeading());
        }

        [Command("enter_house", true, "")]
        public static void EnterHouse(uint id)
        {
            var house = House.All.GetValueOrDefault(id);

            if (house == null)
                return;

            var pos = Player.LocalPlayer.Position;

            house.Position.X = pos.X;
            house.Position.Y = pos.Y;
            house.Position.Z = pos.Z;

            Player.LocalPlayer.SetData($"House::{house.Id}::RotZ", Player.LocalPlayer.GetHeading());
        }

        [Command("save_house", true, "")]
        public static async void SaveHouse(uint id)
        {
            var house = House.All.GetValueOrDefault(id);

            if (house == null)
                return;

            var data = ((string)await Events.CallRemoteProc("debug_gethouseinfo", id))?.Split('_');

            if (house.GaragePosition == null)
                Utils.Misc.DebugServerSaveText($"new House({id}, new Utils.Vector4({house.Position.X}f, {house.Position.Y}f, {house.Position.Z}f, {Player.LocalPlayer.GetData<float?>($"House::{id}::RotZ") ?? float.Parse(data?[0] ?? "0")}f), Style.RoomTypes.{house.RoomType.ToString()}, {house.Price}, null, null);");
            else
                Utils.Misc.DebugServerSaveText($"new House({id}, new Utils.Vector4({house.Position.X}f, {house.Position.Y}f, {house.Position.Z}f, {Player.LocalPlayer.GetData<float?>($"House::{id}::RotZ") ?? float.Parse(data?[0] ?? "0")}f), Style.RoomTypes.{house.RoomType.ToString()}, {house.Price}, Garage.Types.Two, new Utils.Vector4({house.GaragePosition.X}f, {house.GaragePosition.Y}f, {house.GaragePosition.Z}f, {Player.LocalPlayer.GetData<float?>($"House::{id}::GRotZ") ?? float.Parse(data?[0] ?? "0")}f));");
        }
    }
}
