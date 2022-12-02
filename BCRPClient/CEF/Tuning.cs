using RAGE;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.CEF
{
    public class Tuning : Events.Script
    {
        public static bool IsActive => CEF.Browser.IsActive(CEF.Browser.IntTypes.Tuning);

        public Tuning()
        {
            Events.Add("Tuning::NavChange", (args) =>
            {
                var id = (int)args[0];
            });

            Events.Add("Tuning::Choose", (args) =>
            {
                var id = (string)args[0];
            });

            Events.Add("Tuning::ChangeColor", (args) =>
            {
                var id = (string)args[0];

                int num = (int)args[1]; // -1 - delete
            });
        }
    }
}
