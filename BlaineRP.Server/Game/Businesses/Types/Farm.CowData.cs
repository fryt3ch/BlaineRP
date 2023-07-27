using System;
using System.Threading;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;
using Newtonsoft.Json;

namespace BlaineRP.Server.Game.Businesses
{
    public partial class Farm
    {
        public class CowData
        {
            [JsonIgnore]
            public Timer Timer { get; set; }

            [JsonProperty(PropertyName = "P")]
            public Vector4 Position { get; set; }

            public CowData(Vector4 Position)
            {
                this.Position = Position;
            }

            public static CowData GetData(Farm farm, int idx) => farm.Cows == null || idx < 0 || idx >= farm.Cows.Count ? null : farm.Cows[idx];

            public static long? GetGrowTime(Farm farm, int idx) => World.Service.GetSharedData<object>($"FARM::COW_{farm.ID}_{idx}") is object obj ? Convert.ToInt64(obj) : (long?)null;

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