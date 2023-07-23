using RAGE;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Client.Data
{
    [Script(int.MaxValue)]
    public class Dialogue
    {
        public enum TimeTypes
        {
            Morning = 0,
            Day,
            Evening,
            Night,
        }

        public static TimeTypes GetCurrentTimeType()
        {
            var hours = RAGE.Game.Clock.GetClockHours();

            if (hours >= 6 && hours <= 11)
                return TimeTypes.Morning;
            else if (hours >= 12 && hours <= 17)
                return TimeTypes.Day;
            else if (hours >= 18 && hours <= 23)
                return TimeTypes.Evening;

            return TimeTypes.Night;
        }

        public static Dictionary<string, Dialogue> AllDialogues { get; private set; } = new Dictionary<string, Dialogue>();

        public static void CloseCurrentDialogue() => NPC.CurrentNPC?.SwitchDialogue(false);

        public static Dialogue Get(string id) => AllDialogues.GetValueOrDefault(id);

        public class Button
        {
            public static Button DefaultExitButton { get; } = new Button("[Выйти]", CloseCurrentDialogue);

            public static Button DefaultBackButton { get; } = new Button("[Назад]", () =>
            {
                if (NPC.CurrentNPC?.LastDialogues is List<Dialogue.LastInfo> lastDialogues)
                {
                    if (lastDialogues.Count > 1)
                    {
                        var targetDialogueInfo = lastDialogues[lastDialogues.Count - 2];
                        var lastDialogueInfo = lastDialogues[lastDialogues.Count - 1];

                        lastDialogues.Remove(targetDialogueInfo);

                        if (NPC.CurrentNPC.CurrentDialogue == lastDialogueInfo.Dialogue)
                        {
                            lastDialogues.Remove(lastDialogueInfo);

                            NPC.CurrentNPC.ShowDialogue(targetDialogueInfo.Dialogue.Id, targetDialogueInfo.SaveAsLast, targetDialogueInfo.Args, targetDialogueInfo.TextArgs);
                        }
                    }
                }
            });

            public static Button DefaultShopEnterButton { get; } = new Button("[Перейти к товарам]", () => NPC.CurrentNPC?.SellerNpcRequestEnterBusiness());

            /// <summary>Текст</summary>
            public string Text { get; set; }

            public Action Action { get; set; }

            /// <summary>Конструктор кнопки диалога</summary>
            /// <param name="Text"><inheritdoc cref="Text" path="/summary"/></param>
            /// <param name="IsRed"><inheritdoc cref="IsRed" path="/summary"/></param>
            public Button(string Text, Action Action)
            {
                this.Action = Action;

                this.Text = Text;
            }

            public void Execute()
            {
                Action?.Invoke();
            }
        }

        public string Id { get; set; }

        public string Text { get; set; }

        public List<Button> Buttons { get; set; }

        public Action<object[]> Action { get; set; }

        public Dictionary<TimeTypes, string> TimedTexts { get; set; }

        public Dialogue(string Id, string Text, Action<object[]> Action = null, params Button[] Buttons)
        {
            this.Id = Id;

            this.Action = Action;

            this.Text = Text;

            this.Buttons = Buttons.ToList();

            AllDialogues.Add(Id, this);
        }

        public Dialogue()
        {
            Events.Add("Dialogues::Show", (object[] args) =>
            {
                string dialogueId = (string)args[0];

                if (NPC.CurrentNPC == null)
                    return;

                NPC.CurrentNPC.ShowDialogue(dialogueId);
            });
        }

        public void InvokeButtonAction(int buttonId)
        {
            if (buttonId < 0 || buttonId >= Buttons.Count)
                return;

            Buttons[buttonId].Action?.Invoke();
        }

        /// <summary>Метод для показа диалога</summary>
        /// <param name="npcHolder">NPC-держатель диалога</param>
        /// <param name="args">Аргументы (если выполняется invokeAction, то аргументы для действия, иначе - массив ID кнопок (int), которые нужно показать. Если args пустой, то будут показаны все кнопки</param>
        public void Show(NPC npcHolder, object[] args = null, params object[] textArgs)
        {
            Action?.Invoke(args);

            if (Text == null)
                return;

            var buttons = Buttons;

            if (!CEF.NPC.IsActive)
            {
                CEF.NPC.Show();
            }

            var currentTimeType = GetCurrentTimeType();

            var text = (TimedTexts == null ? Text : TimedTexts.GetValueOrDefault(currentTimeType) ?? Text) ?? "null";

            var tArr = text.Split('`');

            if (tArr.Length > 2 && tArr.Length % 2 != 0)
            {
                for (int i = 1; i < tArr.Length; i += 2)
                {
                    if (tArr[i] == "ddg")
                    {
                        text = text.Replace($"`{tArr[i]}`", Text ?? "null");

                        continue;
                    }

                    var tArr2 = tArr[i].Split('-');

                    if (tArr2.Length < 2)
                        continue;

                    object dTypeObj = null;

                    if (!Enum.TryParse(typeof(TimeTypes), tArr2[0], out dTypeObj))
                        continue;

                    var dType = (TimeTypes)dTypeObj;

                    int dNum = 0;

                    if (!int.TryParse(tArr2[1], out dNum))
                        continue;

                    var textToReplace = Locale.General.NPC.TimeWords.GetValueOrDefault(dType)?.GetValueOrDefault(dNum);

                    if (textToReplace == null)
                        continue;

                    text = text.Replace($"`{tArr[i]}`", textToReplace);
                }
            }

            if (textArgs.Length > 0)
                text = string.Format(Text, textArgs);

            var btnsData = new List<object>();

            for (int i = 0; i < buttons.Count; i++)
                btnsData.Add(new object[] { i, buttons[i].Text });

            CEF.NPC.Draw(npcHolder.GetDisplayName() ?? npcHolder.Id, text, btnsData.ToArray());
        }

        public class LastInfo
        {
            public Dialogue Dialogue { get; set; }

            public object[] Args { get; set; }

            public object[] TextArgs { get; set; }

            public bool SaveAsLast { get; set; }

            public LastInfo(Dialogue Dialogue, bool SaveAsLast, object[] Args, object[] TextArgs)
            {
                this.Dialogue = Dialogue;

                this.SaveAsLast = SaveAsLast;

                this.Args = Args;
                this.TextArgs = TextArgs;
            }
        }
    }
}
