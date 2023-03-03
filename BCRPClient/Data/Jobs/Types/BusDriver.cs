using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Data.Jobs
{
    public class BusDriver : Job
    {
        public List<(uint Reward, List<Vector3> Positions)> Routes { get; set; }

        public BusDriver(int Id, Utils.Vector4 Position, List<(uint, List<Vector3>)> Routes) : base(Id, Types.BusDriver)
        {
            Blip = new Blip(513, Position.Position, "Автовокзал", 1f, 2, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

            this.Routes = Routes;
        }

        public override void OnStartJob(object[] data)
        {
            base.OnStartJob(data);

            SetCurrentData("JVEH", RAGE.Elements.Entities.Vehicles.GetAtRemote((ushort)data[0].ToDecimal()));
        }

        public override void OnEndJob()
        {
            base.OnEndJob();
        }

        public void ShowRouteSelection()
        {
            if (Routes.Count == 0)
                return;

            var counter = 0;

            var dict = new Dictionary<int, float>();

            for (int i = 0; i < Routes.Count; i++)
            {
                dict.Add(i, 0f);

                for (int j = 0; j < Routes[i].Positions.Count - 1; j++)
                {
                    dict[i] += Routes[i].Positions[j].DistanceTo(Routes[i].Positions[j + 1]);
                }
            }

            CEF.ActionBox.ShowSelect(CEF.ActionBox.Contexts.JobBusDriverRouteSelect, Locale.Actions.JobVehicleRouteSelectTitle, Routes.Select(x => (counter, string.Format(Locale.Actions.JobBusDriverRouteText, counter + 1, Utils.GetPriceString(x.Reward), Math.Round(dict[counter++] / 1000f, 2)))).ToArray(), Locale.Actions.SelectOkBtn2, Locale.Actions.SelectCancelBtn1, Player.LocalPlayer.Vehicle);
        }
    }
}