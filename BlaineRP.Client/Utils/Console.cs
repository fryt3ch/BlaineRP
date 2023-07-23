using System;

namespace BlaineRP.Client.Utils
{
    internal static class Console
    {
        private static DateTime _lastLimitedConsoleMsgTime;

        public static void ConsoleOutput(object obj, bool line = true)
        {
            if (line)
                RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Info, obj.ToString());
            else
                RAGE.Ui.Console.Log(RAGE.Ui.ConsoleVerbosity.Info, obj.ToString());
        }

        public static void ConsoleOutputLimited(object obj, bool line = true, int ms = 2000)
        {
            if (Sync.World.ServerTime.Subtract(_lastLimitedConsoleMsgTime).TotalMilliseconds < ms)
                return;

            _lastLimitedConsoleMsgTime = Sync.World.ServerTime;

            if (line)
                RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Info, obj.ToString());
            else
                RAGE.Ui.Console.Log(RAGE.Ui.ConsoleVerbosity.Info, obj.ToString());
        }
    }
}
