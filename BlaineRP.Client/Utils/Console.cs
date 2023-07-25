using System;
using BlaineRP.Client.Game.World;

namespace BlaineRP.Client.Utils
{
    internal static class Console
    {
        private static DateTime _lastLimitedConsoleMsgTime;

        public static void Output(object obj, bool line = true)
        {
            if (line)
                RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Info, obj.ToString());
            else
                RAGE.Ui.Console.Log(RAGE.Ui.ConsoleVerbosity.Info, obj.ToString());
        }

        public static void OutputLimited(object obj, bool line = true, int ms = 2000)
        {
            if (Core.ServerTime.Subtract(_lastLimitedConsoleMsgTime).TotalMilliseconds < ms)
                return;

            _lastLimitedConsoleMsgTime = Core.ServerTime;

            if (line)
                RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Info, obj.ToString());
            else
                RAGE.Ui.Console.Log(RAGE.Ui.ConsoleVerbosity.Info, obj.ToString());
        }
    }
}
