using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace BCRPClient.CEF
{
    class MapEditor : Events.Script
    {
        public static bool IsActive { get; private set; }

        private static GameEntity Entity { get; set; }

        private static List<int> TempBinds { get; set; }

        private static bool RotationModeOn { get; set; }

        private static ModeTypes? CurrentModeType { get; set; }

        private static DateTime LastUpdatedJs { get; set; }

        private static Vector3 LastPos { get; set; }

        public enum ModeTypes
        {
            Default = 0,

            FurnitureEdit,
        }

        public class Mode
        {
            public static Dictionary<ModeTypes, Mode> All { get; private set; } = new Dictionary<ModeTypes, Mode>()
            {
                { ModeTypes.Default, new Mode(true, true, true, true, true, true) },

                { ModeTypes.FurnitureEdit, new Mode(true, true, true, false, true, false) },
            };

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
                if (!IsActive || Entity?.Exists != true)
                    return;

                if ((bool)args[0])
                {
                    RAGE.Game.Entity.SetEntityCoordsNoOffset(Entity.Handle, Convert.ToSingle(args[1]), Convert.ToSingle(args[2]), Convert.ToSingle(args[3]), false, false, false);
                }
                else
                {
                    RAGE.Game.Entity.SetEntityRotation(Entity.Handle, Convert.ToSingle(args[1]), Convert.ToSingle(args[2]), Convert.ToSingle(args[3]), 2, true);
                }
            });
        }

        public static void Show(GameEntity gEntity, ModeTypes mType = ModeTypes.Default, bool enableRotation = false)
        {
            if (IsActive)
                return;

            Entity = gEntity;

            var mode = Mode.All[mType];

            RotationModeOn = false;

            if (enableRotation)
            {
                if (mode.IsRotationAllowed)
                    RotationModeOn = true;
            }

            if (RotationModeOn)
            {
                CEF.Browser.Window.ExecuteJs("mapEditor_init", true, mode.RotationEnableX, mode.RotationEnableY, mode.RotationEnableZ);
            }
            else
            {
                CEF.Browser.Window.ExecuteJs("mapEditor_init", false, mode.EnableX, mode.EnableY, mode.EnableZ);
            }

            LastUpdatedJs = DateTime.MinValue;

            GameEvents.Render -= Render;
            GameEvents.Render += Render;

            CurrentModeType = mType;

            IsActive = true;

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Control, true, () => ToggleRotationMode(!RotationModeOn)));
            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close()));

            if (CurrentModeType == ModeTypes.FurnitureEdit)
            {
                CEF.HouseMenu.FurnitureEditOnStart(gEntity as MapObject);

                TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Return, true, () => { if (Entity?.Exists == true) CEF.HouseMenu.FurntureEditFinish(Entity as MapObject, RAGE.Game.Entity.GetEntityCoords(Entity.Handle, false), RAGE.Game.Entity.GetEntityRotation(Entity.Handle, 2)); }));
            }

            Sync.WeaponSystem.DisabledFiring = true;
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            GameEvents.Render -= Render;

            CEF.Browser.Window.ExecuteCachedJs("mapEditor_destroy();");

            if (CurrentModeType == ModeTypes.FurnitureEdit)
            {
                CEF.HouseMenu.FurnitureEditOnEnd(Entity as MapObject);
            }

            if (CurrentModeType != ModeTypes.Default)
            {
                Entity?.Destroy();
            }

            CurrentModeType = null;

            IsActive = false;

            Entity = null;

            LastPos = null;

            foreach (var x in TempBinds)
                RAGE.Input.Unbind(x);

            TempBinds.Clear();

            Sync.WeaponSystem.DisabledFiring = false;
        }

        private static void ToggleRotationMode(bool state)
        {
            if (!IsActive)
                return;

            if (CurrentModeType is ModeTypes mType)
            {
                var mode = Mode.All[mType];

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

        private static void Render()
        {
            if (Entity?.Exists != true)
            {
                Close();

                return;
            }

            var ePos = RAGE.Game.Entity.GetEntityCoords(Entity.Handle, true);

            var lastPos = LastPos;

            LastPos = ePos;

            if (CurrentModeType == ModeTypes.FurnitureEdit)
            {
                if (Player.LocalPlayer.Position.DistanceTo(ePos) > 7.5f)
                {
                    if (LastPos == null)
                        LastPos = Player.LocalPlayer.Position;

                    ePos = lastPos;

                    LastPos = ePos;

                    Entity.Position = ePos;
                }
            }

            var eRot = RAGE.Game.Entity.GetEntityRotation(Entity.Handle, 2);

            if (DateTime.Now.Subtract(LastUpdatedJs).TotalMilliseconds > 200)
            {
                var camPos = RAGE.Game.Cam.GetGameplayCamCoord();

                var lookAtPos = Utils.GetWorldCoordFromScreenCoord(camPos, RAGE.Game.Cam.GetGameplayCamRot(0), 0.5f, 0.5f, 10);

                CEF.Browser.Window.ExecuteJs("mapEditor_update", ePos.X, ePos.Y, ePos.Z, eRot.X, eRot.Y, eRot.Z, camPos.X, camPos.Y, camPos.Z, lookAtPos.X, lookAtPos.Y, lookAtPos.Z);

                LastUpdatedJs = DateTime.Now;
            }

            bool showRotZ = false;

            float diffPos = RAGE.Input.IsDown(RAGE.Ui.VirtualKeys.Shift) ? 0.015f : 0.005f;

            if (!RAGE.Input.IsDown(RAGE.Ui.VirtualKeys.Menu))
            {
                float xOff = 0f, yOff = 0f;

                if (RAGE.Input.IsDown(RAGE.Ui.VirtualKeys.Left))
                    xOff -= diffPos;
                else if (RAGE.Input.IsDown(RAGE.Ui.VirtualKeys.Right))
                    xOff += diffPos;

                if (RAGE.Input.IsDown(RAGE.Ui.VirtualKeys.Up))
                    yOff -= diffPos;
                else if (RAGE.Input.IsDown(RAGE.Ui.VirtualKeys.Down))
                    yOff += diffPos;

                RAGE.Game.Entity.SetEntityCoordsNoOffset(Entity.Handle, ePos.X + xOff, ePos.Y + yOff, ePos.Z, false, false, false);
            }
            else
            {
                float diffRot = RAGE.Input.IsDown(RAGE.Ui.VirtualKeys.Shift) ? 0.5f : 0.25f;

                showRotZ = true;

                if (RAGE.Input.IsDown(RAGE.Ui.VirtualKeys.Up))
                {
                    RAGE.Game.Entity.SetEntityCoordsNoOffset(Entity.Handle, ePos.X, ePos.Y, ePos.Z + diffPos, false, false, false);
                }
                else if (RAGE.Input.IsDown(RAGE.Ui.VirtualKeys.Down))
                {
                    RAGE.Game.Entity.SetEntityCoordsNoOffset(Entity.Handle, ePos.X, ePos.Y, ePos.Z - diffPos, false, false, false);
                }

                if (RAGE.Input.IsDown(RAGE.Ui.VirtualKeys.Left))
                {
                    RAGE.Game.Entity.SetEntityRotation(Entity.Handle, eRot.X, eRot.Y, eRot.Z -= diffRot, 2, true);
                }
                else if (RAGE.Input.IsDown(RAGE.Ui.VirtualKeys.Right))
                {
                    RAGE.Game.Entity.SetEntityRotation(Entity.Handle, eRot.X, eRot.Y, eRot.Z += diffRot, 2, true);
                }
            }

            if (RotationModeOn || showRotZ)
            {
                float sX = 0f, sY = 0f;

                if (Entity.GetScreenPosition(ref sX, ref sY))
                {
                    Utils.DrawText($"Угол поворота: {RAGE.Game.Entity.GetEntityHeading(Entity.Handle).ToString("0.00")}", sX, sY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);
                }
            }

            if (CurrentModeType == ModeTypes.FurnitureEdit)
            {

            }
        }
    }
}
