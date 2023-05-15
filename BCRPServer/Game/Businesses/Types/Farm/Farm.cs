using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;

namespace BCRPServer.Game.Businesses
{
    public class Farm : Business
    {
        public static Types DefaultType => Types.Farm;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 9)
        {
            Prices = new Dictionary<string, uint>()
            {
                { $"crop_{(int)CropField.Types.Cabbage}_0", 2 },
                { $"crop_{(int)CropField.Types.Cabbage}_1", 2 },
                { $"crop_{(int)CropField.Types.Cabbage}_2", 100 },

                { $"crop_{(int)CropField.Types.Pumpkin}_0", 2 },
                { $"crop_{(int)CropField.Types.Pumpkin}_1", 2 },
                { $"crop_{(int)CropField.Types.Pumpkin}_2", 100 },

                { $"crop_{(int)CropField.Types.Wheat}_0", 2 },
                { $"crop_{(int)CropField.Types.Wheat}_1", 3 },
                { $"crop_{(int)CropField.Types.Wheat}_2", 50 },

                { $"crop_{(int)CropField.Types.OrangeTree}_0", 2 },
                { $"crop_{(int)CropField.Types.OrangeTree}_1", 2 },

                { $"crop_{(int)CropField.Types.Cow}_1", 2 },
            }
        };

        public class CropField
        {
            public class CropData
            {
                public Timer Timer { get; set; }

                public bool WasIrrigated { get; set; }

                public static long? GetGrowTime(Farm farm, int fieldIdx, byte col, byte row) => Sync.World.GetSharedData<object>($"FARM::CF_{farm.ID}_{fieldIdx}_{col}_{row}") is object obj ? Convert.ToInt64(obj) : (long?)null;

                public void UpdateGrowTime(Farm farm, int fieldIdx, byte col, byte row, long? value, bool updateDb)
                {
                    var key = $"FARM::CF_{farm.ID}_{fieldIdx}_{col}_{row}";

                    if (value is long valueL)
                    {
                        if (valueL == 0)
                        {
                            if (Timer != null)
                            {
                                Timer.Dispose();

                                Timer = null;

                                WasIrrigated = false;
                            }
                        }
                        else
                        {
                            var curTime = Utils.GetCurrentTime();

                            var ms = (int)DateTimeOffset.FromUnixTimeSeconds(valueL).Subtract(curTime).TotalMilliseconds;

                            if (ms <= 0)
                            {
                                if (Timer != null)
                                {
                                    Timer.Dispose();

                                    Timer = null;
                                }

                                value = 0;

                                WasIrrigated = false;
                            }
                            else
                            {
                                if (Timer == null)
                                {
                                    Timer = new Timer((obj) =>
                                    {
                                        NAPI.Task.Run(() =>
                                        {
                                            UpdateGrowTime(farm, fieldIdx, col, row, 0, true);
                                        });
                                    }, null, ms, Timeout.Infinite);
                                }
                                else
                                {
                                    Timer.Change(ms, Timeout.Infinite);
                                }
                            }
                        }

                        Sync.World.SetSharedData(key, value);
                    }
                    else
                    {
                        if (Timer != null)
                        {
                            Timer.Dispose();

                            Timer = null;
                        }

                        WasIrrigated = false;

                        Sync.World.ResetSharedData(key);
                    }

                    if (updateDb)
                        MySQL.FarmEntityUpdateData(key, value, WasIrrigated ? (byte?)1 : null);
                }
            }

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
            public (Utils.Vector2 Pos, byte Count)[] Columns { get; set; }

            [JsonProperty(PropertyName = "O")]
            public Utils.Vector2 Offset { get; set; }

            [JsonProperty(PropertyName = "IRP")]
            public List<Vector3> IrrigationPoints { get; set; }

            [JsonIgnore]
            public List<List<CropData>> CropsData { get; set; }

            [JsonIgnore]
            public Timer Timer { get; set; }

            public CropField(Types Type, float CoordZ, Utils.Vector2 Offset, (Utils.Vector2, byte)[] Columns)
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

