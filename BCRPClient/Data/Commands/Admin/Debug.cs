using RAGE;
using RAGE.Elements;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Data
{
    partial class Commands
    {
        [Command("colshape_delete", true, "Удалить колшейп", "cs_del")]
        public static void ColshapeDelete(uint id)
        {
            var cs = Additional.ExtraColshape.GetById((int)id);

            cs?.Destroy();
        }

        [Command("poly_stop", true, "Закончить создание полигона")]
        public static void PolygonStop()
        {
            if (Additional.ExtraColshapes.PolygonCreationTask != null)
            {
                Additional.ExtraColshapes.PolygonCreationTask.Cancel();
                Additional.ExtraColshapes.PolygonCreationTask = null;

                Additional.ExtraColshapes.TempPolygon = null;
            }
        }

        [Command("poly_start", true, "Начать создание полигона")]
        public static void PolygonStart(float height = 0, float step = 1f)
        {
            if (step <= 0f)
                return;

            var newVertice = Player.LocalPlayer.Position;
            newVertice.Z -= 0.5f;

            if (Additional.ExtraColshapes.PolygonCreationTask != null)
            {
                Additional.ExtraColshapes.PolygonCreationTask.Cancel();

                Additional.ExtraColshapes.TempPolygon?.Destroy();
            }

            Additional.ExtraColshapes.TempPolygon = new Additional.Polygon(new List<Vector3>() { newVertice }, height, 0f, false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension, null);

            Additional.ExtraColshapes.PolygonCreationTask = new AsyncTask(() =>
            {
                if (Additional.ExtraColshapes.TempPolygon == null)
                    return true;

                var newVertice = Player.LocalPlayer.Position;
                newVertice.Z -= 1f;

                var vertices = Additional.ExtraColshapes.TempPolygon.Vertices;

                if (vertices[vertices.Count - 1].DistanceTo(newVertice) < step)
                    return false;

                Additional.ExtraColshapes.TempPolygon.AddVertice(newVertice);

                //Events.CallLocal("Chat::ShowServerMessage", $"[TColshapes::Polygon_{(height > 0 ? "3D" : "2D")}] New pos: {newVertice}");

                return false;
            }, 100, true, 0);

            Additional.ExtraColshapes.PolygonCreationTask.Run();
        }

        [Command("poly_rotate", true, "Повернуть полигон", "poly_rt")]
        public static void PolygonRotate(uint id, float angle)
        {
            var col = Additional.ExtraColshape.GetById((int)id);

            if (!(col is Additional.Polygon))
                return;

            (col as Additional.Polygon).Rotate(angle);
        }

        [Command("poly_angle", true, "Задать поворот полигона", "poly_ang")]
        public static void PolygonAngle(uint id, float angle)
        {
            var col = Additional.ExtraColshape.GetById((int)id);

            if (!(col is Additional.Polygon))
                return;

            (col as Additional.Polygon).SetHeading(angle);
        }

        [Command("colshape_new_circle", true, "Создать КШ Круг", "cs_n_crl")]
        public static void ColshapeNewCircle(float radius)
        {
            if (radius <= 0)
                return;

            var newVertice = Player.LocalPlayer.Position;
            newVertice.Z -= 1f;

            new Additional.Circle(newVertice, radius, false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension, null);

            Events.CallLocal("Chat::ShowServerMessage", $"[TColshapes::Circle_2D] Pos: {newVertice} | Radius: {radius}");
        }

        [Command("colshape_new_tube", true, "Создать КШ Цилиндр", "cs_n_tube")]
        public static void ColshapeNewTube(float radius, float height)
        {
            if (radius <= 0 || height <= 0)
                return;

            var newVertice = Player.LocalPlayer.Position;
            newVertice.Z -= 1f;

            new Additional.Cylinder(newVertice, radius, height, false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension, null);

            Events.CallLocal("Chat::ShowServerMessage", $"[TColshapes::Tube_3D] Pos: {newVertice} | Radius: {radius} | Height: {height}");
        }

        [Command("colshape_new_sphere", true, "Создать КШ Сфера", "cs_n_sph")]
        public static void ColshapeNewSphere(float radius)
        {
            if (radius <= 0)
                return;

            var newVertice = Player.LocalPlayer.Position;
            newVertice.Z -= 1f;

            new Additional.Sphere(newVertice, radius, false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension, null);

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
                new Additional.Cuboid(newVertice, sX, sY, height, 0, false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension);

                Events.CallLocal("Chat::ShowServerMessage", $"[TColshapes::Cuboid_3D] Pos: {newVertice} | Width: {sX} | Depth: {sY} | Height: {height}");
            }
        }

        [Command("cuboid_setwidth", true, "Добавить вершину к полигону", "c3d_width")]
        public static void CuboidSetWidth(uint id, float width)
        {
            var poly = Additional.ExtraColshape.GetById((int)id) as Additional.Cuboid;

            if (poly == null)
                return;

            poly.SetWidth(width);
        }

        [Command("cuboid_setdepth", true, "Добавить вершину к полигону", "c3d_depth")]
        public static void CuboidSetDepth(uint id, float depth)
        {
            var poly = Additional.ExtraColshape.GetById((int)id) as Additional.Cuboid;

            if (poly == null)
                return;

            poly.SetDepth(depth);
        }

        [Command("poly_addvertice", true, "Добавить вершину к полигону", "poly_addv")]
        public static void PolyAddVertice(uint id, int idx = -1)
        {
            var poly = Additional.ExtraColshape.GetById((int)id) as Additional.Polygon;

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
            var poly = Additional.ExtraColshape.GetById((int)id) as Additional.Polygon;

            if (poly == null)
                return;

            poly.RemoveVertice((int)vertId);
        }

        [Command("poly_setheight", true, "Задать высоту полигону", "poly_sheight")]
        public static void PolySetHeight(uint id, float height)
        {
            if (height < 0f)
                return;

            var poly = Additional.ExtraColshape.GetById((int)id) as Additional.Polygon;

            if (poly == null)
                return;

            poly.SetHeight(height);
        }

        [Command("colshape_save_server", true, "Задать высоту полигону", "poly_sheight")]
        public static void ColshapeSaveServer(uint id)
        {
            var poly = Additional.ExtraColshape.GetById((int)id);

            if (poly == null)
                return;

            Utils.DebugServerSaveText(poly.ShortData);
        }

        [Command("highpolymode", true, "Сменить режим вида полигонов", "hpolymode")]
        public static void HighPolyMode(bool? state = null)
        {
            if (state == null)
                Settings.Other.HighPolygonsMode = !Settings.Other.HighPolygonsMode;
            else
                Settings.Other.HighPolygonsMode = (bool)state;

            CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Notifications.Commands.Header, string.Format(Settings.Other.HighPolygonsMode ? Locale.Notifications.Commands.Enabled : Locale.Notifications.Commands.Disabled, "HighPolyMode"));
        }

        [Command("colshapes_visible", true, "Сменить видимость колшейпов", "cs_vis")]
        public static void ColshapesVisible(bool? state = null)
        {
            if (state == null)
                Settings.Other.ColshapesVisible = !Settings.Other.ColshapesVisible;
            else
                Settings.Other.ColshapesVisible = (bool)state;

            CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Notifications.Commands.Header, string.Format(Settings.Other.ColshapesVisible ? Locale.Notifications.Commands.Enabled : Locale.Notifications.Commands.Disabled, "ColshapesVisible"));
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
            if (BCRPClient.Interaction.CurrentEntity is Vehicle veh)
                Sync.Vehicles.BoatFromTrailerToWater(veh);
        }

        [Command("attachtool_start", true, "Установить сытость игроку")]
        public static async void AttachToolStart(string model, int boneId, float offX = 0f, float offY = 0f, float offZ = 0f, float rotX = 0f, float rotY = 0f, float rotZ = 0f, bool fixedRot = true)
        {
            AttachToolStop();

            var modelNum = RAGE.Util.Joaat.Hash(model);

            await Utils.RequestModel(modelNum);

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
                KeyBinds.Bind(RAGE.Ui.VirtualKeys.X, true, () =>
                {
                    Player.LocalPlayer.SetData("Temp::ATTOOL::XYZ", 0);

                    Events.CallLocal("Chat::ShowServerMessage", $"[AttachTool] Using X axis now!");
                }),
                KeyBinds.Bind(RAGE.Ui.VirtualKeys.Y, true, () =>
                {
                    Player.LocalPlayer.SetData("Temp::ATTOOL::XYZ", 1);

                    Events.CallLocal("Chat::ShowServerMessage", $"[AttachTool] Using Y axis now!");
                }),
                KeyBinds.Bind(RAGE.Ui.VirtualKeys.Z, true, () =>
                {
                    Player.LocalPlayer.SetData("Temp::ATTOOL::XYZ", 2);

                    Events.CallLocal("Chat::ShowServerMessage", $"[AttachTool] Using Z axis now!");
                }),

                KeyBinds.Bind(RAGE.Ui.VirtualKeys.Left, true, () =>
                {
                    if (Utils.IsAnyCefActive(true))
                        return;

                    var sens = Player.LocalPlayer.GetData<float>("Temp::ATTOOL::Sens");

                    var xyz = Player.LocalPlayer.GetData<int>("Temp::ATTOOL::XYZ");

                    var rot = Player.LocalPlayer.GetData<Vector3>("Temp::ATTOOL::Rot");
                    var pos = Player.LocalPlayer.GetData<Vector3>("Temp::ATTOOL::PosOff");

                    var fr = Player.LocalPlayer.GetData<bool>("Temp::ATTOOL::FR");

                    if (KeyBinds.IsDown(RAGE.Ui.VirtualKeys.Menu))
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

                KeyBinds.Bind(RAGE.Ui.VirtualKeys.Right, true, () =>
                {
                    if (Utils.IsAnyCefActive(true))
                        return;

                    var sens = Player.LocalPlayer.GetData<float>("Temp::ATTOOL::Sens");

                    var xyz = Player.LocalPlayer.GetData<int>("Temp::ATTOOL::XYZ");

                    var rot = Player.LocalPlayer.GetData<Vector3>("Temp::ATTOOL::Rot");
                    var pos = Player.LocalPlayer.GetData<Vector3>("Temp::ATTOOL::PosOff");

                    var fr = Player.LocalPlayer.GetData<bool>("Temp::ATTOOL::FR");

                    if (KeyBinds.IsDown(RAGE.Ui.VirtualKeys.Menu))
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

            Player.LocalPlayer.GetData<List<int>>("Temp::ATTOOL::Binds").ForEach((x) => KeyBinds.Unbind(x));

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

            if (!await Utils.RequestModel(hash))
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
            var gEntity = handle is int handleInt ? Utils.GetMapObjectByHandle(handleInt) as GameEntity : Player.LocalPlayer.GetData<GameEntity>("Temp::SEntity");

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

            if (CEF.MapEditor.IsActive)
                CEF.MapEditor.Close();

            CEF.MapEditor.Show(gEntity, CEF.MapEditor.ModeTypes.Default, false);
        }

        [Command("entity_select_edit_stop", true, "Установить сытость игроку")]
        public static void EntitySelectorEditStop()
        {
            if (!CEF.MapEditor.IsActive)
                return;

            CEF.MapEditor.Close();
        }

        [Command("colshape_edit_start", true, "Установить сытость игроку")]
        public static void ColshapeEditStart(int id)
        {
            var colshape = Additional.ExtraColshape.GetById(id);

            if (colshape == null)
                return;

            if (CEF.MapEditor.IsActive)
                CEF.MapEditor.Close();

            CEF.MapEditor.Show(colshape, CEF.MapEditor.ModeTypes.Colshape, false);
        }

        [Command("colshape_edit_stop", true, "Установить сытость игроку")]
        public static void ColshapeEditStop()
        {
            if (!CEF.MapEditor.IsActive)
                return;

            CEF.MapEditor.Close();
        }
    }
}
