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

        public string Id { get; set; }

        public Types Type { get; set; }

        public Ped Ped { get; private set; }

        private bool _Invincible { get; set; }
        public bool Invincible { get => _Invincible; set { _Invincible = value; Ped.SetInvincible(value); } }

        public float DefaultHeading { get; set; }

        public string Name { get; set; }

        public bool IsFamiliar { get; set; }

        public string[] Dialogues { get; set; }

        public string DefaultDialogueId { get; set; }

        public Dialogue CurrentDialogue { get; set; }

        public Additional.Cylinder Colshape { get; set; }

        public object Data { get; set; }

        public NPC(string Id, string Name, Types Type, uint Model, Vector3 Position, float Heading = 0f, uint Dimension = 0, params string[] Dialogues)
        {
            this.Id = Id;

            Ped = new Ped(Model, Position, Heading, Dimension);

            this.Type = Type;

            this.DefaultHeading = Heading;
            this.Name = Name;

            this._Invincible = true;

            this.Dialogues = Dialogues;

            if (Dialogues.Length > 0)
                this.DefaultDialogueId = Dialogues[0];

            if (Type == Types.Talkable)
            {
                this.Colshape = new Additional.Cylinder(new Vector3(Position.X, Position.Y, Position.Z - 1f), 2f, 2f, false, new Utils.Colour(255, 0, 0, 255), Settings.MAIN_DIMENSION, null);

                this.Colshape.ActionType = Additional.ExtraColshape.ActionTypes.NpcDialogue;
                this.Colshape.InteractionType = Additional.ExtraColshape.InteractionTypes.NpcDialogue;

                this.Colshape.Data = this;
            }

            AllNPCs.Add(Ped, this);
        }

        public NPC(string Id, string Name, Types Type, string Model, Vector3 Position, float Heading, uint Dimension = 0, params string[] Dialogues) : this(Id, Name, Type, RAGE.Util.Joaat.Hash(Model), Position, Heading, Dimension, Dialogues) { }

        public NPC()
        {
            LastSent = DateTime.MinValue;

            TempBinds = new List<int>();

            Events.OnEntityStreamIn += (Entity entity) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Ped)
                    return;

                var data = GetData(entity as Ped);

                if (data == null)
                    return;

                data.Ped.SetHeading(data.DefaultHeading);
            };

            Events.OnEntityStreamOut += (Entity entity) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Ped)
                    return;
            };

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

                    if (data.IsFamiliar)
                    {
                        Utils.DrawText(data.Name, screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);

                        if (data.Type != Types.Static)
                            Utils.DrawText(Locale.General.NPC.TypeNames[data.Type], screenX, screenY += NameTags.Interval / 2f, 255, 215, 0, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                    }
                    else
                    {
                        Utils.DrawText(x.IsMale() ? Locale.General.NPC.NotFamiliarMale : Locale.General.NPC.NotFamiliarFemale, screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                    }
                }
            };
        }

        public void Interact(bool state = true)
        {
/*            if (state)
            {
                if (Type == Types.Static)
                    return;

                CEF.Notification.ClearAll();

                CurrentNPC = this;

                var pedPos = Ped.GetRealPosition();
                var playerPos = Player.LocalPlayer.GetRealPosition();

                var t = Utils.RadiansToDegrees((float)Math.Atan2(pedPos.Y - playerPos.Y, pedPos.X - playerPos.X)) - 90f;

                Player.LocalPlayer.SetHeading(t);
                Ped.SetHeading(t + 180f);

                Additional.Camera.Enable(Additional.Camera.StateTypes.NpcTalk, Ped, Ped, -1);

                CEF.NPC.Show();

                ShowDialogue(DefaultDialogueId);
            }
            else
            {
                Additional.Camera.Disable(750);

                Ped.SetHeading(DefaultHeading);

                CurrentNPC = null;

                CurrentDialogue = null;

                CEF.NPC.Close();
            }*/
        }

        public void SwitchDialogue(bool state = true)
        {
            if (state)
            {
                if (CurrentNPC != null)
                    return;

                CEF.Notification.ClearAll();

                CurrentNPC = this;

                SetFamiliar(true);

                var pedPos = Ped.GetRealPosition();
                var playerPos = Player.LocalPlayer.GetRealPosition();

                var t = Utils.RadiansToDegrees((float)Math.Atan2(pedPos.Y - playerPos.Y, pedPos.X - playerPos.X)) - 90f;

                Player.LocalPlayer.SetHeading(t);
                Ped.SetHeading(t + 180f);

                Additional.Camera.Enable(Additional.Camera.StateTypes.NpcTalk, Ped, Ped, -1);

                TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => SwitchDialogue(false)));
            }
            else
            {
                if (CurrentNPC == null)
                    return;

                foreach (var x in TempBinds)
                    RAGE.Input.Unbind(x);

                TempBinds.Clear();

                Additional.Camera.Disable(750);

                Ped.SetHeading(DefaultHeading);

                CurrentNPC = null;

                CurrentDialogue = null;

                CEF.NPC.Close();
            }
        }

        public void ShowDialogue(string dialogueId)
        {
            if (dialogueId == null)
                return;

            var dialogue = Dialogue.Get(dialogueId);

            if (dialogue == null)
                return;

            dialogue.Show(this);

            CurrentDialogue = dialogue;
        }

        public void SetFamiliar(bool state)
        {
            if (IsFamiliar == state)
                return;

            var familiars = Settings.Other.FamiliarNPCs;

            if (state)
            {
                familiars.Add(Id);
            }
            else
            {
                familiars.Remove(Id);
            }

            IsFamiliar = state;

            Settings.Other.FamiliarNPCs = familiars;
        }
    }

    public class Dialogue : Events.Script
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

        public static Dictionary<string, Dialogue> AllDialogues = new Dictionary<string, Dialogue>()
        {
            #region Bank
            {
                "bank_preprocess",

                new Dialogue(null,

                    async (args) =>
                    {
                        if (NPC.CurrentNPC == null)
                            return;

                        var hasAccount = (bool)await Events.CallRemoteProc("Bank::HasAccount");

                        if (NPC.CurrentNPC == null)
                            return;

                        NPC.CurrentNPC.ShowDialogue(hasAccount ? "bank_has_account" : "bank_no_account_0");
                    }

                    )
            },

            {
                "bank_no_account_0",

                new Dialogue("Здравствуйте, могу ли я Вам чем-нибудь помочь?",

                    null,

                    new Button("Да, хочу стать клиентом вашего банка", () => { NPC.CurrentNPC?.ShowDialogue("bank_no_account_1"); }, true),

                    new Button("[Выйти]", () => { NPC.CurrentNPC?.SwitchDialogue(false); }, false)

                    )
            },

            {
                "bank_no_account_1",

                new Dialogue("Отлично!\n У нас есть несколько выгодных тарифов, ознакомьтесь с ними и выберите интересующий",

                    null,

                    new Button("[Перейти к тарифам]", () =>
                    {
                        if (NPC.CurrentNPC?.Data is Data.Locations.Bank bankData)
                        {
                            if (NPC.LastSent.IsSpam(1000, false, false))
                                return;

                            Events.CallRemote("Bank::Show", false, bankData.Id);
                        }
                    }, true),

                    new Button("[Выйти]", () => { NPC.CurrentNPC?.SwitchDialogue(false); }, false)

                    )
            },

            {
                "bank_has_account",

                new Dialogue("Здравствуйте, могу ли я Вам чем-нибудь помочь?",

                    null,

                    new Button("[Перейти к управлению счетом]", () =>
                    {
                        if (NPC.CurrentNPC?.Data is Data.Locations.Bank bankData)
                        {
                            if (NPC.LastSent.IsSpam(1000, false, false))
                                return;

                            Events.CallRemote("Bank::Show", false, bankData.Id);
                        }
                    }, true),

                    new Button("[Выйти]", () => { NPC.CurrentNPC?.SwitchDialogue(false); }, false)

                    )
            },
            #endregion

            {
                "seller_clothes_greeting_0",
                
                new Dialogue("Приветствуем в нашем магазине!\nЖелаете ознакомиться с ассортиментом? У нас есть новые поступления, уверена, вам понравится!",
                    
                    null,

                    new Button("[Смотреть товары]", () => { Events.CallRemote("Business::Enter", (((Data.Locations.Business)NPC.CurrentNPC.Data).Id)); }, true),

                    new Button("Есть ли работа для меня?", () => { }, true),

                    new Button("[Выйти]", () => { NPC.CurrentNPC?.SwitchDialogue(false); }, false)

                    )
            },

            {
                "seller_shop_greeting_0",

                new Dialogue("Здравствуйте, хорошо, что вы заглянули к нам сегодня, как раз привезли свежайшие продукты!\n",

                    null,

                    new Button("[Смотреть товары]", () => { }, true),

                    new Button("Есть ли работа для меня?", () => { }, true),

                    new Button("[Выйти]", () => { NPC.CurrentNPC?.SwitchDialogue(false); },false)

                    )
            },

            {
                "seller_gas_greeting_0",

                new Dialogue("Здравствуйте, хотите что-то приобрести?",

                    null,

                    new Button("[Смотреть товары]", () => { }, true),

                    new Button("Я хочу заправить транспорт", () => { }, true),

                    new Button("Нет, спасибо", () => { CEF.Interaction.CloseMenu(); }, false)

                    )
            },

            {
                "seller_gas_info_0",

                new Dialogue("Не, это не ко мне, у меня можно купить разные приблуды для дороги, например, ремонтный набор или канистру с топливом, а заправиться вы можете самостоятельно, поставив транспорт возле бензоколонки.",

                    null,

                    new Button("[Назад]", () => { NPC.CurrentNPC?.ShowDialogue("seller_gas_greeting_0"); }, true),

                    new Button("[Выйти]", () => { NPC.CurrentNPC?.SwitchDialogue(false); },false)

                    )
            },

            {
                "seller_no_job_0",

                new Dialogue("К сожалению, пока что ваша помощь ни в чем не требуется, магазин с работой справляется.",

                    null,

                    new Button("[Назад]", () => { NPC.CurrentNPC?.ShowDialogue("seller_greeting_0"); }, true),

                    new Button("[Выйти]", () => { NPC.CurrentNPC?.SwitchDialogue(false); },false)

                    )
            },
        };

        public static Dialogue Get(string id) => AllDialogues.GetValueOrDefault(id);

        public class Button
        {
            /// <summary>Красная ли кнопка?</summary>
            public bool IsRed { get; set; }

            /// <summary>Текст</summary>
            public string Text { get; set; }

            /// <summary>ID кнопки</summary>
            public int Id { get; set; }

            public Action Action { get; set; }

            /// <summary>Конструктор кнопки диалога</summary>
            /// <param name="Text"><inheritdoc cref="Text" path="/summary"/></param>
            /// <param name="IsRed"><inheritdoc cref="IsRed" path="/summary"/></param>
            public Button(string Text, Action Action, bool IsRed = true)
            {
                this.Action = Action;

                this.Text = Text;

                this.IsRed = IsRed;
            }

            public void Execute()
            {
                Action?.Invoke();
            }
        }

        public string Id { get; set; }

        public string Text { get; set; }

        public Button[] Buttons { get; set; }

        public Action<object[]> Action { get; set; }

        public Dialogue(string Text, Action<object[]> Action = null, params Button[] Buttons)
        {
            this.Action = Action;

            this.Text = Text;

            this.Buttons = Buttons;

            for (int i = 0; i < Buttons.Length; i++)
                this.Buttons[i].Id = i;
        }

        public Dialogue()
        {
            foreach (var x in AllDialogues.Keys)
                AllDialogues[x].Id = x;

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
            if (buttonId >= Buttons.Length)
                return;

            Buttons[buttonId].Action?.Invoke();
        }

        public static Dialogue CreateCustom(string id, string text, Action<object[]> action = null, params Button[] buttons)
        {
            var dialogue = new Dialogue(text, action, buttons);

            dialogue.Id = id;

            return dialogue;
        }

        /// <summary>Метод для показа диалога</summary>
        /// <param name="npcHolder">NPC-держатель диалога</param>
        /// <param name="invokeAction">Выполнить ли действие, заданное диалогу? Если true - выполнится действие, если false (или нет действия) - покажется диалог</param>
        /// <param name="args">Аргументы (если выполняется invokeAction, то аргументы для действия, иначе - массив ID кнопок (int), которые нужно показать. Если args пустой, то будут показаны все кнопки</param>
        public void Show(NPC npcHolder, params object[] args)
        {
            Action?.Invoke(args);

            if (Text == null)
                return;

            var buttons = Buttons;

            if (args.Length > 0)
            {
                var newButtons = new List<Button>();

                for (int i = 0; i < args.Length; i++)
                    if (args[i] is int)
                        newButtons.Add(Buttons[(int)args[i]]);

                if (newButtons.Count > 0)
                    buttons = newButtons.ToArray();
            }

            if (!CEF.NPC.IsActive)
            {
                CEF.NPC.Show();
            }

            CEF.NPC.Draw(npcHolder.Name, Text, buttons.Select(x => new object[] { x.IsRed, x.Id, x.Text }).ToArray());
        }
    }
}
