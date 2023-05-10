using RAGE;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Data
{
    public partial class Locations
    {
        public partial class Casino
        {
            public class SlotMachine
            {
                public enum ModelTypes : byte
                {
                    vw_prop_casino_slot_03a,
                    vw_prop_casino_slot_04a,
                    vw_prop_casino_slot_02a,
                    vw_prop_casino_slot_05a,
                }

                public ModelTypes ModelType { get; set; }

                public Vector3 Position { get; set; }

                public SlotMachine(int CasinoId, int Id, ModelTypes ModelType, float PosX, float PosY, float PosZ)
                {
                    this.ModelType = ModelType;

                    this.Position = new Vector3(PosX, PosY, PosZ);
                }
            }
        }
    }
}
