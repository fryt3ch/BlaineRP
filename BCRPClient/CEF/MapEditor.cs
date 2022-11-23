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

        private static MapObject Object { get; set; }

        private static List<int> TempBinds { get; set; }

        private static bool RotationModeOn { get; set; }

        private static ModeTypes? CurrentModeType { get; set; }

        private static byte Tick { get; set; }

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
                if (!IsActive || Object == null)
                    return;

                if ((bool)args[0])
                {
                    Object.SetCoords(Convert.ToSingle(args[1]), Convert.ToSingle(args[2]), Convert.ToSingle(args[3]), false, false, false, false);
                }
                else
                {
                    Object.SetRotation(Convert.ToSingle(args[1]), Convert.ToSingle(args[2]), Convert.ToSingle(args[3]), 2, true);
                }
            });

            RAGE.Input.Bind(RAGE.Ui.VirtualKeys.X, true, () => Show(ModeTypes.FurnitureEdit, false));
            RAGE.Input.Bind(RAGE.Ui.VirtualKeys.C, true, () => Close());
        }

        public static void Show(ModeTypes mType = ModeTypes.Default, bool enableRotation = false)
        {
            if (IsActive)
                return;

            Object = new MapObject(RAGE.Util.Joaat.Hash("v_res_msonbed_s"), Additional.Camera.GetFrontOf(Player.LocalPlayer.Position, Player.LocalPlayer.GetHeading(), 2f), Player.LocalPlayer.GetRotation(2), 255, Player.LocalPlayer.Dimension);

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

            Tick = 0;

            GameEvents.Render -= Render;
            GameEvents.Render += Render;

            CurrentModeType = mType;

            IsActive = true;

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Shift, true, () => ToggleRotationMode(!RotationModeOn)));
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            GameEvents.Render -= Render;

            CEF.Browser.Window.ExecuteCachedJs("mapEditor_destroy();");

            CurrentModeType = null;

            IsActive = false;

            Object?.Destroy();

            Object = null;

            LastPos = null;

            foreach (var x in TempBinds)
                RAGE.Input.Unbind(x);

            TempBinds.Clear();
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
            if (Object?.Exists != true)
                return;

            var ePos = Object.GetCoords(true);

            if (Player.LocalPlayer.Position.DistanceTo(ePos) > 7.5f)
            {
                if (LastPos == null)
                    LastPos = Player.LocalPlayer.Position;

                ePos = LastPos;

                Object.Position = ePos;
            }
            else
                LastPos = ePos;

            var eRot = Object.GetRotation(2);

            if (++Tick % 4 == 0)
            {
                var camPos = RAGE.Game.Cam.GetGameplayCamCoord();

                var lookAtPos = Utils.GetWorldCoordFromScreenCoord(camPos, RAGE.Game.Cam.GetGameplayCamRot(0), 0.5f, 0.5f, 10);

                CEF.Browser.Window.ExecuteJs("mapEditor_update", ePos.X, ePos.Y, ePos.Z, eRot.X, eRot.Y, eRot.Z, camPos.X, camPos.Y, camPos.Z, lookAtPos.X, lookAtPos.Y, lookAtPos.Z);
            }
            else
            {
                if (Tick == byte.MaxValue)
                    Tick = 0;


            }
        }
    }
}
