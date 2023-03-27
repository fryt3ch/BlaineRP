using RAGE;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.CEF
{
    public class Note : Events.Script
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.Note);

        private static int EscBind { get; set; } = -1;

        public static ContextTypes CurrentContext { get; set; } = ContextTypes.None;

        public enum ContextTypes
        {
            None = 0,
        }

        public static Action CurrentCloseAction { get; set; }
        public static Action<string> CurrentSubmitAction { get; set; }

        public Note()
        {
            Events.Add("Note::UpdateText", (args) =>
            {
                if (args == null || args.Length == 0)
                    return;

                var text = (string)args[0];

                if (text == null)
                    return;

                CurrentSubmitAction?.Invoke(text);
            });
        }

        public static async void ShowWrite(ContextTypes contextType, string text = "")
        {
            if (IsActive)
                return;

            await CEF.Browser.Render(Browser.IntTypes.Note, true, true);

            CurrentContext = contextType;

            CEF.Browser.Window.ExecuteJs("Note.draw", true, text);

            CEF.Cursor.Show(true, true);

            EscBind = KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () =>
            {
                Close();
            });
        }

        public static async void ShowRead(ContextTypes contextType, string text)
        {
            if (IsActive)
                return;

            await CEF.Browser.Render(Browser.IntTypes.Note, true, true);

            CurrentContext = contextType;

            CEF.Browser.Window.ExecuteJs("Note.draw", false, text);

            EscBind = KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () =>
            {
                Close();
            });
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            CurrentCloseAction?.Invoke();

            CurrentCloseAction = null;
            CurrentSubmitAction = null;

            if (EscBind >= 0)
            {
                KeyBinds.Unbind(EscBind);

                EscBind = -1;
            }

            CurrentContext = ContextTypes.None;

            CEF.Browser.Render(Browser.IntTypes.Note, false, false);

            CEF.Cursor.Show(false, false);
        }

        public static void SetText(string text)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("Note.setText", text);
        }
    }
}
