using System;
using System.Collections.Generic;
using System.Linq;
using RAGE;

namespace BlaineRP.Client.Game.Management.DoorSystem
{
    [Script(int.MaxValue)]
    public class Service
    {
        public static DateTime LastSent;

        public Service()
        {
            #region DOORS_TO_REPLACE

            #endregion

            // Garages
            ToggleLock("v_ilev_rc_door2", 179.6684f, -1004.762f, -98.85f, true);
            ToggleLock("v_ilev_rc_door2", 207.7825f, -999.6905f, -98.85f, true);
            ToggleLock("v_ilev_garageliftdoor", 239.0922f, -1005.571f, 100f, true);

            // Bank
            ToggleLock("v_ilev_bk_gate", 256.3116f, 220.6579f, 106.4296f, true);
            ToggleLock("v_ilev_bk_door", 237.7704f, 227.87f, 106.426f, true);

            // Shooting range
            ToggleLock("v_ilev_gc_door01", 6.81789f, -1098.209f, 29.94685f, true);

            // Paleto Police Cells
            ToggleLock("v_ilev_ph_cellgate", -431.1921f, 5999.741f, 31.87312f, true);
            ToggleLock("v_ilev_ph_cellgate", -428.0646f, 5996.672f, 31.87312f, true);

            // Mission Row Police Cells
            ToggleLock("v_ilev_ph_cellgate", 461.8065f, -994.4086f, 25.06443f, true);
            ToggleLock("v_ilev_ph_cellgate", 461.8065f, -997.6584f, 25.06443f, true);
            ToggleLock("v_ilev_ph_cellgate", 461.8065f, -1001.302f, 25.06443f, true);
            ToggleLock("v_ilev_gtdoor", 467.1922f, -996.4594f, 25.00599f, true);
            ToggleLock("v_ilev_gtdoor", 471.4755f, -996.4594f, 25.00599f, true);
            ToggleLock("v_ilev_gtdoor", 475.7543f, -996.4594f, 25.00599f, true);
            ToggleLock("v_ilev_gtdoor", 480.0301f, -996.4594f, 25.00599f, true);

            // Amunnation Club 1
            ToggleLock("v_ilev_gc_door04", 16.12787f, -1114.606f, 29.94694f, true);
            ToggleLock("v_ilev_gc_door03", 18.572f, -1115.495f, 29.94694f, true);

            // Amunnation Club 2
            ToggleLock("v_ilev_gc_door04", 813.1779f, -2148.27f, 29.76892f, true);
            ToggleLock("v_ilev_gc_door03", 810.5769f, -2148.27f, 29.76892f, true);
        }

        public static Dictionary<HashSet<uint>, string> Names { get; set; } = new Dictionary<HashSet<uint>, string>()
        {
            {
                new HashSet<uint>()
                {
                    1,
                },
                "Центр управления"
            },
        };

        public static List<Door> AllDoors { get; set; } = new List<Door>();

        public static List<Door> All { get; set; } = new List<Door>();

        public static Door GetDoorById(uint id)
        {
            return All.Where(x => x.Id == id).FirstOrDefault();
        }

        public static void ToggleLock(Door doorInfo, bool state)
        {
            ToggleLock(doorInfo.Model, doorInfo.Position, state);
        }

        public static void ToggleLock(string model, float x, float y, float z, bool state)
        {
            ToggleLock(RAGE.Util.Joaat.Hash(model), x, y, z, state);
        }

        public static void ToggleLock(string model, Vector3 position, bool state)
        {
            ToggleLock(RAGE.Util.Joaat.Hash(model), position, state);
        }

        public static void ToggleLock(uint model, Vector3 position, bool state)
        {
            ToggleLock(model, position.X, position.Y, position.Z, state);
        }

        public static void ToggleLock(uint model, float x, float y, float z, bool state)
        {
            RAGE.Game.Object.DoorControl(model, x, y, z, state, 0f, 0f, 0f);
        }
    }
}