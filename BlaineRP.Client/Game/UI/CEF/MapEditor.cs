using System;
using System.Collections.Generic;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;
using Core = BlaineRP.Client.Game.Management.Weapons.Core;

namespace BlaineRP.Client.UI.CEF
{
    [Script(int.MaxValue)]
    public class MapEditor
    {
        public static bool IsActive { get; private set; }

        private static GameEntity Entity { get; set; }

        private static ExtraColshape Colshape { get; set; }

        private static List<int> TempBinds { get; set; }

        private static bool RotationModeOn { get; set; }

        private static DateTime LastUpdatedJs { get; set; }

        private static Vector3 LastPos { get; set; }
        private static Vector3 LastRot { get; set; }

        public static string CurrentContext { get; private set; }

        private static Mode CurrentMode { get; set; }

        private static Action CurrentCloseAction { get; set; }
        private static Action CurrentRenderAction { get; set; }
        private static Action<Vector3, Vector3> CurrentFinishAction { get; set; }

        public static Utils.Vector4 PositionLimit { get; set; }

        public class Mode
        {
            public bool EnableX { get; set; }

            public bool EnableY { get; set; }

            public bool EnableZ { get; set; }

            public bool RotationEnableX { get; set; }

            public bool RotationEnableY { get; set; }

            public bool RotationEnableZ { get; set; }

            public bool IsRotationAllowed => RotationEnableX || RotationEnableY || RotationEnableZ;

            public bool IsTranslateAllowed => EnableX || EnableY || EnableZ;

            public Mode(bool EnableX, bool EnableY, bool EnableZ, bool RotationEnableX, bool RotationEnableY, bool RotationEnableZ)
            {
                this.EnableX = EnableX;
                this.EnableY = EnableY;
                this.EnableZ = EnableZ;

                this.RotationEnableX = RotationEnableX;
                this.RotationEnableY = RotationEnableY;
                this.RotationEnableZ = RotationEnableZ;
            }
        }

        public MapEditor()
        {
            TempBinds = new List<int>();

            Events.Add("MapEditor::Update", (object[] args) =>
            {
                if (!IsActive)
                    return;

                if ((bool)args[0])
                {
                    if (LastPos == null)
                        return;

                    LastPos.X = Utils.Convert.ToSingle(args[1]);
                    LastPos.Y = Utils.Convert.ToSingle(args[2]);
                    LastPos.Z = Utils.Convert.ToSingle(args[3]);
                }
                else
                {
                    if (LastRot == null)
                        return;

                    LastRot.X = Utils.Convert.ToSingle(args[1]);
                    LastRot.Y = Utils.Convert.ToSingle(args[2]);
                    LastRot.Z = Utils.Convert.ToSingle(args[3]);
                }
            });
        }

        public static void Show(object gEntity, string context, Mode mode, Action startAction = null, Action renderAction = null, Action closeAction = null, Action<Vector3, Vector3> finishAction = null)
        {
            if (IsActive)
                return;

            if (gEntity == null || renderAction == null)
                return;

            CurrentMode = mode ?? new Mode(true, true, true, true, true, true);

            CEF.Browser.Window.ExecuteJs("mapEditor_init", false, CurrentMode.EnableX, CurrentMode.EnableZ, CurrentMode.EnableY); // rot mode on: CEF.Browser.Window.ExecuteJs("mapEditor_init", true, mode.RotationEnableX, mode.RotationEnableY, mode.RotationEnableZ);

            CurrentContext = context;

            CurrentCloseAction = closeAction;
            CurrentRenderAction = renderAction;
            CurrentFinishAction = finishAction;

            IsActive = true;

            Core.DisabledFiring = true;

            startAction?.Invoke();

            if (gEntity is GameEntity gEntityT)
            {
                Entity = gEntityT;

                LastPos = RAGE.Game.Entity.GetEntityCoords(Entity.Handle, false);
                LastRot = RAGE.Game.Entity.GetEntityRotation(Entity.Handle, 2);

                Main.Render += CurrentRenderAction.Invoke;
            }
            else
            {
                Colshape = gEntity as ExtraColshape;

                LastPos = new Vector3(Colshape.Position.X, Colshape.Position.Y, Colshape.Position.Z);
                LastRot = new Vector3(0f, 0f, (Colshape as Polygon)?.Heading ?? 0f);

                Main.Render += CurrentRenderAction.Invoke;
            }

            TempBinds.Add(Input.Core.Bind(RAGE.Ui.VirtualKeys.Control, true, () => ToggleRotationMode(!RotationModeOn)));
            TempBinds.Add(Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close()));
            TempBinds.Add(Input.Core.Bind(RAGE.Ui.VirtualKeys.Return, true, () => finishAction?.Invoke(LastPos == null ? null : new Vector3(LastPos.X, LastPos.Y, LastPos.Z), LastRot == null ? null : new Vector3(LastRot.X, LastRot.Y, LastRot.Z))));
        }

