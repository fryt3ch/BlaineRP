using System;
using System.Threading;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Businesses
{
    public partial class Farm
    {
        public partial class CropField
        {
            public class CropData
            {
                public Timer Timer { get; set; }

                public bool WasIrrigated { get; set; }

                public static long? GetGrowTime(Farm farm, int fieldIdx, byte col, byte row) => World.Service.GetSharedData<object>($"FARM::CF_{farm.ID}_{fieldIdx}_{col}_{row}") is object obj ? Convert.ToInt64(obj) : (long?)null;

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

                        World.Service.SetSharedData(key, value);
                    }
                    else
                    {
                        if (Timer != null)
                        {
                            Timer.Dispose();

                            Timer = null;
                        }

                        WasIrrigated = false;

                        World.Service.ResetSharedData(key);
                    }

                    if (updateDb)
                        MySQL.FarmEntityUpdateData(key, value, WasIrrigated ? (byte?)1 : null);
                }
            }
        }
    }
}