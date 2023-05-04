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
            var blip = new Additional.ExtraBlip(513, Position.Position, "Автовокзал", 1f, 2, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

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

        public async void ShowRouteSelection()
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

            var vehicle = Player.LocalPlayer.Vehicle;

            await CEF.ActionBox.ShowSelect
            (
                "JobBusDriverRouteSelect", Locale.Actions.JobVehicleRouteSelectTitle, Routes.Select(x => ((decimal)counter, string.Format(Locale.Actions.JobBusDriverRouteText, counter + 1, Utils.GetPriceString(x.Reward), Math.Round(dict[counter++] / 1000f, 2)))).ToArray(), Locale.Actions.SelectOkBtn2, Locale.Actions.SelectCancelBtn1,

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

                async (rType, id) =>
                {
                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (rType == CEF.ActionBox.ReplyTypes.OK)
                    {
                        var quest = Sync.Quest.GetPlayerQuest(pData, Sync.Quest.QuestData.Types.JBD1);

                        if (quest == null || quest.Step > 0)
                            return;

                        var routes = (pData.CurrentJob as Data.Jobs.BusDriver)?.Routes;

                        if (routes == null)
                            return;

                        if (id >= routes.Count)
                            return;

                        var res = await quest.CallProgressUpdateProc(id);

                        if (res == byte.MaxValue)
                        {
                            CEF.ActionBox.Close(true);
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
    }
}