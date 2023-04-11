using RAGE;

namespace BCRPClient.CEF
{
    public class Cursor
    {
        private static bool ShouldBlockEscMenu()
        {
            return IsActive || CEF.Browser.IsAnyCEFActive;
        }

        public static bool IsActive { get; private set; }

        /// <summary>Отображается ли курсор на экране?</summary>
        public static bool IsVisible { get => RAGE.Ui.Cursor.Visible; set => RAGE.Ui.Cursor.Visible = value; }

        private static AsyncTask StopBlockingEscTask { get; set; }

        /// <summary>Отобразить/спрятать курсор</summary>
        /// <param name="freezeInput">Заблокировать игроку возможность двигаться</param>
        /// <param name="value">true - отобразить, false - спрятать</param>
        public static void Show(bool freezeInput, bool value)
        {
            IsActive = value;

            RAGE.Ui.Cursor.ShowCursor(freezeInput, value);

            if (value)
            {
                SwitchEscMenuAccess(false);

                //CEF.Browser.Window.ExecuteCachedJs("focusLastBluredElem();");
            }
            else
            {
                if (!ShouldBlockEscMenu())
                    SwitchEscMenuAccess(true);

                CEF.Browser.Window.ExecuteCachedJs("blurFocusedDomElement();");
            }
        }

        /// <summary>Блокировать клавишу ESC в игре</summary>
        public static void OnTickCursor()
        {
            RAGE.Game.Pad.DisableControlAction(32, 156, true);
            RAGE.Game.Pad.DisableControlAction(32, 199, true);
            RAGE.Game.Pad.DisableControlAction(32, 200, true);
        }

        public static void SwitchEscMenuAccess(bool state)
        {
            if (!state)
            {
                if (StopBlockingEscTask != null)
                {
                    StopBlockingEscTask.Cancel();

                    StopBlockingEscTask = null;
                }

                GameEvents.Render -= OnTickCursor;
                GameEvents.Render += OnTickCursor;
            }
            else
            {
                if (StopBlockingEscTask == null)
                {
                    StopBlockingEscTask = new AsyncTask(() =>
                    {
                        GameEvents.Render -= OnTickCursor;

                        StopBlockingEscTask = null;
                    }, 500, false, 0);

                    StopBlockingEscTask.Run();
                }
            }
        }
    }
}
