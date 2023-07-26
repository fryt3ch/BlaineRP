using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.Items;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Misc
{
    public partial class MarketStall
    {
        public MarketStall(int Id, Vector4 Position)
        {
            this.Id = Id;

            this.Position = Position;

            var cs = new Sphere(new Vector3(Position.X, Position.Y, Position.Z), 2.5f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
            {
                ApproveType = ApproveTypes.OnlyByFoot,
                ActionType = ActionTypes.MarketStallInteract,
                InteractionType = InteractionTypes.MarketStallInteract,
                Data = this,
            };

            World.Core.AddDataHandler($"MARKETSTALL_{Id}_R", OnRenterRIDChanged);
        }

        public static List<(uint, string, decimal, decimal)> SellHistory
        {
            get => Player.LocalPlayer.GetData<List<(uint, string, decimal, decimal)>>("MarketStall::SH");
            set
            {
                if (value == null)
                    Player.LocalPlayer.ResetData("MarketStall::SH");
                else
                    Player.LocalPlayer.SetData("MarketStall::SH", value);
            }
        }

        public int Id { get; set; }

        public ushort CurrentRenterRID => Convert.ToUInt16(World.Core.GetSharedData<object>($"MARKETSTALL_{Id}_R", ushort.MaxValue));

        public Vector4 Position { get; set; }

        public static MarketStall GetCurrentRentedMarketStall(out ExtraColshape colshape)
        {
            ushort rid = Player.LocalPlayer.RemoteId;

            colshape = ExtraColshape.All.Where(x => x.Data is MarketStall marketStall && marketStall.CurrentRenterRID == rid).FirstOrDefault();

            return (MarketStall)colshape?.Data;
        }

        private static void OnRenterRIDChanged(string key, object value, object oldValue)
        {
            string[] d = key.Split('_');

            var stallIdx = int.Parse(d[1]);

            ExtraColshape cs = ExtraColshape.All.Where(x => x.Data is MarketStall marketStall && marketStall.Id == stallIdx).FirstOrDefault();

            if (cs == null)
                return;

            var marketStall = (MarketStall)cs.Data;

            var newRid = Convert.ToUInt16(value ?? ushort.MaxValue);
            var oldRid = Convert.ToUInt16(oldValue ?? ushort.MaxValue);

            if (newRid == Player.LocalPlayer.RemoteId)
            {
                SellHistory = new List<(uint, string, decimal, decimal)>();

                var pos = new Vector3(cs.Position.X, cs.Position.Y, cs.Position.Z);

                ExtraColshape subCs = null;
                ExtraColshape mainCs = null;

                subCs = new Sphere(pos, 20f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                {
                    ApproveType = ApproveTypes.None,
                    OnExit = (cancel) =>
                    {
                        if (subCs?.Exists != true)
                            return;

                        Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_WARN"), Locale.Get("MARKETSTALL_R_ODIST_0", 10));
                    },
                };

                mainCs = new Sphere(pos, 30f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                {
                    ApproveType = ApproveTypes.None,
                    OnExit = async (cancel) =>
                    {
                        if (mainCs?.Exists != true)
                            return;

                        if ((bool)await RAGE.Events.CallRemoteProc("MarketStall::Close", marketStall.Id))
                            Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_WARN"), Locale.Get("MARKETSTALL_R_ODIST_1"));
                    },
                    Data = subCs,
                    Name = "MARKETSTALL_RENT_DIST_CS",
                };
            }
            else if (oldRid == Player.LocalPlayer.RemoteId && newRid == ushort.MaxValue)
            {
                List<(uint, string, decimal, decimal)> sellHistory = SellHistory;

                SellHistory = null;

                var distColshape = ExtraColshape.GetByName("MARKETSTALL_RENT_DIST_CS");

                if (distColshape != null)
                {
                    var subColshape = distColshape.Data as ExtraColshape;

                    subColshape?.Destroy();

                    distColshape.Destroy();
                }
            }

            bool isNear = Player.LocalPlayer.GetCoords(false).DistanceTo(cs.Position) <= 10f;

            if (cs.IsInside)
            {
                cs.OnExit?.Invoke(null);
                cs.OnEnter?.Invoke(null);
            }

            if (!isNear)
                return;

            if (ActionBox.CurrentContextStr == $"MarketStallStartRent_{stallIdx}")
                ActionBox.Close(true);
        }

        public static async void OnInteractionKeyPressed()
        {
            MarketStall marketStall = Player.LocalPlayer.GetData<MarketStall>("CurrentMarketStall");

            if (marketStall == null)
                return;

            ushort currentRenterRid = marketStall.CurrentRenterRID;

            if (currentRenterRid == Player.LocalPlayer.RemoteId)
            {
                if (ExtraColshape.LastSent.IsSpam(500, false, true))
                    return;

                ExtraColshape.LastSent = World.Core.ServerTime;

                string[] res = ((string)await RAGE.Events.CallRemoteProc("MarketStall::GMD", marketStall.Id))?.Split('_');

                if (res == null)
                    return;

                bool _isStallLocked = res[0] == "1";

                showManageMenu(_isStallLocked);

                async void showManageMenu(bool isStallLocked)
                {
                    var options = new List<(decimal, string)>();

                    options.Add((1, Locale.Get(isStallLocked ? "MARKETSTALL_MG_UNLOCK" : "MARKETSTALL_MG_LOCK")));
                    options.Add((2, Locale.Get("MARKETSTALL_MG_CHOOSE")));
                    options.Add((3, Locale.Get("MARKETSTALL_MG_SELLHIST")));
                    //options.Add((77, Locale.Get("MARKETSTALL_DEBUG_SHOW")));

                    options.Add((255, Locale.Get("MARKETSTALL_MG_CLOSE")));

                    await ActionBox.ShowSelect("MarketStallStartManage_0",
                        Locale.Get("MARKETSTALL_MG_HEADER"),
                        options.ToArray(),
                        null,
                        null,
                        ActionBox.DefaultBindAction,
                        async (ActionBox.ReplyTypes rType, decimal opt) =>
                        {
                            if (rType == ActionBox.ReplyTypes.Cancel)
                            {
                                ActionBox.Close(true);

                                return;
                            }

                            if (opt == 1)
                            {
                                if (ExtraColshape.LastSent.IsSpam(500, false, true))
                                    return;

                                ExtraColshape.LastSent = World.Core.ServerTime;

                                var res = Convert.ToByte(await RAGE.Events.CallRemoteProc("MarketStall::Lock", marketStall.Id, !isStallLocked));

                                if (res == 255)
                                    return;

                                ActionBox.Close(false);

                                showManageMenu(!isStallLocked);

                                if (res == 1)
                                    Notification.Show(Notification.Types.Success,
                                        Locale.Get("NOTIFICATION_HEADER_DEF"),
                                        Locale.Get(isStallLocked ? "MARKETSTALL_MG_UNLOCKED" : "MARKETSTALL_MG_LOCKED")
                                    );
                            }
                            else if (opt == 2)
                            {
                                var items = new List<object>();

                                for (var i = 0; i < Inventory.ItemsParams.Length; i++)
                                {
                                    Inventory.ItemParams x = Inventory.ItemsParams[i];

                                    if (x == null)
                                        continue;

                                    if (x.InUse)
                                        continue;

                                    var y = (object[])Inventory.ItemsData[i][0];

                                    items.Add(new object[]
                                        {
                                            new object[]
                                            {
                                                i,
                                                y[0],
                                            },
                                            (string)y[1],
                                            0,
                                            y[3],
                                            y[4],
                                            null,
                                            null,
                                        }
                                    );
                                }

                                if (items.Count == 0)
                                {
                                    Notification.ShowError(Locale.Get("MARKETSTALL_NOITEMS_SELL"));

                                    return;
                                }

                                if (ExtraColshape.LastSent.IsSpam(500, false, true))
                                    return;

                                ExtraColshape.LastSent = World.Core.ServerTime;

                                List<string> res = (await RAGE.Events.CallRemoteProc("MarketStall::OSIM", marketStall.Id) as JArray)?.ToObject<List<string>>();

                                if (res == null)
                                    return;

                                foreach (string x in res)
                                {
                                    string[] d = x.Split('_');

                                    var idx = int.Parse(d[0]);

                                    object t = items.Where(x => (int)((object[])((object[])x)[0])[0] == idx).FirstOrDefault();

                                    if (t == null)
                                        continue;

                                    var amount = int.Parse(d[1]);
                                    var price = decimal.Parse(d[2]);

                                    ((object[])t)[6] = new object[]
                                    {
                                        price,
                                        amount,
                                    };
                                }

                                ActionBox.Close(false);

                                PlayerMarket.Show($"MARKETSTALL@SELLER_{marketStall.Id}",
                                    new object[]
                                    {
                                        items,
                                    }
                                );
                            }
                            else if (opt == 3)
                            {
                                List<(uint, string, decimal, decimal)> sellHist = SellHistory;

                                if (sellHist == null || sellHist.Count == 0)
                                {
                                    Notification.ShowError(Locale.Get("MARKETSTALL_MG_HISTEMPTY"));

                                    return;
                                }

                                var strings = new List<string>();

                                var totalEarned = 0m;

                                for (var i = 0; i < sellHist.Count; i++)
                                {
                                    (uint, string, decimal, decimal) x = sellHist[i];

                                    totalEarned += x.Item4;

                                    strings.Add(Locale.Get("MARKETSTALL_SH_0", i + 1, Items.Core.GetName(x.Item2), x.Item3, Locale.Get("GEN_MONEY_0", x.Item4)));
                                }

                                strings.Add("\n\n" + Locale.Get("MARKETSTALL_SH_1", Locale.Get("GEN_MONEY_0", totalEarned)));

                                ActionBox.Close(false);

                                UI.CEF.Note.ShowRead($"MARKETSTALL@SELLER_SELLHIST_{marketStall.Id}", string.Join("\n\n", strings), UI.CEF.Note.DefaultBindAction, null);
                            }
                            else if (opt == 255)
                            {
                                if (ExtraColshape.LastSent.IsSpam(500, false, true))
                                    return;

                                ExtraColshape.LastSent = World.Core.ServerTime;

                                var res = (bool)await RAGE.Events.CallRemoteProc("MarketStall::Close", marketStall.Id);

                                if (res)
                                {
                                    ActionBox.Close(true);

                                    return;
                                }
                            }
                            else if (opt == 77)
                            {
                                ActionBox.Close(false);

                                ShowGoods(marketStall);
                            }
                        },
                        null
                    );
                }
            }
            else if (currentRenterRid == ushort.MaxValue)
            {
                uint rentPrice = await GetRentPrice();

                await ActionBox.ShowMoney($"MarketStallStartRent_{marketStall.Id}",
                    Locale.Get("MARKETSTALL_R_HEADER"),
                    Locale.Get("MARKETSTALL_R_CONTENT", Locale.Get("GEN_MONEY_0", rentPrice)),
                    ActionBox.DefaultBindAction,
                    async (ActionBox.ReplyTypes rType) =>
                    {
                        bool useCash = rType == ActionBox.ReplyTypes.OK;

                        if (useCash || rType == ActionBox.ReplyTypes.Cancel)
                        {
                            if (ExtraColshape.LastSent.IsSpam(1000, false, true))
                                return;

                            ExtraColshape.LastSent = World.Core.ServerTime;

                            var res = (bool)await RAGE.Events.CallRemoteProc("MarketStall::Rent", marketStall.Id, useCash);

                            if (res)
                            {
                                ActionBox.Close(true);

                                Vector3 pos = Player.LocalPlayer.GetCoords(false);

                                if (pos.DistanceTo(marketStall.Position.Position) <= 15f)
                                {
                                    Vector3 newPos = RAGE.Game.Object.GetObjectOffsetFromCoords(marketStall.Position.X,
                                        marketStall.Position.Y,
                                        marketStall.Position.Z,
                                        marketStall.Position.RotationZ,
                                        0f,
                                        -1f,
                                        +0.15f
                                    );

                                    Player.LocalPlayer.SetCoordsNoOffset(newPos.X, newPos.Y, newPos.Z, false, false, false);
                                    Player.LocalPlayer.SetHeading(marketStall.Position.RotationZ);
                                }
                            }
                        }
                        else
                        {
                            ActionBox.Close(true);
                        }
                    },
                    null
                );
            }
            else
            {
                if (ExtraColshape.LastSent.IsSpam(1000, false, true))
                    return;

                ExtraColshape.LastSent = World.Core.ServerTime;

                ShowGoods(marketStall);
            }
        }

        private static async void ShowGoods(MarketStall marketStall)
        {
            object res = await RAGE.Events.CallRemoteProc("MarketStall::Show", marketStall.Id);

            if (res == null)
                return;

            List<string> resList = (res as JArray)?.ToObject<List<string>>();

            if (resList == null || resList.Count == 0)
            {
                Notification.ShowError(Locale.Get("MARKETSTALL_NOITEMS_BUY"));

                return;
            }

            var items = new List<object>();

            foreach (string x in resList)
            {
                string[] d = x.Split('&');

                string id = d[1];

                var weight = float.Parse(d[2]);
                var price = decimal.Parse(d[4]);
                var amount = uint.Parse(d[5]);

                string tag = d[3];

                System.Type iType = Items.Core.GetType(id, true);

                string iName = Items.Core.GetNameWithTag(id, iType, tag, out _);

                items.Add(new object[]
                    {
                        new object[]
                        {
                            uint.Parse(d[0]),
                            Items.Core.GetImageId(id, iType),
                        },
                        iName,
                        price,
                        amount,
                        weight,
                        typeof(Clothes).IsAssignableFrom(iType) ? Locale.Get("SHOP_RET_DRESS_L") : null,
                    }
                );
            }

            Player seller = Entities.Players.GetAtRemote(marketStall.CurrentRenterRID);

            string sellerName = Players.GetPlayerName(seller, true, false, true);

            PlayerMarket.Show($"MARKETSTALL@BUYER_{marketStall.Id}",
                new object[]
                {
                    items,
                    sellerName,
                }
            );
        }

        public static async System.Threading.Tasks.Task<uint> GetRentPrice()
        {
            object res = await World.Core.GetRetrievableData<object>("MARKETSTALL_RP", 0);

            return Convert.ToUInt32(res);
        }
    }
}