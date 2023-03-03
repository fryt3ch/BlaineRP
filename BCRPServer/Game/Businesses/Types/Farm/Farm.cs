using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BCRPServer.Game.Businesses
{
    public class Farm : Business
    {
        public class CropField
        {
            public class CropData
            {
                public CancellationTokenSource CTS { get; set; }

                public static long? GetGrowTime(Farm farm, int fieldIdx, byte row, byte col) => Sync.World.GetSharedData<object>($"FARM::CF_{farm.ID}_{fieldIdx}_{row}_{col}") is object obj ? Convert.ToInt64(obj) : (long?)null;

                public static void SetGrowTime(Farm farm, int fieldIdx, byte row, byte col, long? value) { var key = $"FARM::CF_{farm.ID}_{fieldIdx}_{row}_{col}";  if (value == null) Sync.World.ResetSharedData(key); else Sync.World.SetSharedData(key, value); }

                public void UpdateGrowTime(Farm farm, int fieldIdx, byte row, byte col, long? value)
                {
                    if (value is long valueL)
                    {
                        if (valueL == 0)
                        {
                            if (CTS != null)
                            {
                                CTS.Cancel();

                                CTS.Dispose();

                                CTS = null;
                            }
                        }
                        else
                        {
                            if (CTS != null)
                            {
                                CTS.Cancel();

                                CTS.Dispose();
                            }

                            var ms = (int)DateTimeOffset.FromUnixTimeSeconds(valueL).Subtract(Utils.GetCurrentTime()).TotalMilliseconds;

                            if (ms <= 0)
                            {
                                CTS = null;

                                value = 0;
                            }
                            else
                            {
                                CTS = new CancellationTokenSource();

                                System.Threading.Tasks.Task.Run(async () =>
                                {
                                    try
                                    {
                                        await System.Threading.Tasks.Task.Delay(ms, CTS.Token);

                                        NAPI.Task.Run(() =>
                                        {
                                            UpdateGrowTime(farm, fieldIdx, row, col, 0);
                                        });
                                    }
                                    catch (Exception ex) { }
                                });
                            }
                        }
                    }
                    else
                    {
                        if (CTS != null)
                        {
                            CTS.Cancel();

                            CTS.Dispose();

                            CTS = null;
                        }
                    }

                    SetGrowTime(farm, fieldIdx, row, col, value);
                }
            }

            public enum Types
            {
                Cabbage = 0,
                Pumpkin,
            }

            [JsonProperty(PropertyName = "T")]
            public Types Type { get; set; }

            [JsonProperty(PropertyName = "CZ")]
            public float CoordZ { get; set; }

            [JsonProperty(PropertyName = "RC")]
            public byte RowsCount { get; set; }

            [JsonProperty(PropertyName = "C")]
            public Utils.Vector2[] Columns { get; set; }

            [JsonProperty(PropertyName = "O")]
            public Utils.Vector2 Offset { get; set; }

            [JsonIgnore]
            public List<List<CropData>> CropsData { get; set; }

            public CropField(Types Type, float CoordZ, Utils.Vector2 Offset, byte RowsCount, Utils.Vector2[] Columns)
            {
                this.Type = Type;

                this.CoordZ = CoordZ;
                this.Columns = Columns;
                this.Offset = Offset;

                this.RowsCount = RowsCount;

                CropsData = new List<List<CropData>>();

                for (int i = 0; i < RowsCount; i++)
                {
                    CropsData.Add(new List<CropData>());

                    for (int j = 0; j < Columns.Length; j++)
                    {
                        CropsData[i].Add(new CropData());
                    }
                }
            }

            public Vector3 GetCropPosition3D(byte row, byte col) => row >= CropsData.Count || col >= Columns.Length ? null : new Vector3(Columns[col].X + Offset.X * row, Columns[col].Y + Offset.Y * row, CoordZ);

            public Utils.Vector2 GetCropPosition2D(byte row, byte col) => row >= CropsData.Count || col >= Columns.Length ? null : new Utils.Vector2(Columns[col].X + Offset.X * row, Columns[col].Y + Offset.Y * row);

            public static CropData GetData(Farm farm, int fieldIdx, byte row, byte col) => farm.CropFields == null || fieldIdx < 0 || fieldIdx >= farm.CropFields.Count || row >= farm.CropFields[fieldIdx].RowsCount || col >= farm.CropFields[fieldIdx].Columns.Length ? null : fieldIdx < 0 || fieldIdx >= farm.CropFields.Count ? null : farm.CropFields[fieldIdx].CropsData[row][col];
        }

        public List<CropField> CropFields { get; set; }

        public void SetCropSharedData(string key, int fieldIdx, byte row, byte col, object data) => Sync.World.SetSharedData($"CF_{ID}_{fieldIdx}_{row}_{col}::{key}", data);
        public T GetCropSharedData<T>(string key, int fieldIdx, byte row, byte col) => Sync.World.GetSharedData<T>($"CF_{ID}_{fieldIdx}_{row}_{col}::{key}");

        public Farm(int ID, Vector3 PositionInfo, Utils.Vector4 PositionInteract) : base(ID, PositionInfo, PositionInteract, Types.Farm)
        {

        }

        public override string ClientData => $"{ID}, {PositionInfo.ToCSharpStr()}, {PositionInteract.ToCSharpStr()}, {GovPrice}, {Rent}, {Tax}f, \"{CropFields.SerializeToJson().Replace('"', '\'')}\"";
    }
}
