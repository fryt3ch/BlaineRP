using Newtonsoft.Json;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;

namespace BCRPClient.Data
{
    public partial class Locations : Events.Script
    {
        public class Farm : Business
        {
            public class CropField
            {
                public class CropData
                {
                    public float RotationZ { get; set; }

                    public MapObject MapObject { get; set; }

                    public Additional.ExtraColshape Colshape { get; set; }

                    public static TextLabel TextLabel { get; set; }

                    public static AsyncTask Task { get; set; }

                    public static int BindIdx { get; set; } = -1;

                    public static DateTime LastSent;

                    public static long? GetGrowTime(Farm farm, int fieldIdx, byte row, byte col) => Sync.World.GetSharedData<object>(GetSharedDataKey(farm, fieldIdx, row, col), null) is object obj ? Convert.ToInt64(obj) : (long?)null;

                    public static string GetSharedDataKey(Farm farm, int fieldIdx, byte row, byte col) => $"FARM::CF_{farm.Id}_{fieldIdx}_{row}_{col}";

                    public static void OnSharedDataChanged(string dataKey, object value, object oldValue)
                    {
                        var data = dataKey.Split('_');

                        var farm = Business.All[int.Parse(data[1])] as Farm;

                        if (farm?.MainColshape.IsInside != true)
                            return;

                        var fieldIdx = int.Parse(data[2]);
                        var row = byte.Parse(data[3]);
                        var col = byte.Parse(data[4]);

                        var cropData = farm.CropFields[fieldIdx].CropsData[row][col];

                        cropData.GrowTimeChanged(farm, fieldIdx, row, col, value == null ? (long?)null : Convert.ToInt64(value));
                    }

                    public void GrowTimeChanged(Farm farm, int fieldIdx, byte row, byte col, long? growTimeN)
                    {
                        if (farm.CropFields[fieldIdx].Colshape?.IsInside == true)
                        {
                            farm.CropFields[fieldIdx].UpdateLabels(farm, fieldIdx);
                        }

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
                                            KeyBinds.Unbind(BindIdx);

                                        BindIdx = KeyBinds.Bind(RAGE.Ui.VirtualKeys.E, false, () => ProcessCrop(farm, fieldIdx, row, col));

                                        TextLabel.Text = "Плод созрел\nНажмите E, чтобы собрать его";
                                    }
                                }

                                if (MapObject == null)
                                {
                                    float x, y;

                                    var field = farm.CropFields[fieldIdx];

                                    field.GetCropPosition2DNotSafe(row, col, out x, out y);

                                    var prop = new RAGE.Elements.MapObject(RAGE.Game.Object.CreateObjectNoOffset(CropModels[field.Type], x, y, field.CoordZ, false, false, false));

                                    prop.SetDisableFragDamage(true);
                                    prop.SetInvincible(true);
                                    prop.SetCanBeDamaged(false);

                                    prop.FreezePosition(true);

                                    prop.SetHeading(RotationZ);
                                }
                            }
                            else
                            {
                                if (Colshape?.IsInside == true)
                                {
                                    if (Task != null)
                                    {
                                        Task.Cancel();
                                    }

                                    if (TextLabel != null)
                                    {
                                        if (BindIdx >= 0)
                                            KeyBinds.Unbind(BindIdx);

                                        var growDateTime = DateTimeOffset.FromUnixTimeSeconds(growTime).DateTime;

                                        Task = new AsyncTask(() =>
                                        {
                                            if (TextLabel == null)
                                                return;

                                            if (GetGrowTime(farm, fieldIdx, row, col) is long growTime && growTime != 0)
                                            {
                                                TextLabel.Text = $"Грядка засеяна, до созревания плода осталось\n{growDateTime.Subtract(Sync.World.ServerTime).GetBeautyString()}";
                                            }
                                        }, 1000, true, 0);

                                        Task.Run();
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
                            {
                                if (TextLabel != null)
                                {
                                    if (BindIdx >= 0)
                                        KeyBinds.Unbind(BindIdx);

                                    BindIdx = KeyBinds.Bind(RAGE.Ui.VirtualKeys.E, false, () => ProcessCrop(farm, fieldIdx, row, col));

                                    TextLabel.Text = "Грядка пустая\nНажмите E, чтобы засеять её";
                                }
                            }
                        }
                    }

                    public void OnEnter(Farm farm, int fieldIdx, byte row, byte col)
                    {
                        var fieldData = farm.CropFields[fieldIdx];

                        float x, y, z;

                        fieldData.GetCropPosition3DNotSafe(row, col, out x, out y, out z);

                        TextLabel?.Destroy();

                        TextLabel = new TextLabel(new Vector3(x, y, z + 0.25f), "", new RGBA(255, 255, 255, 255), 300f, 0, false, Settings.MAIN_DIMENSION) {  Font = 4, LOS = false };

                        GrowTimeChanged(farm, fieldIdx, row, col, GetGrowTime(farm, fieldIdx, row, col));
                    }

                    public void OnExit(Farm farm, int fieldIdx, byte row, byte col)
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
                            KeyBinds.Unbind(BindIdx);
                    }

