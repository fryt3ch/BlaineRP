using System;
using System.Collections.Generic;

namespace BlaineRP.Client.Additional
{
    public class ExtraTimer
    {
        private static List<System.Threading.Timer> allTimers;

        private System.Threading.Timer rTimer { get; set; }

        // Fix to dispose all previous session timers!
        private static DateTime lastTime { get; set; }

        private static System.Threading.Timer sTimer;

        public static void Activate()
        {
            allTimers = new List<System.Threading.Timer>();

            sTimer = new System.Threading.Timer(async (obj) =>
            {
                sTimer.Change(-1, -1);

                await RAGE.Game.Invoker.WaitAsync(0);

                var curTime = Sync.World.ServerTime;

                if (lastTime == curTime)
                {
                    for (int i = 0; i < allTimers.Count; i++)
                    {
                        allTimers[i]?.Dispose();
                    }

                    allTimers.Clear();

                    sTimer.Dispose();

                    sTimer = null;

                    allTimers = null;

                    return;
                }

                lastTime = curTime;

                sTimer.Change(2_500, 2_500);
            }, null, 0, 2_500);
        }

        public ExtraTimer(Func<System.Threading.Tasks.Task> Action, int DueTime, int Period)
        {
            rTimer = new System.Threading.Timer(async (obj) =>
            {
                rTimer.Change(-1, -1);

                await RAGE.Game.Invoker.WaitAsync(0);

                await Action.Invoke();

                rTimer.Change(Period, Period);

            }, null, DueTime, Period);

            allTimers.Add(rTimer);
        }

        public void Dispose()
        {
            if (rTimer != null)
            {
                allTimers.Remove(rTimer);

                rTimer.Dispose();

                rTimer = null;
            }
        }
    }
}