            public static long? GetIrrigationEndTime(Farm farm, int fieldIdx) => Sync.World.GetSharedData<object>($"FARM::CFI_{farm.ID}_{fieldIdx}") is object obj ? Convert.ToInt64(obj) : (long?)null;

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

                    Sync.World.SetSharedData(key, value);
                }
                else
                {
                    if (Timer != null)
                    {
                        Timer.Dispose();

                        Timer = null;
                    }

                    Sync.World.ResetSharedData(key);
                }

                if (updateDb)
                    MySQL.FarmEntityUpdateData(key, value);
            }

            public bool IsIrrigated => Timer != null;

            public Vector3 GetCropPosition3D(byte col, byte row) => col >= CropsData.Count || row >= CropsData[col].Count ? null : new Vector3(Columns[col].Pos.X + Offset.X * row, Columns[col].Pos.Y + Offset.Y * row, CoordZ);

            public Utils.Vector2 GetCropPosition2D(byte col, byte row) => col >= CropsData.Count || row >= CropsData[col].Count ? null : new Utils.Vector2(Columns[col].Pos.X + Offset.X * row, Columns[col].Pos.Y + Offset.Y * row);

            public static CropData GetData(Farm farm, int fieldIdx, byte col, byte row) => farm.CropFields == null || fieldIdx < 0 || fieldIdx >= farm.CropFields.Count || col >= farm.CropFields[fieldIdx].CropsData.Count || row >= farm.CropFields[fieldIdx].CropsData[col].Count ? null : farm.CropFields[fieldIdx].CropsData[col][row];
        }

        public class OrangeTreeData
        {
            [JsonIgnore]
            public Timer Timer { get; set; }

            [JsonIgnore]
            public byte OrangesAmount { get; set; }

            [JsonProperty(PropertyName = "P")]
            public Vector3 Position { get; set; }

            public OrangeTreeData(Vector3 Position)
            {
                this.Position = Position;
            }

            public static OrangeTreeData GetData(Farm farm, int idx) => farm.OrangeTrees == null || idx >= farm.OrangeTrees.Count ? null : farm.OrangeTrees[idx];

            public static long? GetGrowTime(Farm farm, int idx) => Sync.World.GetSharedData<object>($"FARM::OT_{farm.ID}_{idx}") is object obj ? Convert.ToInt64(obj) : (long?)null;

            public void UpdateGrowTime(Farm farm, int idx, long? value, bool updateDb)
            {
                var key = $"FARM::OT_{farm.ID}_{idx}";

                if (value is long valueL)
                {
                    if (valueL == 0)
                    {
                        if (Timer != null)
                        {
                            Timer.Dispose();

                            Timer = null;
                        }

                        OrangesAmount = (byte)SRandom.NextInt32(Game.Jobs.Farmer.ORANGES_ON_TREE_MIN_AMOUNT, Game.Jobs.Farmer.ORANGES_ON_TREE_MAX_AMOUNT + 1);
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

                            OrangesAmount = (byte)SRandom.NextInt32(Game.Jobs.Farmer.ORANGES_ON_TREE_MIN_AMOUNT, Game.Jobs.Farmer.ORANGES_ON_TREE_MAX_AMOUNT + 1);
                        }
                        else
                        {
                            if (Timer == null)
                            {
                                Timer = new Timer((obj) =>
                                {
                                    NAPI.Task.Run(() =>
                                    {
                                        UpdateGrowTime(farm, idx, 0, true);
                                    });
                                }, null, ms, Timeout.Infinite);
                            }
                            else
                            {
                                Timer.Change(ms, Timeout.Infinite);
                            }
                        }
                    }

                    Sync.World.SetSharedData(key, value);
                }
                else
                {
                    if (Timer != null)
                    {
                        Timer.Dispose();

                        Timer = null;
                    }

                    Sync.World.ResetSharedData(key);
                }

                if (updateDb)
                    MySQL.FarmEntityUpdateData(key, value);
            }
        }

        public class CowData
        {
            [JsonIgnore]
            public Timer Timer { get; set; }

            [JsonProperty(PropertyName = "P")]
            public Utils.Vector4 Position { get; set; }

            public CowData(Utils.Vector4 Position)
            {
                this.Position = Position;
            }

            public static CowData GetData(Farm farm, int idx) => farm.Cows == null || idx < 0 || idx >= farm.Cows.Count ? null : farm.Cows[idx];

            public static long? GetGrowTime(Farm farm, int idx) => Sync.World.GetSharedData<object>($"FARM::COW_{farm.ID}_{idx}") is object obj ? Convert.ToInt64(obj) : (long?)null;

            public void UpdateGrowTime(Farm farm, int idx, long? value, bool updateDb)
            {
                var key = $"FARM::COW_{farm.ID}_{idx}";

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
                                        UpdateGrowTime(farm, idx, 0, true);
                                    });
                                }, null, ms, Timeout.Infinite);
                            }
                            else
                            {
                                Timer.Change(ms, Timeout.Infinite);
                            }
                        }
                    }

                    Sync.World.SetSharedData(key, value);
                }
                else
                {
                    if (Timer != null)
                    {
                        Timer.Dispose();

                        Timer = null;
                    }

                    Sync.World.ResetSharedData(key);
                }

                if (updateDb)
                    MySQL.FarmEntityUpdateData(key, value);
            }
        }

        public List<CropField> CropFields { get; set; }

        public List<OrangeTreeData> OrangeTrees { get; set; }

        public List<Vector3> OrangeTreeBoxPositions { get; set; }

        public List<Vector3> CowBucketPositions { get; set; }

        public List<CowData> Cows { get; set; }

        public Farm(int ID, Vector3 PositionInfo, Utils.Vector4 PositionInteract) : base(ID, PositionInfo, PositionInteract, Types.Farm)
        {

        }

        public override string ClientData => $"{ID}, {PositionInfo.ToCSharpStr()}, {PositionInteract.ToCSharpStr()}, {GovPrice}, {Rent}, {Tax}f, \"{CropFields.SerializeToJson().Replace('"', '\'')}\", \"{OrangeTrees.SerializeToJson().Replace('"', '\'')}\", \"{Cows.SerializeToJson().Replace('"', '\'')}\", \"{OrangeTreeBoxPositions.SerializeToJson().Replace('"', '\'')}\", \"{CowBucketPositions.SerializeToJson().Replace('"', '\'')}\"";

        public bool TryProceedPayment(PlayerData pData, string itemId, decimal salaryCoef, out uint newMats, out ulong newBalance, out uint newPlayerBalance)
        {
            try
            {
                var matData = MaterialsData;

                var matPrice = matData.Prices[itemId];

                var hasMaterials = true;

                if (Owner != null)
                {
                    if (!TryRemoveMaterials(matPrice, out newMats, false, null))
                    {
                        hasMaterials = false;

                        newMats = Materials;
                    }
                }
                else
                {
                    newMats = 0;
                }

                var realPriceP = Math.Floor((decimal)matPrice * matData.RealPrice * (2m - Margin) * salaryCoef);

                if (realPriceP < 0)
                    realPriceP = 0;

                newPlayerBalance = Game.Jobs.Job.GetPlayerTotalCashSalary(pData) + (uint)realPriceP;

                if (hasMaterials)
                {
                    var bizPrice = GetBusinessPrice(matPrice, false);

                    if (!TryAddMoneyCash(bizPrice, out newBalance, true, pData))
                        return false;
                }
                else
                {
                    newBalance = Cash;
                }

                return true;
            }
            catch (Exception)
            {
                newMats = 0;
                newBalance = 0;
                newPlayerBalance = 0;

                return false;
            }
        }

        public void ProceedPayment(PlayerData pData, uint newMats, ulong newBalance, uint newPlayerBalance)
        {
            if (Owner != null)
            {
                if (newMats != Materials)
                    SetMaterials(newMats);

                if (newBalance > Cash)
                    UpdateStatistics(newBalance - Cash);

                SetCash(newBalance);

                MySQL.BusinessUpdateBalances(this, false);
            }

            Game.Jobs.Job.SetPlayerTotalCashSalary(pData, newPlayerBalance, true);
        }
    }
}
