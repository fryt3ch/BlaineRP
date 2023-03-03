using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Data.Jobs
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

            CEF.ActionBox.ShowSelect(CEF.ActionBox.Contexts.JobCollectorOrderSelect, Locale.Actions.JobVehicleOrderSelectTitle, activeOrders.Select(x => (counter++, string.Format(Locale.Actions.JobTruckerOrderText, counter, Math.Round(x.TargetBusiness.InfoColshape.Position.DistanceTo(Player.LocalPlayer.Position) / 1000f, 2), Math.Round(x.TargetBusiness.InfoColshape.Position.DistanceTo(Position) / 1000f, 2), Utils.GetPriceString(x.Reward)))).ToArray(), Locale.Actions.SelectOkBtn2, Locale.Actions.SelectCancelBtn1, Player.LocalPlayer.Vehicle);
        }
    }
}
