using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Data.Jobs
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
            Blip = new Blip(198, Position.Position, "Таксопарк", 1f, 5, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
        }

        public void ShowOrderSelection(List<OrderInfo> activeOrders)
        {
            if (activeOrders.Count == 0)
            {
                CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Notifications.DefHeader, Locale.Notifications.General.JobNoOrders);

                return;
            }

            var counter = 0;

            var pPos = Player.LocalPlayer.Position;

            var dict = new Dictionary<uint, float>();

            activeOrders = activeOrders.OrderBy(x => { var dist = pPos.DistanceTo(x.Position); dict.Add(x.Id, dist); return dist; }).ToList();

            CEF.ActionBox.ShowSelect(CEF.ActionBox.Contexts.JobCabbieOrderSelect, Locale.Actions.JobVehicleOrderSelectTitle, activeOrders.Select(x => (counter++, string.Format(Locale.Actions.JobCabbieOrderText, counter, Utils.GetStreetName(x.Position), Math.Round(dict.GetValueOrDefault(x.Id) / 1000f, 2)))).ToArray(), Locale.Actions.SelectOkBtn2, Locale.Actions.SelectCancelBtn1, Player.LocalPlayer.Vehicle);
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

            SetCurrentData("JVEH", RAGE.Elements.Entities.Vehicles.GetAtRemote((ushort)data[0].ToDecimal()));
        }

        public override void OnEndJob()
        {
            GetCurrentData<Blip>("Blip")?.Destroy();
            GetCurrentData<Additional.ExtraColshape>("CS")?.Destroy();

            base.OnEndJob();
        }
    }
}
