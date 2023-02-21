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

                JCAB1,
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
                                    var qData = quest.CurrentData?.Split('&');

                                    if (qData == null || qData.Length != 4)
                                        return;

                                    var job = pData.CurrentJob as Data.Locations.Trucker;

                                    if (job == null)
                                        return;

                                    var currentOrder = new Data.Locations.Trucker.OrderInfo() { Id = uint.Parse(qData[0]), MPIdx = int.Parse(qData[1]), TargetBusiness = BCRPClient.Data.Locations.Business.All[int.Parse(qData[2])], Reward = uint.Parse(qData[3]) };

                                    job.SetCurrentData("CO", currentOrder);

                                    var destPos = new Vector3(job.MaterialsPositions[currentOrder.MPIdx].X, job.MaterialsPositions[currentOrder.MPIdx].Y, job.MaterialsPositions[currentOrder.MPIdx].Z);

                                    var colshape = new Additional.Sphere(destPos, 10f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                                    {
                                        ApproveType = Additional.ExtraColshape.ApproveTypes.OnlyServerVehicleDriver,

                                        OnEnter = async (cancel) =>
                                        {
                                            var jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                                            if (jobVehicle == null)
                                                return;

                                            if (Player.LocalPlayer.Vehicle != jobVehicle)
                                            {
                                                return;
                                            }
/*
                                            var scaleform = Additional.Scaleform.CreateCounter("job_trucker_0", "Загрузка материалов", "Подождите еще {0} сек.", 5, Scaleform.CounterSoundTypes.None);

                                            var task = new AsyncTask(() =>
                                            {
                                                if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                                {
                                                    return;
                                                }
                                            }, 25, true, 0);*/

                                            var res = await quest.CallProgressUpdateProc(1, $"{currentOrder.Id}");
                                        },

                                        OnExit = (cancel) =>
                                        {

                                        }
                                    };

                                    quest.SetActualData("BP_M", new Blip(9, destPos, "", 1f, 3, 125, 0f, true, 0, 10f, Settings.MAIN_DIMENSION));
                                    quest.SetActualData("CS_0", colshape);
                                    quest.SetActualData("MKR_0", new Marker(1, destPos, 10f, Vector3.Zero, Vector3.Zero, new RGBA(255, 0, 0, 125), true, Settings.MAIN_DIMENSION));

                                    destPos.Z += 1f;

                                    quest.SetActualData("TXL_0", new TextLabel(destPos, Locale.General.Blip.JobTruckerPointAText, new RGBA(255, 255, 255, 255), 25f, 0, true, Settings.MAIN_DIMENSION) { Font = 4, LOS = false });
                                },

                                EndAction = (pData, quest) =>
                                {
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

                                    var colshape = new Additional.Sphere(destPos, 10f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                                    {
                                        ApproveType = Additional.ExtraColshape.ApproveTypes.OnlyServerVehicleDriver,

                                        OnEnter = async (cancel) =>
                                        {
                                            var jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                                            if (jobVehicle == null)
                                                return;

                                            if (Player.LocalPlayer.Vehicle != jobVehicle)
                                            {
                                                return;
                                            }

                                            var res = await quest.CallProgressUpdateProc(1, $"{currentOrder.Id}");
                                        }
                                    };

                                    quest.SetActualData("BP_M", new Blip(9, destPos, "", 1f, 3, 125, 0f, true, 0, 10f, Settings.MAIN_DIMENSION));
                                    quest.SetActualData("CS_0", colshape);
                                    quest.SetActualData("MKR_0", new Marker(1, destPos, 10f, Vector3.Zero, Vector3.Zero, new RGBA(255, 0, 0, 125), true, Settings.MAIN_DIMENSION));
                                },

                                EndAction = (pData, quest) =>
                                {
                                    quest.ClearAllActualData();
                                }
                            }
                        }
                    })
                },

                {
                    Types.JCAB1,

                    new QuestData(Types.JCAB1, "Доставка пассажира", "Такси", new Dictionary<byte, StepData>()
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

                            new StepData("Приедьте за клиентом", 1)
                            {
                                StartAction = (pData, quest) =>
                                {

                                }
                            }
                        },

                        {
                            2,

                            new StepData("Отвезите клиента", 1)
                            {
                                StartAction = (pData, quest) =>
                                {

                                }
                            }
                        },
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

        public void SetActive(bool state)
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

                var mBlip = GetActualData<Blip>("BP_M");

                if (mBlip != null)
                {
                    var coords = mBlip.GetInfoIdCoord();

                    Utils.SetWaypoint(coords.X, coords.Y);
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

            if (ActualQuest == null)
                SetActive(true);

            var sData = CurrentStepData;

            if (sData != null)
            {
                sData.StartAction?.Invoke(pData, this);
            }
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
            foreach (var x in ActualData)
            {
                if (x.Key.StartsWith("BP"))
                {
                    (x.Value as Blip)?.Destroy();
                }
                else if (x.Key.StartsWith("TXL"))
                {
                    (x.Value as TextLabel)?.Destroy();
                }
                else if (x.Key.StartsWith("MKR"))
                {
                    (x.Value as Marker)?.Destroy();
                }
                else if (x.Key.StartsWith("CS"))
                {
                    (x.Value as ExtraColshape)?.Delete();
                }
            }

            ActualData.Clear();
        }

        public async System.Threading.Tasks.Task<byte> CallProgressUpdateProc(int newProgress, string data = null) => (byte)(int)await Events.CallRemoteProc("Quest::PU", (int)Type, newProgress, data ?? "");
    }
}
