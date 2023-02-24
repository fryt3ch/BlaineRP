using RAGE;

namespace BCRPClient.CEF
{
    class Cursor : Events.Script
    {
        public static bool IsActive { get; private set; }

        /// <summary>Отображается ли курсор на экране?</summary>
        public static bool IsVisible { get => RAGE.Ui.Cursor.Visible; set => RAGE.Ui.Cursor.Visible = value; }

        /// <summary>Запущен ли процесс скрытия курсора?</summary>
        private static bool IsHiding { get; set; }

        public Cursor()
        {
            IsHiding = false;
        }

        /// <summary>Отобразить/спрятать курсор</summary>
        /// <param name="freezeInput">Заблокировать игроку возможность двигатьсяу</param>
        /// <param name="value">true - отобразить, false - спрятать</param>
        public static void Show(bool freezeInput, bool value)
        {
            IsActive = value;

            RAGE.Ui.Cursor.ShowCursor(freezeInput, value);

            if (value)
            {
                SwitchEscMenuAccess(false);
            }
            else
            {
                if (!Utils.IsAnyCefActive(true))
                    SwitchEscMenuAccess(true);
            }
        }

        /// <summary>Блокировать клавишу ESC в игре</summary>
        public static void OnTickCursor()
        {
            RAGE.Game.Pad.DisableControlAction(32, 156, true);
            RAGE.Game.Pad.DisableControlAction(32, 199, true);
            RAGE.Game.Pad.DisableControlAction(32, 200, true);
        }

        private static async void OnCursorHidden()
        {
            IsHiding = true;

            await RAGE.Game.Invoker.WaitAsync(500);

            GameEvents.Render -= OnTickCursor;

            RAGE.Game.Pad.EnableControlAction(32, 200, true);

            IsHiding = false;
        }

        public static void SwitchEscMenuAccess(bool state)
        {
            if (!state)
            {
                if (IsHiding)
                {
                    (new AsyncTask(() =>
                    {
                        if (!IsHiding)
                        {
                            if (IsVisible)
                            {
                                GameEvents.Render -= OnTickCursor;
                                GameEvents.Render += OnTickCursor;
                            }

                            return true;
                        }

                        return false;
                    }, 10, true, 0)).Run();
                }
                else
                {
                    GameEvents.Render -= OnTickCursor;
                    GameEvents.Render += OnTickCursor;
                }
            }
            else
                OnCursorHidden();
        }
    }
}
