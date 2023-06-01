using BCRPClient.CEF;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace BCRPClient.Data
{
    public partial class Locations
    {
        public class MarketStall
        {
            public static uint RentPrice => Utils.ToUInt32(Sync.World.GetSharedData<object>("MARKETSTALL_RP", 0));

            public static List<string> SellHistory { get => Player.LocalPlayer.GetData<List<string>>("MarketStall::SH"); set { if (value == null) Player.LocalPlayer.ResetData("MarketStall::SH"); else Player.LocalPlayer.SetData("MarketStall::SH", value); } }

            public int Id { get; set; }

            public ushort CurrentRenterRID => Utils.ToUInt16(Sync.World.GetSharedData<object>($"MARKETSTALL_{Id}_R", ushort.MaxValue));

            public Utils.Vector4 Position { get; set; }

            public MarketStall(int Id, Utils.Vector4 Position)
            {
                this.Id = Id;

                this.Position = Position;

                var cs = new Additional.Sphere(new Vector3(Position.X, Position.Y, Position.Z), 2.5f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                {
                    ApproveType = Additional.ExtraColshape.ApproveTypes.OnlyByFoot,

                    ActionType = Additional.ExtraColshape.ActionTypes.MarketStallInteract,

                    InteractionType = Additional.ExtraColshape.InteractionTypes.MarketStallInteract,

                    Data = this,
                };

                Sync.World.AddDataHandler($"MARKETSTALL_{Id}_R", OnRenterRIDChanged);
            }

            public static MarketStall GetCurrentRentedMarketStall(out Additional.ExtraColshape colshape)
            {
                var rid = Player.LocalPlayer.RemoteId;

                colshape = Additional.ExtraColshape.All.Where(x => x.Data is MarketStall marketStall && marketStall.CurrentRenterRID == rid).FirstOrDefault();

                return (MarketStall)colshape?.Data;
            }

            private static void OnRenterRIDChanged(string key, object value, object oldValue)
            {
                var d = key.Split('_');

                var stallIdx = int.Parse(d[1]);

                var cs = Additional.ExtraColshape.All.Where(x => x.Data is MarketStall marketStall && marketStall.Id == stallIdx).FirstOrDefault();

                if (cs == null)
                    return;

                var marketStall = (MarketStall)cs.Data;

                var newRid = Utils.ToUInt16(value ?? ushort.MaxValue);
                var oldRid = Utils.ToUInt16(oldValue ?? ushort.MaxValue);

                if (newRid == Player.LocalPlayer.RemoteId)
                {
                    SellHistory = new List<string>();

                    var pos = new Vector3(cs.Position.X, cs.Position.Y, cs.Position.Z);

                    var subCs = new Additional.Sphere(pos, 20f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                    {
                        ApproveType = Additional.ExtraColshape.ApproveTypes.None,

                        OnExit = (cancel) =>
                        {
                            CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_WARN"), Locale.Get("MARKETSTALL_R_ODIST_0", 10));
                        },
                    };

                    var mainCs = new Additional.Sphere(pos, 30f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                    {
                        ApproveType = Additional.ExtraColshape.ApproveTypes.None,

                        OnExit = async (cancel) =>
                        {
                            if ((bool)await Events.CallRemoteProc("MarketStall::Close", marketStall.Id))
                                CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_WARN"), Locale.Get("MARKETSTALL_R_ODIST_1"));
                        },

                        Data = subCs,

                        Name = "MARKETSTALL_RENT_DIST_CS",
                    };
                }
                else if (oldRid == Player.LocalPlayer.RemoteId && newRid == ushort.MaxValue)
                {
                    var sellHistory = SellHistory;

                    SellHistory = null;

                    var distColshape = Additional.ExtraColshape.GetByName("MARKETSTALL_RENT_DIST_CS");

                    if (distColshape != null)
                    {
                        var subColshape = distColshape.Data as Additional.ExtraColshape;

                        subColshape?.Destroy();

                        distColshape.Destroy();
                    }
                }

                var isNear = Player.LocalPlayer.GetCoords(false).DistanceTo(cs.Position) <= 10f;

                if (cs.IsInside)
                {
                    cs.OnExit?.Invoke(null);
                    cs.OnEnter?.Invoke(null);
                }

                if (!isNear)
                    return;

                if (CEF.ActionBox.CurrentContextStr == $"MarketStallStartRent_{stallIdx}")
                {
                    CEF.ActionBox.Close(true);
                }
            }

            public static async void OnInteractionKeyPressed()
            {
                var marketStall = Player.LocalPlayer.GetData<MarketStall>("CurrentMarketStall");

                if (marketStall == null)
                    return;

                var currentRenterRid = marketStall.CurrentRenterRID;

                if (currentRenterRid == Player.LocalPlayer.RemoteId)
                {
                    if (Additional.ExtraColshape.LastSent.IsSpam(500, false, true))
                        return;

                    Additional.ExtraColshape.LastSent = Sync.World.ServerTime;

                    var res = ((string)await Events.CallRemoteProc("MarketStall::GMD", marketStall.Id))?.Split('_');

                    if (res == null)
                        return;

                    var _isStallLocked = res[0] == "1";

                    showManageMenu(_isStallLocked);

                    async void showManageMenu(bool isStallLocked)
                    {
                        var options = new List<(decimal, string)>();

                        options.Add((1, Locale.Get(isStallLocked ? "MARKETSTALL_MG_UNLOCK" : "MARKETSTALL_MG_LOCK")));
                        options.Add((2, Locale.Get("MARKETSTALL_MG_CHOOSE")));
                        options.Add((3, Locale.Get("MARKETSTALL_MG_SELLHIST")));

                        options.Add((255, Locale.Get("MARKETSTALL_MG_CLOSE")));

                        await CEF.ActionBox.ShowSelect
                        (
                            "MarketStallStartManage_0", Locale.Get("MARKETSTALL_MG_HEADER"), options.ToArray(), null, null,

                            CEF.ActionBox.DefaultBindAction,

                            async (CEF.ActionBox.ReplyTypes rType, decimal opt) =>
                            {
                                if (rType == ActionBox.ReplyTypes.Cancel)
                                {
                                    CEF.ActionBox.Close(true);

                                    return;
                                }

                                if (opt == 1)
                                {
                                    if (Additional.ExtraColshape.LastSent.IsSpam(500, false, true))
                                        return;

                                    Additional.ExtraColshape.LastSent = Sync.World.ServerTime;

                                    var res = Utils.ToByte(await Events.CallRemoteProc("MarketStall::Lock", marketStall.Id, !isStallLocked));

                                    if (res == 255)
                                        return;

                                    CEF.ActionBox.Close(false);

                                    showManageMenu(!isStallLocked);

                                    if (res == 1)
                                    {
                                        CEF.Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Get(isStallLocked ? "MARKETSTALL_MG_UNLOCKED" : "MARKETSTALL_MG_LOCKED"));
                                    }
                                }
                                else if (opt == 2)
                                {
                                    var items = new List<object>();

                                    for (int i = 0; i < CEF.Inventory.ItemsParams.Length; i++)
                                    {
                                        var x = CEF.Inventory.ItemsParams[i];

                                        if (x == null)
                                            continue;

                                        var y = (object[])CEF.Inventory.ItemsData[i][0];

                                        items.Add(new object[] { new object[] { i, y[0] }, y[1], 0, y[3], y[4], false });
                                    }

                                    if (items.Count == 0)
                                    {
                                        CEF.Notification.Show(Notification.Types.Error, Locale.Get("NOTIFICATION_HEADER_ERROR"), Locale.Get("MARKETSTALL_NOITEMS_SELL"));

                                        return;
                                    }

                                    if (Additional.ExtraColshape.LastSent.IsSpam(500, false, true))
                                        return;

                                    Additional.ExtraColshape.LastSent = Sync.World.ServerTime;

                                    CEF.ActionBox.Close(false);

                                    CEF.PlayerMarket.Show($"MARKETSTALL@SELLER_{marketStall.Id}", new object[] { items, });
                                }
                                else if (opt == 3)
                                {
                                    var sellHist = SellHistory;

                                    if (sellHist == null || sellHist.Count == 0)
                                    {
                                        CEF.Notification.Show(Notification.Types.Error, Locale.Get("NOTIFICATION_HEADER_ERROR"), Locale.Get("MARKETSTALL_MG_HISTEMPTY"));

                                        return;
                                    }
                                }
                                else if (opt == 255)
                                {
                                    if (Additional.ExtraColshape.LastSent.IsSpam(500, false, true))
                                        return;

                                    Additional.ExtraColshape.LastSent = Sync.World.ServerTime;

                                    var res = (bool)await Events.CallRemoteProc("MarketStall::Close", marketStall.Id);

                                    if (res)
                                    {
                                        CEF.ActionBox.Close(true);

                                        return;
                                    }
                                }
                            },

                            null
                        );
                    }
                }
                else if (currentRenterRid == ushort.MaxValue)
                {
                    await CEF.ActionBox.ShowMoney
                    (
                        $"MarketStallStartRent_{marketStall.Id}", Locale.Get("MARKETSTALL_R_HEADER"), Locale.Get("MARKETSTALL_R_CONTENT", Utils.GetPriceString(RentPrice)),

                        CEF.ActionBox.DefaultBindAction,

                        async (CEF.ActionBox.ReplyTypes rType) =>
                        {
                            var useCash = rType == ActionBox.ReplyTypes.OK;

                            if (useCash || rType == ActionBox.ReplyTypes.Cancel)
                            {
                                if (Additional.ExtraColshape.LastSent.IsSpam(1000, false, true))
                                    return;

                                Additional.ExtraColshape.LastSent = Sync.World.ServerTime;

                                var res = (bool)await Events.CallRemoteProc("MarketStall::Rent", marketStall.Id, useCash);

                                if (res)
                                {
                                    CEF.ActionBox.Close(true);

                                    var pos = Player.LocalPlayer.GetCoords(false);

                                    if (pos.DistanceTo(marketStall.Position.Position) <= 15f)
                                    {
                                        var newPos = RAGE.Game.Object.GetObjectOffsetFromCoords(marketStall.Position.X, marketStall.Position.Y, marketStall.Position.Z, marketStall.Position.RotationZ, 0f, -1f, +0.15f);

                                        Player.LocalPlayer.SetCoordsNoOffset(newPos.X, newPos.Y, newPos.Z, false, false, false);
                                        Player.LocalPlayer.SetHeading(marketStall.Position.RotationZ);
                                    }
                                }
                            }
                            else
                            {
                                CEF.ActionBox.Close(true);
                            }
                        },

                        null
                    );
                }
                else
                {

                }
            }
        }
    }
}