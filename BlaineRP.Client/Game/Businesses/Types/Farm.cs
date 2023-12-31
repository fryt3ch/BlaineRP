﻿using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Animations;
using BlaineRP.Client.Game.Attachments;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Players;
using BlaineRP.Client.Game.Helpers;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.Jobs;
using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Game.Quests;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Game.UI.CEF.Phone.Apps;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using Newtonsoft.Json;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Businesses
{
    public class Farm : Business
    {
        public const float TRACTOR_MAX_SPEED_KM_H = 15f;

        private static Dictionary<int, Dictionary<int, float>> _cropFieldsRotations = new Dictionary<int, Dictionary<int, float>>()
        {
            {
                1, new Dictionary<int, float>()
                {
                    { 0, float.PositiveInfinity },
                    { 1, float.PositiveInfinity },
                    { 2, float.PositiveInfinity },
                    { 3, float.PositiveInfinity },
                    { 4, float.PositiveInfinity },
                    { 5, -225f },
                    { 6, -225f },
                    { 7, -225f },
                    { 8, -225f },
                }
            },
        };

        private static Dictionary<CropField.Types, (float GrownZOffset, float SownZOffset, string Name, uint Model)> _cropTypesData =
            new Dictionary<CropField.Types, (float, float, string, uint)>()
            {
                { CropField.Types.Cabbage, (0f, -0.22326f, "Капуста", RAGE.Util.Joaat.Hash("prop_veg_crop_03_cab")) },
                { CropField.Types.Pumpkin, (0f, -0.39441f, "Тыква", RAGE.Util.Joaat.Hash("prop_veg_crop_03_pump")) },
                { CropField.Types.Wheat, (0.5f, 0f, "Пшеница", RAGE.Util.Joaat.Hash("prop_veg_crop_05")) },
                { CropField.Types.OrangeTree, (0f, 0f, "Апельсиновое дерево", RAGE.Util.Joaat.Hash("prop_veg_crop_orange")) },
                { CropField.Types.Cow, (0f, 0f, "Корова", RAGE.Util.Joaat.Hash("a_c_cow")) },
            };

        public Farm(int id,
                    Vector3 positionInfo,
                    Vector4 positionInteract,
                    uint price,
                    uint rent,
                    float tax,
                    string cropFieldsStr,
                    string orangeTreesStr,
                    string cowsStr,
                    string orangeTreeBoxPositionsStr,
                    string cowBucketPositionsStr) : base(id, positionInfo, BusinessType.Farm, price, rent, tax)
        {
            CropFields = RAGE.Util.Json.Deserialize<List<CropField>>(cropFieldsStr);

            OrangeTrees = RAGE.Util.Json.Deserialize<List<OrangeTreeData>>(orangeTreesStr);

            OrangeTreeBoxPositions = RAGE.Util.Json.Deserialize<List<Vector3>>(orangeTreeBoxPositionsStr).Select(x => new Tuple<Vector3, ExtraColshape>(x, null)).ToList();

            CowBucketPositions = RAGE.Util.Json.Deserialize<List<Vector3>>(cowBucketPositionsStr).Select(x => new Tuple<Vector3, ExtraColshape>(x, null)).ToList();

            Cows = RAGE.Util.Json.Deserialize<List<CowData>>(cowsStr);

            for (var i = 0; i < CropFields.Count; i++)
            {
                CropFields[i].PreInitialize(this, i);
            }

            OrangeTreeData.PreInitialize(this);

            CowData.PreInitialize(this);

            if (SubId == 1)
                MainColshape = new Polygon(new List<Vector3>()
                    {
                        new Vector3(2191.28223f, 4849.38428f, 75f),
                        new Vector3(1909.05542f, 4717.912f, 75f),
                        new Vector3(1812.82751f, 4817.513f, 75f),
                        new Vector3(1807.89917f, 4833.14941f, 75f),
                        new Vector3(1809.6333f, 4878.728f, 75f),
                        new Vector3(1805.541f, 4890.7583f, 75f),
                        new Vector3(1736.1438f, 4978.261f, 75f),
                        new Vector3(1923.747f, 5137.78564f, 75f),
                    },
                    0f,
                    0f,
                    false,
                    Utils.Misc.RedColor,
                    Settings.App.Static.MainDimension,
                    null
                )
                {
                    ApproveType = ApproveTypes.None,
                    OnEnter = (cancel) => OnEnterFarm(this),
                    OnExit = (cancel) => OnExitFarm(this),
                };

            Blip = new ExtraBlip(569, positionInteract.Position, "Ферма", 1f, 9, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            Seller = new NPCs.NPC($"farmer_{id}",
                "Райнер",
                NPCs.NPC.Types.Talkable,
                "a_m_m_hillbilly_01",
                positionInteract.Position,
                positionInteract.RotationZ,
                Settings.App.Static.MainDimension
            )
            {
                SubName = "NPC_SUBNAME_JOB_FARM_BOSS",
                DefaultDialogueId = "job_farm_pl_0",
                Data = this,
            };

            var gpsPos = new RAGE.Ui.Cursor.Vector2(positionInteract.X, positionInteract.Y);

            GPS.AddPosition("bizother", "farm", $"bizother_{id}", $"{Name} #{SubId}", gpsPos);
            GPS.AddPosition("jobs", "jobfarm", $"jobfarm_{id}", $"{Name} #{SubId}", gpsPos);
        }

        public AsyncTask LoadTask { get; set; }

        public static Dictionary<int, Dictionary<int, List<List<float>>>> CropFieldsZCoords { get; private set; } =
            RAGE.Util.Json.Deserialize<Dictionary<int, Dictionary<int, List<List<float>>>>>(
                "{\"1\":{\"5\":[[42.5336838,42.64844,42.7764168,42.89217,42.86189,42.79722,42.6871262,42.68867],[42.3295364,42.3898125,42.42328,42.4887123,42.4823227,42.4578247,42.4056854,42.3499947],[41.9964943,42.1416855,42.14894,42.093586,42.0516052,41.9847221,41.9349823,41.8873177]],\"6\":[[41.0347252,41.2381859,41.4085274,41.46087,41.3224945,41.4318428,41.4992676,41.5380859,41.62737],[40.8451233,41.15395,41.2773,41.48984,41.6455078,41.77222,41.78264,41.8097076,41.82335,41.96824],[40.5231743,40.9371643,41.2473145,41.62332,41.8919678,41.92287,42.0365143,42.0438347,42.05152,42.1918831],[40.48166,41.01341,41.56176,41.8789,42.04512,42.143158,42.34362,42.4107,42.4367752,42.5841064]],\"7\":[[40.83733,41.5813065,42.2078934,42.7316971,43.1412048,43.31361,43.4344444,43.6241074,43.668602,43.73133],[41.1258469,41.8893,42.4115334,42.84376,43.3128128,43.61136,43.90068,44.1271172,44.22177,44.4324036],[41.2688,41.9774857,42.6814041,43.2070351,43.4911461,43.77423,44.09675,44.3787231,44.518158,44.7212448]],\"8\":[[42.50969,42.7982674,43.1128731,43.47725,43.80024,44.02668,44.2374649,44.3879356,44.4127579,44.52336],[42.7892036,42.9787331,43.30298,43.5958748,43.8641777,44.0514374,44.14557,44.20916,44.20378,44.30516],[42.85647,43.1364975,43.3894768,43.637,43.78091,43.85371,43.880043,43.8782043,43.8117332,43.8009758]]}}"
            );

        public List<Entity> TempEntities { get; set; } = new List<Entity>();

        public ExtraColshape MainColshape { get; set; }

        public List<CropField> CropFields { get; set; }

        public List<OrangeTreeData> OrangeTrees { get; set; }

        public List<Tuple<Vector3, ExtraColshape>> OrangeTreeBoxPositions { get; set; }

        public List<Tuple<Vector3, ExtraColshape>> CowBucketPositions { get; set; }

        public List<CowData> Cows { get; set; }

        private static void OnEnterFarm(Farm farm)
        {
            farm.LoadTask?.Cancel();

            AsyncTask task = null;

            task = new AsyncTask(async () =>
                {
                    if (farm.SubId == 1)
                    {
                        if (farm.OrangeTrees != null)
                        {
                            for (var i = 0; i < farm.OrangeTrees.Count; i++)
                            {
                                OrangeTreeData.Initialize(farm, i);
                            }

                            for (var i = 0; i < farm.OrangeTreeBoxPositions.Count; i++)
                            {
                                farm.OrangeTreeBoxPositions[i].Item2?.Destroy();

                                Vector3 pos = farm.OrangeTreeBoxPositions[i].Item1;

                                int idx = i;

                                farm.OrangeTreeBoxPositions[i] = new Tuple<Vector3, ExtraColshape>(pos,
                                    new Cylinder(new Vector3(pos.X, pos.Y, pos.Z - 1f), 1.5f, 2.5f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                                    {
                                        OnEnter = (cancel) =>
                                        {
                                            var pData = PlayerData.GetData(Player.LocalPlayer);

                                            if (pData == null)
                                                return;

                                            if (!pData.AttachedObjects.Where(x => x.Type == AttachmentType.FarmOrangeBoxCarry).Any())
                                                return;

                                            Events.CallRemote("Job::FARM::OTF", idx);
                                        },
                                    }
                                );
                            }
                        }

                        if (farm.Cows != null)
                        {
                            for (var i = 0; i < farm.Cows.Count; i++)
                            {
                                CowData.Initialize(farm, i);
                            }

                            for (var i = 0; i < farm.CowBucketPositions.Count; i++)
                            {
                                farm.CowBucketPositions[i].Item2?.Destroy();

                                Vector3 pos = farm.CowBucketPositions[i].Item1;

                                int idx = i;

                                farm.CowBucketPositions[i] = new Tuple<Vector3, ExtraColshape>(pos,
                                    new Cylinder(new Vector3(pos.X, pos.Y, pos.Z - 1f), 1.5f, 2.5f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                                    {
                                        OnEnter = (cancel) =>
                                        {
                                            var pData = PlayerData.GetData(Player.LocalPlayer);

                                            if (pData == null)
                                                return;

                                            if (!pData.AttachedObjects.Where(x => x.Type == AttachmentType.FarmMilkBucketCarry).Any())
                                                return;

                                            Events.CallRemote("Job::FARM::COWF", idx);
                                        },
                                    }
                                );
                            }
                        }

                        for (var i = 0; i < farm.CropFields.Count; i++)
                        {
                            int fieldIdx = i;

                            farm.CropFields[i].Initialize(farm, i);

                            if (i == 0)
                                farm.CropFields[i].Colshape = new Cuboid(new Vector3(2042.795f, 4942.11133f, 63.9537277f),
                                    50f,
                                    38f,
                                    50f,
                                    45f,
                                    false,
                                    Utils.Misc.RedColor,
                                    Settings.App.Static.MainDimension,
                                    null
                                );
                            else if (i == 1)
                                farm.CropFields[i].Colshape = new Cuboid(new Vector3(2069.84937f, 4915.067f, 63.9537277f),
                                    50f,
                                    38f,
                                    50f,
                                    45f,
                                    false,
                                    Utils.Misc.RedColor,
                                    Settings.App.Static.MainDimension,
                                    null
                                );
                            else if (i == 2)
                                farm.CropFields[i].Colshape = new Cuboid(new Vector3(2004.82715f, 4904.285f, 63.9537277f + 1.80652f),
                                    45f,
                                    38f,
                                    50f,
                                    45f,
                                    false,
                                    Utils.Misc.RedColor,
                                    Settings.App.Static.MainDimension,
                                    null
                                );
                            else if (i == 3)
                                farm.CropFields[i].Colshape = new Cuboid(new Vector3(2031.902f, 4877.307f, 63.9537277f + 1.80652f),
                                    45f,
                                    38f,
                                    50f,
                                    45f,
                                    false,
                                    Utils.Misc.RedColor,
                                    Settings.App.Static.MainDimension,
                                    null
                                );
                            else if (i == 4)
                                farm.CropFields[i].Colshape = new Cuboid(new Vector3(1952.97778f, 4856.847f, 68.3354877f),
                                    55f,
                                    35f,
                                    50f,
                                    45f,
                                    false,
                                    Utils.Misc.RedColor,
                                    Settings.App.Static.MainDimension,
                                    null
                                );
                            else if (i == 5)
                                farm.CropFields[i].Colshape = new Cuboid(new Vector3(1956.74463f, 4796.612f, 68.3354877f - 1.55087f),
                                    50f,
                                    40f,
                                    50f,
                                    45f,
                                    false,
                                    Utils.Misc.RedColor,
                                    Settings.App.Static.MainDimension,
                                    null
                                );
                            else if (i == 6)
                                farm.CropFields[i].Colshape = new Cuboid(new Vector3(1915.74133f, 4761.87549f, 68.3354877f - 2.95878f),
                                    55f,
                                    40f,
                                    50f,
                                    45f,
                                    false,
                                    Utils.Misc.RedColor,
                                    Settings.App.Static.MainDimension,
                                    null
                                );
                            else if (i == 7)
                                farm.CropFields[i].Colshape = new Cuboid(new Vector3(1887.2063f, 4790.97266f, 68.3354877f - 1.30222f),
                                    55f,
                                    35f,
                                    50f,
                                    45f,
                                    false,
                                    Utils.Misc.RedColor,
                                    Settings.App.Static.MainDimension,
                                    null
                                );
                            else if (i == 8)
                                farm.CropFields[i].Colshape = new Cuboid(new Vector3(1860.861f, 4817.32959f, 68.3354877f - 1.30222f),
                                    55f,
                                    35f,
                                    50f,
                                    45f,
                                    false,
                                    Utils.Misc.RedColor,
                                    Settings.App.Static.MainDimension,
                                    null
                                );

                            farm.CropFields[i].Colshape.ApproveType = ApproveTypes.None;
                            farm.CropFields[i].Colshape.OnEnter = (cancel) => farm.CropFields[fieldIdx].OnEnter(farm, fieldIdx);
                            farm.CropFields[i].Colshape.OnExit = (cancel) => farm.CropFields[fieldIdx].OnExit(farm, fieldIdx);

                            await RAGE.Game.Invoker.WaitAsync(25);

                            if (task?.IsCancelled != false)
                                return;
                        }
                    }
                },
                0,
                false,
                0
            );

            farm.LoadTask = task;

            task.Run();
        }

        private static async void OnExitFarm(Farm farm)
        {
            farm.LoadTask?.Cancel();

            farm.LoadTask = null;

            if (farm.OrangeTrees != null)
            {
                foreach (OrangeTreeData x in farm.OrangeTrees)
                {
                    if (x.Colshape != null)
                    {
                        x.Colshape.Destroy();

                        x.Colshape = null;
                    }
                }

                for (var i = 0; i < farm.OrangeTreeBoxPositions.Count; i++)
                {
                    farm.OrangeTreeBoxPositions[i].Item2?.Destroy();

                    farm.OrangeTreeBoxPositions[i] = new Tuple<Vector3, ExtraColshape>(farm.OrangeTreeBoxPositions[i].Item1, null);
                }
            }

            if (farm.Cows != null)
                foreach (CowData x in farm.Cows)
                {
                    if (x.Ped != null)
                    {
                        x.Ped.Destroy();

                        x.Ped = null;
                    }

                    if (x.Colshape != null)
                    {
                        x.Colshape.Destroy();

                        x.Colshape = null;
                    }
                }

            foreach (CropField x in farm.CropFields)
            {
                x.Destroy();

                if (x.Colshape != null)
                {
                    x.Colshape.Destroy();

                    x.Colshape = null;
                }
            }

            /*                var pData = Sync.Players.GetData(Player.LocalPlayer);

                            if (pData == null)
                                return;

                            var curJob = pData.CurrentJob;

                            if (curJob?.Type == Jobs.Types.Farmer)
                            {
                                if (curJob == Jobs.Job.AllJobs.Values.Select(x => x as Jobs.Farmer).Where(x => x?.FarmBusiness == farm).FirstOrDefault())
                                {
                                    if ((bool)await Events.CallRemoteProc("Job::FARM::FJ"))
                                    {
                                        CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), "Вы ушли слишком далеко от фермы и были уволены!");
                                    }
                                }
                            }*/
        }

        public void UpdateTractorTakerData(Quest quest)
        {
            float x, y, z;

            var count = 0;

            for (var i = 0; i < CropFields.Count; i++)
            {
                if (CropFields[i].Type != CropField.Types.Wheat)
                    continue;

                for (byte j = 0; j < CropFields[i].CropsData.Count; j++)
                {
                    for (byte k = 0; k < CropFields[i].CropsData[j].Count; k++)
                    {
                        if (CropField.CropData.GetGrowTime(this, i, j, k) == 0)
                        {
                            count++;

                            if (quest.GetActualData<object>($"E_MKR_{i}_{j}_{k}") == null)
                            {
                                int fieldIdx = i;
                                byte col = j;
                                byte row = k;

                                CropFields[i].GetCropPosition2DNotSafe(col, row, out x, out y);

                                z = CropFieldsZCoords.GetValueOrDefault(SubId)?.GetValueOrDefault(fieldIdx)?[col][row] ?? CropFields[i].CoordZ;

                                z += 0.5f;

                                var pos = new Vector3(x, y, z);

                                var marker = new Marker(27,
                                    pos,
                                    2.5f,
                                    new Vector3(0f, 0f, 0f),
                                    Vector3.Zero,
                                    new RGBA(255, 255, 255, 255),
                                    true,
                                    Settings.App.Static.MainDimension
                                );
                                var blip = new ExtraBlip(469, pos, _cropTypesData[CropFields[i].Type].Name, 0.5f, 36, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                                var cs = new Cylinder(pos, 2.5f, 5f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                                {
                                    ApproveType = ApproveTypes.None,
                                    OnEnter = async (cancel) =>
                                    {
                                        Vehicle jobVehicle = PlayerData.GetData(Player.LocalPlayer)?.CurrentJob?.GetCurrentData<Vehicle>("JVEH");

                                        if (jobVehicle == null || Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                        {
                                            Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                                            return;
                                        }

                                        if (jobVehicle.GetSpeedVector(true).Y < 0) // reverse check
                                            return;

                                        if (System.Math.Floor(jobVehicle.GetSpeedKm()) > TRACTOR_MAX_SPEED_KM_H)
                                        {
                                            Notification.ShowError(
                                                $"Скорость Вашего трактора была слишком высока и урожай не был собран!\nНе превышайте скорость в {TRACTOR_MAX_SPEED_KM_H} км/ч"
                                            );

                                            return;
                                        }

                                        byte res = await quest.CallProgressUpdateProc(fieldIdx, col, row);

                                        if (res == byte.MaxValue)
                                        {
                                        }
                                    },
                                };

                                quest.SetActualData($"E_MKR_{i}_{j}_{k}", marker);
                                quest.SetActualData($"E_BP_{i}_{j}_{k}", blip);
                                quest.SetActualData($"CS_{i}_{j}_{k}", cs);
                            }
                        }
                        else
                        {
                            if (quest.GetActualData<object>($"E_MKR_{i}_{j}_{k}") is Entity marker)
                            {
                                marker.Destroy();

                                quest.ResetActualData($"E_MKR_{i}_{j}_{k}");

                                if (quest.GetActualData<object>($"E_BP_{i}_{j}_{k}") is Entity blip)
                                {
                                    blip.Destroy();

                                    quest.ResetActualData($"E_BP_{i}_{j}_{k}");
                                }

                                if (quest.GetActualData<object>($"CS_{i}_{j}_{k}") is ExtraColshape cs)
                                {
                                    cs.Destroy();

                                    quest.ResetActualData($"CS_{i}_{j}_{k}");
                                }
                            }
                        }
                    }
                }
            }

            if (count <= 0)
                Notification.Show(Notification.Types.Information,
                    Locale.Get("NOTIFICATION_HEADER_DEF"),
                    "На данный момент ни одна пшеница не созрела, ждите, пока на миникарте появится значок!"
                );
        }

        public void UpdatePlaneIrrigatorData(Quest quest)
        {
            float x, y;

            var count = 0;

            for (var i = 0; i < CropFields.Count; i++)
            {
                if (CropField.GetIrrigationEndTime(this, i) == null)
                {
                    count++;

                    for (var j = 0; j < CropFields[i].IrrigationPoints.Count; j++)
                    {
                        if (quest.GetActualData<object>($"E_CHP_{i}_{j}") == null)
                        {
                            int fieldIdx = i;
                            int pointIdx = j;

                            Vector3 pos = CropFields[i].IrrigationPoints[j];

                            var blip = new ExtraBlip(441, pos, "Чекпоинт", 0.5f, 3, 255, 0f, false, 180, 0f, Settings.App.Static.MainDimension);

                            var checkpoint = new Checkpoint(40, pos, 5f, Vector3.Zero, new RGBA(255, 255, 255, 255), true, Settings.App.Static.MainDimension)
                            {
                                OnEnter = async (cancel) =>
                                {
                                    Vehicle jobVehicle = PlayerData.GetData(Player.LocalPlayer)?.CurrentJob?.GetCurrentData<Vehicle>("JVEH");

                                    if (jobVehicle == null || Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                    {
                                        Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                                        return;
                                    }

                                    await Streaming.RequestPtfx("core");

                                    byte res = await quest.CallProgressUpdateProc(fieldIdx, pointIdx);

                                    if (res == byte.MaxValue || res == 254)
                                    {
                                        quest.GetActualData<Entity>($"E_CHP_{fieldIdx}_{pointIdx}")?.Destroy();
                                        quest.ResetActualData($"E_CHP_{fieldIdx}_{pointIdx}");

                                        quest.GetActualData<Entity>($"E_BP_{fieldIdx}_{pointIdx}")?.Destroy();
                                        quest.ResetActualData($"E_BP_{fieldIdx}_{pointIdx}");

                                        if (res == 254)
                                            Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), $"Поле #{fieldIdx + 1} полностью орошено!");

                                        if (jobVehicle.Exists)
                                        {
                                            List<int> effects = quest.GetActualData<List<int>>("FARMJOBTEMPFX::PW");

                                            if (effects != null)
                                            {
                                                quest.SetActualData("FARMJOBTEMPFXT::PW", World.Core.ServerTimestampMilliseconds);

                                                if (effects.Count == 0)
                                                {
                                                    RAGE.Game.Graphics.UseParticleFxAssetNextCall("core");
                                                    effects.Add(RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("water_cannon_jet",
                                                            jobVehicle.Handle,
                                                            -4.293f,
                                                            -0.72f,
                                                            -0.88f,
                                                            -10f,
                                                            190f,
                                                            180f,
                                                            0.5f,
                                                            false,
                                                            false,
                                                            false
                                                        )
                                                    );

                                                    RAGE.Game.Graphics.UseParticleFxAssetNextCall("core");
                                                    effects.Add(RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("water_cannon_jet",
                                                            jobVehicle.Handle,
                                                            -4.1f,
                                                            -0.7f,
                                                            -0.885f,
                                                            -10f,
                                                            190f,
                                                            180f,
                                                            0.5f,
                                                            false,
                                                            false,
                                                            false
                                                        )
                                                    );

                                                    RAGE.Game.Graphics.UseParticleFxAssetNextCall("core");
                                                    effects.Add(RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("water_cannon_jet",
                                                            jobVehicle.Handle,
                                                            4.293f,
                                                            -0.72f,
                                                            -0.88f,
                                                            -10f,
                                                            190f,
                                                            180f,
                                                            0.5f,
                                                            false,
                                                            false,
                                                            false
                                                        )
                                                    );

                                                    RAGE.Game.Graphics.UseParticleFxAssetNextCall("core");
                                                    effects.Add(RAGE.Game.Graphics.StartParticleFxLoopedOnEntity("water_cannon_jet",
                                                            jobVehicle.Handle,
                                                            4.1f,
                                                            -0.7f,
                                                            -0.885f,
                                                            -10f,
                                                            190f,
                                                            180f,
                                                            0.5f,
                                                            false,
                                                            false,
                                                            false
                                                        )
                                                    );
                                                }
                                            }
                                        }
                                    }
                                },
                            };

                            quest.SetActualData($"E_CHP_{i}_{j}", checkpoint);
                            quest.SetActualData($"E_BP_{i}_{j}", blip);
                        }
                    }
                }
                else
                {
                    for (var j = 0; j < CropFields[i].IrrigationPoints.Count; j++)
                    {
                        if (quest.GetActualData<object>($"E_CHP_{i}_{j}") is Entity checkpoint)
                        {
                            checkpoint.Destroy();

                            quest.ResetActualData($"E_CHP_{i}_{j}");

                            if (quest.GetActualData<object>($"E_BP_{i}_{j}") is Entity blip)
                            {
                                blip.Destroy();

                                quest.ResetActualData($"E_BP_{i}_{j}");
                            }
                        }
                    }
                }
            }

            if (count <= 0)
                Notification.Show(Notification.Types.Information,
                    Locale.Get("NOTIFICATION_HEADER_DEF"),
                    "На данный момент нет ни одного не орошенного, ждите, пока на миникарте появится значок!"
                );
        }

        public class CropField
        {
            public enum Types
            {
                Cabbage = 0,
                Pumpkin,

                Wheat,

                OrangeTree,

                Cow,
            }

            public static ExtraLabel MainLabel { get; set; }
            public static ExtraLabel InfoLabel { get; set; }

            public static AsyncTask LabelTask { get; set; }

            public ExtraColshape Colshape { get; set; }

            [JsonProperty(PropertyName = "T")]
            public Types Type { get; set; }

            [JsonProperty(PropertyName = "CZ")]
            public float CoordZ { get; set; }

            [JsonProperty(PropertyName = "C")]
            public (RAGE.Ui.Cursor.Vector2 Pos, byte Count)[] Columns { get; set; }

            [JsonProperty(PropertyName = "O")]
            public RAGE.Ui.Cursor.Vector2 Offset { get; set; }

            [JsonProperty(PropertyName = "IRP")]
            public List<Vector3> IrrigationPoints { get; set; }

            public List<List<CropData>> CropsData { get; set; } = new List<List<CropData>>();

            public Vector3 GetCropPosition3D(byte col, byte row)
            {
                return col >= CropsData.Count || row >= CropsData[col].Count ? null : new Vector3(Columns[col].Pos.X + Offset.X * row, Columns[col].Pos.Y + Offset.Y * row, CoordZ);
            }

            public void GetCropPosition3DNotSafe(byte col, byte row, out float x, out float y, out float z)
            {
                x = Columns[col].Pos.X + Offset.X * row;
                y = Columns[col].Pos.Y + Offset.Y * row;

                z = CoordZ;
            }

            public RAGE.Ui.Cursor.Vector2 GetCropPosition2D(byte col, byte row)
            {
                return col >= CropsData.Count || row >= CropsData[col].Count
                    ? null
                    : new RAGE.Ui.Cursor.Vector2(Columns[col].Pos.X + Offset.X * row, Columns[col].Pos.Y + Offset.Y * row);
            }

            public void GetCropPosition2DNotSafe(byte col, byte row, out float x, out float y)
            {
                x = Columns[col].Pos.X + Offset.X * row;
                y = Columns[col].Pos.Y + Offset.Y * row;
            }

            public static long? GetIrrigationEndTime(Farm farm, int fieldIdx)
            {
                return World.Core.GetSharedData<object>($"FARM::CFI_{farm.Id}_{fieldIdx}") is object obj ? Utils.Convert.ToInt64(obj) : (long?)null;
            }

            public void IrrigationEndTimeChanged(Farm farm, int fieldIdx, long? irrigTimeN)
            {
                if (farm.CropFields[fieldIdx].Colshape?.IsInside == true)
                    farm.CropFields[fieldIdx].UpdateLabels(farm, fieldIdx);
            }

            public static void OnSharedDataChanged(string dataKey, object value, object oldValue)
            {
                string[] data = dataKey.Split('_');

                var farm = All[int.Parse(data[1])] as Farm;

                var fieldIdx = int.Parse(data[2]);

                if (farm?.MainColshape.IsInside == true)
                    farm.CropFields[fieldIdx].IrrigationEndTimeChanged(farm, fieldIdx, value == null ? (long?)null : Utils.Convert.ToInt64(value));

                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData != null)
                {
                    var quest = Quest.GetPlayerQuest(pData, QuestTypes.JFRM2);

                    if (quest != null)
                        farm.UpdatePlaneIrrigatorData(quest);
                }
            }

            public void PreInitialize(Farm farm, int fieldIdx)
            {
                int fSubId = farm.SubId;

                for (byte i = 0; i < Columns.Length; i++)
                {
                    CropsData.Add(new List<CropData>());

                    for (byte j = 0; j < Columns[i].Count; j++)
                    {
                        var cropData = new CropData();

                        CropsData[i].Add(cropData);

                        float definedRot = _cropFieldsRotations.GetValueOrDefault(fSubId)?.GetValueOrDefault(fieldIdx) ?? float.PositiveInfinity;

                        cropData.RotationZ = definedRot == float.PositiveInfinity ? 360f * (float)Utils.Misc.Random.NextDouble() : definedRot;

                        World.Core.AddDataHandler($"FARM::CF_{farm.Id}_{fieldIdx}_{i}_{j}", CropData.OnSharedDataChanged);
                    }
                }

                World.Core.AddDataHandler($"FARM::CFI_{farm.Id}_{fieldIdx}", OnSharedDataChanged);
            }

            public void Initialize(Farm farm, int fieldIdx)
            {
                (float GrownZOffset, float SownZOffset, string Name, uint Model) cropTData = _cropTypesData[farm.CropFields[fieldIdx].Type];

                float x, y, z;

                for (byte i = 0; i < CropsData.Count; i++)
                {
                    for (byte j = 0; j < CropsData[i].Count; j++)
                    {
                        CropData cropData = CropsData[i][j];

                        if (CropData.GetGrowTime(farm, fieldIdx, i, j) is long growTime)
                        {
                            GetCropPosition2DNotSafe(i, j, out x, out y);

                            z = CropFieldsZCoords.GetValueOrDefault(farm.SubId)?.GetValueOrDefault(fieldIdx)?[i][j] ?? CoordZ;

                            float zOffset = growTime == 0 ? cropTData.GrownZOffset : cropTData.SownZOffset;

                            MapObject prop = Streaming.CreateObjectNoOffsetImmediately(cropTData.Model, x, y, z + zOffset);

                            prop.SetTotallyInvincible(true);

                            prop.FreezePosition(true);

                            prop.SetRotation(0f, 0f, cropData.RotationZ, 2, false);

                            cropData.MapObject = prop;
                        }
                    }
                }
            }

            public void Destroy()
            {
                for (byte i = 0; i < CropsData.Count; i++)
                {
                    for (byte j = 0; j < CropsData[i].Count; j++)
                    {
                        if (CropsData[i][j].MapObject != null)
                        {
                            CropsData[i][j].MapObject.Destroy();

                            CropsData[i][j].MapObject = null;
                        }
                    }
                }
            }

            public void OnEnter(Farm farm, int fieldIdx)
            {
                var posCs = new Vector3(Colshape.Position.X, Colshape.Position.Y, Colshape.Position.Z - 20f);

                MainLabel = new ExtraLabel(posCs, $"Поле #{farm.CropFields.IndexOf(this) + 1}", new RGBA(255, 255, 255, 255), 300f, 0, false, Settings.App.Static.MainDimension)
                {
                    Font = 7,
                    LOS = false,
                };

                posCs.Z -= 0.5f;

                InfoLabel = new ExtraLabel(posCs, "", new RGBA(255, 255, 255, 255), 300f, 0, false, Settings.App.Static.MainDimension)
                {
                    Font = 4,
                    LOS = false,
                };

                UpdateLabels(farm, fieldIdx);

                float csRange = farm.CropFields[fieldIdx].Type == Types.Wheat ? 2.5f : 0.75f;

                float x, y;

                for (byte i = 0; i < CropsData.Count; i++)
                {
                    for (byte j = 0; j < CropsData[i].Count; j++)
                    {
                        byte col = i;
                        byte row = j;

                        CropData cropData = CropsData[i][j];

                        GetCropPosition2DNotSafe(i, j, out x, out y);

                        var pos = new Vector3(x, y, CoordZ - 1.5f);

                        cropData.Colshape = new Cylinder(pos, csRange, 5f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                        {
                            OnEnter = (cancel) =>
                            {
                                CropField fieldData = farm.CropFields[fieldIdx];

                                float x, y, z;

                                fieldData.GetCropPosition3DNotSafe(col, row, out x, out y, out z);

                                float groundZ = z;

                                if (RAGE.Game.Misc.GetGroundZFor3dCoord(x, y, z + 1f, ref groundZ, true))
                                    z = groundZ;
                                else
                                    z += 0.5f;

                                CropData.TextLabel?.Destroy();

                                CropData.TextLabel =
                                    new ExtraLabel(new Vector3(x, y, z + 0.5f), "", new RGBA(255, 255, 255, 255), 300f, 0, false, Settings.App.Static.MainDimension)
                                    {
                                        Font = 4,
                                        LOS = false,
                                    };

                                fieldData.CropsData[col][row].GrowTimeChanged(farm, fieldIdx, col, row, CropData.GetGrowTime(farm, fieldIdx, col, row));
                            },
                            OnExit = (cancel) =>
                            {
                                if (CropData.TextLabel != null)
                                {
                                    CropData.TextLabel.Destroy();

                                    CropData.TextLabel = null;
                                }

                                if (CropData.Task != null)
                                {
                                    CropData.Task.Cancel();

                                    CropData.Task = null;
                                }

                                if (CropData.BindIdx >= 0)
                                {
                                    Input.Core.Unbind(CropData.BindIdx);

                                    CropData.BindIdx = -1;
                                }
                            },
                        };

                        //var blip = new Additional.ExtraBlip(1, pos, "", 0.35f, 2, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                        //prop.SetData("BP", blip);
                    }
                }
            }

            public void OnExit(Farm farm, int fieldIdx)
            {
                MainLabel?.Destroy();
                InfoLabel?.Destroy();

                MainLabel = null;
                InfoLabel = null;

                LabelTask?.Cancel();

                LabelTask = null;

                for (byte i = 0; i < CropsData.Count; i++)
                {
                    for (byte j = 0; j < CropsData[i].Count; j++)
                    {
                        CropData cropData = CropsData[i][j];

                        cropData.Colshape?.Destroy();

                        cropData.Colshape = null;
                    }
                }
            }

            public void UpdateLabels(Farm farm, int fieldIdx)
            {
                if (InfoLabel == null)
                    return;

                LabelTask?.Cancel();

                var allAmount = 0;

                int grownCount = 0, seedCount = 0;

                for (byte i = 0; i < CropsData.Count; i++)
                {
                    for (byte j = 0; j < CropsData[i].Count; j++)
                    {
                        allAmount++;

                        long? t = CropData.GetGrowTime(farm, fieldIdx, i, j);

                        if (t == 0)
                            grownCount++;
                        else if (t != null)
                            seedCount++;
                    }
                }

                (float GrownZOffset, float SownZOffset, string Name, uint Model) cropTData = _cropTypesData[Type];

                InfoLabel.Text =
                    $"Тип плодов - {cropTData.Name.ToLower()}\nСвободно {allAmount - (grownCount + seedCount)} из {allAmount} грядок\nСозрело {grownCount} плодов, а созревает - {seedCount}\n\n";

                if (GetIrrigationEndTime(farm, fieldIdx) is long irrigEndTime)
                {
                    DateTime irrigEndDateTime = DateTimeOffset.FromUnixTimeSeconds(irrigEndTime).DateTime;

                    LabelTask = new AsyncTask(() =>
                        {
                            if (InfoLabel == null)
                                return;

                            if (GetIrrigationEndTime(farm, fieldIdx) is long irrigEndTime && irrigEndTime != 0)
                            {
                                string curText = InfoLabel.Text;

                                int startIdx = curText.LastIndexOf('\n');

                                if (startIdx > 0)
                                    InfoLabel.Text = curText.Substring(0, startIdx + 1) + $"Почва орошена еще {irrigEndDateTime.Subtract(World.Core.ServerTime).GetBeautyString()}";
                            }
                        },
                        1000,
                        true,
                        0
                    );

                    LabelTask.Run();
                }
                else
                {
                    LabelTask = null;

                    InfoLabel.Text += $"Почва не орошена";
                }
            }

            public class CropData
            {
                public static DateTime LastSent;
                public float RotationZ { get; set; }

                public MapObject MapObject { get; set; }

                public ExtraColshape Colshape { get; set; }

                public static ExtraLabel TextLabel { get; set; }

                public static AsyncTask Task { get; set; }

                public static int BindIdx { get; set; } = -1;

                public static long? GetGrowTime(Farm farm, int fieldIdx, byte col, byte row)
                {
                    return World.Core.GetSharedData<object>($"FARM::CF_{farm.Id}_{fieldIdx}_{col}_{row}", null) is object obj ? Utils.Convert.ToInt64(obj) : (long?)null;
                }

                public static void OnSharedDataChanged(string dataKey, object value, object oldValue)
                {
                    string[] data = dataKey.Split('_');

                    var farm = All[int.Parse(data[1])] as Farm;

                    var fieldIdx = int.Parse(data[2]);

                    if (farm?.MainColshape.IsInside == true)
                    {
                        var col = byte.Parse(data[3]);
                        var row = byte.Parse(data[4]);

                        CropData cropData = farm.CropFields[fieldIdx].CropsData[col][row];

                        cropData.GrowTimeChanged(farm, fieldIdx, col, row, value == null ? (long?)null : Utils.Convert.ToInt64(value));
                    }

                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData != null && farm.CropFields[fieldIdx].Type == Types.Wheat)
                    {
                        var quest = Quest.GetPlayerQuest(pData, QuestTypes.JFRM1);

                        if (quest != null)
                            farm.UpdateTractorTakerData(quest);
                    }
                }

                public void GrowTimeChanged(Farm farm, int fieldIdx, byte col, byte row, long? growTimeN)
                {
                    if (farm.CropFields[fieldIdx].Colshape?.IsInside == true)
                        farm.CropFields[fieldIdx].UpdateLabels(farm, fieldIdx);

                    if (growTimeN is long growTime)
                    {
                        if (growTime == 0)
                        {
                            if (Colshape?.IsInside == true)
                            {
                                if (Task != null)
                                {
                                    Task.Cancel();

                                    Task = null;
                                }

                                if (TextLabel != null)
                                {
                                    if (BindIdx >= 0)
                                    {
                                        Input.Core.Unbind(BindIdx);

                                        BindIdx = -1;
                                    }

                                    if (farm.CropFields[fieldIdx].Type != Types.Wheat)
                                    {
                                        BindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.E, false, () => ProcessCrop(farm, fieldIdx, col, row));

                                        TextLabel.Text = "Плод созрел\nНажмите E, чтобы собрать его";
                                    }
                                    else
                                    {
                                        TextLabel.Text = "Плод созрел\nЧтобы собрать его, воспользуйтесь комбайном";
                                    }
                                }
                            }

                            if (MapObject == null)
                            {
                                float x, y, z;

                                CropField field = farm.CropFields[fieldIdx];

                                (float GrownZOffset, float SownZOffset, string Name, uint Model) cropTData = _cropTypesData[field.Type];

                                field.GetCropPosition2DNotSafe(col, row, out x, out y);

                                z = CropFieldsZCoords.GetValueOrDefault(farm.SubId)?.GetValueOrDefault(fieldIdx)?[col][row] ?? field.CoordZ;

                                float zOffset = cropTData.GrownZOffset;

                                MapObject prop = Streaming.CreateObjectNoOffsetImmediately(cropTData.Model, x, y, z + zOffset);

                                MapObject = prop;

                                prop.SetTotallyInvincible(true);

                                prop.FreezePosition(true);

                                MapObject.SetRotation(0f, 0f, RotationZ, 2, false);
                            }
                            else
                            {
                                CropField field = farm.CropFields[fieldIdx];

                                (float GrownZOffset, float SownZOffset, string Name, uint Model) cropTData = _cropTypesData[field.Type];

                                float x, y, z;

                                field.GetCropPosition2DNotSafe(col, row, out x, out y);

                                z = CropFieldsZCoords.GetValueOrDefault(farm.SubId)?.GetValueOrDefault(fieldIdx)?[col][row] ?? field.CoordZ;

                                float zOffset = cropTData.GrownZOffset;

                                MapObject.SetCoordsNoOffset(x, y, z + zOffset, false, false, false);
                            }
                        }
                        else
                        {
                            if (Colshape?.IsInside == true)
                            {
                                if (Task != null)
                                    Task.Cancel();

                                if (TextLabel != null)
                                {
                                    if (BindIdx >= 0)
                                    {
                                        Input.Core.Unbind(BindIdx);

                                        BindIdx = -1;
                                    }

                                    DateTime growDateTime = DateTimeOffset.FromUnixTimeSeconds(growTime).DateTime;

                                    Task = new AsyncTask(() =>
                                        {
                                            if (TextLabel == null)
                                                return;

                                            if (GetGrowTime(farm, fieldIdx, col, row) is long growTime && growTime != 0)
                                                TextLabel.Text = $"Грядка засеяна, до созревания плода осталось\n{growDateTime.Subtract(World.Core.ServerTime).GetBeautyString()}";
                                        },
                                        1000,
                                        true,
                                        0
                                    );

                                    Task.Run();
                                }

                                if (MapObject == null)
                                {
                                    float x, y, z;

                                    CropField field = farm.CropFields[fieldIdx];

                                    (float GrownZOffset, float SownZOffset, string Name, uint Model) cropTData = _cropTypesData[field.Type];

                                    field.GetCropPosition2DNotSafe(col, row, out x, out y);

                                    z = CropFieldsZCoords.GetValueOrDefault(farm.SubId)?.GetValueOrDefault(fieldIdx)?[col][row] ?? field.CoordZ;

                                    float zOffset = cropTData.SownZOffset;

                                    MapObject prop = Streaming.CreateObjectNoOffsetImmediately(cropTData.Model, x, y, z + zOffset);

                                    MapObject = prop;

                                    prop.SetTotallyInvincible(true);

                                    prop.FreezePosition(true);

                                    MapObject.SetRotation(0f, 0f, RotationZ, 2, false);
                                }
                                else
                                {
                                    CropField field = farm.CropFields[fieldIdx];

                                    (float GrownZOffset, float SownZOffset, string Name, uint Model) cropTData = _cropTypesData[field.Type];

                                    float x, y, z;

                                    field.GetCropPosition2DNotSafe(col, row, out x, out y);

                                    z = CropFieldsZCoords.GetValueOrDefault(farm.SubId)?.GetValueOrDefault(fieldIdx)?[col][row] ?? field.CoordZ;

                                    float zOffset = cropTData.SownZOffset;

                                    MapObject.SetCoordsNoOffset(x, y, z + zOffset, false, false, false);

                                    MapObject.SetRotation(0f, 0f, RotationZ, 2, false);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (MapObject != null)
                        {
                            MapObject.Destroy();

                            MapObject = null;
                        }

                        if (Colshape?.IsInside == true)
                            if (TextLabel != null)
                            {
                                if (BindIdx >= 0)
                                {
                                    Input.Core.Unbind(BindIdx);

                                    BindIdx = -1;
                                }

                                BindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.E, false, () => ProcessCrop(farm, fieldIdx, col, row));

                                TextLabel.Text = "Грядка пустая\nНажмите E, чтобы засеять её";
                            }
                    }
                }

                public static async void ProcessCrop(Farm farm, int fieldIdx, byte col, byte row)
                {
                    if (Cursor.IsVisible)
                        return;

                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    var farmJob = pData.CurrentJob as Farmer;

                    if (farmJob == null || farmJob.FarmBusiness != farm)
                    {
                        Notification.ShowError("Вы не работаете на этой ферме!");

                        return;
                    }

                    if (PlayerActions.IsAnyActionActive(true,
                            PlayerActions.Types.Knocked,
                            PlayerActions.Types.Frozen,
                            PlayerActions.Types.Cuffed,
                            PlayerActions.Types.Animation,
                            PlayerActions.Types.Scenario,
                            PlayerActions.Types.FastAnimation,
                            PlayerActions.Types.InVehicle,
                            PlayerActions.Types.Shooting,
                            PlayerActions.Types.Reloading,
                            PlayerActions.Types.Climbing,
                            PlayerActions.Types.Falling,
                            PlayerActions.Types.Ragdoll,
                            PlayerActions.Types.Jumping,
                            PlayerActions.Types.NotOnFoot,
                            PlayerActions.Types.IsSwimming,
                            PlayerActions.Types.HasItemInHands,
                            PlayerActions.Types.IsAttachedTo
                        ))
                        return;

                    if (LastSent.IsSpam(500))
                        return;

                    LastSent = World.Core.ServerTime;

                    var res = (int)await Events.CallRemoteProc("Job::FARM::CP", fieldIdx, col, row);

                    if (res == byte.MaxValue)
                    {
                        float x, y;

                        farm.CropFields[fieldIdx].GetCropPosition2DNotSafe(col, row, out x, out y);

                        Vector3 pPos = Player.LocalPlayer.Position;

                        Player.LocalPlayer.SetHeading(Geometry.RadiansToDegrees((float)System.Math.Atan2(y - pPos.Y, x - pPos.X)) - 90f);
                    }
                    else
                    {
                        if (res == 1)
                            Notification.ShowError("Кто-то уже работает с этим растением!");
                    }
                }
            }
        }

        public class OrangeTreeData
        {
            public OrangeTreeData()
            {
            }

            [JsonProperty(PropertyName = "P")]
            public Vector3 Position { get; set; }

            public ExtraColshape Colshape { get; set; }

            public static AsyncTask Task { get; set; }

            public static AsyncTask TextTask { get; set; }

            public static string Text { get; set; }

            public static int BindIdx { get; set; } = -1;

            public static void PreInitialize(Farm farm)
            {
                for (var i = 0; i < farm.OrangeTrees.Count; i++)
                {
                    World.Core.AddDataHandler($"FARM::OT_{farm.Id}_{i}", OnSharedDataChanged);
                }
            }

            public static void OnSharedDataChanged(string dataKey, object value, object oldValue)
            {
                string[] data = dataKey.Split('_');

                var farm = All[int.Parse(data[1])] as Farm;

                if (farm?.MainColshape.IsInside != true)
                    return;

                var idx = int.Parse(data[2]);

                farm.OrangeTrees[idx].GrowTimeChanged(farm, idx, value == null ? (long?)null : Utils.Convert.ToInt64(value));
            }

            public static void Initialize(Farm farm, int idx)
            {
                OrangeTreeData orangeTreeData = farm.OrangeTrees[idx];

                orangeTreeData.Colshape = new Cylinder(orangeTreeData.Position, 2.5f, 5f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                {
                    OnEnter = (cancel) =>
                    {
                        TextTask?.Cancel();

                        Text = "";

                        var pos = new Vector3(orangeTreeData.Colshape.Position.X, orangeTreeData.Colshape.Position.Y, orangeTreeData.Colshape.Position.Z + 1f);

                        float x = 0f, y = 0f;

                        TextTask = new AsyncTask(() =>
                            {
                                if (Graphics.GetScreenCoordFromWorldCoord(pos, ref x, ref y))
                                    Graphics.DrawText(Text, x, y, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                            },
                            0,
                            true,
                            0
                        );

                        TextTask.Run();

                        orangeTreeData.GrowTimeChanged(farm, idx, GetGrowTime(farm, idx));
                    },
                    OnExit = (cancel) =>
                    {
                        if (TextTask != null)
                        {
                            TextTask.Cancel();

                            TextTask = null;
                        }

                        Text = null;

                        if (Task != null)
                        {
                            Task.Cancel();

                            Task = null;
                        }

                        if (BindIdx >= 0)
                        {
                            Input.Core.Unbind(BindIdx);

                            BindIdx = -1;
                        }
                    },
                };
            }

            public static long? GetGrowTime(Farm farm, int idx)
            {
                return World.Core.GetSharedData<object>($"FARM::OT_{farm.Id}_{idx}") is object obj ? Utils.Convert.ToInt64(obj) : (long?)null;
            }

            public void GrowTimeChanged(Farm farm, int idx, long? growTimeN)
            {
                if (growTimeN is long growTime)
                {
                    if (growTime == 0)
                    {
                        if (Colshape?.IsInside == true)
                        {
                            if (Task != null)
                            {
                                Task.Cancel();

                                Task = null;
                            }

                            if (Text != null)
                            {
                                if (BindIdx >= 0)
                                {
                                    Input.Core.Unbind(BindIdx);

                                    BindIdx = -1;
                                }

                                BindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.E, false, () => ProcessTree(farm, idx));

                                Text = "Апельсины созрели\nНажмите E, чтобы собрать их";
                            }
                        }
                    }
                    else
                    {
                        if (Colshape?.IsInside == true)
                        {
                            if (Task != null)
                                Task.Cancel();

                            if (Text != null)
                            {
                                if (BindIdx >= 0)
                                {
                                    Input.Core.Unbind(BindIdx);

                                    BindIdx = -1;
                                }

                                DateTime growDateTime = DateTimeOffset.FromUnixTimeSeconds(growTime).DateTime;

                                Task = new AsyncTask(() =>
                                    {
                                        if (Text == null)
                                            return;

                                        if (GetGrowTime(farm, idx) is long growTime && growTime != 0)
                                            Text = $"До созревания апельсинов осталось\n{growDateTime.Subtract(World.Core.ServerTime).GetBeautyString()}";
                                    },
                                    1000,
                                    true,
                                    0
                                );

                                Task.Run();
                            }
                        }
                    }
                }
                else
                {
                    if (Colshape?.IsInside == true)
                        if (Text != null)
                        {
                            if (BindIdx >= 0)
                            {
                                Input.Core.Unbind(BindIdx);

                                BindIdx = -1;
                            }

                            BindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.E, false, () => ProcessTree(farm, idx));

                            Text = "Дерево не полито водой\nНажмите E, чтобы полить его";
                        }
                }
            }

            public static async void ProcessTree(Farm farm, int idx)
            {
                if (Cursor.IsVisible)
                    return;

                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var farmJob = pData.CurrentJob as Farmer;

                if (farmJob == null || farmJob.FarmBusiness != farm)
                {
                    Notification.ShowError("Вы не работаете на этой ферме!");

                    return;
                }

                if (PlayerActions.IsAnyActionActive(true,
                        PlayerActions.Types.Knocked,
                        PlayerActions.Types.Frozen,
                        PlayerActions.Types.Cuffed,
                        PlayerActions.Types.Animation,
                        PlayerActions.Types.Scenario,
                        PlayerActions.Types.FastAnimation,
                        PlayerActions.Types.InVehicle,
                        PlayerActions.Types.Shooting,
                        PlayerActions.Types.Reloading,
                        PlayerActions.Types.Climbing,
                        PlayerActions.Types.Falling,
                        PlayerActions.Types.Ragdoll,
                        PlayerActions.Types.Jumping,
                        PlayerActions.Types.NotOnFoot,
                        PlayerActions.Types.IsSwimming,
                        PlayerActions.Types.HasItemInHands,
                        PlayerActions.Types.IsAttachedTo
                    ))
                    return;

                if (CropField.CropData.LastSent.IsSpam(500))
                    return;

                CropField.CropData.LastSent = World.Core.ServerTime;

                var res = (int)await Events.CallRemoteProc("Job::FARM::OTP", idx);

                if (res == byte.MaxValue)
                {
                    Vector3 pPos = Player.LocalPlayer.Position;

                    Player.LocalPlayer.SetHeading(
                        Geometry.RadiansToDegrees((float)System.Math.Atan2(farm.OrangeTrees[idx].Position.Y - pPos.Y, farm.OrangeTrees[idx].Position.X - pPos.X)) - 90f
                    );
                }
                else
                {
                    if (res == 1)
                        Notification.ShowError("Кто-то уже работает с этим растением!");
                }
            }
        }

        public class CowData
        {
            public Ped Ped { get; set; }

            public ExtraColshape Colshape { get; set; }

            public static AsyncTask Task { get; set; }

            public static ExtraLabel TextLabel { get; set; }

            public static int BindIdx { get; set; }

            [JsonProperty(PropertyName = "P")]
            public Vector4 Position { get; set; }

            public static void PreInitialize(Farm farm)
            {
                for (var i = 0; i < farm.Cows.Count; i++)
                {
                    World.Core.AddDataHandler($"FARM::COW_{farm.Id}_{i}", OnSharedDataChanged);
                }
            }

            public static void OnSharedDataChanged(string dataKey, object value, object oldValue)
            {
                string[] data = dataKey.Split('_');

                var farm = All[int.Parse(data[1])] as Farm;

                if (farm?.MainColshape.IsInside != true)
                    return;

                var idx = int.Parse(data[2]);

                farm.Cows[idx].GrowTimeChanged(farm, idx, value == null ? (long?)null : Utils.Convert.ToInt64(value));
            }

            public static long? GetGrowTime(Farm farm, int idx)
            {
                return World.Core.GetSharedData<object>($"FARM::COW_{farm.Id}_{idx}") is object obj ? Utils.Convert.ToInt64(obj) : (long?)null;
            }

            public async void GrowTimeChanged(Farm farm, int idx, long? growTimeN)
            {
                if (growTimeN is long growTime)
                {
                    if (growTime == 0)
                    {
                        if (Ped != null)
                            Ped.ClearTasks();

                        if (Colshape?.IsInside == true)
                        {
                            if (Task != null)
                            {
                                Task.Cancel();

                                Task = null;
                            }

                            if (TextLabel != null)
                            {
                                if (BindIdx >= 0)
                                {
                                    Input.Core.Unbind(BindIdx);

                                    BindIdx = -1;
                                }

                                BindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.E, false, () => ProcessCow(farm, idx));

                                TextLabel.Text = "Корова готова дать молоко\nНажмите E, чтобы подоить её";
                            }
                        }
                    }
                    else
                    {
                        if (Ped != null)
                        {
                            await Streaming.RequestAnimDict("creatures@cow@amb@world_cow_grazing@base");

                            Ped.TaskPlayAnim("creatures@cow@amb@world_cow_grazing@base", "base", 2f, 2f, -1, 1, 0f, false, false, false);
                        }

                        if (Colshape?.IsInside == true)
                        {
                            if (Task != null)
                                Task.Cancel();

                            if (TextLabel != null)
                            {
                                if (BindIdx >= 0)
                                {
                                    Input.Core.Unbind(BindIdx);

                                    BindIdx = -1;
                                }

                                DateTime growDateTime = DateTimeOffset.FromUnixTimeSeconds(growTime).DateTime;

                                Task = new AsyncTask(() =>
                                    {
                                        if (TextLabel == null)
                                            return;

                                        if (GetGrowTime(farm, idx) is long growTime && growTime != 0)
                                            TextLabel.Text = $"Корова ест траву, будет готова дать молоко через\n{growDateTime.Subtract(World.Core.ServerTime).GetBeautyString()}";
                                    },
                                    1000,
                                    true,
                                    0
                                );

                                Task.Run();
                            }
                        }
                    }
                }
            }

            public static async void ProcessCow(Farm farm, int idx)
            {
                if (Cursor.IsVisible)
                    return;

                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var farmJob = pData.CurrentJob as Farmer;

                if (farmJob == null || farmJob.FarmBusiness != farm)
                {
                    Notification.ShowError("Вы не работаете на этой ферме!");

                    return;
                }

                if (PlayerActions.IsAnyActionActive(true,
                        PlayerActions.Types.Knocked,
                        PlayerActions.Types.Frozen,
                        PlayerActions.Types.Cuffed,
                        PlayerActions.Types.Animation,
                        PlayerActions.Types.Scenario,
                        PlayerActions.Types.FastAnimation,
                        PlayerActions.Types.InVehicle,
                        PlayerActions.Types.Shooting,
                        PlayerActions.Types.Reloading,
                        PlayerActions.Types.Climbing,
                        PlayerActions.Types.Falling,
                        PlayerActions.Types.Ragdoll,
                        PlayerActions.Types.Jumping,
                        PlayerActions.Types.NotOnFoot,
                        PlayerActions.Types.IsSwimming,
                        PlayerActions.Types.HasItemInHands,
                        PlayerActions.Types.IsAttachedTo
                    ))
                    return;

                if (CropField.CropData.LastSent.IsSpam(500))
                    return;

                CropField.CropData.LastSent = World.Core.ServerTime;

                uint emptyBucketModel = RAGE.Util.Joaat.Hash("brp_p_farm_bucket_0");

                var res = (int)await Events.CallRemoteProc("Job::FARM::COWP", idx);

                if (res == byte.MaxValue)
                {
                    Vector3 playerPos = Player.LocalPlayer.Position;

                    Vector3 cowLeftPos = farm.Cows[idx].Ped.GetOffsetFromInWorldCoords(-0.5f, -0.5f, 0f);
                    Vector3 cowRightPos = farm.Cows[idx].Ped.GetOffsetFromInWorldCoords(0.5f, -0.5f, 0f);

                    Vector3 pPos = playerPos.DistanceTo2D(cowLeftPos) < playerPos.DistanceTo2D(cowRightPos) ? cowLeftPos : cowRightPos;

                    Vector3 tPos = farm.Cows[idx].Ped.GetOffsetFromInWorldCoords(0f, -0.5f, -1f);

                    Player.LocalPlayer.Position = pPos;

                    Animations.Service.Play(Player.LocalPlayer, GeneralType.MilkCow0);

                    Player.LocalPlayer.SetHeading(Geometry.RadiansToDegrees((float)System.Math.Atan2(tPos.Y - pPos.Y, tPos.X - pPos.X)) - 90f);

                    Player.LocalPlayer.GetData<MapObject>("FARMAT::TEMPBUCKET0")?.Destroy();

                    MapObject tempBucket = Streaming.CreateObjectNoOffsetImmediately(emptyBucketModel, tPos.X, tPos.Y, tPos.Z);

                    Player.LocalPlayer.SetData("FARMAT::TEMPBUCKET0", tempBucket);
                }
                else
                {
                    if (res == 1)
                        Notification.ShowError("Кто-то уже доет эту корову!");
                }
            }

            public static void Initialize(Farm farm, int idx)
            {
                CowData cow = farm.Cows[idx];

                cow.Ped?.Destroy();

                cow.Ped = new Ped(_cropTypesData[CropField.Types.Cow].Model,
                    farm.Cows[idx].Position.Position,
                    farm.Cows[idx].Position.RotationZ,
                    Settings.App.Static.MainDimension
                );
                cow.Ped.StreamInCustomActionsAdd(CowStreamInAction);

                cow.Ped.SetData("LGT", GetGrowTime(farm, idx));

                cow.Colshape?.Destroy();

                cow.Colshape = new Cylinder(new Vector3(cow.Position.X, cow.Position.Y, cow.Position.Z - 1f),
                    1.5f,
                    2.5f,
                    false,
                    Utils.Misc.RedColor,
                    Settings.App.Static.MainDimension,
                    null
                )
                {
                    OnEnter = (cancel) =>
                    {
                        TextLabel?.Destroy();

                        TextLabel = new ExtraLabel(cow.Ped.GetCoords(false), "", new RGBA(255, 255, 255, 255), 300f, 0, false, Settings.App.Static.MainDimension)
                        {
                            Font = 4,
                            LOS = false,
                        };

                        cow.GrowTimeChanged(farm, idx, GetGrowTime(farm, idx));
                    },
                    OnExit = (cancel) =>
                    {
                        if (TextLabel != null)
                        {
                            TextLabel.Destroy();

                            TextLabel = null;
                        }

                        if (Task != null)
                        {
                            Task.Cancel();

                            Task = null;
                        }

                        if (BindIdx >= 0)
                        {
                            Input.Core.Unbind(BindIdx);

                            BindIdx = -1;
                        }
                    },
                };
            }

            private static async void CowStreamInAction(Entity entity)
            {
                var ped = entity as Ped;

                if (ped == null)
                    return;

                ped.SetComponentVariation(0, 0, 2, 2);
                ped.SetComponentVariation(3, 1, 0, 2);

                long? lastGrowTime = ped.GetData<long?>("LGT");

                ped.ResetData("LGT");

                if (lastGrowTime != null && lastGrowTime != 0)
                {
                    await Streaming.RequestAnimDict("creatures@cow@amb@world_cow_grazing@base");

                    ped.TaskPlayAnim("creatures@cow@amb@world_cow_grazing@base", "base", 2f, 2f, -1, 1, 0f, false, false, false);
                }
            }
        }
    }
}