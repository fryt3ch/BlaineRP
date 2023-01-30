using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Data
{
    public partial class Locations : Events.Script
    {
        public Locations()
        {
            CayoPerico.Initialize();

            Garage.Style.LoadAll();

            #region BIZS_TO_REPLACE

            #endregion

            #region ATM_TO_REPLACE

            #endregion

            #region BANKS_TO_REPLACE

            #endregion

            #region AROOTS_TO_REPLACE

            #endregion

            #region APARTMENTS_TO_REPLACE

            #endregion

            #region HOUSES_TO_REPLACE

            #endregion

            #region GROOTS_TO_REPLACE

            #endregion

            #region GARAGES_TO_REPLACE

            #endregion

            new NPC("vpound_w_0", "Джон", NPC.Types.Talkable, "ig_trafficwarden", new Vector3(485.6506f, -54.18661f, 78.30058f), 55.38f, Settings.MAIN_DIMENSION)
            {
                Blip = new Blip(832, new Vector3(485.6506f, -54.18661f, 78.30058f), "Штрафстоянка", 1f, 47, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION),

                DefaultDialogueId = "vpound_preprocess",
            };

            new NPC("vrent_s_0", "Джон", NPC.Types.Talkable, "s_m_m_trucker_01", new Vector3(-718.6724f, 5821.765f, 17.21804f), 106.9247f, Settings.MAIN_DIMENSION)
            {
                Blip = new Blip(76, new Vector3(-718.6724f, 5821.765f, 17.21804f), "Аренда мопедов", 0.85f, 47, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION),

                DefaultDialogueId = "vrent_s_preprocess",
            };
        }
    }
}