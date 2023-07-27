using System;
using System.Threading;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;
using Newtonsoft.Json;

namespace BlaineRP.Server.Game.Businesses
{
    public partial class Farm
    {
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

            public static long? GetGrowTime(Farm farm, int idx) => World.Service.GetSharedData<object>($"FARM::OT_{farm.ID}_{idx}") is object obj ? Convert.ToInt64(obj) : (long?)null;

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
        }
    }
}