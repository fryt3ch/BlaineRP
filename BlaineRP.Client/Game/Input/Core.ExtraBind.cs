using System;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Game.Input.Enums;
using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Game.UI.CEF;

namespace BlaineRP.Client.Game.Input
{
    partial class Core
    {
        public class ExtraBind
        {
            /// <summary>Тип бинда</summary>
            public BindTypes Type { get; private set; }

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
            public BindTypes Parent { get; private set; }

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
                            if (!Core.IsDown(Keys[i]))
                                return false;

                        return true;
                    }
                    else
                    {
                        for (int i = 0; i < Keys.Length; i++)
                            if (!Core.IsUp(Keys[i]))
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
                            if (!Core.IsJustDown(Keys[i]))
                                return false;

                        return true;
                    }
                    else
                    {
                        for (int i = 0; i < Keys.Length; i++)
                            if (!Core.IsJustUp(Keys[i]))
                                return false;

                        return true;
                    }
                }
            }

            public ExtraBind(BindTypes type, Action action, bool isDown, bool changeable, BindTypes familiar = BindTypes.None, bool invOnly = false)
            {
                this.BindIndex = -1;

                this.Type = type;
                this.Action = action;
                this.IsDown = isDown;

                this.Changeable = changeable;
                this.Parent = familiar;

                this.InvOnly = invOnly;

                if (changeable)
                {
                    this.Keys = RageStorage.GetData<RAGE.Ui.VirtualKeys[]>($"KeyBinds::{type}");

                    if (this.Keys == null)
                    {
                        this.Keys = _defaultBinds[type];

                        RageStorage.SetData($"KeyBinds::{type}", this.Keys);
                    }
                }
                else if (familiar == BindTypes.None)
                    this.Keys = _defaultBinds[type];
                else
                    this.Keys = Binds.TryGetValue(familiar, out var bind) ? bind.Keys : _defaultBinds[type];

                Description = type.ToString();
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

                switch (Keys.Length)
                {
                    case 0:
                        return;
                    case 1:
                        BindIndex = Core.Bind(Keys[0], IsDown, async () =>
                        {
                            Action.Invoke();
                        });
                        break;
                    default:
                        BindIndex = Core.Bind(Keys[Keys.Length - 1], IsDown, async () =>
                        {
                            Func<RAGE.Ui.VirtualKeys, bool> checkFunc = Core.IsDown;

                            if (!IsDown)
                                checkFunc = Core.IsUp;

                            for (int i = 0; i < Keys.Length - 1; i++)
                                if (!checkFunc.Invoke(Keys[i]))
                                    return;

                            Action.Invoke();
                        });
                        break;
                }
            }

            public void Disable()
            {
                DisabledCounter++;

                if (BindIndex == -1)
                    return;

                Core.Unbind(BindIndex);

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
                    RageStorage.SetData($"KeyBinds::{Type}", keys);

                    if (_helpBinds.ContainsKey(Type))
                    {
                        Browser.Window.ExecuteJs("Hud.changeHelpKey", _helpBinds[Type], GetKeyString());
                    }

                    if (HudMenuBinds.ContainsKey(Type))
                    {
                        HUD.Menu.UpdateCurrentTypes(Keys.Length == 0, HudMenuBinds[Type]);
                    }
                }

                foreach (var bind in Binds.Where(x => x.Value.Parent == Type))
                    bind.Value.ChangeKeys(keys);

                int lastState = BindIndex;

                Disable();

                if (lastState != -1 || lastKeysNone)
                    Enable();
            }

            public string GetKeyString() => Core.GetKeyString(Keys);
        }
    }
}
