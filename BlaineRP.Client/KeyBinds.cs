using BlaineRP.Client.CEF;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Client
{
    class KeyBinds
    {
        public static bool CursorNotFreezeInput { get; set; }

        private static Dictionary<Types, int> HelpBinds = new Dictionary<Types, int>()
        {
            { Types.ChatInput, 0 },
            { Types.MicrophoneOn, 1 },
            { Types.Menu, 2 },
            { Types.Inventory, 3 },
            { Types.Phone, 4 },
            { Types.Engine, 5 },
            { Types.Help, 6 },
        };

        public static Dictionary<Types, CEF.HUD.Menu.Types> HudMenuBinds { get; private set; } = new Dictionary<Types, CEF.HUD.Menu.Types>()
        {
            { Types.Animations, CEF.HUD.Menu.Types.Animations },
            { Types.Inventory, CEF.HUD.Menu.Types.Inventory },
            { Types.Phone, CEF.HUD.Menu.Types.Phone },
            { Types.BlipsMenu, CEF.HUD.Menu.Types.BlipsMenu },
        };

        public static Action CurrentExtraAction0 { get; set; }
        public static Action CurrentExtraAction1 { get; set; }

        private static HashSet<int> _justDownKeys = new HashSet<int>();
        private static HashSet<int> _justUpKeys = new HashSet<int>();

        public enum Types
        {
            None = -1,
            Cursor = 0,
            RadarSize,
            ChatInput,
            Inventory,
            FingerPointStart, FingerPointStop,
            Crouch, Crawl,
            MicrophoneOn, MicrophoneOff, MicrophoneReload,
            DoorsLock, TrunkLook, Engine, LeftArrow, BothArrows, RightArrow, Lights, Belt, CruiseControl, AutoPilot,
            Interaction,
            Phone,
            Menu,
            HUD,
            TakeItem, ReloadWeapon,
            Whistle,
            Animations,
            CancelAnimation,
            Help,
            BlipsMenu,
            AnchorBoat,
            SendCoordsToDriver,
            FlashlightToggle,
            TakeScreenshot,

            ExtraAction0, ExtraAction1,

            EnterVehicle, PlaneLandingGear,

            weapon0, weapon1, weapon2,
            pockets0, pockets1, pockets2, pockets3, pockets4, pockets5, pockets6, pockets7, pockets8, pockets9, pockets10, pockets11, pockets12, pockets13, pockets14, pockets15, pockets16, pockets17, pockets18, pockets19
        }

        private static Dictionary<int, string> _keyNames = new Dictionary<int, string>()
        {
            { 0, "-" },
            { 3, "Cancel" },
            { 6, "Help" },
            { 8, "Backspace" },
            { 9, "⭾" },
            { 12, "Clear" },
            { 13, "Enter" },
            { 14, "Enter S." },
            { 16, "Shift" },
            { 17, "Ctrl" },
            { 18, "Alt" },
            { 19, "Pause" },
            { 20, "Capslock" },
            { 21, "KANA" },
            { 22, "EISU" },
            { 23, "JUNJA" },
            { 24, "FINAL" },
            { 25, "HANJA" },
            { 27, "Esc" },
            { 28, "Convert" },
            { 29, "NonConvert" },
            { 30, "Accept" },
            { 31, "ModeChange" },
            { 32, "Space" },
            { 33, "Page ↑" },
            { 34, "Page ↓" },
            { 35, "End" },
            { 36, "Home" },
            { 37, "🠘" },
            { 38, "🠙" },
            { 39, "🠚" },
            { 40, "🠛" },
            { 41, "Select" },
            { 42, "Print" },
            { 43, "Execute" },
            { 44, "PrtSc" },
            { 45, "Ins" },
            { 46, "Del" },
            { 48, "0" },
            { 49, "1" },
            { 50, "2" },
            { 51, "3" },
            { 52, "4" },
            { 53, "5" },
            { 54, "6" },
            { 55, "7" },
            { 56, "8" },
            { 57, "9" },
            { 58, ":" },
            { 59, ";" },
            { 60, "<" },
            { 61, "=" },
            { 62, ">" },
            { 63, "?" },
            { 64, "AT" },
            { 65, "A" },
            { 66, "B" },
            { 67, "C" },
            { 68, "D" },
            { 69, "E" },
            { 70, "F" },
            { 71, "G" },
            { 72, "H" },
            { 73, "I" },
            { 74, "J" },
            { 75, "K" },
            { 76, "L" },
            { 77, "M" },
            { 78, "N" },
            { 79, "O" },
            { 80, "P" },
            { 81, "Q" },
            { 82, "R" },
            { 83, "S" },
            { 84, "T" },
            { 85, "U" },
            { 86, "V" },
            { 87, "W" },
            { 88, "X" },
            { 89, "Y" },
            { 90, "Z" },
            { 91, "OS" },
            { 93, "Contex Menu" },
            { 95, "Sleep" },
            { 96, "Num. 0" },
            { 97, "Num. 1" },
            { 98, "Num. 2" },
            { 99, "Num. 3" },
            { 100, "Num. 4" },
            { 101, "Num. 5" },
            { 102, "Num. 6" },
            { 103, "Num. 7" },
            { 104, "Num. 8" },
            { 105, "Num. 9" },
            { 106, "*" },
            { 107, "+" },
            { 108, "," },
            { 109, "-" },
            { 110, "." },
            { 111, "/" },
            { 112, "F1" },
            { 113, "F2" },
            { 114, "F3" },
            { 115, "F4" },
            { 116, "F5" },
            { 117, "F6" },
            { 118, "F7" },
            { 119, "F8" },
            { 120, "F9" },
            { 121, "F10" },
            { 122, "F11" },
            { 123, "F12" },
            { 124, "F13" },
            { 125, "F14" },
            { 126, "F15" },
            { 127, "F16" },
            { 128, "F17" },
            { 129, "F18" },
            { 130, "F19" },
            { 131, "F20" },
            { 132, "F21" },
            { 133, "F22" },
            { 134, "F23" },
            { 135, "F24" },
            { 144, "Num" },
            { 145, "Scr Lk" },
            { 146, "WIN_OEM_FJ_JISHO" },
            { 147, "WIN_OEM_FJ_MASSHOU" },
            { 148, "WIN_OEM_FJ_TOUROKU" },
            { 149, "WIN_OEM_FJ_LOYA" },
            { 150, "WIN_OEM_FJ_ROYA" },
            { 160, "[" },
            { 161, "!" },
            { 162, "\"" },
            { 163, "#" },
            { 164, "$" },
            { 165, "%" },
            { 166, "&" },
            { 167, "_" },
            { 168, "(" },
            { 169, ")" },
            { 170, "*" },
            { 171, "+" },
            { 172, "|" },
            { 173, "-" },
            { 174, "{" },
            { 175, "}" },
            { 176, "~" },
            { 181, "Vol. Mute" },
            { 182, "Vol. ↓" },
            { 183, "Vol. ↑" },
            { 186, ";" },
            { 187, "=" },
            { 188, "," },
            { 189, "-" },
            { 190, "." },
            { 191, "/" },
            { 192, "~" },
            { 219, "{" },
            { 220, "\\" },
            { 221, "]" },
            { 222, "'" },
            { 224, "Meta" },
            { 225, "AltGr" },
            { 227, "WIN_ICO_HELP" },
            { 228, "WIN_ICO_00" },
            { 230, "WIN_ICO_CLEAR" },
            { 233, "WIN_OEM_RESET" },
            { 234, "WIN_OEM_JUMP" },
            { 235, "WIN_OEM_PA1" },
            { 236, "WIN_OEM_PA2" },
            { 237, "WIN_OEM_PA3" },
            { 238, "WIN_OEM_WSCTRL" },
            { 239, "WIN_OEM_CUSEL" },
            { 240, "WIN_OEM_ATTN" },
            { 241, "WIN_OEM_FINISH" },
            { 242, "WIN_OEM_COPY" },
            { 243, "WIN_OEM_AUTO" },
            { 244, "WIN_OEM_ENLW" },
            { 245, "WIN_OEM_BACKTAB" },
            { 246, "Attin" },
            { 247, "Crsel" },
            { 248, "Exsel" },
            { 249, "Ereof" },
            { 250, "Play" },
            { 251, "Zoo," },
            { 253, "PA1" },
            { 254, "WIN_OEM_CLEAR" },
    };

        private static Dictionary<Types, RAGE.Ui.VirtualKeys[]> DefaultBinds = new Dictionary<Types, RAGE.Ui.VirtualKeys[]>()
        {
            { Types.Cursor, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.OEM3 } },
            { Types.RadarSize, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.Z } },
            { Types.ChatInput, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.T } },
            { Types.FingerPointStart, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.N } },
            { Types.FingerPointStop, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.N } },
            { Types.Crouch, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.Control } },
            { Types.Crawl, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.Control, RAGE.Ui.VirtualKeys.Shift } },
            { Types.MicrophoneOn, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.Tab } },
            { Types.MicrophoneOff, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.Tab } },
            { Types.MicrophoneReload, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.F3 } },
            { Types.DoorsLock, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.L } },
            { Types.Engine, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.N } },
            { Types.LeftArrow, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.Numpad4 } },
            { Types.RightArrow, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.Numpad6 } },
            { Types.BothArrows, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.Numpad5 } },
            { Types.Lights, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.Numpad8 } },
            { Types.Belt, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.J } },
            { Types.CruiseControl, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.K } },
            { Types.AutoPilot, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.X } },
            { Types.Interaction, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.G } },
            { Types.Phone, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.P } },
            { Types.Menu, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.M } },
            { Types.Help, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.F10 } },

            { Types.HUD, new RAGE.Ui.VirtualKeys[] { } },
            { Types.TrunkLook, new RAGE.Ui.VirtualKeys[] { } },
            { Types.Inventory, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.I } },
            { Types.TakeItem, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.E } },
            { Types.ReloadWeapon, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.R } },

            { Types.Whistle, new RAGE.Ui.VirtualKeys[] { } },

            { Types.Animations, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.U} },
            { Types.CancelAnimation, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.C } },

            { Types.BlipsMenu, new RAGE.Ui.VirtualKeys[] { } },

            { Types.AnchorBoat, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.Z } },

            { Types.SendCoordsToDriver, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.O } },

            { Types.FlashlightToggle, new RAGE.Ui.VirtualKeys[] { } },
            { Types.TakeScreenshot, new RAGE.Ui.VirtualKeys[] { } },

            { Types.ExtraAction0, new RAGE.Ui.VirtualKeys[] { } },
            { Types.ExtraAction1, new RAGE.Ui.VirtualKeys[] { } },

            { Types.EnterVehicle, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.F, } },
            { Types.PlaneLandingGear, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.X, } },

            { Types.weapon0, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.N1 } },
            { Types.weapon1, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.N2 } },
            { Types.weapon2, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.N3 } },

            { Types.pockets0, new RAGE.Ui.VirtualKeys[] { } }, { Types.pockets1, new RAGE.Ui.VirtualKeys[] { } }, { Types.pockets2, new RAGE.Ui.VirtualKeys[] { } },
            { Types.pockets3, new RAGE.Ui.VirtualKeys[] { } }, { Types.pockets4, new RAGE.Ui.VirtualKeys[] { } }, { Types.pockets5, new RAGE.Ui.VirtualKeys[] { } },
            { Types.pockets6, new RAGE.Ui.VirtualKeys[] { } }, { Types.pockets7, new RAGE.Ui.VirtualKeys[] { } }, { Types.pockets8, new RAGE.Ui.VirtualKeys[] { } },
            { Types.pockets9, new RAGE.Ui.VirtualKeys[] { } }, { Types.pockets10, new RAGE.Ui.VirtualKeys[] { } }, { Types.pockets11, new RAGE.Ui.VirtualKeys[] { } },
            { Types.pockets12, new RAGE.Ui.VirtualKeys[] { } }, { Types.pockets13, new RAGE.Ui.VirtualKeys[] { } }, { Types.pockets14, new RAGE.Ui.VirtualKeys[] { } },
            { Types.pockets15, new RAGE.Ui.VirtualKeys[] { } }, { Types.pockets16, new RAGE.Ui.VirtualKeys[] { } }, { Types.pockets17, new RAGE.Ui.VirtualKeys[] { } },
            { Types.pockets18, new RAGE.Ui.VirtualKeys[] { } }, { Types.pockets19, new RAGE.Ui.VirtualKeys[] { } },
        };

        public static Dictionary<Types, ExtraBind> Binds { get; set; } = new Dictionary<Types, ExtraBind>();

        #region Bind Class

        public class ExtraBind
        {
            /// <summary>Тип бинда</summary>
            public Types Type { get; private set; }

            /// <summary>Выполняемое действие</summary>
            public Action Action { get; private set; }

            /// <summary>Клавиши</summary>
            public RAGE.Ui.VirtualKeys[] Keys { get; private set; }

            /// <summary>Срабатывает ли сразу при нажатии (без отпускания)?</summary>
            public bool IsDown { get; private set; }

            /// <summary>Индекс бинда</summary>
            /// <value>-1, если бинд не активен, число в противном случае</value>
            public int BindIndex { get; private set; }

            public bool IsDisabled => DisabledCounter > 0;

            private int DisabledCounter { get; set; }

            /// <summary>Тип родителя бинда</summary>
            public Types Parent { get; private set; }

            /// <summary>Описание бинда</summary>
            public string Description { get; set; }

            /// <summary>Изменяемый ли бинд?</summary>
            public bool Changeable { get; private set; }

            /// <summary>Бинд для инвентаря?</summary>
            public bool InvOnly { get; private set; }

            public bool IsPressed
            {
                get
                {
                    if (Keys.Length == 0)
                        return false;

                    if (IsDown)
                    {
                        for (int i = 0; i < Keys.Length; i++)
                            if (!KeyBinds.IsDown(Keys[i]))
                                return false;

                        return true;
                    }
                    else
                    {
                        for (int i = 0; i < Keys.Length; i++)
                            if (!KeyBinds.IsUp(Keys[i]))
                                return false;

                        return true;
                    }
                }
            }

            public bool IsJustPressed
            {
                get
                {
                    if (Keys.Length == 0)
                        return false;

                    if (IsDown)
                    {
                        for (int i = 0; i < Keys.Length; i++)
                            if (!KeyBinds.IsJustDown(Keys[i]))
                                return false;

                        return true;
                    }
                    else
                    {
                        for (int i = 0; i < Keys.Length; i++)
                            if (!KeyBinds.IsJustUp(Keys[i]))
                                return false;

                        return true;
                    }
                }
            }

            public ExtraBind(Types Type, Action Action, bool IsDown, bool Changeable, Types Familiar = Types.None, bool InvOnly = false)
            {
                this.BindIndex = -1;

                this.Type = Type;
                this.Action = Action;
                this.IsDown = IsDown;

                this.Changeable = Changeable;
                this.Parent = Familiar;

                this.InvOnly = InvOnly;

                if (Changeable)
                {
                    this.Keys = Additional.Storage.GetData<RAGE.Ui.VirtualKeys[]>($"KeyBinds::{Type}");

                    if (this.Keys == null)
                    {
                        this.Keys = DefaultBinds[Type];

                        Additional.Storage.SetData($"KeyBinds::{Type}", this.Keys);
                    }
                }
                else if (Familiar == Types.None)
                    this.Keys = DefaultBinds[Type];
                else
                    this.Keys = Binds.ContainsKey(Familiar) ? Binds[Familiar].Keys : DefaultBinds[Type];

                Description = Type.ToString();
            }

            public void Enable()
            {
                if (DisabledCounter > 0)
                {
                    DisabledCounter--;

                    if (DisabledCounter > 0)
                        return;
                }

                if (BindIndex != -1)
                    return;

                if (Keys.Length == 0)
                    return;

                if (Keys.Length == 1)
                {
                    BindIndex = KeyBinds.Bind(Keys[0], IsDown, async () =>
                    {
                        Action.Invoke();
                    });
                }
                else
                {
                    BindIndex = KeyBinds.Bind(Keys[Keys.Length - 1], IsDown, async () =>
                    {
                        Func<RAGE.Ui.VirtualKeys, bool> checkFunc = KeyBinds.IsDown;

                        if (!IsDown)
                            checkFunc = KeyBinds.IsUp;

                        for (int i = 0; i < Keys.Length - 1; i++)
                            if (!checkFunc.Invoke(Keys[i]))
                                return;

                        Action.Invoke();
                    });
                }
            }

            public void Disable()
            {
                DisabledCounter++;

                if (BindIndex == -1)
                    return;

                KeyBinds.Unbind(BindIndex);

                BindIndex = -1;
            }

            public void EnableAnyway()
            {
                DisabledCounter = 0;

                Enable();
            }

            public void ChangeKeys(RAGE.Ui.VirtualKeys[] keys)
            {
                bool lastKeysNone = Keys.Length == 0;

                Keys = keys;

                if (Changeable)
                {
                    Additional.Storage.SetData($"KeyBinds::{Type}", keys);

                    if (HelpBinds.ContainsKey(Type))
                    {
                        CEF.Browser.Window.ExecuteJs("Hud.changeHelpKey", HelpBinds[Type], GetKeyString());
                    }

                    if (HudMenuBinds.ContainsKey(Type))
                    {
                        CEF.HUD.Menu.UpdateCurrentTypes(Keys.Length == 0, HudMenuBinds[Type]);
                    }
                }

                foreach (var bind in Binds.Where(x => x.Value.Parent == Type))
                    bind.Value.ChangeKeys(keys);

                int lastState = BindIndex;

                Disable();

                if (lastState != -1 || lastKeysNone)
                    Enable();
            }

            public string GetKeyString() => GetKeyString(Keys);

            public static string GetKeyString(params RAGE.Ui.VirtualKeys[] keys) => keys.Length == 0 ? "???" : string.Join(" + ", keys.Select(x => _keyNames.GetValueOrDefault((int)x) ?? ""));
        }

        public static void Add(ExtraBind bind, bool enable = true)
        {
            if (HelpBinds.ContainsKey(bind.Type))
            {
                CEF.Browser.Window.ExecuteJs("Hud.changeHelpKey", HelpBinds[bind.Type], bind.GetKeyString());
            }

            if (HudMenuBinds.ContainsKey(bind.Type))
            {
                CEF.HUD.Menu.UpdateCurrentTypes(bind.Keys.Length == 0, HudMenuBinds[bind.Type]);
            }

            ExtraBind oldBind;

            if (Binds.Remove(bind.Type, out oldBind))
            {
                oldBind.Disable();
            }

            if (enable)
                bind.Enable();

            Binds.Add(bind.Type, bind);
        }

        public static void Remove(Types type)
        {
            var bind = Binds.GetValueOrDefault(type);

            if (bind == null)
                return;

            bind.Disable();
            Binds.Remove(type);
        }

        public static ExtraBind Get(Types type)
        {
            return Binds.GetValueOrDefault(type);
        }

        #endregion

        #region Loaders
        #region Main
        public static void LoadMain()
        {
            foreach (var x in Enum.GetValues(typeof(RAGE.Ui.VirtualKeys)).Cast<RAGE.Ui.VirtualKeys>())
            {
                RAGE.Input.Bind(x, true, async () =>
                {
                    _justDownKeys.Add((int)x);

                    await RAGE.Game.Invoker.WaitAsync(0);

                    _justDownKeys.Remove((int)x);
                });

                RAGE.Input.Bind(x, false, async () =>
                {
                    _justUpKeys.Add((int)x);

                    await RAGE.Game.Invoker.WaitAsync(0);

                    _justUpKeys.Remove((int)x);
                });
            }

            // ~ - Toggle Cursor
            Add(new ExtraBind(Types.Cursor, () =>
            {
                if (!RAGE.Game.Ui.IsPauseMenuActive())
                {
                    if (CEF.Cursor.IsVisible)
                    {
                        CEF.Cursor.Show(false, false);
                    }
                    else
                    {
                        CEF.Cursor.Show(!CursorNotFreezeInput, true);
                    }
                }
            }, true, true)
            { Description = "Скрыть / показать курсор" }, true);
        }
        #endregion

        #region All
        public static void LoadAll()
        {
            // Toggle Chat Input
            Add(new ExtraBind(Types.ChatInput, () =>
            {
                if (Misc.CanShowCEF(true, true))
                {
                    CEF.Chat.ShowInput(true);
                }
            }, true, true)
            { Description = "Открыть чат" });

            // Open Menu
            Add(new ExtraBind(Types.Menu, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.HUD.Menu.Switch(true, null);
            }, true, true)
            { Description = "Меню" });

            // Toggle Radar Size
            Add(new ExtraBind(Types.RadarSize, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    MiniMap.Toggle();

            }, true, true)
            { Description = "Масштаб миникарты" });

            // Use Micro Start
            Add(new ExtraBind(Types.MicrophoneOn, () =>
            {
                if (Misc.CanShowCEF(false, true))
                    Sync.Microphone.Start();
            }, true, true)
            { Description = "Голосовой чат" });

            // Use Micro Stop
            Add(new ExtraBind(Types.MicrophoneOff, () =>
            {
                Sync.Microphone.Stop();
            }, false, false, Types.MicrophoneOn));

            // Interaction
            Add(new ExtraBind(Types.Interaction, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Interaction.TryShowMenu();
            }, true, true)
            { Description = "Меню взаимодействия" });

            // Phone
            Add(new ExtraBind(Types.Phone, () =>
            {
                if (!Sync.Phone.Toggled)
                {
                    if (Misc.CanShowCEF(true, true))
                        Sync.Phone.Toggle();
                }
                else
                {
                    Sync.Phone.Toggle();
                }
            }, true, true)
            { Description = "Телефон" });

            // Finger Point Start
            Add(new ExtraBind(Types.FingerPointStart, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    Sync.Finger.Start();
            }, true, true)
            { Description = "Показать пальцем" });

            // Finger Point Stop
            Add(new ExtraBind(Types.FingerPointStop, () =>
            {
                Sync.Finger.Stop();
            }, false, false, Types.FingerPointStart));

            // Crouch
            Add(new ExtraBind(Types.Crouch, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    Sync.Crouch.Toggle();
            }, true, true)
            { Description = "Присесть" });

            // Crawl
            Add(new ExtraBind(Types.Crawl, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    Sync.Crawl.Toggle();
            }, true, true)
            { Description = "Ползти" });

            // Engine Toggle
            Add(new ExtraBind(Types.Engine, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    Sync.Vehicles.ToggleEngine(Player.LocalPlayer.Vehicle, null);
            }, true, true)
            { Description = "Двигатель Т/С" });

            // Cruise Control
            Add(new ExtraBind(Types.CruiseControl, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    Sync.Vehicles.ToggleCruiseControl(false);
            }, true, true)
            { Description = "Круиз-контроль" });

            // Auto Pilot
            Add(new ExtraBind(Types.AutoPilot, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    Sync.Vehicles.ToggleAutoPilot(null);
            }, true, true)
            { Description = "Автопилот" });

            // Vehicle Doors Lock Toggle
            Add(new ExtraBind(Types.DoorsLock, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    Sync.Vehicles.Lock(null, Interaction.CurrentEntity as Vehicle);
            }, true, true)
            { Description = "Блокировка Т/С" });

            // Vehicle Look in Trunk
            Add(new ExtraBind(Types.TrunkLook, () =>
            {
                if (Misc.CanShowCEF(true, true) && Interaction.CurrentEntity is Vehicle veh)
                    Sync.Vehicles.ShowContainer(veh);
            }, true, true)
            { Description = "Смотреть багажник" });

            // Seat Belt Toggle
            Add(new ExtraBind(Types.Belt, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    Sync.Vehicles.ToggleBelt();
            }, true, true)
            { Description = "Пристегнуться" });

            // Left Arrow Veh
            Add(new ExtraBind(Types.LeftArrow, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    Sync.Vehicles.ToggleIndicator(Player.LocalPlayer.Vehicle, 1);
            }, true, true)
            { Description = "Левый поворотник" });

            // Right Arrow Veh
            Add(new ExtraBind(Types.RightArrow, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    Sync.Vehicles.ToggleIndicator(Player.LocalPlayer.Vehicle, 0);
            }, true, true)
            { Description = "Правый поворотник" });

            // Both Arrows Veh
            Add(new ExtraBind(Types.BothArrows, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    Sync.Vehicles.ToggleIndicator(Player.LocalPlayer.Vehicle, 2);
            }, true, true)
            { Description = "Аварийка" });

            // Lights Veh
            Add(new ExtraBind(Types.Lights, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    Sync.Vehicles.ToggleLights(Player.LocalPlayer.Vehicle);
            }, true, true)
            { Description = "Фары" });

            // Toggle HUD 
            Add(new ExtraBind(Types.HUD, () =>
            {
                if (Misc.CanShowCEF(false, true))
                {
                    Settings.User.Interface.HideHUD = !Settings.User.Interface.HideHUD;
                }
            }, true, true)
            { Description = "HUD" });

            // Reload Voice Chat 
            Add(new ExtraBind(Types.MicrophoneReload, () =>
            {
                if (Misc.CanShowCEF(false, true))
                    Sync.Microphone.Reload();
            }, true, true)
            { Description = "Перезапустить голосовой чат" });

            // Inventory Open 
            Add(new ExtraBind(Types.Inventory, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.Show(CEF.Inventory.Types.Inventory);
            }, true, true)
            { Description = "Инвентарь" });

            // Take Item on Ground
            Add(new ExtraBind(Types.TakeItem, () =>
            {
                if (Misc.CanShowCEF(true, true))
                {
                    if (Sync.World.ClosestItemOnGround == null)
                        return;

                    Sync.World.ClosestItemOnGround.TakeItem();
                }
            }, true, true)
            { Description = "Подобрать предмет" });

            // ReloadWeapon
            Add(new ExtraBind(Types.ReloadWeapon, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    Sync.WeaponSystem.ReloadWeapon();
            }, true, true)
            { Description = "Перезарядить оружие" });

            // Whistle
            Add(new ExtraBind(Types.Whistle, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    Sync.Animations.PlayFastSync(Sync.Animations.FastTypes.Whistle);
            }, true, true)
            { Description = "Свистеть" });

            // Whistle
            Add(new ExtraBind(Types.SendCoordsToDriver, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    Sync.Vehicles.SendCoordsToDriver();
            }, true, true)
            { Description = "Передать метку водителю" });

            Add(new ExtraBind(Types.Animations, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Animations.Open();
            }, true, true)
            { Description = "Анимации" });

            Add(new ExtraBind(Types.CancelAnimation, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Animations.Cancel();
            }, true, true)
            { Description = "Отмена анимации" });

            Add(new ExtraBind(Types.Help, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Menu.Show(CEF.Menu.SectionTypes.Help);
            }, true, true)
            { Description = "Помощь" });

            Add(new ExtraBind(Types.BlipsMenu, () =>
            {
                if (Misc.CanShowCEF(true, true))
                {
                    BlipsMenu.Show();
                }
            }, true, true)
            { Description = "Меню меток" });

            Add(new ExtraBind(Types.AnchorBoat, () =>
            {
                if (Misc.CanShowCEF(true, true))
                {
                    Sync.Vehicles.ToggleAnchor();
                }
            }, true, true)
            { Description = "Якорь (для лодок)" });

            Add(new ExtraBind(Types.FlashlightToggle, () =>
            {
                if (Misc.CanShowCEF(true, true))
                {
                    Events.CallRemote("Players::FLT");
                }
            }, true, false)
            { Description = "Фонарик (вкл/выкл)" }); // deprecated

            Add(new ExtraBind(Types.TakeScreenshot, () =>
            {
                CEF.PhoneApps.CameraApp.SavePicture(false, false, true);
            }, true, true)
            { Description = "Сделать скриншот" });

            Add(new ExtraBind(Types.ExtraAction0, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CurrentExtraAction0?.Invoke();
            }, true, true)
            { Description = "Быстрое действие 1" });

            Add(new ExtraBind(Types.ExtraAction1, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CurrentExtraAction1?.Invoke();
            }, true, true)
            { Description = "Быстрое действие 2" });

            Add(new ExtraBind(Types.EnterVehicle, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    Sync.Vehicles.TryEnterVehicle(Interaction.CurrentEntity as Vehicle, -1);
            }, true, false, Types.None, false));

            Add(new ExtraBind(Types.PlaneLandingGear, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    Sync.Vehicles.ToggleLandingGearState(Player.LocalPlayer.Vehicle);
            }, true, false, Types.None, false));

            // Inventory Binds
            Add(new ExtraBind(Types.weapon0, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "weapon", 0);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.weapon1, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "weapon", 1);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.weapon2, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "weapon", 2);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.pockets0, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "pockets", 0);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.pockets1, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "pockets", 1);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.pockets2, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "pockets", 2);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.pockets3, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "pockets", 3);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.pockets4, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "pockets", 4);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.pockets5, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "pockets", 5);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.pockets6, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "pockets", 6);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.pockets7, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "pockets", 7);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.pockets8, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "pockets", 8);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.pockets9, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "pockets", 9);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.pockets10, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "pockets", 10);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.pockets11, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "pockets", 11);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.pockets12, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "pockets", 12);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.pockets13, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "pockets", 13);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.pockets14, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "pockets", 14);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.pockets15, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "pockets", 15);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.pockets16, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "pockets", 16);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.pockets17, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "pockets", 17);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.pockets18, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "pockets", 18);

            }, true, true, Types.None, true));

            Add(new ExtraBind(Types.pockets19, () =>
            {
                if (Misc.CanShowCEF(true, true))
                    CEF.Inventory.BindedAction(5, "pockets", 19);

            }, true, true, Types.None, true));
        }
        #endregion
        #endregion

        #region Defaulters
        public static void DefaultAll()
        {
            foreach (var bind in Binds)
                bind.Value.ChangeKeys(DefaultBinds[bind.Key]);
        }
        #endregion

        public static void DisableAll(params Types[] ignoreTypes)
        {
            foreach (var x in Binds)
                if (!ignoreTypes.Contains(x.Value.Type))
                    x.Value.Disable();
        }

        public static void EnableAll()
        {
            foreach (var x in Binds)
                x.Value.Enable();
        }

        public static int Bind(RAGE.Ui.VirtualKeys vk, bool down, Action action) => Bind((int)vk, down, action);

        public static int Bind(int vk, bool down, Action action)
        {
            var bindHandler = RAGE.Input.Bind(vk, down, () =>
            {
                if (!Misc.IsGameWindowFocused)
                    return;

                action?.Invoke();
            });

            return bindHandler;
        }

        public static void Unbind(int bindHandle)
        {
            RAGE.Input.Unbind(bindHandle);
        }

        public static bool IsDown(RAGE.Ui.VirtualKeys vk) => IsDown((int)vk);

        public static bool IsDown(int vk)
        {
            var res = RAGE.Input.IsDown(vk);

            return res;
        }

        public static bool IsUp(RAGE.Ui.VirtualKeys vk) => IsUp((int)vk);

        public static bool IsUp(int vk)
        {
            var res = RAGE.Input.IsUp(vk);

            return res;
        }

        public static bool IsJustDown(RAGE.Ui.VirtualKeys vk) => IsJustDown((int)vk);

        public static bool IsJustDown(int vk)
        {
            var res = _justDownKeys.Contains(vk);

            return res;
        }

        public static bool IsJustUp(RAGE.Ui.VirtualKeys vk) => IsJustUp((int)vk);

        public static bool IsJustUp(int vk)
        {
            var res = _justUpKeys.Contains(vk);

            return res;
        }
    }
}
