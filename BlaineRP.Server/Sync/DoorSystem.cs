using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.IO;

namespace BlaineRP.Server.Sync
{
    public class DoorSystem
    {
        private static Dictionary<uint, Door> AllDoors { get; set; }

        public static Door GetDoorById(uint id) => AllDoors.GetValueOrDefault(id);

        public class Door
        {
            public Func<PlayerData, bool> CheckFunction { get; set; }

            public uint Model { get; set; }

            public Vector3 Position { get; set; }

            public uint Dimension { get; set; }

            public Door(uint Id, string Model, float PosX, float PosY, float PosZ, uint Dimension) : this(Id, NAPI.Util.GetHashKey(Model), PosX, PosY, PosZ, Dimension)
            {

            }

            public Door(uint Id, uint Model, float PosX, float PosY, float PosZ, uint Dimension)
            {
                this.Model = Model;

                this.Position = new Vector3(PosX, PosY, PosZ);

                this.Dimension = Dimension;

                AllDoors.Add(Id, this);
            }

            public bool GetCheckFunctionResult(PlayerData pData)
            {
                if (CheckFunction == null)
                    return true;

                return CheckFunction.Invoke(pData);
            }

            public static bool GetLockState(uint id) => Sync.World.GetSharedData<bool?>($"DOORS_{id}_L") == true;

            public static void SetLockState(uint id, bool state, bool updateDb)
            {
                if (state)
                {
                    Sync.World.SetSharedData($"DOORS_{id}_L", true);
                }
                else
                {
                    Sync.World.ResetSharedData($"DOORS_{id}_L");
                }
            }
        }

        public static void InitializeAll()
        {
            if (AllDoors != null)
                return;

            AllDoors = new Dictionary<uint, Door>();

            new Door(1, "prison_prop_door2", 1780.352f, 2596.023f, 50.83891f, Properties.Settings.Static.MainDimension);

            var lines = new List<string>();

            foreach (var x in AllDoors)
            {
                lines.Add($"new Door({x.Key}, {x.Value.Model}, {x.Value.Position.ToCSharpStr()}, {x.Value.Dimension});");
            }

            Utils.FillFileToReplaceRegion(Directory.GetCurrentDirectory() + Properties.Settings.Static.ClientScriptsTargetPath + @"\Sync\DoorSystem.cs", "DOORS_TO_REPLACE", lines);
        }
    }
}