                    public static async void ProcessCrop(Farm farm, int fieldIdx, byte row, byte col)
                    {
                        var pData = Sync.Players.GetData(Player.LocalPlayer);

                        if (pData == null)
                            return;

                        var farmJob = pData.CurrentJob as Data.Jobs.Farmer;

                        if (farmJob == null || farmJob.FarmBusiness != farm)
                        {
                            CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, "Вы не работаете на этой ферме!");

                            return;
                        }

                        if (!Utils.CanDoSomething(true, Utils.Actions.Knocked, Utils.Actions.Frozen, Utils.Actions.Cuffed, Utils.Actions.Animation, Utils.Actions.Scenario, Utils.Actions.FastAnimation, Utils.Actions.InVehicle, Utils.Actions.Shooting, Utils.Actions.Reloading, Utils.Actions.Climbing, Utils.Actions.Falling, Utils.Actions.Ragdoll, Utils.Actions.Jumping, Utils.Actions.NotOnFoot, Utils.Actions.IsSwimming, Utils.Actions.HasItemInHands, Utils.Actions.IsAttachedTo))
                            return;

                        if (LastSent.IsSpam(500))
                            return;

                        LastSent = Sync.World.ServerTime;

                        var res = (int)await Events.CallRemoteProc("Job::FARM::CP", fieldIdx, row, col);

