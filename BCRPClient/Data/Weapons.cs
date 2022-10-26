using RAGE;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Data
{
    class Weapons : Events.Script
    {
        public static Dictionary<string, Items.Types> AllWeapons = new Dictionary<string, Items.Types>()
        {
            { "w_asrifle", Items.Types.AssaultRifle },
            { "w_pistol", Items.Types.Pistol },

            { "w_wrench", Items.Types.Wrench },
        };

        public static Dictionary<string, Items.Types> AllAmmo = new Dictionary<string, Items.Types>()
        {
            { "am_5.56", Items.Types.Ammo5_56 },
            { "am_7.62", Items.Types.Ammo7_62 },
            { "am_9", Items.Types.Ammo9 },
            { "am_11.43", Items.Types.Ammo11_43 },
            { "am_12", Items.Types.Ammo12 },
            { "am_12.7", Items.Types.Ammo12_7 },
        };
    }
}
