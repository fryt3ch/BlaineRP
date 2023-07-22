using BlaineRP.Client.CEF;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Client.Data.Jobs
{
    public class Cabbie : Job
    {
        public class OrderInfo
        {
            public uint Id { get; set; }

            public Vector3 Position { get; set; }

            public OrderInfo()
            {

            }
        }

        public Cabbie(int Id, Utils.Vector4 Position) : base(Id, Types.Cabbie)
        {
            Blip = new Additional.ExtraBlip(198, Position.Position, "Таксопарк", 1f, 5, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);
        }

        public async void ShowOrderSelection(List<OrderInfo> activeOrders)
        {
            if (activeOrders.Count == 0)
            {
                CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Notifications.General.JobNoOrders);

                return;
            }

            var counter = 0;

            var pPos = Player.LocalPlayer.Position;

            var dict = new Dictionary<uint, float>();

            activeOrders = activeOrders.OrderBy(x => { var dist = pPos.DistanceTo(x.Position); dict.Add(x.Id, dist); return dist; }).ToList();

            var vehicle = Player.LocalPlayer.Vehicle;

            await CEF.ActionBox.ShowSelect
            (
                "JobCabbieOrderSelect", Locale.Actions.JobVehicleOrderSelectTitle, activeOrders.Select(x => ((decimal)counter++, string.Format(Locale.Actions.JobCabbieOrderText, counter, Utils.GetStreetName(x.Position), Math.Round(dict.GetValueOrDefault(x.Id) / 1000f, 2)))).ToArray(), Locale.Actions.SelectOkBtn2, Locale.Actions.SelectCancelBtn1,

                () =>
                {
                    CEF.ActionBox.DefaultBindAction.Invoke();

                    var checkAction = new Action(() =>
                    {
                        if (Player.LocalPlayer.Vehicle != vehicle || vehicle?.Exists != true || vehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                            CEF.ActionBox.Close(true);
                    });

                    Player.LocalPlayer.SetData("ActionBox::Temp::JVRVA", checkAction);

                    GameEvents.Update -= checkAction.Invoke;
                    GameEvents.Update += checkAction.Invoke;
                },

                async (rType, idD) =>
                {
                    var id = (int)idD;

                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (rType == CEF.ActionBox.ReplyTypes.OK)
                    {
                        var orders = pData.CurrentJob?.GetCurrentData<List<Data.Jobs.Cabbie.OrderInfo>>("AOL");

                        if (orders == null)
                            return;

                        if (id >= orders.Count)
                            return;

                        var order = orders[id];

                        var res = Utils.ToByte(await Events.CallRemoteProc("Job::CAB::TO", order.Id));

                        if (res == byte.MaxValue)
                        {
                            if (pData.CurrentJob is Data.Jobs.Cabbie cabbieJob)
                            {
                                CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Notifications.General.Taxi5);

                                cabbieJob.SetCurrentData("CO", order);

                                var pos = new Vector3(order.Position.X, order.Position.Y, order.Position.Z);

                                pos.Z -= 1f;

                                var blip = new Additional.ExtraBlip(280, pos, Locale.General.Blip.JobTaxiTargetPlayer, 1f, 5, 255, 0f, false, 0, 0f, Settings.App.Static.MainDimension);

                                blip.SetRoute(true);

                                var colshape = new Additional.Circle(pos, Settings.App.Static.TAXI_ORDER_MAX_WAIT_RANGE, true, new Utils.Colour(255, 0, 0, 125), Settings.App.Static.MainDimension, null)
                                {
                                    ApproveType = Additional.ExtraColshape.ApproveTypes.OnlyServerVehicleDriver,

                                    OnEnter = (cancel) =>
                                    {
                                        var jobVehicle = cabbieJob.GetCurrentData<Vehicle>("JVEH");

                                        if (jobVehicle == null || Player.LocalPlayer.Vehicle != jobVehicle)
                                        {
                                            CEF.Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                                            return;
                                        }

                                        Events.CallRemote("Job::CAB::OS", order.Id);
                                    }
                                };

                                cabbieJob.SetCurrentData("Blip", blip);
                                cabbieJob.SetCurrentData("CS", colshape);
                            }

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
                        GameEvents.Update -= checkAction.Invoke;

                        Player.LocalPlayer.ResetData("ActionBox::Temp::JVRVA");
                    }
                }
            );
        }

        public override void OnStartJob(object[] data)
        {
            base.OnStartJob(data);

            var activeOrders = ((JArray)data[1]).ToObject<List<string>>().Select(x =>
            {
                var t = x.Split('_');

                var id = uint.Parse(t[0]);

                return new OrderInfo() { Id = id, Position = new Vector3(float.Parse(t[1]), float.Parse(t[2]), float.Parse(t[3])) };
            }).ToList();

            SetCurrentData("AOL", activeOrders);

            SetCurrentData("JVEH", RAGE.Elements.Entities.Vehicles.GetAtRemote(Convert.ToUInt16(data[0])));
        }

        public override void OnEndJob()
        {
            GetCurrentData<Additional.ExtraBlip>("Blip")?.Destroy();
            GetCurrentData<Additional.ExtraColshape>("CS")?.Destroy();

            base.OnEndJob();
        }
    }
}