                        if (res == byte.MaxValue)
                        {
                            float x, y;

                            farm.CropFields[fieldIdx].GetCropPosition2DNotSafe(row, col, out x, out y);

                            var pPos = Player.LocalPlayer.Position;

                            Player.LocalPlayer.SetHeading(Utils.RadiansToDegrees((float)Math.Atan2(y - pPos.Y, x - pPos.X)) - 90f);
                        }
                        else
                        {
                            if (res == 1)
                            {
                                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, "Кто-то уже работает с этим растением!");
                            }
                        }
                    }
                }

                public enum Types
                {
                    Cabbage = 0,
                    Pumpkin,
                }

                public static Dictionary<Types, uint> CropModels = new Dictionary<Types, string>()
                {
                    { Types.Cabbage, "prop_veg_crop_03_cab" },
                    { Types.Pumpkin, "prop_veg_crop_03_pump" },
                }.ToDictionary(x => x.Key, x => RAGE.Util.Joaat.Hash(x.Value));

                public static TextLabel MainLabel { get; set; }
                public static TextLabel InfoLabel { get; set; }

                public Additional.ExtraColshape Colshape { get; set; }

                [JsonProperty(PropertyName ="T")]
                public Types Type { get; set; }

                [JsonProperty(PropertyName = "CZ")]
                public float CoordZ { get; set; }

                [JsonProperty(PropertyName = "C")]
                public RAGE.Ui.Cursor.Vector2[] Columns { get; set; }

                [JsonProperty(PropertyName = "O")]
                public RAGE.Ui.Cursor.Vector2 Offset { get; set; }

                [JsonProperty(PropertyName = "RC")]
                public byte RowsCount { get; set; }

                public List<List<CropData>> CropsData { get; set; } = new List<List<CropData>>();

                public Vector3 GetCropPosition3D(byte row, byte col) => row >= RowsCount || col >= Columns.Length ? null : new Vector3(Columns[col].X + Offset.X * row, Columns[col].Y + Offset.Y * row, CoordZ);

                public void GetCropPosition3DNotSafe(byte row, byte col, out float x, out float y, out float z)
                {
                    x = Columns[col].X + Offset.X * row;
                    y = Columns[col].Y + Offset.Y * row;

                    z = CoordZ;
                }

                public RAGE.Ui.Cursor.Vector2 GetCropPosition2D(byte row, byte col) => row >= RowsCount || col >= Columns.Length ? null : new RAGE.Ui.Cursor.Vector2(Columns[col].X + Offset.X * row, Columns[col].Y + Offset.Y * row);

                public void GetCropPosition2DNotSafe(byte row, byte col, out float x, out float y)
                {
                    x = Columns[col].X + Offset.X * row;
                    y = Columns[col].Y + Offset.Y * row;
                }

                public void PreInitialize(Farm farm, int fieldIdx)
                {
                    for (byte i = 0; i < RowsCount; i++)
                    {
                        CropsData.Add(new List<CropData>());

                        for (byte j = 0; j < Columns.Length; j++)
                        {
                            var cropData = new CropData();

                            CropsData[i].Add(cropData);

                            cropData.RotationZ = 360f * (float)Utils.Random.NextDouble();

                            Sync.World.AddDataHandler(CropData.GetSharedDataKey(farm, fieldIdx, i, j), CropData.OnSharedDataChanged);
                        }
                    }
                }

                public void Initialize(Farm farm, int fieldIdx)
                {
                    var propHash = CropModels[Type];

                    float x, y;

                    for (byte i = 0; i < CropsData.Count; i++)
                    {
                        for (byte j = 0; j < CropsData[i].Count; j++)
                        {
                            var cropData = CropsData[i][j];

                            if (CropData.GetGrowTime(farm, fieldIdx, i, j) == 0)
                            {
                                GetCropPosition2DNotSafe(i, j, out x, out y);

                                var prop = new RAGE.Elements.MapObject(RAGE.Game.Object.CreateObjectNoOffset(propHash, x, y, CoordZ, false, false, false));

                                //prop.SetCollision(false, true);

                                prop.SetDisableFragDamage(true);
                                prop.SetInvincible(true);
                                prop.SetCanBeDamaged(false);

                                prop.FreezePosition(true);

                                prop.SetHeading(cropData.RotationZ);

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

                    MainLabel = new TextLabel(posCs, $"Поле #{farm.CropFields.IndexOf(this) + 1}", new RGBA(255, 255, 255, 255), 300f, 0, false, Settings.MAIN_DIMENSION) { Font = 7, LOS = false };

                    posCs.Z -= 0.5f;

                    InfoLabel = new TextLabel(posCs, "", new RGBA(255, 255, 255, 255), 300f, 0, false, Settings.MAIN_DIMENSION) { Font = 4, LOS = false };

                    UpdateLabels(farm, fieldIdx);

                    float x, y;

                    for (byte i = 0; i < CropsData.Count; i++)
                    {
                        for (byte j = 0; j < CropsData[i].Count; j++)
                        {
                            var row = i;
                            var col = j;

                            var cropData = CropsData[i][j];

                            GetCropPosition2DNotSafe(i, j, out x, out y);

                            var pos = new Vector3(x, y, CoordZ - 0.5f);

                            cropData.Colshape = new Additional.Cylinder(pos, 0.75f, 2.5f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                            {
                                OnEnter = (cancel) => cropData.OnEnter(farm, fieldIdx, row, col),

                                OnExit = (cancel) => cropData.OnExit(farm, fieldIdx, row, col),
                            };

                            //var blip = new Blip(1, pos, "", 0.35f, 2, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

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

                    for (byte i = 0; i < CropsData.Count; i++)
                    {
                        for (byte j = 0; j < CropsData[i].Count; j++)
                        {
                            var cropData = CropsData[i][j];

                            cropData.Colshape?.Destroy();

                            cropData.Colshape = null;
                        }
                    }
                }

                public void UpdateLabels(Farm farm, int fieldIdx)
                {
                    if (InfoLabel == null)
                        return;

                    var allAmount = RowsCount * Columns.Length;

                    int grownCount = 0, seedCount = 0;

                    for (byte i = 0; i < CropsData.Count; i++)
                        for (byte j = 0; j < CropsData[i].Count; j++)
                        {
                            var t = CropData.GetGrowTime(farm, fieldIdx, i, j);

                            if (t == 0)
                                grownCount++;
                            else if (t != null)
                                seedCount++;
                        }

                    InfoLabel.Text = $"Тип плодов - {(Type == Types.Cabbage ? "капуста" : "тыква")}\nСвободно {allAmount - (grownCount + seedCount)} из {allAmount} грядок\nСозрело {grownCount} плодов, а созревает - {seedCount}";
                }
            }

            public Additional.ExtraColshape MainColshape { get; set; }

            public List<CropField> CropFields { get; set; }

            public T GetCropData<T>(string key, int fieldIdx, byte row, byte column, T otherwise = default) => Sync.World.GetSharedData<T>($"FARM::CF_{Id}_{fieldIdx}_{row}_{column}::{key}", otherwise);

            public Farm(int Id, Vector3 PositionInfo, Utils.Vector4 PositionInteract, uint Price, uint Rent, float Tax, string CropFieldsStr) : base(Id, PositionInfo, Types.Farm, Price, Rent, Tax)
            {
                this.CropFields = RAGE.Util.Json.Deserialize<List<CropField>>(CropFieldsStr);

                for (int i = 0; i < CropFields.Count; i++)
                {
                    CropFields[i].PreInitialize(this, i);
                }

                if (SubId == 1)
                {
                    MainColshape = new Additional.Polygon(new List<Vector3>()
                    {
                        new Vector3(2191.28223f, 4849.38428f, 75f),
                        new Vector3(1909.05542f, 4717.912f, 75f),
                        new Vector3(1812.82751f, 4817.513f, 75f),
                        new Vector3(1807.89917f, 4833.14941f, 75f),
                        new Vector3(1809.6333f, 4878.728f, 75f),
                        new Vector3(1805.541f, 4890.7583f, 75f),
                        new Vector3(1736.1438f, 4978.261f, 75f),
                        new Vector3(1923.747f, 5137.78564f, 75f),
                    }, 0f, 0f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                    {
                        ApproveType = Additional.ExtraColshape.ApproveTypes.None,

                        OnEnter = (cancel) => OnEnterFarm(this),

                        OnExit = (cancel) => OnExitFarm(this),
                    };
                }

                Blip = new Blip(569, PositionInteract.Position, "Ферма", 1f, 9, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                this.Seller = new NPC($"farmer_{Id}", "Райнер", NPC.Types.Talkable, "a_m_m_hillbilly_01", PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    DefaultDialogueId = "job_farm_pl_0",

                    Data = this,
                };
            }

            private static void OnEnterFarm(Farm farm)
            {
                if (farm.SubId == 1)
                {
                    for (int i = 0; i < farm.CropFields.Count; i++)
                    {
                        var fieldIdx = i;

                        farm.CropFields[i].Initialize(farm, i);

                        if (i == 0)
                        {
                            farm.CropFields[i].Colshape = new Additional.Cuboid(new Vector3(2042.795f, 4942.11133f, 63.9537277f), 50f, 38f, 50f, 45f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                            {
                                OnEnter = (cancel) => farm.CropFields[fieldIdx].OnEnter(farm, fieldIdx),

                                OnExit = (cancel) => farm.CropFields[fieldIdx].OnExit(farm, fieldIdx),
                            };
                        }
                        else if (i == 1)
                        {
                            farm.CropFields[i].Colshape = new Additional.Cuboid(new Vector3(2069.84937f, 4915.067f, 63.9537277f), 50f, 38f, 50f, 45f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                            {
                                OnEnter = (cancel) => farm.CropFields[fieldIdx].OnEnter(farm, fieldIdx),

                                OnExit = (cancel) => farm.CropFields[fieldIdx].OnExit(farm, fieldIdx),
                            };
                        }
                        else if (i == 2)
                        {
                            farm.CropFields[i].Colshape = new Additional.Cuboid(new Vector3(2004.82715f, 4904.285f, 63.9537277f + 1.80652f), 45f, 38f, 50f, 45f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                            {
                                OnEnter = (cancel) => farm.CropFields[fieldIdx].OnEnter(farm, fieldIdx),

                                OnExit = (cancel) => farm.CropFields[fieldIdx].OnExit(farm, fieldIdx),
                            };
                        }
                        else if (i == 3)
                        {
                            farm.CropFields[i].Colshape = new Additional.Cuboid(new Vector3(2031.902f, 4877.307f, 63.9537277f + 1.80652f), 45f, 38f, 50f, 45f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                            {
                                OnEnter = (cancel) => farm.CropFields[fieldIdx].OnEnter(farm, fieldIdx),

                                OnExit = (cancel) => farm.CropFields[fieldIdx].OnExit(farm, fieldIdx),
                            };
                        }
                    }
                }
            }

            private static void OnExitFarm(Farm farm)
            {
                foreach (var x in farm.CropFields)
                {
                    x.Destroy();

                    x.Colshape?.Destroy();

                    x.Colshape = null;
                }
            }
        }
    }
}