        public static void Close(bool invokeCurrentCloseAction = true)
        {
            if (!IsActive)
                return;

            if (invokeCurrentCloseAction)
                CurrentCloseAction?.Invoke();

            Main.Render -= CurrentRenderAction.Invoke;

            CEF.Browser.Window.ExecuteCachedJs("mapEditor_destroy();");

            CurrentMode = null;
            CurrentContext = null;
            CurrentFinishAction = null;
            CurrentRenderAction = null;
            CurrentCloseAction = null;

            IsActive = false;

            Entity = null;
            Colshape = null;

            LastPos = null;

            foreach (var x in TempBinds)
                Input.Core.Unbind(x);

            TempBinds.Clear();

            Core.DisabledFiring = false;

            PositionLimit = null;
        }

        private static void ToggleRotationMode(bool state)
        {
            if (!IsActive)
                return;

            if (CurrentMode is Mode mode)
            {
                if (state)
                {
                    if (!mode.IsRotationAllowed)
                        return;

                    CEF.Browser.Window.ExecuteJs("mapEditor_toggleMode", true, mode.RotationEnableX, mode.RotationEnableY, mode.RotationEnableZ);
                }
                else
                {
                    if (!mode.IsTranslateAllowed)
                        return;

                    CEF.Browser.Window.ExecuteJs("mapEditor_toggleMode", false, mode.EnableX, mode.EnableY, mode.EnableZ);
                }

                RotationModeOn = state;
            }
        }

