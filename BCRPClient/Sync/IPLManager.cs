using RAGE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            public IPLInfo(string Name, Vector3 Position, Additional.ExtraColshape Colshape, params string[] IPLs)
            {
                this.Name = Name;
                this.Position = Position;
                this.Colshape = Colshape;

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
                new IPLInfo("Garage_20", new Vector3(1286.137f, 245.5f, -49f), Additional.Polygon.CreateCuboid(new Vector3(1286.137f, 245.5f, -49f), 80f, 80f, 30f, 0f, false, Utils.RedColor, uint.MaxValue), "vw_casino_garage"),
            };

            foreach (var x in All)
            {
                if (x.Colshape == null)
                    continue;
            }

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
