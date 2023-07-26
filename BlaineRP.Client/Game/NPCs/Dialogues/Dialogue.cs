using System;
using System.Collections.Generic;
using System.Linq;
using RAGE;

namespace BlaineRP.Client.Game.NPCs.Dialogues
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
            Events.Add("Dialogues::Show",
                (args) =>
                {
                    var dialogueId = (string)args[0];

                    if (NPC.CurrentNPC == null)
                        return;

                    NPC.CurrentNPC.ShowDialogue(dialogueId);
                }
            );
        }

        public static Dictionary<string, Dialogue> AllDialogues { get; private set; } = new Dictionary<string, Dialogue>();

        public string Id { get; set; }

        public string Text { get; set; }

        public List<Button> Buttons { get; set; }

        public Action<object[]> Action { get; set; }

        public Dictionary<TimeTypes, string> TimedTexts { get; set; }

        public static TimeTypes GetCurrentTimeType()
        {
            int hours = RAGE.Game.Clock.GetClockHours();

            if (hours >= 6 && hours <= 11)
                return TimeTypes.Morning;
            else if (hours >= 12 && hours <= 17)
                return TimeTypes.Day;
            else if (hours >= 18 && hours <= 23)
                return TimeTypes.Evening;

            return TimeTypes.Night;
        }

        public static void CloseCurrentDialogue()
        {
            NPC.CurrentNPC?.SwitchDialogue(false);
        }

        public static Dialogue Get(string id)
        {
            return AllDialogues.GetValueOrDefault(id);
        }

        public void InvokeButtonAction(int buttonId)
        {
            if (buttonId < 0 || buttonId >= Buttons.Count)
                return;

            Buttons[buttonId].Action?.Invoke();
        }

        /// <summary>Метод для показа диалога</summary>
        /// <param name="npcHolder">NPC-держатель диалога</param>
        /// <param name="args">
        ///     Аргументы (если выполняется invokeAction, то аргументы для действия, иначе - массив ID кнопок (int),
        ///     которые нужно показать. Если args пустой, то будут показаны все кнопки
        /// </param>
        public void Show(NPC npcHolder, object[] args = null, params object[] textArgs)
        {
            Action?.Invoke(args);

            if (Text == null)
                return;

            List<Button> buttons = Buttons;

            if (!UI.CEF.NPC.IsActive)
                UI.CEF.NPC.Show();

            TimeTypes currentTimeType = GetCurrentTimeType();

            string text = (TimedTexts == null ? Text : TimedTexts.GetValueOrDefault(currentTimeType) ?? Text) ?? "null";

            string[] tArr = text.Split('`');

            if (tArr.Length > 2 && tArr.Length % 2 != 0)
                for (var i = 1; i < tArr.Length; i += 2)
                {
                    if (tArr[i] == "ddg")
                    {
                        text = text.Replace($"`{tArr[i]}`", Text ?? "null");

                        continue;
                    }

                    string[] tArr2 = tArr[i].Split('-');

                    if (tArr2.Length < 2)
                        continue;

                    object dTypeObj = null;

                    if (!Enum.TryParse(typeof(TimeTypes), tArr2[0], out dTypeObj))
                        continue;

                    var dType = (TimeTypes)dTypeObj;

                    var dNum = 0;

                    if (!int.TryParse(tArr2[1], out dNum))
                        continue;

                    string textToReplace = Locale.General.NPC.TimeWords.GetValueOrDefault(dType)?.GetValueOrDefault(dNum);

                    if (textToReplace == null)
                        continue;

                    text = text.Replace($"`{tArr[i]}`", textToReplace);
                }

            if (textArgs.Length > 0)
                text = string.Format(Text, textArgs);

            var btnsData = new List<object>();

            for (var i = 0; i < buttons.Count; i++)
            {
                btnsData.Add(new object[]
                    {
                        i,
                        buttons[i].Text,
                    }
                );
            }

            UI.CEF.NPC.Draw(npcHolder.GetDisplayName() ?? npcHolder.Id, text, btnsData.ToArray());
        }

        public class Button
        {
            /// <summary>Конструктор кнопки диалога</summary>
            /// <param name="Text">
            ///     <inheritdoc cref="Text" path="/summary" />
            /// </param>
            /// <param name="IsRed">
            ///     <inheritdoc cref="IsRed" path="/summary" />
            /// </param>
            public Button(string Text, Action Action)
            {
                this.Action = Action;

                this.Text = Text;
            }

            public static Button DefaultExitButton { get; } = new Button("[Выйти]", CloseCurrentDialogue);

            public static Button DefaultBackButton { get; } = new Button("[Назад]",
                () =>
                {
                    if (NPC.CurrentNPC?.LastDialogues is List<LastInfo> lastDialogues)
                        if (lastDialogues.Count > 1)
                        {
                            LastInfo targetDialogueInfo = lastDialogues[lastDialogues.Count - 2];
                            LastInfo lastDialogueInfo = lastDialogues[lastDialogues.Count - 1];

                            lastDialogues.Remove(targetDialogueInfo);

                            if (NPC.CurrentNPC.CurrentDialogue == lastDialogueInfo.Dialogue)
                            {
                                lastDialogues.Remove(lastDialogueInfo);

                                NPC.CurrentNPC.ShowDialogue(targetDialogueInfo.Dialogue.Id, targetDialogueInfo.SaveAsLast, targetDialogueInfo.Args, targetDialogueInfo.TextArgs);
                            }
                        }
                }
            );

            public static Button DefaultShopEnterButton { get; } = new Button("[Перейти к товарам]", () => NPC.CurrentNPC?.SellerNpcRequestEnterBusiness());

            /// <summary>Текст</summary>
            public string Text { get; set; }

            public Action Action { get; set; }

            public void Execute()
            {
                Action?.Invoke();
            }
        }

        public class LastInfo
        {
            public LastInfo(Dialogue Dialogue, bool SaveAsLast, object[] Args, object[] TextArgs)
            {
                this.Dialogue = Dialogue;

                this.SaveAsLast = SaveAsLast;

                this.Args = Args;
                this.TextArgs = TextArgs;
            }

            public Dialogue Dialogue { get; set; }

            public object[] Args { get; set; }

            public object[] TextArgs { get; set; }

            public bool SaveAsLast { get; set; }
        }
    }
}