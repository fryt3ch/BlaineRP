using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.World;
using BlaineRP.Client.Sync;
using BlaineRP.Client.Utils.Game;
using RAGE;

namespace BlaineRP.Client.Game.Management.IPLs
{
    [Script(int.MaxValue)]
    public class Core
    {
        private static List<IPLInfo> _all;

        public Core()
        {
            _all = new List<IPLInfo>()
            {
                new IPLInfo("Garage_20", new Vector3(1286.137f, 245.5f, -49f), 100f, uint.MaxValue, "vw_casino_garage"),

                new IPLInfo(null, new Vector3(-1114.88f, 306.84f, 0f), 200f, uint.MaxValue, "bh1_47_joshhse_unburnt"),
                new IPLInfo(null, new Vector3(32.02f, 3737.35f, 0f), 200f, uint.MaxValue, "methtrailer_grp1"),
                new IPLInfo(null, new Vector3(2495.55f, 3157.45f, 0f), 100f, uint.MaxValue, "gr_case2_bunkerclosed"),
            };

            RAGE.Game.Streaming.RequestIpl("brp_map_fair_stalls_ls_0");

            RAGE.Game.Streaming.RequestIpl("gabz_pillbox_milo_");

            var pillboxIntId = RAGE.Game.Interior.GetInteriorAtCoords(311.2546f, -592.4204f, 42.32737f);

            if (RAGE.Game.Interior.IsValidInterior(pillboxIntId))
            {
                RAGE.Game.Streaming.RemoveIpl("rc12b_fixed");
                RAGE.Game.Streaming.RemoveIpl("rc12b_destroyed");
                RAGE.Game.Streaming.RemoveIpl("rc12b_default");
                RAGE.Game.Streaming.RemoveIpl("rc12b_hospitalinterior_lod");
                RAGE.Game.Streaming.RemoveIpl("rc12b_hospitalinterior");

                RAGE.Game.Interior.LoadInterior(pillboxIntId);
                RAGE.Game.Interior.RefreshInterior(pillboxIntId);
            }

            RAGE.Game.Streaming.RequestIpl("atriumglstatic");

            RAGE.Game.Streaming.RequestIpl("TrevorsTrailerTidy");

            RAGE.Game.Streaming.RequestIpl("hei_dlc_windows_casino");
            RAGE.Game.Streaming.RequestIpl("hei_dlc_casino_door");

            //RAGE.Game.Streaming.RequestIpl("ch1_02_closed");
            //RAGE.Game.Streaming.RequestIpl("dt1_05_hc_remove");

            World.Core.AddDataHandler("PRISON_ALARMS", async (value, oldValue) =>
            {
                var state = (bool?)value ?? false;

                var prisonIntId = RAGE.Game.Interior.GetInteriorAtCoordsWithType(1787.004f, 2593.1984f, 45.7978f, "int_prison_main");

                if (RAGE.Game.Interior.IsValidInterior(prisonIntId))
                {
                    if (state)
                    {
                        RAGE.Game.Interior.EnableInteriorProp(prisonIntId, "prison_alarm");

                        await Audio.PrepareAlarm("PRISON_ALARMS");

                        RAGE.Game.Audio.StartAlarm("PRISON_ALARMS", true);
                    }
                    else
                    {
                        RAGE.Game.Interior.DisableInteriorProp(prisonIntId, "prison_alarm");

                        await Audio.PrepareAlarm("PRISON_ALARMS");

                        RAGE.Game.Audio.StopAlarm("PRISON_ALARMS", true);
                    }

                    RAGE.Game.Interior.RefreshInterior(prisonIntId);
                }
            });

            Events.Add("IPL::Switch", (object[] args) =>
            {
                string name = (string)args[0];
                bool state = (bool)args[1];

                var iplInfo = _all.Where(x => x.Name == name).FirstOrDefault();

                if (iplInfo == null)
                    return;

                if (state)
                {
                    iplInfo.Load();
                }
                else
                {
                    iplInfo.Unload();
                }
            });
        }
    }
}
