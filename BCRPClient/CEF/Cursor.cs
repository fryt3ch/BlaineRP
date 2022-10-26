using RAGE;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.CEF
{
    class Cursor : Events.Script
    {
        /// <summary>Отображается ли курсор на экране?</summary>
        public static bool Visible { get => RAGE.Ui.Cursor.Visible; }
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
            RAGE.Ui.Cursor.ShowCursor(freezeInput, value);

            if (value)
            {
                if (IsHiding)
                {
                    (new AsyncTask(() =>
                    {
                        if (!IsHiding)
                        {
                            if (Visible)
                            {
                                GameEvents.Update -= OnTickCursor;
                                GameEvents.Update += OnTickCursor;
                            }

                            return true;
                        }

                        return false;
                    }, 10, true, 0)).Run();
                }
                else
                {
                    GameEvents.Update -= OnTickCursor;
                    GameEvents.Update += OnTickCursor;
                }
            }
            else
                OnCursorHidden();
        }

        /// <summary>Блокировать клавишу ESC в игре</summary>
        private static void OnTickCursor() => RAGE.Game.Pad.DisableControlAction(32, 200, true);

        private static async void OnCursorHidden()
        {
            IsHiding = true;

            await RAGE.Game.Invoker.WaitAsync(500);

            GameEvents.Update -= OnTickCursor;

            RAGE.Game.Pad.EnableControlAction(32, 200, true);

            IsHiding = false;
        }
    }
}
