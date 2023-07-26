using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.Businesses;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Quests;
using BlaineRP.Client.Game.UI.CEF;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Jobs
{
    public class Trucker : Job
    {
        public Trucker(int Id, Utils.Vector4 Position, List<Vector3> MaterialsPositions) : base(Id, JobTypes.Trucker)
        {
            this.MaterialsPositions = MaterialsPositions;

            int subId = SubId;

            if (subId == 0)
                JobGiver = new NPCs.NPC($"job_{Id}_{subId}",
                    "Кеннет",
                    NPCs.NPC.Types.Talkable,
                    "ig_oneil",
                    Position.Position,
                    Position.RotationZ,
                    Settings.App.Static.MainDimension
                );

            JobGiver.SubName = "NPC_SUBNAME_JOB_TRUCK_BOSS";

            JobGiver.Data = this;

            JobGiver.DefaultDialogueId = "job_trucker_g_0";

            var blip = new ExtraBlip(477, Position.Position, "Грузоперевозки", 1f, 3, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);
        }

        public List<Vector3> MaterialsPositions { get; set; }

        public override void OnStartJob(object[] data)
        {
            base.OnStartJob(data);

            var activeOrders = ((JArray)data[1]).ToObject<List<string>>()
                                                .Select(x =>
                                                     {
                                                         string[] t = x.Split('_');

                                                         var id = uint.Parse(t[0]);

                                                         Business business = Business.All[int.Parse(t[1])];

                                                         return new OrderInfo()
                                                         {
                                                             Id = id,
                                                             TargetBusiness = business,
                                                             MPIdx = int.Parse(t[2]),
                                                             Reward = uint.Parse(t[3]),
                                                         };
                                                     }
                                                 )
                                                .OrderByDescending(x => x.Reward)
                                                .ToList();

            SetCurrentData("AOL", activeOrders);

            SetCurrentData("JVEH", Entities.Vehicles.GetAtRemote(Utils.Convert.ToUInt16(data[0])));
        }

        public override void OnEndJob()
        {
            base.OnEndJob();
        }

        public async void ShowOrderSelection(List<OrderInfo> activeOrders)
        {
            if (activeOrders.Count == 0)
            {
                Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Notifications.General.JobNoOrders);

                return;
            }

            var counter = 0;

            Vehicle vehicle = Player.LocalPlayer.Vehicle;

            await ActionBox.ShowSelect("JobTruckerOrderSelect",
                Locale.Actions.JobVehicleOrderSelectTitle,
                activeOrders.Select(x => ((decimal)counter++,
                                 string.Format(Locale.Actions.JobTruckerOrderText,
                                     counter,
                                     Math.Round(MaterialsPositions[x.MPIdx].DistanceTo(Player.LocalPlayer.Position) / 1000f, 2),
                                     Math.Round(MaterialsPositions[x.MPIdx].DistanceTo(x.TargetBusiness.InfoColshape.Position) / 1000f, 2),
                                     Locale.Get("GEN_MONEY_0", x.Reward)
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
                        var quest = Quest.GetPlayerQuest(pData, QuestTypes.JTR1);

                        if (quest == null)
                            return;

                        List<OrderInfo> orders = pData.CurrentJob?.GetCurrentData<List<OrderInfo>>("AOL");

                        if (orders == null)
                            return;

                        if (id >= orders.Count)
                            return;

                        byte res = await quest.CallProgressUpdateProc(orders[id].Id);

                        if (res == byte.MaxValue)
                        {
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

        public class OrderInfo
        {
            public OrderInfo()
            {
            }

            public uint Id { get; set; }

            public uint Reward { get; set; }

            public int MPIdx { get; set; }

            public Business TargetBusiness { get; set; }
        }
    }
}