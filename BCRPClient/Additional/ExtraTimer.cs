using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Additional
{
    public class ExtraTimer
    {
        private static List<System.Threading.Timer> allTimers = new List<System.Threading.Timer>();

        private System.Threading.Timer rTimer { get; set; }

        private static DateTime lastTime { get; set; }

        private static System.Threading.Timer sTimer { get; } = new System.Threading.Timer(async (obj) =>
        {
            var startWaitDate = DateTime.Now;

            await RAGE.Game.Invoker.WaitAsync(0);

            if (DateTime.Now.Subtract(startWaitDate).TotalMilliseconds > 1000)
                return;

            var curTime = Sync.World.ServerTime;

            if (lastTime == curTime)
            {
                for (int i = 0; i < allTimers.Count; i++)
                {
                    allTimers[i]?.Dispose();
                }

                allTimers.Clear();

                sTimer.Dispose();

                return;
            }

            lastTime = curTime;
        }, null, 0, 2_500);

        public ExtraTimer(System.Threading.TimerCallback Action, object StateObj, int DueTime, int Period)
        {
            rTimer = new System.Threading.Timer(Action, StateObj, DueTime, Period);

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
