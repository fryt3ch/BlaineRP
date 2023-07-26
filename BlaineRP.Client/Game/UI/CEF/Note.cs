using System;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Game.Input;
using RAGE;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class Note
    {
        public Note()
        {
            Events.Add("Note::UpdateText",
                (args) =>
                {
                    if (args == null || args.Length == 0)
                        return;

                    var text = (string)args[0];

                    if (text == null)
                        return;

                    CurrentSubmitAction?.Invoke(text);
                }
            );
        }

        public static bool IsActive => Browser.IsActive(Browser.IntTypes.Note);

        private static int EscBind { get; set; } = -1;

        public static string CurrentContext { get; private set; }

        private static Action CurrentCloseAction { get; set; }
        private static Action<string> CurrentSubmitAction { get; set; }

        public static Action DefaultBindAction { get; } = () => Bind();

        public static async void ShowWrite(string context, string text = "", Action showAction = null, Action<string> submitAction = null, Action closeAction = null)
        {
            if (IsActive)
                return;

            await Browser.Render(Browser.IntTypes.Note, true, true);

            CurrentContext = context;

            CurrentCloseAction = closeAction;
            CurrentSubmitAction = submitAction;

            Browser.Window.ExecuteJs("Note.draw", true, text);

            Cursor.Show(true, true);

            showAction?.Invoke();
        }

        public static async void ShowRead(string context, string text, Action showAction = null, Action closeAction = null)
        {
            if (IsActive)
                return;

            await Browser.Render(Browser.IntTypes.Note, true, true);

            CurrentContext = context;

            CurrentCloseAction = closeAction;

            Browser.Window.ExecuteJs("Note.draw", false, text);

            showAction?.Invoke();
        }

        public static void Close(bool cursor = true)
        {
            if (!IsActive)
                return;

            CurrentCloseAction?.Invoke();

            CurrentCloseAction = null;
            CurrentSubmitAction = null;

            if (EscBind >= 0)
            {
                Core.Unbind(EscBind);

                EscBind = -1;
            }

            CurrentContext = null;

            Browser.Render(Browser.IntTypes.Note, false, false);

            if (cursor)
                Cursor.Show(false, false);
        }

        public static void SetText(string text)
        {
            if (!IsActive)
                return;

            Browser.Window.ExecuteJs("Note.setText", text);
        }

        private static void Bind()
        {
            EscBind = Core.Bind(RAGE.Ui.VirtualKeys.Escape,
                true,
                () =>
                {
                    Close();
                }
            );
        }
    }
}