using RAGE;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Sync
{
    class IPLManager : Events.Script
    {
        public class IPLInfo
        {
            public string Name { get; set; }
            public string[] IPLs { get; set; }
            public Vector3 Position { get; set; }
            public Additional.ExtraColshape Colshape { get; set; }

            public IPLInfo(string Name, Vector3 Position, float Radius, uint Dimension = uint.MaxValue, params string[] IPLs)
            {
                this.Name = Name;
                this.Position = Position;

                this.Colshape = new Additional.Circle(Position, Radius, false, new Utils.Colour(0, 0, 255, 125), Dimension, null);

                this.Colshape.ActionType = Additional.ExtraColshape.ActionTypes.IPL;

                this.Colshape.Data = this;

                this.IPLs = IPLs;
            }

            public static void Load(Vector3 pos, params string[] ipls)
            {
                foreach (var x in ipls)
                    RAGE.Game.Streaming.RequestIpl(x);

                if (pos != null)
                {
                    var intid = RAGE.Game.Interior.GetInteriorAtCoords(pos.X, pos.Y, pos.Z);

                    RAGE.Game.Interior.LoadInterior(intid);
                    RAGE.Game.Interior.RefreshInterior(intid);
                }
            }

            public static void Unload(Vector3 pos, params string[] ipls)
            {
                foreach (var x in ipls)
                    RAGE.Game.Streaming.RemoveIpl(x);

                if (pos != null)
                {
                    var intid = RAGE.Game.Interior.GetInteriorAtCoords(pos.X, pos.Y, pos.Z);

                    RAGE.Game.Interior.UnpinInterior(intid);
                    RAGE.Game.Interior.RefreshInterior(intid);
                }
            }

            public void Load() => Load(Position, IPLs);
            public void Unload() => Unload(Position, IPLs);
        }

        private static List<IPLInfo> All;

        public IPLManager()
        {
            All = new List<IPLInfo>()
            {
                new IPLInfo("Garage_20", new Vector3(1286.137f, 245.5f, -49f), 100f, uint.MaxValue, "vw_casino_garage"),

                new IPLInfo(null, new Vector3(-1114.88f, 306.84f, 0f), 200f, uint.MaxValue, "bh1_47_joshhse_unburnt"),
                new IPLInfo(null, new Vector3(32.02f, 3737.35f, 0f), 200f, uint.MaxValue, "methtrailer_grp1"),
                new IPLInfo(null, new Vector3(2495.55f, 3157.45f, 0f), 100f, uint.MaxValue, "gr_case2_bunkerclosed"),
            };

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

            RAGE.Game.Streaming.RequestIpl("hei_dlc_windows_casino");
            RAGE.Game.Streaming.RequestIpl("hei_dlc_casino_door");

/*            var casPos = new Vector3(963.41960000f, 47.85423000f, 74.31705000f);

            AsyncTask casTask = null;

            new Additional.Sphere(casPos, 200f, false, Utils.RedColor, uint.MaxValue, null)
            {
                OnEnter = async (cancel) =>
                {
                    await Utils.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_GENERAL");
                    await Utils.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_SLOT_MACHINES_01");
                    await Utils.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_SLOT_MACHINES_02");
                    await Utils.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_SLOT_MACHINES_03");

                    casTask = new AsyncTask(() =>
                    {
                        if (!RAGE.Game.Audio.IsStreamPlaying() && RAGE.Game.Audio.LoadStream("casino_walla", "DLC_VW_Casino_Interior_Sounds"))
                        {
                            Utils.ConsoleOutput("strm");
                            RAGE.Game.Audio.PlayStreamFromPosition(casPos.X, casPos.Y, casPos.Z);
                        }

                        if (RAGE.Game.Audio.IsStreamPlaying() && !RAGE.Game.Audio.IsAudioSceneActive("DLC_VW_Casino_General"))
                        {
                            Utils.ConsoleOutput("scn");
                            RAGE.Game.Audio.StartAudioScene("DLC_VW_Casino_General");
                        }

                        Utils.ConsoleOutput("task");
                    }, 1000, true, 0);

                    casTask.Run();
                },

                OnExit = async (cancel) =>
                {
                    casTask?.Cancel();

                    if (RAGE.Game.Audio.IsStreamPlaying())
                        RAGE.Game.Audio.StopStream();

                    if (RAGE.Game.Audio.IsAudioSceneActive("DLC_VW_Casino_General"))
                        RAGE.Game.Audio.StopAudioScene("DLC_VW_Casino_General");
                },
            };*/

            //RAGE.Game.Streaming.RequestIpl("ch1_02_closed");
            //RAGE.Game.Streaming.RequestIpl("dt1_05_hc_remove");

            Sync.World.AddDataHandler("PRISON_ALARMS", async (value, oldValue) =>
            {
                var state = (bool?)value ?? false;

                var prisonIntId = RAGE.Game.Interior.GetInteriorAtCoordsWithType(1787.004f, 2593.1984f, 45.7978f, "int_prison_main");

                if (RAGE.Game.Interior.IsValidInterior(prisonIntId))
                {
                    if (state)
                    {
                        RAGE.Game.Interior.EnableInteriorProp(prisonIntId, "prison_alarm");

                        await Utils.PrepareAlarm("PRISON_ALARMS");

                        RAGE.Game.Audio.StartAlarm("PRISON_ALARMS", true);
                    }
                    else
                    {
                        RAGE.Game.Interior.DisableInteriorProp(prisonIntId, "prison_alarm");

                        await Utils.PrepareAlarm("PRISON_ALARMS");

                        RAGE.Game.Audio.StopAlarm("PRISON_ALARMS", true);
                    }

                    RAGE.Game.Interior.RefreshInterior(prisonIntId);
                }
            });

            Events.Add("IPL::Switch", (object[] args) =>
            {
                string name = (string)args[0];
                bool state = (bool)args[1];

                var iplInfo = All.Where(x => x.Name == name).FirstOrDefault();

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
