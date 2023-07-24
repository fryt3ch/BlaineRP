using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.EntitiesData;
using BlaineRP.Client.EntitiesData.Enums;
using BlaineRP.Client.Quests;
using BlaineRP.Client.Quests.Enums;
using BlaineRP.Client.Sync;

namespace BlaineRP.Client.Data.Jobs
{
    public class Collector : Job
    {
        public class OrderInfo
        {
            public uint Id { get; set; }

            public uint Reward { get; set; }

            public Data.Locations.Business TargetBusiness { get; set; }

            public OrderInfo()
            {

            }
        }

        public int BankId { get; set; }

        public Vector3 Position { get; set; }

        public Collector(int Id, Utils.Vector4 Position, int BankId) : base(Id, Types.Collector)
        {
            this.Position = Position.Position;

            this.BankId = BankId;
        }

        public override void OnStartJob(object[] data)
        {
            base.OnStartJob(data);

            var activeOrders = ((JArray)data[1]).ToObject<List<string>>().Select(x =>
            {
                var t = x.Split('_');

                var id = uint.Parse(t[0]);

                var business = Data.Locations.Business.All[int.Parse(t[1])];

                return new OrderInfo() { Id = id, TargetBusiness = business, Reward = uint.Parse(t[2]) };
            }).OrderByDescending(x => x.Reward).ToList();

            SetCurrentData("AOL", activeOrders);

            SetCurrentData("JVEH", RAGE.Elements.Entities.Vehicles.GetAtRemote(Utils.Convert.ToUInt16(data[0])));
        }

        public override void OnEndJob()
        {
            base.OnEndJob();
        }

        public async void ShowOrderSelection(List<OrderInfo> activeOrders)
        {
            if (activeOrders.Count == 0)
            {
                CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Notifications.General.JobNoOrders);

                return;
            }

            var counter = 0;

            var vehicle = Player.LocalPlayer.Vehicle;

            await CEF.ActionBox.ShowSelect
            (
                "JobCollectorOrderSelect", Locale.Actions.JobVehicleOrderSelectTitle, activeOrders.Select(x => ((decimal)counter++, string.Format(Locale.Actions.JobTruckerOrderText, counter, System.Math.Round(x.TargetBusiness.InfoColshape.Position.DistanceTo(Player.LocalPlayer.Position) / 1000f, 2), System.Math.Round(x.TargetBusiness.InfoColshape.Position.DistanceTo(Position) / 1000f, 2), Locale.Get("GEN_MONEY_0", x.Reward)))).ToArray(), Locale.Actions.SelectOkBtn2, Locale.Actions.SelectCancelBtn1,

                () =>
                {
                    CEF.ActionBox.DefaultBindAction.Invoke();

                    var checkAction = new Action(() =>
                    {
                        if (Player.LocalPlayer.Vehicle != vehicle || vehicle?.Exists != true || vehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                            CEF.ActionBox.Close(true);
                    });

                    Player.LocalPlayer.SetData("ActionBox::Temp::JVRVA", checkAction);

                    Main.Update -= checkAction.Invoke;
                    Main.Update += checkAction.Invoke;
                },

                async (rType, idD) =>
                {
                    var id = (int)idD;

                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (rType == CEF.ActionBox.ReplyTypes.OK)
                    {
                        var quest = Quest.GetPlayerQuest(pData, QuestTypes.JCL1);

                        if (quest == null)
                            return;

                        var orders = pData.CurrentJob?.GetCurrentData<List<Data.Jobs.Collector.OrderInfo>>("AOL");

                        if (orders == null)
                            return;

                        if (id >= orders.Count)
                            return;

                        var res = await quest.CallProgressUpdateProc(orders[id].Id);

                        if (res == byte.MaxValue)
                        {
                            CEF.ActionBox.Close(true);
                        }
                        else
                        {
                            if (res == 2)
                            {
                                CEF.Notification.ShowError(Locale.Notifications.General.JobOrderAlreadyTaken);
                            }
                            else
                            {
                                CEF.Notification.ShowError(Locale.Notifications.General.JobOrderTakeError);
                            }
                        }
                    }
                    else if (rType == CEF.ActionBox.ReplyTypes.Cancel)
                    {
                        CEF.ActionBox.Close(true);
                    }
                    else
                        return;
                },

                () =>
                {
                    var checkAction = Player.LocalPlayer.GetData<Action>("ActionBox::Temp::JVRVA");

                    if (checkAction != null)
                    {
                        Main.Update -= checkAction.Invoke;

                        Player.LocalPlayer.ResetData("ActionBox::Temp::JVRVA");
                    }
                }
            );
        }
    }
}
