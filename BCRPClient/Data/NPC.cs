using BCRPClient.CEF;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace BCRPClient.Data
{
    public class NPC : Events.Script
    {
        public static NPC CurrentNPC { get; set; }

        private static Dictionary<Ped, NPC> AllNPCs = new Dictionary<Ped, NPC>();

        public static NPC GetData(string id) => AllNPCs.Where(x => x.Value.Id == id).Select(x => x.Value).FirstOrDefault();

        public static NPC GetData(Ped ped) => ped == null ? null : (AllNPCs.ContainsKey(ped) ? AllNPCs[ped] : null);

        public static DateTime LastSent;

        private static List<int> TempBinds { get; set; }

        public enum Types
        {
            Static = -1,

            Quest = 0,
            Talkable,
        }

        public enum PedAnimationTypes
        {
            /// <summary>Ничего</summary>
            None = 0,

            /// <summary>Анимация прерывается при диалоге с игроком, потом - продолжается</summary>
            /// <remarks>Если анимация была запущена во время диалога с игроком, то она будет проигрываться!</remarks>
            InterruptOnDialogue,
            /// <summary>Анимация прерывается при диалоге с игроком, потом - продолжается</summary>
            ///<remarks>Если анимация была запущена во время диалога с игроком, то она НЕ будет проигрываться!</remarks>
            DontPlayOnDialogue,
        }

        public string SubName { get; set; }

        public string Id { get; set; }

        public Types Type { get; set; }

        public Ped Ped { get; private set; }

        private bool _Invincible { get; set; }

        public bool Invincible { get => _Invincible; set { _Invincible = value; Ped.SetInvincible(value); } }

        public PedAnimationTypes AnimationType { get => Ped.GetData<PedAnimationTypes>("AnimType"); set => Ped.SetData("AnimType", value); }

        public Sync.Animations.Animation DefaultAnimation { get => Ped.GetData<Sync.Animations.Animation>("DefaultAnim"); set { if (value != null) { Ped.SetData("DefaultAnim", value); } else { Ped.ResetData("DefaultAnim"); } } }

        public Sync.Animations.Animation CurrentAnimation { get => Ped.GetData<Sync.Animations.Animation>("CurrentAnim") ?? DefaultAnimation; set { if (value != null) { Ped.SetData("CurrentAnim", value); if (IsStreamed) Sync.Animations.Play(Ped, value, -1); } else { Ped.ResetData("CurrentAnim"); if (IsStreamed) Sync.Animations.Stop(Ped); } } }

        public float DefaultHeading { get; set; }

        public string Name { get; set; }

        public string DefaultDialogueId { get; set; }

        public Dialogue CurrentDialogue { get => Ped.GetData<Dialogue>("CurrentDialogue"); set { if (value != null) { Ped.SetData("CurrentDialogue", value); } else { Ped.ResetData("CurrentDialogue"); } } }

        public List<Dialogue.LastInfo> LastDialogues { get => Ped.GetData<List<Dialogue.LastInfo>>("LastDialogues"); set { if (value != null) { Ped.SetData("LastDialogues", value); } else { Ped.ResetData("LastDialogues"); } } }

        public Additional.Cylinder Colshape { get; set; }

        public bool IsStreamed => RAGE.Elements.Entities.Peds.Streamed.Contains(Ped);

        public Blip Blip { get => Ped.GetData<Blip>("Blip"); set { if (value == null) Ped.ResetData("Blip"); else Ped.SetData("Blip", value); } }

        public object Data { get; set; }

        public object TempDialogueData { get => Player.LocalPlayer.HasData($"NPC::{Id}::TDD") ? Player.LocalPlayer.GetData<object>($"NPC::{Id}::TDD") : null; set { if (value == null) Player.LocalPlayer.ResetData($"NPC::{Id}::TDD"); else Player.LocalPlayer.SetData($"NPC::{Id}::TDD", value); } }

        public NPC(string Id, string Name, Types Type, uint Model, Vector3 Position, float Heading = 0f, uint Dimension = 0)
        {
            this.Id = Id;

            Ped = new Ped(Model, Position, Heading, Dimension);

            this.Type = Type;

            this.DefaultHeading = Heading;
            this.Name = Name;

            this._Invincible = true;

            if (Type == Types.Talkable)
            {
                this.Colshape = new Additional.Cylinder(new Vector3(Position.X, Position.Y, Position.Z - 1f), 2f, 2f, false, new Utils.Colour(255, 0, 0, 255), Settings.MAIN_DIMENSION, null)
                {
                    ActionType = Additional.ExtraColshape.ActionTypes.NpcDialogue,
                    InteractionType = Additional.ExtraColshape.InteractionTypes.NpcDialogue,

                    Data = this,
                };
            }

            this.SubName = Locale.General.NPC.TypeNames.GetValueOrDefault(Id.Split('_')[0]);

            AllNPCs.Add(Ped, this);
        }

        public NPC(string Id, string Name, Types Type, string Model, Vector3 Position, float Heading, uint Dimension = 0) : this(Id, Name, Type, RAGE.Util.Joaat.Hash(Model), Position, Heading, Dimension) { }

        public bool SellerNpcRequestEnterBusiness()
        {
            if (NPC.CurrentNPC.Data is Data.Locations.Business businessData)
            {
                Events.CallRemote("Business::Enter", businessData.Id);

                return true;
            }

            return false;
        }

        public static async System.Threading.Tasks.Task OnPedStreamIn(Ped ped)
        {
            var data = GetData(ped);

            if (data == null)
                return;

            ped.FreezePosition(true);

            data.Ped.SetHeading(data.DefaultHeading);

            if (data.CurrentAnimation is Sync.Animations.Animation curAnim)
            {
                Sync.Animations.Play(ped, curAnim, -1);
            }
        }

        public static async System.Threading.Tasks.Task OnPedStreamOut(Ped ped)
        {
            var data = GetData(ped);

            if (data == null)
            {
                ped.ClearTasksImmediately();
            }
            else
            {

            }
        }

        public NPC()
        {
            LastSent = DateTime.MinValue;

            TempBinds = new List<int>();

            GameEvents.Render += () =>
            {
                float screenX = 0f, screenY = 0f;

                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                foreach (var x in Utils.GetPedsOnScreen(5))
                {
                    var data = GetData(x);

                    if (data == null)
                        continue;

                    var pos = x.GetRealPosition();

                    if (Vector3.Distance(pos, Player.LocalPlayer.Position) > 10f)
                        continue;

                    if (Settings.Other.DebugLabels && pData.AdminLevel > -1)
                    {
                        if (Utils.GetScreenCoordFromWorldCoord(pos, ref screenX, ref screenY))
                        {
                            Utils.DrawText($"ID: {data.Id} | Type: {data.Type}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                            Utils.DrawText($"Data: {data.Data}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                        }
                    }

                    pos.Z += 1.1f;

                    if (!Utils.GetScreenCoordFromWorldCoord(pos, ref screenX, ref screenY))
                        continue;

                    Utils.DrawText(data.Name, screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);

                    if (data.SubName != null)
                        Utils.DrawText(data.SubName, screenX, screenY += NameTags.Interval / 2f, 255, 215, 0, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                }
            };
        }

        public void CallRemote(string actionName, params object[] args) => Events.CallRemote("NPC::Action", Id, actionName, string.Join('&', args));

        public async System.Threading.Tasks.Task<object> CallRemoteProc(string actionName, params object[] args) => await Events.CallRemoteProc("NPC::Proc", Id, actionName, string.Join('&', args));

        public void Interact(bool state = true)
        {

        }

        public void SwitchDialogue(bool state = true)
        {
            if (state)
            {
                if (CurrentNPC != null)
                    return;

                LastDialogues = new List<Dialogue.LastInfo>();

                var playerHeadCoord = Utils.GetWorldCoordFromScreenCoord(0.5f, 0.5f, 100f);

                if (playerHeadCoord != null)
                    Ped.TaskLookAtCoord(playerHeadCoord.X, playerHeadCoord.Y, playerHeadCoord.Z, -1f, 2048, 3);

                CEF.Notification.ClearAll();

                CurrentNPC = this;

                var pedPos = Ped.GetRealPosition();
                var playerPos = Player.LocalPlayer.GetRealPosition();

                var t = Utils.RadiansToDegrees((float)Math.Atan2(pedPos.Y - playerPos.Y, pedPos.X - playerPos.X)) - 90f;

                Player.LocalPlayer.SetHeading(t);
                Ped.SetHeading(t + 180f);

                Player.LocalPlayer.SetVisible(false, false);

                Additional.Camera.Enable(Additional.Camera.StateTypes.NpcTalk, Ped, Ped, -1);

                TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => SwitchDialogue(false)));
            }
            else
            {
                if (CurrentNPC == null)
                    return;

                if (LastDialogues is List<Dialogue.LastInfo> lastDialogues)
                {
                    lastDialogues.Clear();

                    LastDialogues = null;
                }

                Ped.TaskClearLookAt();

                foreach (var x in TempBinds)
                    RAGE.Input.Unbind(x);

                TempBinds.Clear();

                Additional.Camera.Disable(750);

                Player.LocalPlayer.SetVisible(true, false);

                Ped.SetHeading(DefaultHeading);

                TempDialogueData = null;

                CurrentNPC = null;

                CurrentDialogue = null;

                CEF.NPC.Close();
            }
        }

        public void ShowDialogue(string dialogueId, bool saveAsLast = true, object[] args = null, params object[] textArgs)
        {
            if (dialogueId == null)
                return;

            var dialogue = Dialogue.Get(dialogueId);

            if (dialogue == null)
                return;

            if (saveAsLast)
            {
                var dgInfo = new Dialogue.LastInfo(dialogue, saveAsLast, args, textArgs);

                if (LastDialogues is List<Dialogue.LastInfo> lastDialogues)
                    lastDialogues.Add(dgInfo);
                else
                    LastDialogues = new List<Dialogue.LastInfo>() { dgInfo };
            }

            CurrentDialogue = dialogue;

            dialogue.Show(this, args, textArgs);
        }
    }

    public partial class Dialogue : Events.Script
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
            public static Button DefaultExitButton { get; private set; } = new Button("[Выйти]", CloseCurrentDialogue);

            public static Button DefaultBackButton { get; private set; } = new Button("[Назад]", () =>
            {
                if (NPC.CurrentNPC?.LastDialogues is List<Dialogue.LastInfo> lastDialogues)
                {
                    if (lastDialogues.Count > 1)
                    {
                        var targetDialogueInfo = lastDialogues[lastDialogues.Count - 2];

                        lastDialogues.Remove(targetDialogueInfo);

                        NPC.CurrentNPC?.ShowDialogue(targetDialogueInfo.Dialogue.Id, targetDialogueInfo.SaveAsLast, targetDialogueInfo.Args, targetDialogueInfo.TextArgs);
                    }
                }
            });

            public static Button DefaultShopEnterButton { get; private set; } = new Button("[Перейти к товарам]", () => NPC.CurrentNPC?.SellerNpcRequestEnterBusiness());

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
            NPCDialogues.Bank.Load();
            NPCDialogues.Job.Load();
            NPCDialogues.Shop.Load();
            NPCDialogues.VehiclePound.Load();
            NPCDialogues.Misc.Load();

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

            CEF.NPC.Draw(npcHolder.Name, text, btnsData.ToArray());
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
