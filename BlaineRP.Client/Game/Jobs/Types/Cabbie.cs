using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Players;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.UI.CEF;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Jobs
{
    public partial class Cabbie : Job
    {
        public Cabbie(int Id, Utils.Vector4 Position) : base(Id, JobType.Cabbie)
        {
            Blip = new ExtraBlip(198, Position.Position, "Таксопарк", 1f, 5, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);
        }

        public async void ShowOrderSelection(List<OrderInfo> activeOrders)
        {
            if (activeOrders.Count == 0)
            {
                Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Notifications.General.JobNoOrders);

                return;
            }

            var counter = 0;

            Vector3 pPos = Player.LocalPlayer.Position;

            var dict = new Dictionary<uint, float>();

            activeOrders = activeOrders.OrderBy(x =>
                                            {
                                                float dist = pPos.DistanceTo(x.Position);
                                                dict.Add(x.Id, dist);
                                                return dist;
                                            }
                                        )
                                       .ToList();

            Vehicle vehicle = Player.LocalPlayer.Vehicle;

            await ActionBox.ShowSelect("JobCabbieOrderSelect",
                Locale.Actions.JobVehicleOrderSelectTitle,
                activeOrders.Select(x => ((decimal)counter++,
                                 string.Format(Locale.Actions.JobCabbieOrderText,
                                     counter,
                                     Utils.Game.Misc.GetStreetName(x.Position),
                                     Math.Round(dict.GetValueOrDefault(x.Id) / 1000f, 2)
                                 ))
                             )
                            .ToArray(),
                Locale.Actions.SelectOkBtn2,
                Locale.Actions.SelectCancelBtn1,
                () =>
                {
                    ActionBox.DefaultBindAction.Invoke();

                    var checkAction = new Action(() =>
                        {
                            if (Player.LocalPlayer.Vehicle != vehicle || vehicle?.Exists != true || vehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                ActionBox.Close(true);
                        }
                    );

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

                    if (rType == ActionBox.ReplyTypes.OK)
                    {
                        List<OrderInfo> orders = pData.CurrentJob?.GetCurrentData<List<OrderInfo>>("AOL");

                        if (orders == null)
                            return;

                        if (id >= orders.Count)
                            return;

                        OrderInfo order = orders[id];

                        var res = Utils.Convert.ToByte(await RAGE.Events.CallRemoteProc("Job::CAB::TO", order.Id));

                        if (res == byte.MaxValue)
                        {
                            if (pData.CurrentJob is Cabbie cabbieJob)
                            {
                                Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Notifications.General.Taxi5);

                                cabbieJob.SetCurrentData("CO", order);

                                var pos = new Vector3(order.Position.X, order.Position.Y, order.Position.Z);

                                pos.Z -= 1f;

                                var blip = new ExtraBlip(280, pos, Locale.General.Blip.JobTaxiTargetPlayer, 1f, 5, 255, 0f, false, 0, 0f, Settings.App.Static.MainDimension);

                                blip.SetRoute(true);

                                var colshape = new Circle(pos,
                                    Settings.App.Static.TAXI_ORDER_MAX_WAIT_RANGE,
                                    true,
                                    new Utils.Colour(255, 0, 0, 125),
                                    Settings.App.Static.MainDimension,
                                    null
                                )
                                {
                                    ApproveType = ApproveTypes.OnlyServerVehicleDriver,
                                    OnEnter = (cancel) =>
                                    {
                                        Vehicle jobVehicle = cabbieJob.GetCurrentData<Vehicle>("JVEH");

                                        if (jobVehicle == null || Player.LocalPlayer.Vehicle != jobVehicle)
                                        {
                                            Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                                            return;
                                        }

                                        RAGE.Events.CallRemote("Job::CAB::OS", order.Id);
                                    },
                                };

                                cabbieJob.SetCurrentData("Blip", blip);
                                cabbieJob.SetCurrentData("CS", colshape);
                            }

                            ActionBox.Close(true);
                        }
                        else
                        {
                            if (res == 2)
                                Notification.ShowError(Locale.Notifications.General.JobOrderAlreadyTaken);
                            else
                                Notification.ShowError(Locale.Notifications.General.JobOrderTakeError);
                        }
                    }
                    else if (rType == ActionBox.ReplyTypes.Cancel)
                    {
                        ActionBox.Close(true);
                    }
                    else
                    {
                        return;
                    }
                },
                () =>
                {
                    Action checkAction = Player.LocalPlayer.GetData<Action>("ActionBox::Temp::JVRVA");

                    if (checkAction != null)
                    {
                        Main.Update -= checkAction.Invoke;

                        Player.LocalPlayer.ResetData("ActionBox::Temp::JVRVA");
                    }
                }
            );
        }

        public override void OnStartJob(object[] data)
        {
            base.OnStartJob(data);

            var activeOrders = ((JArray)data[1]).ToObject<List<string>>()
                                                .Select(x =>
                                                     {
                                                         string[] t = x.Split('_');

                                                         var id = uint.Parse(t[0]);

                                                         return new OrderInfo()
                                                         {
                                                             Id = id,
                                                             Position = new Vector3(float.Parse(t[1]), float.Parse(t[2]), float.Parse(t[3])),
                                                         };
                                                     }
                                                 )
                                                .ToList();

            SetCurrentData("AOL", activeOrders);

            SetCurrentData("JVEH", Entities.Vehicles.GetAtRemote(Utils.Convert.ToUInt16(data[0])));
        }

        public override void OnEndJob()
        {
            GetCurrentData<ExtraBlip>("Blip")?.Destroy();
            GetCurrentData<ExtraColshape>("CS")?.Destroy();

            base.OnEndJob();
        }
    }
}