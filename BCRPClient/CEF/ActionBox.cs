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

        public enum RangeSubTypes
        {
            Default = 0,
            MoneyRange = 1,
        }

        public enum Contexts
        {
            None = -1, Inventory, GiveCash,
            HouseExit, GarageVehicleIn,

            TuningShopDeleteMod,
            NumberplateSelect,
            VehiclePassportSelect,

            GarageVehiclePlaceSelect,

            VehiclePoundSelect,

            WeaponSkinsMenuSelect,

            VehicleTuningVehicleSelect,

            HouseBalanceChange,

            GarageBalanceChange,

            BusinessBalanceChange,

            PlacedItemOnGroundSelect,
            ItemOnGroundTakeRange,

            JobVehicleRentMoney,

            JobTruckerOrderSelect,
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
                        Contexts.ItemOnGroundTakeRange,

                        new Dictionary<ActionTypes, Action<object[]>>()
                        {
                            {
                                ActionTypes.Show, (args) =>
                                {
                                    Player.LocalPlayer.SetData("AB::Temp::IOGTR", args[0]);

                                    Bind();
                                }
                            },

                            {
                                ActionTypes.Close, (args) =>
                                {
                                    Player.LocalPlayer.ResetData("AB::Temp::IOGTR");
                                }
                            },

                            {
                                ActionTypes.Choose, (args) =>
                                {
                                    var rType = (ReplyTypes)args[0];
                                    var amountD = (decimal)(args[1]);

                                    if (Sync.World.ItemOnGround.LastSent.IsSpam(500, false, false))
                                        return;

                                    int amount;

                                    if (!amountD.IsNumberValid(0, int.MaxValue, out amount, true))
                                        return;

                                    var iog = Player.LocalPlayer.GetData<Sync.World.ItemOnGround>("AB::Temp::IOGTR");

                                    Close(true);

                                    if (iog?.Object?.Exists != true)
                                        return;

                                    if (rType == ReplyTypes.OK)
                                    {
                                        Events.CallRemote("Inventory::Take", iog.Uid, amount);

                                        Sync.World.ItemOnGround.LastSent = DateTime.Now;
                                    }
                                }
                            }
                        }
                    },

                    {
                        Contexts.Inventory,

                        new Dictionary<ActionTypes, Action<object[]>>()
                        {
                            {
                                ActionTypes.Show, (args) =>
                                {
                                    CEF.Inventory.FreezeInterface(true, false);
                                }
                            },

                            {
                                ActionTypes.Choose, (args) =>
                                {
                                    var rType = (ReplyTypes)args[0];
                                    var amountD = (decimal)args[1];

                                    int amount;

                                    if (!amountD.IsNumberValid(0, int.MaxValue, out amount, true))
                                        return;

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
                            },

                            {
                                ActionTypes.Close, (args) =>
                                {
                                    CEF.Inventory.FreezeInterface(false, false);
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
                                    var amountD = (decimal)args[1];

                                    int amount;

                                    if (!amountD.IsNumberValid(1, int.MaxValue, out amount, true))
                                        return;

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

                    {
                        Contexts.HouseBalanceChange,

                        new Dictionary<ActionTypes, Action<object[]>>()
                        {
                            {
                                ActionTypes.Show, (args) =>
                                {
                                    Bind();

                                    Player.LocalPlayer.SetData("AB::Temp::HBD::H", (Data.Locations.HouseBase)args[0]);
                                    Player.LocalPlayer.SetData("AB::Temp::HBD::B", (Data.Locations.Bank)args[1]);
                                    Player.LocalPlayer.SetData("AB::Temp::HBD::A", (bool)args[2]);
                                }
                            },

                            {
                                ActionTypes.Choose, async (args) =>
                                {
                                    var rType = (ReplyTypes)args[0];
                                    var amountD = (decimal)args[1];

                                    var house = Player.LocalPlayer.GetData<Data.Locations.HouseBase>("AB::Temp::HBD::H");

                                    var bank = Player.LocalPlayer.GetData<Data.Locations.Bank>("AB::Temp::HBD::B");

                                    var add = Player.LocalPlayer.GetData<bool>("AB::Temp::HBD::A");

                                    if (house == null)
                                        return;

                                    if (LastSent.IsSpam(1000, false, false))
                                        return;

                                    int amount;

                                    if (!amountD.IsNumberValid(1, int.MaxValue, out amount, true))
                                        return;

                                    LastSent = DateTime.Now;

                                    var useCash = rType == ReplyTypes.OK;

                                    var resObj = await Events.CallRemoteProc("Bank::HBC", house.Type == Sync.House.HouseTypes.House, house.Id, bank.Id, amount, useCash, add);

                                    if (resObj == null)
                                        return;

                                    Close(true);
                                }
                            },

                            {
                                ActionTypes.Close, (args) =>
                                {
                                    Player.LocalPlayer.ResetData("AB::Temp::HBD::H");
                                    Player.LocalPlayer.ResetData("AB::Temp::HBD::B");
                                    Player.LocalPlayer.ResetData("AB::Temp::HBD::A");
                                }
                            }
                        }
                    },

                    {
                        Contexts.GarageBalanceChange,

                        new Dictionary<ActionTypes, Action<object[]>>()
                        {
                            {
                                ActionTypes.Show, (args) =>
                                {
                                    Bind();

                                    Player.LocalPlayer.SetData("AB::Temp::GBD::H", (Data.Locations.Garage)args[0]);
                                    Player.LocalPlayer.SetData("AB::Temp::GBD::B", (Data.Locations.Bank)args[1]);
                                    Player.LocalPlayer.SetData("AB::Temp::GBD::A", (bool)args[2]);
                                }
                            },

                            {
                                ActionTypes.Choose, async (args) =>
                                {
                                    var rType = (ReplyTypes)args[0];
                                    var amountD = (decimal)args[1];

                                    var garage = Player.LocalPlayer.GetData<Data.Locations.Garage>("AB::Temp::GBD::H");

                                    var bank = Player.LocalPlayer.GetData<Data.Locations.Bank>("AB::Temp::GBD::B");

                                    var add = Player.LocalPlayer.GetData<bool>("AB::Temp::GBD::A");

                                    if (garage == null)
                                        return;

                                    if (LastSent.IsSpam(1000, false, false))
                                        return;

                                    int amount;

                                    if (!amountD.IsNumberValid(1, int.MaxValue, out amount, true))
                                        return;

                                    LastSent = DateTime.Now;

                                    var useCash = rType == ReplyTypes.OK;

                                    var resObj = await Events.CallRemoteProc("Bank::GBC", garage.Id, bank.Id, amount, useCash, add);

                                    if (resObj == null)
                                        return;

                                    Close(true);
                                }
                            },

                            {
                                ActionTypes.Close, (args) =>
                                {
                                    Player.LocalPlayer.ResetData("AB::Temp::GBD::H");
                                    Player.LocalPlayer.ResetData("AB::Temp::GBD::B");
                                    Player.LocalPlayer.ResetData("AB::Temp::GBD::A");
                                }
                            }
                        }
                    },

                    {
                        Contexts.BusinessBalanceChange,

                        new Dictionary<ActionTypes, Action<object[]>>()
                        {
                            {
                                ActionTypes.Show, (args) =>
                                {
                                    Bind();

                                    Player.LocalPlayer.SetData("AB::Temp::BBD::H", (Data.Locations.Business)args[0]);
                                    Player.LocalPlayer.SetData("AB::Temp::BBD::B", (Data.Locations.Bank)args[1]);
                                    Player.LocalPlayer.SetData("AB::Temp::BBD::A", (bool)args[2]);
                                }
                            },

                            {
                                ActionTypes.Choose, async (args) =>
                                {
                                    var rType = (ReplyTypes)args[0];
                                    var amountD = (decimal)args[1];

                                    var biz = Player.LocalPlayer.GetData<Data.Locations.Business>("AB::Temp::BBD::H");

                                    var bank = Player.LocalPlayer.GetData<Data.Locations.Bank>("AB::Temp::BBD::B");

                                    var add = Player.LocalPlayer.GetData<bool>("AB::Temp::BBD::A");

                                    if (biz == null)
                                        return;

                                    if (LastSent.IsSpam(1000, false, false))
                                        return;

                                    int amount;

                                    if (!amountD.IsNumberValid(1, int.MaxValue, out amount, true))
                                        return;

                                    LastSent = DateTime.Now;

                                    var useCash = rType == ReplyTypes.OK;

                                    var resObj = await Events.CallRemoteProc("Bank::BBC", biz.Id, bank.Id, amount, useCash, add);

                                    if (resObj == null)
                                        return;

                                    Close(true);
                                }
                            },

                            {
                                ActionTypes.Close, (args) =>
                                {
                                    Player.LocalPlayer.ResetData("AB::Temp::BBD::H");
                                    Player.LocalPlayer.ResetData("AB::Temp::BBD::B");
                                    Player.LocalPlayer.ResetData("AB::Temp::BBD::A");
                                }
                            }
                        }
                    },
                }
            },

            {
                Types.Select,

                new Dictionary<Contexts, Dictionary<ActionTypes, Action<object[]>>>()
                {
                    {
                        Contexts.PlacedItemOnGroundSelect,

                        new Dictionary<ActionTypes, Action<object[]>>()
                        {
                            {
                                ActionTypes.Show, (args) =>
                                {
                                    Player.LocalPlayer.SetData("AB::Temp::PIOGS", args[0]);

                                    Bind();
                                }
                            },

                            {
                                ActionTypes.Close, (args) => Player.LocalPlayer.ResetData("AB::Temp::PIOGS")
                            },

                            {
                                ActionTypes.Choose, (args) =>
                                {
                                    var rType = (ReplyTypes)args[0];
                                    var id = (int)args[1];

                                    var iog = Player.LocalPlayer.GetData<Sync.World.ItemOnGround>("AB::Temp::PIOGS");

                                    Close(true);

                                    if (rType == ReplyTypes.OK)
                                    {
                                        if (iog?.Object?.Exists != true)
                                            return;

                                        if (id == 0)
                                        {
                                            CEF.Inventory.Show(Inventory.Types.Workbench, 0, iog.Uid);
                                        }
                                        else if (id == 1)
                                        {
                                            Events.CallRemote("Item::IOGL", iog.Uid, !iog.IsLocked);
                                        }
                                        else if (id == 2)
                                        {
                                            iog.TakeItem();
                                        }
                                    }
                                }
                            }
                        }
                    },

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
                    },

                    {
                        Contexts.VehiclePassportSelect,

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
                                        var player = BCRPClient.Interaction.CurrentEntity as Player;

                                        if (player == null)
                                            return;

                                        var pData = Sync.Players.GetData(Player.LocalPlayer);

                                        if (pData == null)
                                            return;

                                        var allVehs = pData.OwnedVehicles;

                                        if (allVehs.Count <= id)
                                        {
                                            Close(true);

                                            return;
                                        }

                                        Close(true);

                                        Sync.Offers.Request(player, Sync.Offers.Types.ShowVehiclePassport, allVehs[id].VID);
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
                    },

                    {
                        Contexts.GarageVehiclePlaceSelect,

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
                                        var vehicle = BCRPClient.Interaction.CurrentEntity as Vehicle;

                                        if (vehicle == null)
                                            return;

                                        Close(true);

                                        if (id < 0)
                                            id = int.MinValue + id;

                                        Sync.Vehicles.Park(vehicle, id);
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
                    },

                    {
                        Contexts.VehiclePoundSelect,

                        new Dictionary<ActionTypes, Action<object[]>>()
                        {
                            {
                                ActionTypes.Show, (args) =>
                                {
                                    Bind();

                                    Player.LocalPlayer.SetData("ActionBox::Temp::VPSL", args[0]);
                                    Player.LocalPlayer.SetData("ActionBox::Temp::VPSN", args[1]);
                                }
                            },

                            {
                                ActionTypes.Choose, async (args) =>
                                {
                                    var rType = (ReplyTypes)args[0];
                                    var id = (int)args[1];

                                    if (rType == ReplyTypes.OK)
                                    {
                                        var vids = Player.LocalPlayer.GetData<List<uint>>("ActionBox::Temp::VPSL");

                                        if (vids == null)
                                            return;

                                        var npcName = Player.LocalPlayer.GetData<string>("ActionBox::Temp::VPSN");

                                        if (npcName == null)
                                            return;

                                        var vid = vids[id];

                                        if (LastSent.IsSpam(1000, false, false))
                                            return;

                                        var npcData = Data.NPC.GetData(npcName);

                                        if (npcData == null)
                                            return;

                                        if ((bool?)await npcData.CallRemoteProc("vpound_p", vid) ?? false)
                                        {
                                            Close(true);
                                        }
                                    }
                                    else if (rType == ReplyTypes.Cancel)
                                    {
                                        Close(true);
                                    }
                                    else
                                        return;
                                }
                            },

                            {
                                ActionTypes.Close, (args) =>
                                {
                                    Player.LocalPlayer.ResetData("ActionBox::Temp::VPSL");
                                    Player.LocalPlayer.ResetData("ActionBox::Temp::VPSN");
                                }
                            }
                        }
                    },

                    {
                        Contexts.WeaponSkinsMenuSelect,

                        new Dictionary<ActionTypes, Action<object[]>>()
                        {
                            {
                                ActionTypes.Show, (args) =>
                                {
                                    Bind();
                                }
                            },

                            {
                                ActionTypes.Choose, async (args) =>
                                {
                                    var rType = (ReplyTypes)args[0];
                                    var id = (int)args[1];

                                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                                    if (pData == null)
                                        return;

                                    var wSkins = pData.WeaponSkins;

                                    if (rType == ReplyTypes.OK)
                                    {
                                        if (!wSkins.Keys.Where(x => (int)x == id).Any())
                                        {
                                            Close(true);

                                            return;
                                        }

                                        if (LastSent.IsSpam(1000, false, false))
                                            return;

                                        if ((bool)await Events.CallRemoteProc("WSkins::Rm", id))
                                        {
                                            Close(true);
                                        }
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

                    {
                        Contexts.VehicleTuningVehicleSelect,

                        new Dictionary<ActionTypes, Action<object[]>>()
                        {
                            {
                                ActionTypes.Show, (args) =>
                                {
                                    Bind();

                                    Player.LocalPlayer.SetData("ActionBox::Temp::VTVST", args[0]);
                                    Player.LocalPlayer.SetData("ActionBox::Temp::VTVSV1", args[1]);
                                    Player.LocalPlayer.SetData("ActionBox::Temp::VTVSV2", args[2]);
                                }
                            },

                            {
                                ActionTypes.Choose, async (args) =>
                                {
                                    var rType = (ReplyTypes)args[0];
                                    var id = (int)args[1];

                                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                                    if (pData == null)
                                        return;

                                    if (rType == ReplyTypes.OK)
                                    {
                                        if (id != 1 && id != 2)
                                            return;

                                        var vehicle = Player.LocalPlayer.GetData<Vehicle>($"ActionBox::Temp::VTVSV{id}");

                                        Events.CallRemote("TuningShop::Enter", Player.LocalPlayer.GetData<int>("ActionBox::Temp::VTVST"), vehicle);

                                        Close(true);
                                    }
                                    else if (rType == ReplyTypes.Cancel)
                                    {
                                        Close(true);

                                        Player.LocalPlayer.ResetData("ActionBox::Temp::VTVST");
                                        Player.LocalPlayer.ResetData("ActionBox::Temp::VTVSV1");
                                        Player.LocalPlayer.ResetData("ActionBox::Temp::VTVSV2");
                                    }
                                    else
                                        return;
                                }
                            },
                        }
                    },

                    {
                        Contexts.JobTruckerOrderSelect,

                        new Dictionary<ActionTypes, Action<object[]>>()
                        {
                            {
                                ActionTypes.Show, (args) =>
                                {
                                    var vehicle = (Vehicle)args[0];

                                    Bind();

                                    var checkAction = new Action(() =>
                                    {
                                        if (Player.LocalPlayer.Vehicle != vehicle || vehicle?.Exists != true || vehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                            Close(true);
                                    });

                                    Player.LocalPlayer.SetData("ActionBox::Temp::JVRVA", checkAction);

                                    GameEvents.Update -= checkAction.Invoke;
                                    GameEvents.Update += checkAction.Invoke;
                                }
                            },

                            {
                                ActionTypes.Choose, async (args) =>
                                {
                                    var rType = (ReplyTypes)args[0];
                                    var id = (int)args[1];

                                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                                    if (pData == null)
                                        return;

                                    if (rType == ReplyTypes.OK)
                                    {
                                        var orders = pData.CurrentJob?.GetCurrentData<List<Data.Locations.Trucker.OrderInfo>>("AOL");

                                        if (orders == null)
                                            return;

                                        if (id >= orders.Count)
                                            return;

                                        var res = (byte)(await Events.CallRemoteProc("Job::TR::TO", orders[id].Id)).ToDecimal();

                                        if (res == byte.MaxValue)
                                        {
                                            Close(true);
                                        }
                                        else
                                        {

                                        }
                                    }
                                    else if (rType == ReplyTypes.Cancel)
                                    {
                                        Close(true);
                                    }
                                    else
                                        return;
                                }
                            },

                            {
                                ActionTypes.Close, (args) =>
                                {
                                    var checkAction = Player.LocalPlayer.GetData<Action>("ActionBox::Temp::JVRVA");

                                    if (checkAction != null)
                                    {
                                        GameEvents.Update -= checkAction.Invoke;

                                        Player.LocalPlayer.ResetData("ActionBox::Temp::JVRVA");
                                    }
                                }
                            }
                        }
                    },
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
                                        if ((bool)await Events.CallRemoteProc("Shop::Buy", id, rType == ReplyTypes.OK))
                                        {
                                            var idData = id.Split('_');

                                            if (CEF.Shop.IsRenderedTuning)
                                            {
                                                CEF.Browser.Window.ExecuteJs("Tuning.switchColor", false, idData[0]);

                                                if (idData[0] == "neon")
                                                {
                                                    CEF.Shop.TempVehicle?.SetNeonEnabled(false);
                                                }
                                                else if (idData[0] == "tsmoke")
                                                {
                                                    CEF.Shop.TempVehicle?.SetTyreSmokeColor(255, 255, 255);
                                                }
                                                else if (idData[0] == "wcolour")
                                                {
                                                    CEF.Shop.TempVehicle?.SetWheelsColour(0);
                                                }
                                                else if (idData[0] == "pearl")
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
                                }
                            }
                        }
                    },

                    {
                        Contexts.JobVehicleRentMoney,

                        new Dictionary<ActionTypes, Action<object[]>>()
                        {
                            {
                                ActionTypes.Show, (args) =>
                                {
                                    Bind();

                                    var vehicle = (Vehicle)args[0];

                                    Player.LocalPlayer.SetData("ActionBox::Temp::JVRVE", vehicle);

                                    var checkAction = new Action(() =>
                                    {
                                        if (Player.LocalPlayer.Vehicle != vehicle || vehicle?.Exists != true || vehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                            Close(true);
                                    });

                                    Player.LocalPlayer.SetData("ActionBox::Temp::JVRVA", checkAction);

                                    GameEvents.Update -= checkAction.Invoke;
                                    GameEvents.Update += checkAction.Invoke;
                                }
                            },

                            {
                                ActionTypes.Choose, async (args) =>
                                {
                                    if (LastSent.IsSpam(500))
                                        return;

                                    var rType = (ReplyTypes)args[0];

                                    var vehicle = Player.LocalPlayer.GetData<Vehicle>("ActionBox::Temp::JVRVE");

                                    if (vehicle == null)
                                    {
                                        Close(true);

                                        return;
                                    }

                                    if (rType == ReplyTypes.OK || rType == ReplyTypes.Cancel)
                                    {
                                        LastSent = DateTime.Now;

                                        if ((bool)await Events.CallRemoteProc("Vehicles::JVRS", rType == ReplyTypes.OK))
                                        {
                                            Close(true, true);
                                        }
                                    }
                                    else if (rType == ReplyTypes.Additional1)
                                    {
                                        Close(true);
                                    }
                                }
                            },

                            {
                                ActionTypes.Close, (args) =>
                                {
                                    var checkAction = Player.LocalPlayer.GetData<Action>("ActionBox::Temp::JVRVA");

                                    if (checkAction != null)
                                    {
                                        GameEvents.Update -= checkAction.Invoke;

                                        Player.LocalPlayer.ResetData("ActionBox::Temp::JVRVA");
                                    }

                                    if (args == null || args.Length < 1)
                                    {
                                        var vehicle = Player.LocalPlayer.GetData<Vehicle>("ActionBox::Temp::JVRVE");

                                        if (Player.LocalPlayer.Vehicle == vehicle)
                                        {
                                            Player.LocalPlayer.TaskLeaveAnyVehicle(0, 0);
                                        }
                                    }

                                    Player.LocalPlayer.ResetData("ActionBox::Temp::JVRVE");
                                }
                            }
                        }
                    },
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
                    var amount = decimal.Parse((string)args[1]);

                    if (amount < Player.LocalPlayer.GetData<decimal>("ActionBox::Temp::MinValue"))
                    {
                        CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, string.Format(Locale.Notifications.General.LessThanMinValue, Player.LocalPlayer.GetData<decimal>("ActionBox::Temp::MinValue")));

                        return;
                    }
                    else if (amount > Player.LocalPlayer.GetData<decimal>("ActionBox::Temp::MaxValue"))
                    {
                        CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, string.Format(Locale.Notifications.General.BiggerThanMaxValue, Player.LocalPlayer.GetData<decimal>("ActionBox::Temp::MaxValue")));

                        return;
                    }

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

        public static async System.Threading.Tasks.Task ShowSelect(Contexts context, string name, (int Id, string Text)[] variants, string btnTextOk = null, string btnTextCancel = null, params object[] args)
        {
            if (!await CEF.Browser.Render(Browser.IntTypes.ActionBox, true, true))
                return;

            CurrentType = Types.Select;
            CurrentContext = context;

            TryInvokeAction(CurrentType, CurrentContext, ActionTypes.Show, args);

            CEF.Browser.Window.ExecuteJs("ActionBox.fill", false, CurrentType, name, variants.Select(x => new object[] { x.Id, x.Text }), new object[] { btnTextOk ?? Locale.Actions.SelectOkBtn0, btnTextCancel ?? Locale.Actions.SelectCancelBtn0 });

            Cursor.Show(true, true);
        }

        public static async System.Threading.Tasks.Task ShowRange(Contexts context, string name, decimal minValue, decimal maxValue, decimal curValue = -1, decimal step = -1, RangeSubTypes rsType = RangeSubTypes.Default, params object[] args)
        {
            if (!await CEF.Browser.Render(Browser.IntTypes.ActionBox, true, true))
                return;

            CurrentType = Types.Range;
            CurrentContext = context;

            TryInvokeAction(CurrentType, CurrentContext, ActionTypes.Show, args);

            Player.LocalPlayer.SetData("ActionBox::Temp::MinValue", minValue);
            Player.LocalPlayer.SetData("ActionBox::Temp::MaxValue", maxValue);

            CEF.Browser.Window.ExecuteJs("ActionBox.fill", false, CurrentType, name, new object[] { minValue, maxValue, curValue == -1 ? maxValue : curValue, step == -1 ? 1 : step, (int)rsType });

            Cursor.Show(true, true);
        }

        public static async System.Threading.Tasks.Task ShowInput(Contexts context, string name, int maxChars = 100, params object[] args)
        {
            if (!await CEF.Browser.Render(Browser.IntTypes.ActionBox, true, true))
                return;

            CurrentType = Types.Input;
            CurrentContext = context;

            TryInvokeAction(CurrentType, CurrentContext, ActionTypes.Show, args);

            CEF.Browser.Window.ExecuteJs("ActionBox.fill", false, CurrentType, name);

            Cursor.Show(true, true);
        }

        public static async System.Threading.Tasks.Task ShowMoney(Contexts context, string name, string text, params object[] args)
        {
            if (!await CEF.Browser.Render(Browser.IntTypes.ActionBox, true, true))
                return;

            CurrentType = Types.Money;
            CurrentContext = context;

            TryInvokeAction(CurrentType, CurrentContext, ActionTypes.Show, args);

            CEF.Browser.Window.ExecuteJs("ActionBox.fill", false, CurrentType, name, new object[] { Utils.ReplaceNewLineHtml(text) });

            Cursor.Show(true, true);
        }

        public static async void Close(bool cursor = true, params object[] args)
        {
            if (!await CEF.Browser.Render(Browser.IntTypes.ActionBox, false, false))
                return;

            for (int i = 0; i < TempBinds.Count; i++)
                KeyBinds.Unbind(TempBinds[i]);

            TempBinds.Clear();

            CEF.Browser.Render(Browser.IntTypes.ActionBox, false, false);

            if (cursor)
                Cursor.Show(false, false);

            TryInvokeAction(CurrentType, CurrentContext, ActionTypes.Close, args);

            CurrentType = Types.None;
            CurrentContext = Contexts.None;

            Player.LocalPlayer.ResetData("ActionBox::Temp::MinValue");
            Player.LocalPlayer.ResetData("ActionBox::Temp::MaxValue");
        }

        private static void Bind()
        {
            TempBinds.Add(KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(true)));
        }
    }
}
