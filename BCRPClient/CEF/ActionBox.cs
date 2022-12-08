using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.CEF
{
    class ActionBox : Events.Script
    {
        public static bool IsActive { get => CEF.Browser.IsActive(Browser.IntTypes.ActionBox); }

        public enum ReplyTypes
        {
            /// <summary>Принять</summary>
            /// <remarks>Для Money - наличные</remarks>
            OK = 0,
            /// <summary>Отменить</summary>
            /// <remarks>Для Money - банк</remarks>
            Cancel = 1,
            /// <summary>Доп. кнопка (вариативая)</summary>
            /// <remarks>Для Money - отменить</remarks>
            Additional1 = 2,
        }

        private enum ActionTypes
        {
            Show = 0,
            Choose,
            Close,
        }

        public enum Types
        {
            None = -1, Select, Input, Range, Money,
        }

        public enum Contexts
        {
            None = -1, Inventory, GiveCash,
            HouseExit, GarageVehicleIn,

            TuningShopDeleteMod,
            NumberplateSelect,
        }

        public static Types CurrentType { get; private set; }

        public static Contexts CurrentContext { get; private set; }

        private static List<int> TempBinds { get; set; }

        private static DateTime LastSent;

        private static Dictionary<Types, Dictionary<Contexts, Dictionary<ActionTypes, Action<object[]>>>> ContextsActions = new Dictionary<Types, Dictionary<Contexts, Dictionary<ActionTypes, Action<object[]>>>>()
        {
            {
                Types.Range,

                new Dictionary<Contexts, Dictionary<ActionTypes, Action<object[]>>>()
                {
                    {
                        Contexts.Inventory,

                        new Dictionary<ActionTypes, Action<object[]>>()
                        {
                            {
                                ActionTypes.Choose, (args) =>
                                {
                                    var rType = (ReplyTypes)args[0];
                                    var amount = (int)args[1];

                                    if (rType == ReplyTypes.OK)
                                    {
                                        CEF.Inventory.Action(amount);
                                    }
                                    else if (rType == ReplyTypes.Cancel)
                                    {
                                        CEF.Inventory.Action(-1);
                                    }
                                    else
                                        return;
                                }
                            }
                        }
                    },

                    {
                        Contexts.GiveCash,

                        new Dictionary<ActionTypes, Action<object[]>>()
                        {
                            {
                                ActionTypes.Show, (args) =>
                                {
                                    Bind();
                                }
                            },

                            {
                                ActionTypes.Choose, (args) =>
                                {
                                    var rType = (ReplyTypes)args[0];
                                    var amount = (int)args[1];

                                    if (rType == ReplyTypes.OK)
                                    {
                                        Close(true);

                                        if (BCRPClient.Interaction.CurrentEntity is Player targetPlayer)
                                            Sync.Offers.Request(targetPlayer, Sync.Offers.Types.Cash, amount);
                                    }
                                    else if (rType == ReplyTypes.Cancel)
                                    {
                                        Close(true);
                                    }
                                    else
                                        return;
                                }
                            },
                        }
                    },
                }
            },

            {
                Types.Select,

                new Dictionary<Contexts, Dictionary<ActionTypes, Action<object[]>>>()
                {
                    {
                        Contexts.NumberplateSelect,

                        new Dictionary<ActionTypes, Action<object[]>>()
                        {
                            {
                                ActionTypes.Show, (args) =>
                                {
                                    Player.LocalPlayer.SetData("ActionBox::Temp::NPSI", args[0]);

                                    Bind();
                                }
                            },

                            {
                                ActionTypes.Close, (args) => Player.LocalPlayer.ResetData("ActionBox::Temp::NPSI")
                            },

                            {
                                ActionTypes.Choose, (args) =>
                                {
                                    var rType = (ReplyTypes)args[0];
                                    var id = (int)args[1];

                                    if (rType == ReplyTypes.OK)
                                    {
                                        var items = Player.LocalPlayer.GetData<Dictionary<uint, string>>("ActionBox::Temp::NPSI");

                                        if (BCRPClient.Interaction.CurrentEntity is Vehicle veh)
                                        {
                                            Events.CallRemote("Vehicles::SetupPlate", veh, items.ElementAt(id).Key);
                                        }
                                    }

                                    Close(true);
                                }
                            }
                        }
                    },

                    {
                        Contexts.HouseExit,

                        new Dictionary<ActionTypes, Action<object[]>>()
                        {
                            {
                                ActionTypes.Show, (args) =>
                                {
                                    Bind();
                                }
                            },

                            {
                                ActionTypes.Choose, (args) =>
                                {
                                    var rType = (ReplyTypes)args[0];
                                    var id = (int)args[1];

                                    if (rType == ReplyTypes.OK)
                                    {
                                        // house/houseGarage -> outside
                                        if (id == 0)
                                        {
                                            if (LastSent.IsSpam(1000, false, false))
                                                return;

                                            Events.CallRemote("House::Exit");

                                            Close(true);
                                        }
                                        // house -> garage
                                        else if (id == 1)
                                        {
                                            if (LastSent.IsSpam(1000, false, false))
                                                return;

                                            Events.CallRemote("House::Garage", true);

                                            Close(true);
                                        }
                                        // garage -> house
                                        else if (id == 2)
                                        {
                                            if (LastSent.IsSpam(1000, false, false))
                                                return;

                                            Events.CallRemote("House::Garage", false);

                                            Close(true);
                                        }
                                        else
                                            return;
                                    }
                                    else if (rType == ReplyTypes.Cancel)
                                    {
                                        Close(true);
                                    }
                                    else
                                        return;
                                }
                            }
                        }
                    }
                }
            },

            {
                Types.Money,

                new Dictionary<Contexts, Dictionary<ActionTypes, Action<object[]>>>()
                {
                    {
                        Contexts.TuningShopDeleteMod,

                        new Dictionary<ActionTypes, Action<object[]>>()
                        {
                            {
                                ActionTypes.Show, (args) =>
                                {
                                    string modToDelete = (string)args[0];

                                    Player.LocalPlayer.SetData("ActionBox::Temp::MTD", modToDelete);

                                    Bind();
                                }
                            },

                            {
                                ActionTypes.Choose, async (args) =>
                                {
                                    var rType = (ReplyTypes)args[0];

                                    var id = Player.LocalPlayer.GetData<string>("ActionBox::Temp::MTD");

                                    if (id == null)
                                    {
                                        Close(false);

                                        return;
                                    }

                                    if (rType == ReplyTypes.OK || rType == ReplyTypes.Cancel)
                                    {
                                        if ((bool)await Events.CallRemoteProc("TuningShop::Buy", id, rType == ReplyTypes.OK))
                                        {
                                            var idData = id.Split('_');

                                            if (CEF.Shop.IsActiveTuning)
                                            {
                                                CEF.Browser.Window.ExecuteJs("Tuning.switchColor", false, idData[0]);

                                                var sId = id.Split('_')[0];

                                                if (sId == "neon")
                                                {
                                                    Player.LocalPlayer.SetData("TuningShop::Temp::CurNeon", (string)null);

                                                    CEF.Shop.TempVehicle?.SetNeonEnabled(false);
                                                }
                                                else if (sId == "tsmoke")
                                                {
                                                    Player.LocalPlayer.SetData("TuningShop::Temp::TyreSmokeCol", (string)null);
                                                }
                                                else if (sId == "wcolour")
                                                {
                                                    CEF.Shop.TempVehicle?.SetWheelsColour(0);
                                                }
                                                else if (sId == "pearl")
                                                {
                                                    CEF.Shop.TempVehicle?.SetPearlColour(0);
                                                }
                                            }
                                        }
                                    }
                                    else if (rType == ReplyTypes.Additional1)
                                    {
                                        Close(false);
                                    }

                                    Close(false);
                                }
                            },

                            {
                                ActionTypes.Close, (args) =>
                                {
                                    Player.LocalPlayer.ResetData("ActionBox::Temp::MTD");

                                    if (CEF.Shop.IsActiveTuning)
                                        CEF.Cursor.Show(true, true);
                                }
                            }
                        }
                    }
                }
            }
        };

        private static void TryInvokeAction(Types type, Contexts context, ActionTypes aType, params object[] args) => ContextsActions.GetValueOrDefault(type)?.GetValueOrDefault(context)?.GetValueOrDefault(aType)?.Invoke(args);

        public ActionBox()
        {
            LastSent = DateTime.MinValue;

            TempBinds = new List<int>();

            CurrentType = Types.None;
            CurrentContext = Contexts.None;

            Events.Add("ActionBox::Reply", async (object[] args) =>
            {
                if (CurrentType == Types.None || CurrentContext == Contexts.None)
                    return;

                ReplyTypes rType = (ReplyTypes)args[0];

                if (CurrentType == Types.Range)
                {
                    int amount = int.Parse((string)args[1]);

                    TryInvokeAction(CurrentType, CurrentContext, ActionTypes.Choose, rType, amount);
                }
                else if (CurrentType == Types.Select)
                {
                    int id = args[1] is int ? (int)args[1] : int.Parse((string)args[1]);

                    TryInvokeAction(CurrentType, CurrentContext, ActionTypes.Choose, rType, id);
                }
                else if (CurrentType == Types.Input)
                {
                    string text = (string)args[1];

                    TryInvokeAction(CurrentType, CurrentContext, ActionTypes.Choose, rType, text);
                }
                else if (CurrentType == Types.Money)
                {
                    TryInvokeAction(CurrentType, CurrentContext, ActionTypes.Choose, rType);
                }
                else
                    return;
            });
        }

        public static async System.Threading.Tasks.Task ShowSelect(Contexts context, string name, (int Id, string Text)[] variants, params object[] args)
        {
            if (IsActive)
                return;

            await CEF.Browser.Render(Browser.IntTypes.ActionBox, true, true);

            CurrentType = Types.Select;
            CurrentContext = context;

            TryInvokeAction(CurrentType, CurrentContext, ActionTypes.Show, args);

            CEF.Browser.Window.ExecuteJs("ActionBox.fill", false, CurrentType, name, variants.Select(x => new object[] { x.Id, x.Text }));

            Cursor.Show(true, true);
        }

        public static async System.Threading.Tasks.Task ShowRange(Contexts context, string name, int minValue, int maxValue, int curValue = -1, int step = -1, params object[] args)
        {
            if (IsActive)
                return;

            await CEF.Browser.Render(Browser.IntTypes.ActionBox, true, true);

            CurrentType = Types.Range;
            CurrentContext = context;

            TryInvokeAction(CurrentType, CurrentContext, ActionTypes.Show, args);

            CEF.Browser.Window.ExecuteJs("ActionBox.fill", false, CurrentType, name, new object[] { minValue, maxValue, curValue == -1 ? maxValue : curValue, step == -1 ? 1 : step });

            Cursor.Show(true, true);
        }

        public static async System.Threading.Tasks.Task ShowInput(Contexts context, string name, int maxChars = 100, params object[] args)
        {
            if (IsActive)
                return;

            await CEF.Browser.Render(Browser.IntTypes.ActionBox, true, true);

            CurrentType = Types.Input;
            CurrentContext = context;

            TryInvokeAction(CurrentType, CurrentContext, ActionTypes.Show, args);

            CEF.Browser.Window.ExecuteJs("ActionBox.fill", false, CurrentType, name);

            Cursor.Show(true, true);
        }

        public static async System.Threading.Tasks.Task ShowMoney(Contexts context, string name, string text, params object[] args)
        {
            if (IsActive)
                return;

            await CEF.Browser.Render(Browser.IntTypes.ActionBox, true, true);

            CurrentType = Types.Money;
            CurrentContext = context;

            TryInvokeAction(CurrentType, CurrentContext, ActionTypes.Show, args);

            CEF.Browser.Window.ExecuteJs("ActionBox.fill", false, CurrentType, name, new object[] { Utils.ReplaceNewLineHtml(text) });

            Cursor.Show(true, true);
        }

        public static void Close(bool cursor = true)
        {
            if (!IsActive)
                return;

            for (int i = 0; i < TempBinds.Count; i++)
                RAGE.Input.Unbind(TempBinds[i]);

            TempBinds.Clear();

            CEF.Browser.Render(Browser.IntTypes.ActionBox, false, false);

            if (cursor)
                Cursor.Show(false, false);

            TryInvokeAction(CurrentType, CurrentContext, ActionTypes.Close);

            CurrentType = Types.None;
            CurrentContext = Contexts.None;
        }

        private static void Bind()
        {
            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(true)));
        }
    }
}
