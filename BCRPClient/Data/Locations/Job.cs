using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.Data
{
    public partial class Locations : Events.Script
    {
        public abstract class Job
        {
            private static Dictionary<int, Job> AllJobs { get; set; } = new Dictionary<int, Job>();

            public static Job Get(int id) => AllJobs.GetValueOrDefault(id);

            private static Dictionary<Types, Action<Sync.Players.PlayerData, Job>> ShowJobMenuActions { get; set; } = new Dictionary<Types, Action<Sync.Players.PlayerData, Job>>()
            {
                {
                    Types.Trucker,

                    (pData, job) =>
                    {
                        var truckerJob = job as Trucker;

                        var jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                        if (jobVehicle == null)
                            return;

                        if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                        {
                            CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.JobVehicleNotInVeh);

                            return;
                        }

                        var activeOrders = job.GetCurrentData<List<Trucker.OrderInfo>>("AOL");

                        if (activeOrders == null)
                            return;

                        truckerJob.ShowOrderSelection(activeOrders);
                    }
                },

                {
                    Types.Cabbie,

                    (pData, job) =>
                    {
                        var cabbieJob = job as Cabbie;

                        var jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                        if (jobVehicle == null)
                            return;

                        if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                        {
                            CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.JobVehicleNotInVeh);

                            return;
                        }

                        var activeOrders = job.GetCurrentData<List<Cabbie.OrderInfo>>("AOL");

                        if (activeOrders == null)
                            return;

                        cabbieJob.ShowOrderSelection(activeOrders);
                    }
                },
            };

            public static void ShowJobMenu()
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var job = pData.CurrentJob;

                if (job == null)
                    return;

                var showAction = ShowJobMenuActions.GetValueOrDefault(job.Type);

                if (showAction == null)
                    return;

                showAction.Invoke(pData, job);
            }

            public enum Types
            {
                /// <summary>Работа дальнобойщика</summary>
                Trucker = 0,

                Cabbie,
            }

            public int Id { get; set; }

            public int SubId => AllJobs.Values.Where(x => x.Type == Type).ToList().IndexOf(this);

            public Types Type { get; set; }

            public string Name => Locale.Property.JobNames.GetValueOrDefault(Type) ?? "null";

            public NPC JobGiver { get; set; }

            private Dictionary<string, object> CurrentData { get; set; }

            public Job(int Id, Types Type)
            {
                this.Id = Id;

                AllJobs.Add(Id, this);
            }

            public void SetCurrentData(string key, object data)
            {
                if (CurrentData == null)
                    return;

                if (!CurrentData.TryAdd(key, data))
                    CurrentData[key] = data;
            }

            public T GetCurrentData<T>(string key)
            {
                var data = CurrentData.GetValueOrDefault(key);

                if (data is T dataT)
                    return dataT;

                return default(T);
            }

            public bool ResetCurrentData(string key) => CurrentData.Remove(key);

            public virtual void OnStartJob(object[] data)
            {
                CurrentData = new Dictionary<string, object>();
            }

            public virtual void OnEndJob()
            {
                if (CurrentData != null)
                {
                    CurrentData.Clear();

                    CurrentData = null;
                }
            }
        }

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

                //CEF.ActionBox.ShowSelect(CEF.ActionBox.Contexts.JobTruckerOrderSelect, Locale.Actions.JobVehicleOrderSelectTitle, activeOrders.Select(x => (counter++, string.Format(Locale.Actions.JobTruckerOrderText, counter, Math.Round(MaterialsPositions[x.MPIdx].DistanceTo(Player.LocalPlayer.Position) / 1000f, 2), Math.Round(MaterialsPositions[x.MPIdx].DistanceTo(x.TargetBusiness.InfoColshape.Position) / 1000f, 2), Utils.GetPriceString(x.Reward)))).ToArray(), Locale.Actions.SelectOkBtn2, Locale.Actions.SelectCancelBtn1, Player.LocalPlayer.Vehicle);
                CEF.ActionBox.ShowSelect(CEF.ActionBox.Contexts.JobTruckerOrderSelect, Locale.Actions.JobVehicleOrderSelectTitle, activeOrders.Select(x => (counter++, string.Format(Locale.Actions.JobTruckerOrderText, counter, Math.Round(MaterialsPositions[x.MPIdx].GetPathfindTravelDistance(Player.LocalPlayer.Position) / 1000f, 2), Math.Round(MaterialsPositions[x.MPIdx].GetPathfindTravelDistance(x.TargetBusiness.InfoColshape.Position) / 1000f, 2), Utils.GetPriceString(x.Reward)))).ToArray(), Locale.Actions.SelectOkBtn2, Locale.Actions.SelectCancelBtn1, Player.LocalPlayer.Vehicle);
            }
        }

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
            }

            public void ShowOrderSelection(List<OrderInfo> activeOrders)
            {
                if (activeOrders.Count == 0)
                {
                    CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Notifications.DefHeader, Locale.Notifications.General.JobNoOrders);

                    return;
                }

                var counter = 0;

                //CEF.ActionBox.ShowSelect(CEF.ActionBox.Contexts.JobTruckerOrderSelect, Locale.Actions.JobVehicleOrderSelectTitle, activeOrders.Select(x => (counter++, string.Format(Locale.Actions.JobTruckerOrderText, counter, Math.Round(MaterialsPositions[x.MPIdx].DistanceTo(Player.LocalPlayer.Position) / 1000f, 2), Math.Round(MaterialsPositions[x.MPIdx].DistanceTo(x.TargetBusiness.InfoColshape.Position) / 1000f, 2), Utils.GetPriceString(x.Reward)))).ToArray(), Locale.Actions.SelectOkBtn2, Locale.Actions.SelectCancelBtn1, Player.LocalPlayer.Vehicle);
                //CEF.ActionBox.ShowSelect(CEF.ActionBox.Contexts.JobCabbieOrderSelect, Locale.Actions.JobVehicleOrderSelectTitle, activeOrders.Select(x => (counter++, string.Format(Locale.Actions.JobCabbieOrderText, counter, Utils.GetStreetName(x.Position), Math.Round(Player.LocalPlayer.Position.GetPathfindTravelDistance(x.Position) / 1000f, 2)))).ToArray(), Locale.Actions.SelectOkBtn2, Locale.Actions.SelectCancelBtn1, Player.LocalPlayer.Vehicle);
            }
        }
    }
}
