using System;
using System.Collections.Generic;
using System.Threading;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;
using Newtonsoft.Json;

namespace BlaineRP.Server.Game.Businesses
{
    public partial class Farm
    {
        public partial class CropField
        {
            public enum Types
            {
                Cabbage = 0,
                Pumpkin,

                Wheat,

                OrangeTree,
                Cow,
            }

            [JsonProperty(PropertyName = "T")]
            public Types Type { get; set; }

            [JsonProperty(PropertyName = "CZ")]
            public float CoordZ { get; set; }

            [JsonProperty(PropertyName = "C")]
            public (Vector2 Pos, byte Count)[] Columns { get; set; }

            [JsonProperty(PropertyName = "O")]
            public Vector2 Offset { get; set; }

            [JsonProperty(PropertyName = "IRP")]
            public List<Vector3> IrrigationPoints { get; set; }

            [JsonIgnore]
            public List<List<CropData>> CropsData { get; set; }

            [JsonIgnore]
            public Timer Timer { get; set; }

            public CropField(Types Type, float CoordZ, Vector2 Offset, (Vector2, byte)[] Columns)
            {
                this.Type = Type;

                this.CoordZ = CoordZ;
                this.Columns = Columns;
                this.Offset = Offset;

                CropsData = new List<List<CropData>>();

                for (int i = 0; i < Columns.Length; i++)
                {
                    CropsData.Add(new List<CropData>());

                    for (int j = 0; j < Columns[i].Item2; j++)
                    {
                        CropsData[i].Add(new CropData());
                    }
                }
            }

            public static long? GetIrrigationEndTime(Farm farm, int fieldIdx) => World.Service.GetSharedData<object>($"FARM::CFI_{farm.ID}_{fieldIdx}") is object obj ? Convert.ToInt64(obj) : (long?)null;

            public void UpdateIrrigationEndTime(Farm farm, int fieldIdx, long? value, bool updateDb)
            {
                var key = $"FARM::CFI_{farm.ID}_{fieldIdx}";

                if (value is long valueL)
                {
                    if (valueL == 0)
                    {
                        if (Timer != null)
                        {
                            Timer.Dispose();

                            Timer = null;
                        }
                    }
                    else
                    {
                        var ms = (int)DateTimeOffset.FromUnixTimeSeconds(valueL).Subtract(Utils.GetCurrentTime()).TotalMilliseconds;

                        if (ms <= 0)
                        {
                            if (Timer != null)
                            {
                                Timer.Dispose();

                                Timer = null;
                            }

                            value = 0;
                        }
                        else
                        {
                            if (Timer == null)
                            {
                                Timer = new Timer((obj) =>
                                {
                                    NAPI.Task.Run(() =>
                                    {
                                        UpdateIrrigationEndTime(farm, fieldIdx, null, true);
                                    });
                                }, null, ms, Timeout.Infinite);
                            }
                            else
                            {
                                Timer.Change(ms, Timeout.Infinite);
                            }
                        }
                    }

                    World.Service.SetSharedData(key, value);
                }
                else
                {
                    if (Timer != null)
                    {
                        Timer.Dispose();

                        Timer = null;
                    }

                    World.Service.ResetSharedData(key);
                }

                if (updateDb)
                    MySQL.FarmEntityUpdateData(key, value);
            }

            public bool IsIrrigated => Timer != null;

            public Vector3 GetCropPosition3D(byte col, byte row) => col >= CropsData.Count || row >= CropsData[col].Count ? null : new Vector3(Columns[col].Pos.X + Offset.X * row, Columns[col].Pos.Y + Offset.Y * row, CoordZ);

            public Vector2 GetCropPosition2D(byte col, byte row) => col >= CropsData.Count || row >= CropsData[col].Count ? null : new Vector2(Columns[col].Pos.X + Offset.X * row, Columns[col].Pos.Y + Offset.Y * row);

            public static CropData GetData(Farm farm, int fieldIdx, byte col, byte row) => farm.CropFields == null || fieldIdx < 0 || fieldIdx >= farm.CropFields.Count || col >= farm.CropFields[fieldIdx].CropsData.Count || row >= farm.CropFields[fieldIdx].CropsData[col].Count ? null : farm.CropFields[fieldIdx].CropsData[col][row];
        }
    }
}