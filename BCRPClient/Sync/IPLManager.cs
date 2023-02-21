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

            //RAGE.Game.Streaming.RequestIpl("ch1_02_closed");
            //RAGE.Game.Streaming.RequestIpl("dt1_05_hc_remove");

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
