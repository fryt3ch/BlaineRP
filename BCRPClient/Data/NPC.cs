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

        public string SubName { get; set; }

        public string Id { get; set; }

        public Types Type { get; set; }

        public Ped Ped { get; private set; }

        private bool _Invincible { get; set; }
        public bool Invincible { get => _Invincible; set { _Invincible = value; Ped.SetInvincible(value); } }

        public float DefaultHeading { get; set; }

        public string Name { get; set; }

        public string DefaultDialogueId { get; set; }

        public Dialogue CurrentDialogue { get; set; }

        public Additional.Cylinder Colshape { get; set; }

        public Blip Blip { get => Player.LocalPlayer.GetData<Blip>($"NPC::{Id}::Blip"); set { if (value == null) Player.LocalPlayer.ResetData($"NPC::{Id}::Blip"); else Player.LocalPlayer.SetData($"NPC::{Id}::Blip", value); } }

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

            data.Ped.SetHeading(data.DefaultHeading);
        }

        public static async System.Threading.Tasks.Task OnPedStreamOut(Ped ped)
        {

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

        public void ShowDialogue(string dialogueId, params object[] textArgs)
        {
            if (dialogueId == null)
                return;

            var dialogue = Dialogue.Get(dialogueId);

            if (dialogue == null)
                return;

            CurrentDialogue = dialogue;

            dialogue.Show(this, null, textArgs);
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
            {
                "vrent_s_preprocess",

                new Dialogue(null,

                    async (args) =>
                    {
                        if (NPC.CurrentNPC == null)
                            return;

                        var pData = Sync.Players.GetData(Player.LocalPlayer);

                        if (pData == null)
                            return;

                        var data = (int?)await NPC.CurrentNPC.CallRemoteProc("vrent_s_d") ?? -1;

                        if (NPC.CurrentNPC == null)
                            return;

                        if (data < 0)
                        {
                            NPC.CurrentNPC.SwitchDialogue(false);
                        }
                        else
                        {
                            var dg = AllDialogues["vrent_s_def_0"];

                            dg.Buttons[0].Text = $"Конечно [{Utils.GetPriceString(data)}]";

                            NPC.CurrentNPC.ShowDialogue("vrent_s_def_0");
                        }
                    }

                    )
            },

            {
                "vrent_s_def_0",

                new Dialogue("Привет! Хочешь недорого арендовать простенький мопед, с которым будет проще изучать наш округ?",

                    null,

                    new Button(null, async () =>
                    {
                        if (NPC.CurrentNPC == null)
                            return;

                        if ((bool?)await NPC.CurrentNPC.CallRemoteProc("vrent_s_p") ?? false)
                            NPC.CurrentNPC?.SwitchDialogue(false);
                    }, true),

                    new Button("[Выйти]", CloseCurrentDialogue, false)

                    )
                {

                }
            },

            #region Vehicle Pound
            {
                "vpound_preprocess",

                new Dialogue(null,

                    async (args) =>
                    {
                        if (NPC.CurrentNPC == null)
                            return;

                        var pData = Sync.Players.GetData(Player.LocalPlayer);

                        if (pData == null)
                            return;

                        var data = (string)await NPC.CurrentNPC.CallRemoteProc("vpound_d");

                        if (NPC.CurrentNPC == null)
                            return;

                        if (data == null)
                        {
                            NPC.CurrentNPC.ShowDialogue("vpound_no_vehicles_0");
                        }
                        else
                        {
                            var dataList = data.Split('_').ToList();

                            var price = Utils.GetPriceString(int.Parse(dataList[0]));

                            var vehs = dataList.Skip(1).Select(x => uint.Parse(x)).ToList();

                            if (vehs.Count == 1)
                            {
                                var vid = vehs[0];

                                NPC.CurrentNPC.TempDialogueData = vid;

                                NPC.CurrentNPC.ShowDialogue("vpound_def_dg_0", pData.OwnedVehicles.Where(x => x.VID == vid).Select(x => x.Data.Name).FirstOrDefault() ?? "null", price);
                            }
                            else
                            {
                                NPC.CurrentNPC.TempDialogueData = vehs;

                                NPC.CurrentNPC.ShowDialogue("vpound_def_dg_1", price);
                            }
                        }
                    }

                    )
            },

            {
                "vpound_no_vehicles_0",

                new Dialogue("Так-с, не вижу ни одного вашего транспорта в нашей системе.\nВот когда он у нас окажется - тогда и приходите!",

                    null,

                    new Button("[Выйти]", CloseCurrentDialogue, true)

                    )
                {
                    TimedTexts = new Dictionary<TimeTypes, string>()
                    {
                        { TimeTypes.Morning, "Доброе утро! `ddg`" },
                        { TimeTypes.Night, "[зевает]\n\n`ddg` И желательно - не ночью..." },
                    }
                }
            },

            {
                "vpound_def_dg_0",

                new Dialogue("Здравствуйте, {0} находится на нашей штрафстоянке, чтобы забрать его оплатите штраф - {1}\nЕсли что, мы принимаем только наличные!",

                    null,

                    new Button("[Оплатить]", async () =>
                    {
                        var vid = NPC.CurrentNPC?.TempDialogueData as uint?;

                        if (vid == null || NPC.LastSent.IsSpam(500, false, false))
                        {
                            return;
                        }
                        else
                        {
                            if ((bool?)await NPC.CurrentNPC.CallRemoteProc("vpound_p", vid) == true)
                                NPC.CurrentNPC?.SwitchDialogue(false);
                        }
                    }, true),

                    new Button("[Выйти]", CloseCurrentDialogue, false)

                    )
            },

            {
                "vpound_def_dg_1",

                new Dialogue("Здравствуйте, Ваш транспорт находится на нашей штрафстоянке, выберите нужный и оплатите штраф - {0}\nЕсли что, мы принимаем только наличные!",

                    null,

                    new Button("[Перейти к выбору]", () =>
                    {
                        if (NPC.CurrentNPC == null)
                            return;

                        var pData = Sync.Players.GetData(Player.LocalPlayer);

                        if (pData == null)
                            return;

                        var npcId = NPC.CurrentNPC.Id;

                        var vids = NPC.CurrentNPC.TempDialogueData as List<uint>;

                        if (vids == null)
                            return;

                        NPC.CurrentNPC.SwitchDialogue(false);

                        var counter = 0;

                        CEF.ActionBox.ShowSelect(ActionBox.Contexts.VehiclePoundSelect, Locale.Actions.VehiclePoundSelectHeader, vids.Select(x => (counter++, pData.OwnedVehicles.Where(y => y.VID == x).Select(x => $"{x.Data.Name} [#{x.VID}]").FirstOrDefault() ?? "null")).ToArray(), vids, npcId);
                    }, true),

                    new Button("[Выйти]", CloseCurrentDialogue, false)

                    )
            },
            #endregion

            #region Bank
            {
                "bank_preprocess",

                new Dialogue(null,

                    async (args) =>
                    {
                        if (NPC.CurrentNPC == null)
                            return;

                        var hasAccount = (bool?)await Events.CallRemoteProc("Bank::HasAccount") == true;

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

                    new Button("[Выйти]", CloseCurrentDialogue, false)

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

                    new Button("[Выйти]", CloseCurrentDialogue, false)

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

                    new Button("[Выйти]", CloseCurrentDialogue, false)

                    )
            },
            #endregion

            {
                "seller_bags_preprocess",

                new Dialogue(null, (args) =>
                {
                    if (NPC.CurrentNPC == null)
                        return;

                    if (Player.LocalPlayer.GetDrawableVariation(5) > 0)
                        NPC.CurrentNPC.ShowDialogue("seller_bags_b_0");
                    else
                        NPC.CurrentNPC.ShowDialogue("seller_bags_n_0");

                })
            },

            {
                "seller_bags_b_0",

                new Dialogue("Привет, вижу у вас уже есть сумка, не у меня покупали случаем? Но не думайте, скидку я никому не даю!", null,
                    Button.DefaultShopEnterButton,
                    Button.DefaultExitButton
                    )
            },

            {
                "seller_bags_n_0",

                new Dialogue("Приветствую, вижу, вам чего-то не хватает... Как насчёт новенькой сумочки или рюкзака?", null,
                    Button.DefaultShopEnterButton,
                    Button.DefaultExitButton
                    )
            },

            {
                "seller_clothes_greeting_0",
                
                new Dialogue("Приветствуем в нашем магазине!\nЖелаете ознакомиться с ассортиментом? У нас есть новые поступления, уверена, вам понравится!",
                    
                    null,

                    Button.DefaultShopEnterButton,

                    new Button("Есть ли работа для меня?", () => { }, true),

                    Button.DefaultExitButton

                    )
            },

            {
                "seller_shop_greeting_0",

                new Dialogue("Здравствуйте, хорошо, что вы заглянули к нам сегодня, как раз привезли свежайшие продукты!\n",

                    null,

                    new Button("[Смотреть товары]", () => { }, true),

                    new Button("Есть ли работа для меня?", () => { }, true),

                    new Button("[Выйти]", CloseCurrentDialogue, false)

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

                    new Button("[Выйти]", CloseCurrentDialogue,false)

                    )
            },

            {
                "seller_no_job_0",

                new Dialogue("К сожалению, пока что ваша помощь ни в чем не требуется, магазин с работой справляется.",

                    null,

                    new Button("[Назад]", () => { NPC.CurrentNPC?.ShowDialogue("seller_greeting_0"); }, true),

                    new Button("[Выйти]", CloseCurrentDialogue, false)

                    )
            },
        };

        private static void CloseCurrentDialogue() => NPC.CurrentNPC?.SwitchDialogue(false);

        public static Dialogue Get(string id) => AllDialogues.GetValueOrDefault(id);

        public class Button
        {
            public static Button DefaultExitButton { get; private set; } = new Button("[Выйти]", CloseCurrentDialogue, false);

            public static Button DefaultShopEnterButton { get; private set; } = new Button("[Перейти к товарам]", () => NPC.CurrentNPC?.SellerNpcRequestEnterBusiness(), true);

            /// <summary>Красная ли кнопка?</summary>
            public bool IsRed { get; set; }

            /// <summary>Текст</summary>
            public string Text { get; set; }

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

        public List<Button> Buttons { get; set; }

        public Action<object[]> Action { get; set; }

        public Dictionary<TimeTypes, string> TimedTexts { get; set; }

        public Dialogue(string Text, Action<object[]> Action = null, params Button[] Buttons)
        {
            this.Action = Action;

            this.Text = Text;

            this.Buttons = Buttons.ToList();
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
            if (buttonId < 0 || buttonId >= Buttons.Count)
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
        /// <param name="args">Аргументы (если выполняется invokeAction, то аргументы для действия, иначе - массив ID кнопок (int), которые нужно показать. Если args пустой, то будут показаны все кнопки</param>
        public void Show(NPC npcHolder, object[] args = null, params object[] textArgs)
        {
            Action?.Invoke(args);

            if (Text == null)
                return;

            var buttons = Buttons;

            if (args != null)
            {
                // ?
            }

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
                btnsData.Add(new object[] { buttons[i].IsRed, i, buttons[i].Text });

            CEF.NPC.Draw(npcHolder.Name, text, btnsData.ToArray());
        }
    }
}
