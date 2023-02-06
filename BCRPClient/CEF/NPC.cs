using RAGE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.CEF
{
    class NPC : Events.Script
    {
        public static bool IsActive { get => CEF.Browser.IsActive(Browser.IntTypes.NPC); }

        public NPC()
        {
            Events.Add("NPC::Dialogue::Reply", (object[] args) =>
            {
                int num = (int)args[0];

                Reply(num);
            });
        }

        public static void Show()
        {
            if (IsActive)
                return;

            CEF.Browser.Switch(Browser.IntTypes.NPC, true);

            CEF.Cursor.Show(true, true);
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            CEF.Browser.Switch(Browser.IntTypes.NPC, false);

            CEF.Cursor.Show(false, false);
        }

        /// <summary>Метод для отрисовки диалога</summary>
        /// <param name="npcName">Имя NPC</param>
        /// <param name="text">Текст</param>
        /// <param name="buttons">Массив кнопок, где элемент - массив из bool, int, string (красная кнопка, id кнопки, строка)</param>
        public static void Draw(string npcName, string text, params object[] buttons)
        {
            if (!IsActive)
                return;

            Utils.ConsoleOutput(RAGE.Util.Json.Serialize(new object[] { npcName, Utils.ReplaceNewLineHtml(text), buttons }));

            CEF.Browser.Window.ExecuteJs("NPC.fill", npcName, Utils.ReplaceNewLineHtml(text), buttons);
        }

        public static void Reply(int buttonId)
        {
            if (Data.NPC.CurrentNPC == null)
                return;

            Data.NPC.CurrentNPC.CurrentDialogue?.InvokeButtonAction(buttonId);
        }
    }
}