        public static void RenderFurnitureEdit()
        {
            if (LastPos == null || Entity?.Exists != true)
            {
                Close();

                return;
            }

            var playerPos = Player.LocalPlayer.GetCoords(false);

            var curPos = RAGE.Game.Entity.GetEntityCoords(Entity.Handle, false);

            if (PositionLimit != null)
            {
                if (LastPos.DistanceTo(PositionLimit.Position) > PositionLimit.RotationZ)
                {
                    if (curPos.DistanceTo(playerPos) > 10f)
                        curPos = playerPos;

                    LastPos = curPos;
                }
            }

            RAGE.Game.Entity.SetEntityCoordsNoOffset(Entity.Handle, LastPos.X, LastPos.Y, LastPos.Z, false, false, false);
            RAGE.Game.Entity.SetEntityRotation(Entity.Handle, LastRot.X, LastRot.Y, LastRot.Z, 2, false);

            if (Game.World.Core.ServerTime.Subtract(LastUpdatedJs).TotalMilliseconds > 100)
            {
                var camPos = RAGE.Game.Cam.GetGameplayCamCoord();

                var lookAtPos = Graphics.GetWorldCoordFromScreenCoord(camPos, RAGE.Game.Cam.GetGameplayCamRot(0), 0.5f, 0.5f, 10);

                CEF.Browser.Window.ExecuteJs("mapEditor_update", LastPos.X, LastPos.Y, LastPos.Z, LastRot.X, LastRot.Y, LastRot.Z, camPos.X, camPos.Y, camPos.Z, lookAtPos.X, lookAtPos.Y, lookAtPos.Z);

                LastUpdatedJs = Game.World.Core.ServerTime;
            }

            var showRotZ = false;

            var diffPos = Input.Core.IsDown(RAGE.Ui.VirtualKeys.Shift) ? 0.015f : 0.005f;

            if (!Input.Core.IsDown(RAGE.Ui.VirtualKeys.Menu))
            {
                float xOff = 0f, yOff = 0f;

                if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Left))
                    xOff -= diffPos;
                else if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Right))
                    xOff += diffPos;

                if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Up))
                    yOff -= diffPos;
                else if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Down))
                    yOff += diffPos;

                LastPos.X += xOff;
                LastPos.Y += yOff;
            }
            else
            {
                var diffRot = Input.Core.IsDown(RAGE.Ui.VirtualKeys.Shift) ? 0.5f : 0.25f;

                showRotZ = true;

                if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Up))
                {
                    LastPos.Z += diffPos;
                }
                else if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Down))
                {
                    LastPos.Z -= diffPos;
                }

                if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Left))
                {
                    LastRot.Z -= diffRot;
                }
                else if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Right))
                {
                    LastRot.Z += diffRot;
                }
            }

            if (RotationModeOn || showRotZ)
            {
                float sX = 0f, sY = 0f;

                if (Entity.GetScreenPosition(ref sX, ref sY))
                {
                    var text = Locale.Get("MAPEDITOR_ROTATION_ANGLE", LastRot.Z.ToString("0.00"));

                    Graphics.DrawText(text, sX, sY, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                }
            }
        }

        public static void RenderPlaceItem()
        {
            if (LastPos == null || Entity?.Exists != true)
            {
                Close();

                return;
            }

            var curPos = RAGE.Game.Entity.GetEntityCoords(Entity.Handle, false);

            if (Player.LocalPlayer.Position.DistanceTo(LastPos) > 7.5f)
            {
                if (curPos.DistanceTo(Player.LocalPlayer.Position) > 7.5f)
                    curPos = Player.LocalPlayer.Position;

                LastPos = curPos;
            }

            var zCoord = 0f;

            if (RAGE.Game.Misc.GetGroundZFor3dCoord(LastPos.X, LastPos.Y, LastPos.Z + 10f, ref zCoord, true))
                LastPos.Z = zCoord;

            RAGE.Game.Entity.SetEntityCoordsNoOffset(Entity.Handle, LastPos.X, LastPos.Y, LastPos.Z, false, false, false);
            RAGE.Game.Entity.SetEntityRotation(Entity.Handle, LastRot.X, LastRot.Y, LastRot.Z, 2, false);

            if (Game.World.Core.ServerTime.Subtract(LastUpdatedJs).TotalMilliseconds > 100)
            {
                var camPos = RAGE.Game.Cam.GetGameplayCamCoord();

                var lookAtPos = Graphics.GetWorldCoordFromScreenCoord(camPos, RAGE.Game.Cam.GetGameplayCamRot(0), 0.5f, 0.5f, 10);

                CEF.Browser.Window.ExecuteJs("mapEditor_update", LastPos.X, LastPos.Y, LastPos.Z, LastRot.X, LastRot.Y, LastRot.Z, camPos.X, camPos.Y, camPos.Z, lookAtPos.X, lookAtPos.Y, lookAtPos.Z);

                LastUpdatedJs = Game.World.Core.ServerTime;
            }

            var showRotZ = false;

            var diffPos = Input.Core.IsDown(RAGE.Ui.VirtualKeys.Shift) ? 0.015f : 0.005f;

            if (!Input.Core.IsDown(RAGE.Ui.VirtualKeys.Menu))
            {
                float xOff = 0f, yOff = 0f;

                if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Left))
                    xOff -= diffPos;
                else if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Right))
                    xOff += diffPos;

                if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Up))
                    yOff -= diffPos;
                else if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Down))
                    yOff += diffPos;

                LastPos.X += xOff;
                LastPos.Y += yOff;
            }
            else
            {
                var diffRot = Input.Core.IsDown(RAGE.Ui.VirtualKeys.Shift) ? 0.5f : 0.25f;

                showRotZ = true;

                if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Up))
                {
                    LastPos.Z += diffPos;
                }
                else if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Down))
                {
                    LastPos.Z -= diffPos;
                }

                if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Left))
                {
                    LastRot.Z -= diffRot;
                }
                else if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Right))
                {
                    LastRot.Z += diffRot;
                }
            }

            if (RotationModeOn || showRotZ)
            {
                float sX = 0f, sY = 0f;

                if (Entity.GetScreenPosition(ref sX, ref sY))
                {
                    var text = Locale.Get("MAPEDITOR_ROTATION_ANGLE", LastRot.Z.ToString("0.00"));

                    Graphics.DrawText(text, sX, sY, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                }
            }
        }

        public static void Render()
        {
            if (LastPos == null || Entity?.Exists != true)
            {
                Close();

                return;
            }

            RAGE.Game.Entity.SetEntityCoordsNoOffset(Entity.Handle, LastPos.X, LastPos.Y, LastPos.Z, false, false, false);
            RAGE.Game.Entity.SetEntityRotation(Entity.Handle, LastRot.X, LastRot.Y, LastRot.Z, 2, false);

            if (Game.World.Core.ServerTime.Subtract(LastUpdatedJs).TotalMilliseconds > 100)
            {
                var camPos = RAGE.Game.Cam.GetGameplayCamCoord();

                var lookAtPos = Graphics.GetWorldCoordFromScreenCoord(camPos, RAGE.Game.Cam.GetGameplayCamRot(0), 0.5f, 0.5f, 10);

                CEF.Browser.Window.ExecuteJs("mapEditor_update", LastPos.X, LastPos.Y, LastPos.Z, LastRot.X, LastRot.Y, LastRot.Z, camPos.X, camPos.Y, camPos.Z, lookAtPos.X, lookAtPos.Y, lookAtPos.Z);

                LastUpdatedJs = Game.World.Core.ServerTime;
            }

            var showRotZ = false;

            var diffPos = Input.Core.IsDown(RAGE.Ui.VirtualKeys.Shift) ? 0.015f : 0.005f;

            if (!Input.Core.IsDown(RAGE.Ui.VirtualKeys.Menu))
            {
                float xOff = 0f, yOff = 0f;

                if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Left))
                    xOff -= diffPos;
                else if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Right))
                    xOff += diffPos;

                if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Up))
                    yOff -= diffPos;
                else if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Down))
                    yOff += diffPos;

                LastPos.X += xOff;
                LastPos.Y += yOff;
            }
            else
            {
                var diffRot = Input.Core.IsDown(RAGE.Ui.VirtualKeys.Shift) ? 0.5f : 0.25f;

                showRotZ = true;

                if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Up))
                {
                    LastPos.Z += diffPos;
                }
                else if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Down))
                {
                    LastPos.Z -= diffPos;
                }

                if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Left))
                {
                    LastRot.Z -= diffRot;
                }
                else if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Right))
                {
                    LastRot.Z += diffRot;
                }
            }

            if (RotationModeOn || showRotZ)
            {
                float sX = 0f, sY = 0f;

                if (Entity.GetScreenPosition(ref sX, ref sY))
                {
                    var text = Locale.Get("MAPEDITOR_ROTATION_ANGLE", LastRot.Z.ToString("0.00"));

                    Graphics.DrawText(text, sX, sY, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                }
            }
        }

        public static void RenderColshape()
        {
            if (Colshape?.Exists != true)
            {
                Close();

                return;
            }

            if (Game.World.Core.ServerTime.Subtract(LastUpdatedJs).TotalMilliseconds > 100)
            {
                var camPos = RAGE.Game.Cam.GetGameplayCamCoord();

                var lookAtPos = Graphics.GetWorldCoordFromScreenCoord(camPos, RAGE.Game.Cam.GetGameplayCamRot(0), 0.5f, 0.5f, 10);

                CEF.Browser.Window.ExecuteJs("mapEditor_update", LastPos.X, LastPos.Y, LastPos.Z, 0, LastRot.Z, 0, camPos.X, camPos.Y, camPos.Z, lookAtPos.X, lookAtPos.Y, lookAtPos.Z);

                LastUpdatedJs = Game.World.Core.ServerTime;
            }

            Colshape.SetPosition(new Vector3(LastPos.X, LastPos.Y, LastPos.Z));
            (Colshape as Polygon)?.SetHeading(LastRot.Z);

            var showRotZ = false;

            var diffPos = Input.Core.IsDown(RAGE.Ui.VirtualKeys.Shift) ? 0.015f : 0.005f;

            if (!Input.Core.IsDown(RAGE.Ui.VirtualKeys.Menu))
            {
                float xOff = 0f, yOff = 0f;

                if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Left))
                    xOff -= diffPos;
                else if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Right))
                    xOff += diffPos;

                if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Up))
                    yOff -= diffPos;
                else if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Down))
                    yOff += diffPos;

                LastPos.X += xOff;
                LastPos.Y += yOff;
            }
            else
            {
                var diffRot = Input.Core.IsDown(RAGE.Ui.VirtualKeys.Shift) ? 0.5f : 0.25f;

                showRotZ = true;

                if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Up))
                {
                    LastPos.Y += diffPos;
                }
                else if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Down))
                {
                    LastPos.Y -= diffPos;
                }

                if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Left))
                {
                    LastRot.Z -= diffRot;
                }
                else if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Right))
                {
                    LastRot.Z += diffRot;
                }
            }

            if (RotationModeOn || showRotZ)
            {
                float sX = 0f, sY = 0f;

                if (Graphics.GetScreenCoordFromWorldCoord(LastPos, ref sX, ref sY))
                {
                    var text = Locale.Get("MAPEDITOR_ROTATION_ANGLE", LastRot.Z.ToString("0.00"));

                    Graphics.DrawText(text, sX, sY, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                }
            }
        }
    }
}
