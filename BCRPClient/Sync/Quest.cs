using BCRPClient.Additional;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Sync
{
    public class Quest
    {
        public static Quest ActualQuest { get; private set; }

        public class QuestData
        {
            public enum Types
            {
                TQ1 = 0,

                /// <summary>Job Trucker 1</summary>
                JTR1,
            }

            public enum ColourTypes
            {
                Dark = 0,
                Red,
            }

            public static Dictionary<Types, QuestData> All { get; private set; } = new Dictionary<Types, QuestData>()
            {
                {
                    Types.TQ1,

                    new QuestData(Types.TQ1, "Пиздилово", "Snow Brawl", new Dictionary<byte, StepData>()
                    {
                        {
                            0,

                            new StepData("Попадите снежком в {1-0} игроков", 10)
                            {
                                StartAction = (pData, quest) =>
                                {
                                    var mBlip = new Blip(304, new Vector3(0f, 0f, 0f), "asdas", 1f, 5, 255, 0, false, 0, 0, Settings.MAIN_DIMENSION);

                                    quest.SetActualData("BP_M", mBlip);
                                },

                                EndAction = null,
                            }
                        }
                    })
                },

                {
                    Types.JTR1,

                    new QuestData(Types.JTR1, "Доставка груза", "Дальнобойщики", new Dictionary<byte, StepData>()
                    {
                        {
                            0,

                            new StepData("Возьмите новый заказ", 1)
                            {
                                StartAction = (pData, quest) =>
                                {

                                }
                            }
                        },

                        {
                            1,

                            new StepData("Заберите груз со склада", 1)
                            {
                                StartAction = (pData, quest) =>
                                {
                                    Utils.ConsoleOutput(quest.CurrentData ?? "null");

                                    var qData = quest.CurrentData?.Split('&');

                                    if (qData == null || qData.Length != 4)
                                        return;

                                    var job = pData.CurrentJob as Data.Locations.Trucker;

                                    if (job == null)
                                        return;

                                    var currentOrder = new Data.Locations.Trucker.OrderInfo() { Id = uint.Parse(qData[0]), MPIdx = int.Parse(qData[1]), TargetBusiness = BCRPClient.Data.Locations.Business.All[int.Parse(qData[2])], Reward = uint.Parse(qData[3]) };

                                    job.SetCurrentData("CO", currentOrder);

                                    var destPos = new Vector3(job.MaterialsPositions[currentOrder.MPIdx].X, job.MaterialsPositions[currentOrder.MPIdx].Y, job.MaterialsPositions[currentOrder.MPIdx].Z);

                                    destPos.Z -= 1f;

                                    var colshape = new Additional.Cylinder(destPos, 10f, 10f, true, new Utils.Colour(255, 0, 0, 125), Settings.MAIN_DIMENSION, null)
                                    {
                                        ApproveType = Additional.ExtraColshape.ApproveTypes.OnlyServerVehicleDriver,

                                        OnEnter = (cancel) =>
                                        {
                                            var jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                                            if (jobVehicle == null || Player.LocalPlayer.Vehicle != jobVehicle)
                                            {
                                                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.JobVehicleNotInVeh);

                                                return;
                                            }

                                            var task = new AsyncTask(async () =>
                                            {
                                                if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                                {
                                                    CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.JobVehicleNotInVeh);

                                                    return;
                                                }

                                                var res = await quest.CallProgressUpdateProc(1, $"{currentOrder.Id}");
                                            }, 5000, false, 0);

                                            var scaleform = Additional.Scaleform.CreateCounter("job_trucker_0", Locale.Scaleform.JobTruckerLoadMaterialsTitle, Locale.Scaleform.JobTruckerLoadMaterialsText, 5, Scaleform.CounterSoundTypes.None);

                                            scaleform.OnRender += () =>
                                            {
                                                if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                                {
                                                    CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.JobVehicleNotInVeh);

                                                    scaleform.Destroy();
                                                    task.Cancel();

                                                    return;
                                                }
                                            };

                                            quest.SetActualData("Scaleform", scaleform);
                                            quest.SetActualData("Task", task);

                                            task.Run();
                                        },

                                        OnExit = (cancel) =>
                                        {
                                            quest.GetActualData<Additional.Scaleform>("Scaleform")?.Destroy();
                                            quest.GetActualData<AsyncTask>("Task")?.Cancel();

                                            quest.ResetActualData("Scaleform");
                                            quest.ResetActualData("Task");
                                        }
                                    };

                                    quest.SetActualData("BP_M", new Blip(9, destPos, "", 1f, 3, 125, 0f, true, 0, 10f, Settings.MAIN_DIMENSION));
                                    quest.SetActualData("CS_0", colshape);

                                    quest.SetActualData("TXL_0", new TextLabel(new Vector3(destPos.X, destPos.Y, destPos.Z + 2f), Locale.General.Blip.JobTruckerPointAText, new RGBA(255, 255, 255, 255), 25f, 0, true, Settings.MAIN_DIMENSION) { Font = 4, LOS = false });
                                },

                                EndAction = (pData, quest) =>
                                {
                                    quest.GetActualData<Additional.Scaleform>("Scaleform")?.Destroy();
                                    quest.GetActualData<AsyncTask>("Task")?.Cancel();

                                    quest.ClearAllActualData();
                                }
                            }
                        },

                        {
                            2,

                            new StepData("Доставьте груз получателю", 1)
                            {
                                StartAction = (pData, quest) =>
                                {
                                    var qData = quest.CurrentData?.Split('&');

                                    if (qData == null || qData.Length != 4)
                                        return;

                                    var job = pData.CurrentJob as Data.Locations.Trucker;

                                    if (job == null)
                                        return;

                                    var currentOrder =  job.GetCurrentData<Data.Locations.Trucker.OrderInfo>("CO") ?? new Data.Locations.Trucker.OrderInfo() { Id = uint.Parse(qData[0]), MPIdx = int.Parse(qData[1]), TargetBusiness = BCRPClient.Data.Locations.Business.All[int.Parse(qData[2])], Reward = uint.Parse(qData[3]) };

                                    job.SetCurrentData("CO", currentOrder);

                                    var destPos = new Vector3(currentOrder.TargetBusiness.InfoColshape.Position.X, currentOrder.TargetBusiness.InfoColshape.Position.Y, currentOrder.TargetBusiness.InfoColshape.Position.Z);

                                    var colshape = new Additional.Cylinder(destPos, 10f, 10f, true, new Utils.Colour(255, 0, 0, 125), Settings.MAIN_DIMENSION, null)
                                    {
                                        ApproveType = Additional.ExtraColshape.ApproveTypes.OnlyServerVehicleDriver,

                                        OnEnter = (cancel) =>
                                        {
                                            var jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                                            if (jobVehicle == null || Player.LocalPlayer.Vehicle != jobVehicle)
                                            {
                                                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.JobVehicleNotInVeh);

                                                return;
                                            }

                                            var task = new AsyncTask(async () =>
                                            {
                                                if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                                {
                                                    CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.JobVehicleNotInVeh);

                                                    return;
                                                }

                                                var res = await quest.CallProgressUpdateProc(1, $"{currentOrder.Id}");
                                            }, 5000, false, 0);

                                            var scaleform = Additional.Scaleform.CreateCounter("job_trucker_0", Locale.Scaleform.JobTruckerUnloadMaterialsTitle, Locale.Scaleform.JobTruckerLoadMaterialsText, 5, Scaleform.CounterSoundTypes.None);

                                            scaleform.OnRender += () =>
                                            {
                                                if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                                {
                                                    CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.JobVehicleNotInVeh);

                                                    scaleform.Destroy();
                                                    task.Cancel();

                                                    return;
                                                }
                                            };

                                            quest.SetActualData("Scaleform", scaleform);
                                            quest.SetActualData("Task", task);

                                            task.Run();
                                        },

                                        OnExit = (cancel) =>
                                        {
                                            quest.GetActualData<Additional.Scaleform>("Scaleform")?.Destroy();
                                            quest.GetActualData<AsyncTask>("Task")?.Cancel();

                                            quest.ResetActualData("Scaleform");
                                            quest.ResetActualData("Task");
                                        }
                                    };

                                    quest.SetActualData("BP_M", new Blip(9, destPos, "", 1f, 3, 125, 0f, true, 0, 10f, Settings.MAIN_DIMENSION));
                                    quest.SetActualData("CS_0", colshape);
                                },

                                EndAction = (pData, quest) =>
                                {
                                    quest.GetActualData<Additional.Scaleform>("Scaleform")?.Destroy();
                                    quest.GetActualData<AsyncTask>("Task")?.Cancel();

                                    quest.ClearAllActualData();
                                }
                            }
                        }
                    })
                },
            };

            public Types Type { get; set; }

            public ColourTypes ColourType { get; set; }

            public string Name { get; set; }

            public string GiverName { get; set; }

            public Dictionary<byte, StepData> Steps { get; set; }

            public QuestData(Types Type, string Name, string GiverName, Dictionary<byte, StepData> Steps)
            {
                this.Type = Type;

                this.Name = Name;
                this.GiverName = GiverName;

                this.Steps = Steps;

                this.ColourType = ColourTypes.Dark;
            }

            public class StepData
            {
                public string GoalName { get; set; }

                public int MaxProgress { get; set; }

                public Action<Sync.Players.PlayerData, Quest> StartAction { get; set; }

                public Action<Sync.Players.PlayerData, Quest> EndAction { get; set; }

                public StepData(string GoalName, int MaxProgress = 1)
                {
                    this.GoalName = GoalName;

                    this.MaxProgress = MaxProgress;
                }
            }
        }

        public QuestData.Types Type { get; set; }

        public QuestData Data => QuestData.All[Type];

        public byte Step { get; set; }

        public int StepProgress { get; set; }

        public string CurrentData { get; set; }

        public QuestData.StepData CurrentStepData => Data.Steps.GetValueOrDefault(Step);

        private Dictionary<string, object> ActualData { get; set; } = new Dictionary<string, object>();

        public void SetActualData(string key, object data)
        {
            if (ActualData == null)
                return;

            if (!ActualData.TryAdd(key, data))
                ActualData[key] = data;
        }

        public T GetActualData<T>(string key)
        {
            var data = ActualData.GetValueOrDefault(key);

            if (data is T dataT)
                return dataT;

            return default(T);
        }

        public bool ResetActualData(string key) => ActualData.Remove(key);

        public string GoalWithProgress
        {
            get
            {
                var sData = CurrentStepData;

                if (sData == null)
                    return null;

                if (sData.GoalName.Contains("{1-0}"))
                    return sData.GoalName.Replace("{1-0}", (sData.MaxProgress - StepProgress).ToString());

                return string.Format(sData.GoalName, StepProgress, sData.MaxProgress);
            }
        }

        public Quest(QuestData.Types Type, byte Step, int StepProgress, string CurrentData)
        {
            this.Type = Type;

            this.Step = Step;
            this.StepProgress = StepProgress;

            this.CurrentData = CurrentData;
        }

        public static Quest GetPlayerQuest(Sync.Players.PlayerData pData, QuestData.Types type) => pData.Quests.Where(x => x.Type == type).FirstOrDefault();

        public void MenuIconFunc()
        {
            SetActive(true);
        }

        public void UpdateHudQuest()
        {
            var data = Data;

            CEF.HUD.SetQuestParams(data.GiverName, data.Name, GoalWithProgress, data.ColourType);
        }

        public void UpdateProgress(int newProgress)
        {
            StepProgress = newProgress;

            if (ActualQuest == this)
            {
                UpdateHudQuest();
            }
        }

        public void UpdateStep(byte newStep)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var sData = CurrentStepData;

            if (sData != null)
            {
                sData.EndAction?.Invoke(pData, this);
            }

            sData = Data.Steps.GetValueOrDefault(newStep);

            if (sData != null)
            {
                sData.StartAction?.Invoke(pData, this);
            }

            Step = newStep;

            SetActive(true);
        }

        public void SetActive(bool state, bool route = true)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (state)
            {
                var sData = CurrentStepData;

                if (sData == null)
                    return;

                ActualQuest = this;

                if (!Settings.Interface.HideQuest)
                {
                    CEF.HUD.EnableQuest(true);
                }

                if (route)
                {
                    var mBlip = GetActualData<Blip>("BP_M");

                    if (mBlip != null)
                    {
                        var coords = mBlip.GetInfoIdCoord();

                        Utils.SetWaypoint(coords.X, coords.Y);
                    }
                }
            }
            else
            {
                ActualQuest = pData.Quests.Where(x => x != this).FirstOrDefault();

                if (ActualQuest != null)
                {
                    ActualQuest.SetActive(true);
                }
                else
                {
                    CEF.HUD.EnableQuest(false);
                }
            }
        }

        public void Initialize()
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var sData = CurrentStepData;

            if (sData != null)
            {
                sData.StartAction?.Invoke(pData, this);
            }

            if (ActualQuest == null)
                SetActive(true, false);
        }

        public void Destroy()
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (ActualQuest == this)
            {
                SetActive(false);
            }

            var sData = CurrentStepData;

            if (sData != null)
            {
                sData.EndAction?.Invoke(pData, this);
            }

            ClearAllActualData();
        }

        public void ClearAllActualData()
        {
            foreach (var x in ActualData.Keys.ToList())
            {
                object value;

                if (!ActualData.TryGetValue(x, out value))
                    return;

                if (x.StartsWith("BP"))
                {
                    (value as Blip)?.Destroy();
                }
                else if (x.StartsWith("TXL"))
                {
                    (value as TextLabel)?.Destroy();
                }
                else if (x.StartsWith("MKR"))
                {
                    (value as Marker)?.Destroy();
                }
                else if (x.StartsWith("CS"))
                {
                    (value as ExtraColshape)?.Destroy();
                }
            }

            ActualData.Clear();
        }

        public async System.Threading.Tasks.Task<byte> CallProgressUpdateProc(int newProgress, string data = null) => (byte)(int)await Events.CallRemoteProc("Quest::PU", (int)Type, newProgress, data ?? "");
    }
}
