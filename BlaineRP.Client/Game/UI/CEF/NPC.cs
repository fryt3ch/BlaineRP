using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Utils;
using RAGE;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class NPC
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

            CEF.Browser.Window.ExecuteJs("NPC.fill", npcName, Utils.Misc.ReplaceNewLineHtml(text), buttons);
        }

        public static void Reply(int buttonId)
        {
            if (NPCs.NPC.CurrentNPC == null)
                return;

            NPCs.NPC.CurrentNPC.CurrentDialogue?.InvokeButtonAction(buttonId);
        }
    }
}
