using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.CEF
{
    class ActionBox : Events.Script
    {
        public static bool IsActive { get => CEF.Browser.IsActive(Browser.IntTypes.ActionBox); }

        public enum ReplyTypes
        {
            /// <summary>Принять</summary>
            /// <remarks>Для Money - наличные</remarks>
            OK = 0,
            /// <summary>Отменить</summary>
            /// <remarks>Для Money - банк</remarks>
            Cancel = 1,
            /// <summary>Доп. кнопка (вариативая)</summary>
            /// <remarks>Для Money - отменить</remarks>
            Additional1 = 2,
        }

        private enum ActionTypes
        {
            Show = 0,
            Choose,
            Close,
        }

        public enum Types : sbyte
        {
            None = -1,
            Select = 0,
            Input = 1,
            Range = 2,
            Money = 3,
            Text = 4,
            InputWithText = 5,
        }

        public enum RangeSubTypes
        {
            Default = 0,
            MoneyRange = 1,
        }

        public static Types CurrentType { get; private set; } = Types.None;

        public static string CurrentContextStr { get; private set; }

        private static Action<object[]> CurrentAction { get; set; }
        private static Action CurrentCloseAction { get; set; }

        private static List<int> TempBinds { get; set; } = new List<int>();

        public static DateTime LastSent;

        public ActionBox()
        {
            Events.Add("ActionBox::Reply", async (object[] args) =>
            {
                if (CurrentType == Types.None || CurrentContextStr == null)
                    return;

                var rType = (ReplyTypes)args[0];

                if (CurrentType == Types.Range)
                {
                    var amount = Utils.ToDecimal(args[1]);

                    if (amount < Player.LocalPlayer.GetData<decimal>("ActionBox::Temp::MinValue"))
                    {
                        CEF.Notification.Show(Notification.Types.Error, Locale.Get("NOTIFICATION_HEADER_ERROR"), string.Format(Locale.Notifications.General.LessThanMinValue, Player.LocalPlayer.GetData<decimal>("ActionBox::Temp::MinValue")));

                        return;
                    }
                    else if (amount > Player.LocalPlayer.GetData<decimal>("ActionBox::Temp::MaxValue"))
                    {
                        CEF.Notification.Show(Notification.Types.Error, Locale.Get("NOTIFICATION_HEADER_ERROR"), string.Format(Locale.Notifications.General.BiggerThanMaxValue, Player.LocalPlayer.GetData<decimal>("ActionBox::Temp::MaxValue")));

                        return;
                    }

                    CurrentAction?.Invoke(new object[] { rType, amount });
                }
                else if (CurrentType == Types.Select)
                {
                    var id = Utils.ToDecimal(args[1]);

                    CurrentAction?.Invoke(new object[] { rType, id });
                }
                else if (CurrentType == Types.Money)
                {
                    CurrentAction?.Invoke(new object[] { rType });
                }
                else if (CurrentType == Types.Input)
                {
                    if (args[1] is string str)
                        CurrentAction?.Invoke(new object[] { rType, str });
                    else
                        CurrentAction?.Invoke(new object[] { rType, string.Empty });
                }
                else if (CurrentType == Types.InputWithText)
                {
                    if (args[1] is string str)
                        CurrentAction?.Invoke(new object[] { rType, str });
                    else
                        CurrentAction?.Invoke(new object[] { rType, string.Empty });
                }
                else if (CurrentType == Types.Text)
                {
                    CurrentAction?.Invoke(new object[] { rType });
                }
            });
        }

        public static async System.Threading.Tasks.Task ShowSelect(string context, string name, (decimal Id, string Text)[] variants, string btnTextOk = null, string btnTextCancel = null, Action showAction = null, Action<ReplyTypes, decimal> chooseAction = null, Action closeAction = null)
        {
            if (!await CEF.Browser.Render(Browser.IntTypes.ActionBox, true, true))
                return;

            CurrentType = Types.Select;
            CurrentContextStr = context;

            showAction?.Invoke();

            CurrentCloseAction = closeAction;

            if (chooseAction != null)
                CurrentAction = (args) => chooseAction.Invoke((ReplyTypes)args[0], (decimal)args[1]);

            CEF.Browser.Window.ExecuteJs("ActionBox.fill", false, CurrentType, name, variants.Select(x => new object[] { x.Id, x.Text }), new object[] { btnTextOk ?? Locale.Actions.SelectOkBtn0, btnTextCancel ?? Locale.Actions.SelectCancelBtn0 });

            Cursor.Show(true, true);
        }

        public static async System.Threading.Tasks.Task ShowRange(string context, string name, decimal minValue, decimal maxValue, decimal curValue = -1, decimal step = -1, RangeSubTypes rsType = RangeSubTypes.Default, Action showAction = null, Action<ReplyTypes, decimal> chooseAction = null, Action closeAction = null)
        {
            if (!await CEF.Browser.Render(Browser.IntTypes.ActionBox, true, true))
                return;

            CurrentType = Types.Range;
            CurrentContextStr = context;

            showAction?.Invoke();

            CurrentCloseAction = closeAction;

            if (chooseAction != null)
                CurrentAction = (args) => chooseAction.Invoke((ReplyTypes)args[0], (decimal)args[1]);

            Player.LocalPlayer.SetData("ActionBox::Temp::MinValue", minValue);
            Player.LocalPlayer.SetData("ActionBox::Temp::MaxValue", maxValue);

            CEF.Browser.Window.ExecuteJs("ActionBox.fill", false, CurrentType, name, new object[] { minValue, maxValue, curValue == -1 ? maxValue : curValue, step == -1 ? 1 : step, (int)rsType });

            Cursor.Show(true, true);
        }

        public static async System.Threading.Tasks.Task ShowInput(string context, string name, int maxChars = 100, string defText = "", string btnTextOk = null, string btnTextCancel = null, Action showAction = null, Action<ReplyTypes, string> chooseAction = null, Action closeAction = null)
        {
            if (!await CEF.Browser.Render(Browser.IntTypes.ActionBox, true, true))
                return;

            CurrentType = Types.Input;
            CurrentContextStr = context;

            showAction?.Invoke();

            CurrentCloseAction = closeAction;

            if (chooseAction != null)
                CurrentAction = (args) => chooseAction.Invoke((ReplyTypes)args[0], (string)args[1]);

            CEF.Browser.Window.ExecuteJs("ActionBox.fill", false, CurrentType, name, new object[] { maxChars, defText ?? string.Empty }, new object[] { btnTextOk ?? Locale.Actions.SelectOkBtn0, btnTextCancel ?? Locale.Actions.SelectCancelBtn0 });

            Cursor.Show(true, true);
        }

        public static async System.Threading.Tasks.Task ShowInputWithText(string context, string name, string text, int maxChars = 100, string defText = "", string btnTextOk = null, string btnTextCancel = null, Action showAction = null, Action<ReplyTypes, string> chooseAction = null, Action closeAction = null)
        {
            if (!await CEF.Browser.Render(Browser.IntTypes.ActionBox, true, true))
                return;

            CurrentType = Types.InputWithText;
            CurrentContextStr = context;

            showAction?.Invoke();

            CurrentCloseAction = closeAction;

            if (chooseAction != null)
                CurrentAction = (args) => chooseAction.Invoke((ReplyTypes)args[0], (string)args[1]);

            CEF.Browser.Window.ExecuteJs("ActionBox.fill", false, CurrentType, name, new object[] { Utils.ReplaceNewLineHtml(text), maxChars, defText ?? string.Empty }, new object[] { btnTextOk ?? Locale.Actions.SelectOkBtn0, btnTextCancel ?? Locale.Actions.SelectCancelBtn0 });

            Cursor.Show(true, true);
        }

        public static async System.Threading.Tasks.Task ShowText(string context, string name, string text, string btnTextOk = null, string btnTextCancel = null, Action showAction = null, Action<ReplyTypes> chooseAction = null, Action closeAction = null)
        {
            if (!await CEF.Browser.Render(Browser.IntTypes.ActionBox, true, true))
                return;

            CurrentType = Types.Text;
            CurrentContextStr = context;

            showAction?.Invoke();

            CurrentCloseAction = closeAction;

            if (chooseAction != null)
                CurrentAction = (args) => chooseAction.Invoke((ReplyTypes)args[0]);

            CEF.Browser.Window.ExecuteJs("ActionBox.fill", false, CurrentType, name, new object[] { Utils.ReplaceNewLineHtml(text) }, new object[] { btnTextOk ?? Locale.Actions.SelectOkBtn0, btnTextCancel ?? Locale.Actions.SelectCancelBtn0 });

            Cursor.Show(true, true);
        }

        public static async System.Threading.Tasks.Task ShowMoney(string context, string name, string text, Action showAction = null, Action<ReplyTypes> chooseAction = null, Action closeAction = null)
        {
            if (!await CEF.Browser.Render(Browser.IntTypes.ActionBox, true, true))
                return;

            CurrentType = Types.Money;
            CurrentContextStr = context;

            showAction?.Invoke();

            CurrentCloseAction = closeAction;

            if (chooseAction != null)
                CurrentAction = (args) => chooseAction.Invoke((ReplyTypes)args[0]);

            CEF.Browser.Window.ExecuteJs("ActionBox.fill", false, CurrentType, name, new object[] { Utils.ReplaceNewLineHtml(text) });

            Cursor.Show(true, true);
        }

        public static async void Close(bool cursor = true)
        {
            if (!await CEF.Browser.Render(Browser.IntTypes.ActionBox, false, false))
                return;

            for (int i = 0; i < TempBinds.Count; i++)
                KeyBinds.Unbind(TempBinds[i]);

            TempBinds.Clear();

            CEF.Browser.Render(Browser.IntTypes.ActionBox, false, false);

            if (cursor)
                Cursor.Show(false, false);

            CurrentCloseAction?.Invoke();

            CurrentType = Types.None;

            CurrentContextStr = null;
            CurrentAction = null;
            CurrentCloseAction = null;

            Player.LocalPlayer.ResetData("ActionBox::Temp::MinValue");
            Player.LocalPlayer.ResetData("ActionBox::Temp::MaxValue");
        }

        private static void Bind()
        {
            TempBinds.Add(KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(true)));
        }

        public static Action DefaultBindAction { get; } = () => Bind();
    }
}
