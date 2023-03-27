﻿using RAGE;

namespace BCRPClient.CEF
{
    class Cursor
    {
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

        public static void SwitchEscMenuAccess(bool state)
        {
            StopBlockingEscTask?.Cancel();

            if (!state)
            {
                StopBlockingEscTask = null;

                GameEvents.Render -= OnTickCursor;
                GameEvents.Render += OnTickCursor;
            }
            else
            {
                StopBlockingEscTask = new AsyncTask(() =>
                {
                    GameEvents.Render -= OnTickCursor;
                }, 500, false, 0);

                StopBlockingEscTask.Run();
            }
        }
    }
}
