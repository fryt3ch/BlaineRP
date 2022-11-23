using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.CEF
{
    class ActionBox : Events.Script
    {
        public static bool IsActive { get => CEF.Browser.IsActive(Browser.IntTypes.ActionBox); }

        public enum ReplyTypes
        {
            OK = 0, Cancel = 1,
        }

        public enum Types
        {
            None = -1, Select, Input, Range
        }

        public enum Contexts
        {
            None = -1, Inventory, GiveCash,
        }

        public static Types CurrentType { get; private set; }
        public static Contexts CurrentContext { get; private set; }

        private static List<int> TempBinds { get; set; }

        public ActionBox()
        {
            TempBinds = new List<int>();

            CurrentType = Types.None;
            CurrentContext = Contexts.None;

            Events.Add("ActionBox::Reply", (object[] args) =>
            {
                if (CurrentType == Types.None || CurrentContext == Contexts.None)
                    return;

                ReplyTypes rType = (ReplyTypes)args[0];

                if (CurrentType == Types.Range)
                {
                    int amount = int.Parse((string)args[1]);

                    switch (CurrentContext)
                    {
                        case Contexts.Inventory:
                            if (rType == ReplyTypes.OK)
                            {
                                CEF.Inventory.Action(amount);
                            }
                            else if (rType == ReplyTypes.Cancel)
                            {
                                CEF.Inventory.Action(-1);
                            }
                            else
                                return;
                            break;

                        case Contexts.GiveCash:
                            if (rType == ReplyTypes.OK)
                            {
                                Close(true);

                                if (BCRPClient.Interaction.CurrentEntity?.Type == RAGE.Elements.Type.Player)
                                    Sync.Offers.Request(BCRPClient.Interaction.CurrentEntity as Player, Sync.Offers.Types.Cash, amount);
                            }
                            else if (rType == ReplyTypes.Cancel)
                            {
                                Close(true);
                            }
                            else
                                return;
                            break;
                    }
                }
                else if (CurrentType == Types.Select)
                {
                    int id = int.Parse((string)args[1]);
                }
                else if (CurrentType == Types.Input)
                {
                    string text = (string)args[1];
                }
                else
                    return;
            });
        }

        public static async System.Threading.Tasks.Task ShowSelect(Contexts context, string name, object[][] args)
        {
            if (IsActive)
                return;

            Bind();

            await CEF.Browser.Render(Browser.IntTypes.ActionBox, true, true);

            CEF.Browser.Window.ExecuteJs("ActionBox.fill", false, Types.Select, name, args);

            CurrentType = Types.Select;
            CurrentContext = context;

            Cursor.Show(true, true);
        }

        public static async System.Threading.Tasks.Task ShowRange(Contexts context, string name, int minValue, int maxValue, int curValue = -1, int step = -1)
        {
            if (IsActive)
                return;

            Bind();

            await CEF.Browser.Render(Browser.IntTypes.ActionBox, true, true);

            CEF.Browser.Window.ExecuteJs("ActionBox.fill", false, Types.Range, name, new object[] { minValue, maxValue, curValue == -1 ? maxValue : curValue, step == -1 ? 1 : step });

            CurrentType = Types.Range;
            CurrentContext = context;

            Cursor.Show(true, true);
        }

        public static async System.Threading.Tasks.Task ShowInput(Contexts context, string name, int maxChars = 100)
        {
            if (IsActive)
                return;

            Bind();

            await CEF.Browser.Render(Browser.IntTypes.ActionBox, true, true);

            CEF.Browser.Window.ExecuteJs("ActionBox.fill", false, Types.Input, name);

            CurrentType = Types.Input;
            CurrentContext = context;

            Cursor.Show(true, true);
        }

        public static void Close(bool cursor = true)
        {
            if (!IsActive)
                return;

            for (int i = 0; i < TempBinds.Count; i++)
                RAGE.Input.Unbind(TempBinds[i]);

            TempBinds.Clear();

            CEF.Browser.Render(Browser.IntTypes.ActionBox, false, false);

            CurrentType = Types.None;
            CurrentContext = Contexts.None;

            if (cursor)
                Cursor.Show(false, false);
        }

        private static void Bind()
        {
            if (CurrentContext != Contexts.Inventory)
            {
                TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(true)));
            }
        }
    }
}
