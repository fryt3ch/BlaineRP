using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Game.Input.Enums;
using BlaineRP.Client.Game.UI.CEF;

namespace BlaineRP.Client.Game.Input
{
    internal partial class Core
    {
        public static bool CursorNotFreezeInput { get; set; }

        private static readonly Dictionary<BindTypes, int> _helpBinds = new Dictionary<BindTypes, int>()
        {
            { BindTypes.ChatInput, 0 },
            { BindTypes.MicrophoneOn, 1 },
            { BindTypes.Menu, 2 },
            { BindTypes.Inventory, 3 },
            { BindTypes.Phone, 4 },
            { BindTypes.Engine, 5 },
            { BindTypes.Help, 6 },
        };

        public static Dictionary<BindTypes, HUD.Menu.Types> HudMenuBinds { get; private set; } = new Dictionary<BindTypes, HUD.Menu.Types>()
        {
            { BindTypes.Animations, HUD.Menu.Types.Animations },
            { BindTypes.Inventory, HUD.Menu.Types.Inventory },
            { BindTypes.Phone, HUD.Menu.Types.Phone },
            { BindTypes.BlipsMenu, HUD.Menu.Types.BlipsMenu },
        };

        public static Action CurrentExtraAction0 { get; set; }
        public static Action CurrentExtraAction1 { get; set; }

        private static readonly HashSet<int> _justDownKeys = new HashSet<int>();
        private static readonly HashSet<int> _justUpKeys = new HashSet<int>();

        public static Dictionary<BindTypes, ExtraBind> Binds { get; set; } = new Dictionary<BindTypes, ExtraBind>();

        public static void Add(ExtraBind bind, bool enable = true)
        {
            if (_helpBinds.TryGetValue(bind.Type, out var helpBind))
                Browser.Window.ExecuteJs("Hud.changeHelpKey", helpBind, bind.GetKeyString());

            if (HudMenuBinds.TryGetValue(bind.Type, out var menuBind))
                HUD.Menu.UpdateCurrentTypes(bind.Keys.Length == 0, menuBind);

            if (Binds.Remove(bind.Type, out var oldBind))
                oldBind.Disable();

            if (enable)
                bind.Enable();

            Binds.Add(bind.Type, bind);
        }

        public static void Remove(BindTypes type)
        {
            var bind = Binds.GetValueOrDefault(type);

            if (bind == null)
                return;

            bind.Disable();
            Binds.Remove(type);
        }

        public static ExtraBind Get(BindTypes type)
        {
            return Binds.GetValueOrDefault(type);
        }

        public static void LoadMain()
        {
            foreach (var x in Enum.GetValues(typeof(RAGE.Ui.VirtualKeys)).Cast<RAGE.Ui.VirtualKeys>())
            {
                RAGE.Input.Bind(x,
                    true,
                    async () =>
                    {
                        _justDownKeys.Add((int)x);

                        await RAGE.Game.Invoker.WaitAsync(0);

                        _justDownKeys.Remove((int)x);
                    });

                RAGE.Input.Bind(x,
                    false,
                    async () =>
                    {
                        _justUpKeys.Add((int)x);

                        await RAGE.Game.Invoker.WaitAsync(0);

                        _justUpKeys.Remove((int)x);
                    });
            }

            // ~ - Toggle Cursor
            Add(new ExtraBind(BindTypes.Cursor,
                    () =>
                    {
                        if (!RAGE.Game.Ui.IsPauseMenuActive())
                        {
                            if (Cursor.IsVisible)
                                Cursor.Show(false, false);
                            else
                                Cursor.Show(!CursorNotFreezeInput, true);
                        }
                    },
                    true,
                    true) { Description = "Скрыть / показать курсор", },
                true);
        }

        public static void DefaultAll()
        {
            foreach (var bind in Binds)
            {
                bind.Value.ChangeKeys(_defaultBinds[bind.Key]);
            }
        }

        public static void DisableAll(params BindTypes[] ignoreTypes)
        {
            foreach (var x in Binds)
            {
                if (!ignoreTypes.Contains(x.Value.Type))
                    x.Value.Disable();
            }
        }

        public static void EnableAll()
        {
            foreach (var x in Binds)
            {
                x.Value.Enable();
            }
        }

        public static int Bind(RAGE.Ui.VirtualKeys vk, bool down, Action action)
        {
            return Bind((int)vk, down, action);
        }

        public static int Bind(int vk, bool down, Action action)
        {
            var bindHandler = RAGE.Input.Bind(vk,
                down,
                () =>
                {
                    if (!Utils.Misc.IsGameWindowFocused)
                        return;

                    action?.Invoke();
                });

            return bindHandler;
        }

        public static void Unbind(int bindHandle)
        {
            RAGE.Input.Unbind(bindHandle);
        }

        public static bool IsDown(RAGE.Ui.VirtualKeys vk)
        {
            return IsDown((int)vk);
        }

        public static bool IsDown(int vk)
        {
            var res = RAGE.Input.IsDown(vk);

            return res;
        }

        public static bool IsUp(RAGE.Ui.VirtualKeys vk)
        {
            return IsUp((int)vk);
        }

        public static bool IsUp(int vk)
        {
            var res = RAGE.Input.IsUp(vk);

            return res;
        }

        public static bool IsJustDown(RAGE.Ui.VirtualKeys vk)
        {
            return IsJustDown((int)vk);
        }

        public static bool IsJustDown(int vk)
        {
            var res = _justDownKeys.Contains(vk);

            return res;
        }

        public static bool IsJustUp(RAGE.Ui.VirtualKeys vk)
        {
            return IsJustUp((int)vk);
        }

        public static bool IsJustUp(int vk)
        {
            var res = _justUpKeys.Contains(vk);

            return res;
        }

        public static string GetKeyString(params RAGE.Ui.VirtualKeys[] keys)
        {
            return keys.Length == 0 ? "???" : string.Join(" + ", keys.Select(x => _keyNames.GetValueOrDefault((int)x) ?? ""));
        }
    }
}