using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Data.Jobs
{
    public class Trucker : Job
    {
        public class OrderInfo
        {
            public uint Id { get; set; }

            public uint Reward { get; set; }

            public int MPIdx { get; set; }

            public Data.Locations.Business TargetBusiness { get; set; }

            public OrderInfo()
            {

            }
        }

        public List<Vector3> MaterialsPositions { get; set; }

        public Trucker(int Id, Utils.Vector4 Position, List<Vector3> MaterialsPositions) : base(Id, Types.Trucker)
        {
            this.MaterialsPositions = MaterialsPositions;

            var subId = SubId;

            if (subId == 0)
                JobGiver = new NPC($"job_{Id}_{subId}", "Кеннет", NPC.Types.Talkable, "ig_oneil", Position.Position, Position.RotationZ, Settings.MAIN_DIMENSION);

            JobGiver.SubName = Locale.General.NPC.TypeNames["job_trucker"];

            JobGiver.Data = this;

            JobGiver.DefaultDialogueId = "job_trucker_g_0";

            Blip = new Blip(477, Position.Position, "Грузоперевозки", 1f, 3, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
        }

        public override void OnStartJob(object[] data)
        {
            base.OnStartJob(data);

            var activeOrders = ((JArray)data[1]).ToObject<List<string>>().Select(x =>
            {
                var t = x.Split('_');

                var id = uint.Parse(t[0]);

                var business = Data.Locations.Business.All[int.Parse(t[1])];

                return new OrderInfo() { Id = id, TargetBusiness = business, MPIdx = int.Parse(t[2]), Reward = uint.Parse(t[3]) };
            }).OrderByDescending(x => x.Reward).ToList();

            SetCurrentData("AOL", activeOrders);

            SetCurrentData("JVEH", RAGE.Elements.Entities.Vehicles.GetAtRemote((ushort)data[0].ToDecimal()));
        }

        public override void OnEndJob()
        {
            base.OnEndJob();
        }

        public void ShowOrderSelection(List<OrderInfo> activeOrders)
        {
            if (activeOrders.Count == 0)
            {
                CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Notifications.DefHeader, Locale.Notifications.General.JobNoOrders);

                return;
            }

            var counter = 0;

            CEF.ActionBox.ShowSelect(CEF.ActionBox.Contexts.JobTruckerOrderSelect, Locale.Actions.JobVehicleOrderSelectTitle, activeOrders.Select(x => (counter++, string.Format(Locale.Actions.JobTruckerOrderText, counter, Math.Round(MaterialsPositions[x.MPIdx].DistanceTo(Player.LocalPlayer.Position) / 1000f, 2), Math.Round(MaterialsPositions[x.MPIdx].DistanceTo(x.TargetBusiness.InfoColshape.Position) / 1000f, 2), Utils.GetPriceString(x.Reward)))).ToArray(), Locale.Actions.SelectOkBtn2, Locale.Actions.SelectCancelBtn1, Player.LocalPlayer.Vehicle);
        }
    }
}
