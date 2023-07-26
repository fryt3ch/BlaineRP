using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Quests;
using BlaineRP.Client.Game.UI.CEF;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Jobs
{
    public class BusDriver : Job
    {
        public BusDriver(int Id, Utils.Vector4 Position, List<(uint, List<Vector3>)> Routes) : base(Id, JobTypes.BusDriver)
        {
            var blip = new ExtraBlip(513, Position.Position, "Автовокзал", 1f, 2, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            this.Routes = Routes;
        }

        public List<(uint Reward, List<Vector3> Positions)> Routes { get; set; }

        public override void OnStartJob(object[] data)
        {
            base.OnStartJob(data);

            SetCurrentData("JVEH", Entities.Vehicles.GetAtRemote(Utils.Convert.ToUInt16(data[0])));
        }

        public override void OnEndJob()
        {
            base.OnEndJob();
        }

        public async void ShowRouteSelection()
        {
            if (Routes.Count == 0)
                return;

            var counter = 0;

            var dict = new Dictionary<int, float>();

            for (var i = 0; i < Routes.Count; i++)
            {
                dict.Add(i, 0f);

                for (var j = 0; j < Routes[i].Positions.Count - 1; j++)
                {
                    dict[i] += Routes[i].Positions[j].DistanceTo(Routes[i].Positions[j + 1]);
                }
            }

            Vehicle vehicle = Player.LocalPlayer.Vehicle;

            await ActionBox.ShowSelect("JobBusDriverRouteSelect",
                Locale.Actions.JobVehicleRouteSelectTitle,
                Routes.Select(x => ((decimal)counter,
                           string.Format(Locale.Actions.JobBusDriverRouteText, counter + 1, Locale.Get("GEN_MONEY_0", x.Reward), Math.Round(dict[counter++] / 1000f, 2)))
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
                async (rType, id) =>
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (rType == ActionBox.ReplyTypes.OK)
                    {
                        var quest = Quest.GetPlayerQuest(pData, QuestTypes.JBD1);

                        if (quest == null || quest.Step > 0)
                            return;

                        List<(uint Reward, List<Vector3> Positions)> routes = (pData.CurrentJob as BusDriver)?.Routes;

                        if (routes == null)
                            return;

                        if (id >= routes.Count)
                            return;

                        byte res = await quest.CallProgressUpdateProc(id);

                        if (res == byte.MaxValue)
                            ActionBox.Close(true);
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
    }
}