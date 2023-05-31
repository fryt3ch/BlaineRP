using RAGE; using RAGE.Elements; using System.Collections.Generic;
//using System.Collections.Generic;

namespace BCRPClient.Data
{
    public partial class Locations : Events.Script
    {
        public Locations()
        {
            NorthYankton.Initialize();

            CayoPerico.Initialize();

            Garage.Style.LoadAll();

            #region MARKETSTALLS_TO_REPLACE

            #endregion

            #region ELEVATORS_TO_REPLACE

            #endregion

            #region BIZS_TO_REPLACE

            #endregion

            #region JOBS_TO_REPLACE

            #endregion

            #region FRACTIONS_TO_REPLACE

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

            #region DRIVINGSCHOOLS_TO_REPLACE

            #endregion

            #region FISHBUYERS_TO_REPLACE

            #endregion

            #region VEHICLEDESTR_TO_REPLACE

            #endregion

            #region ESTAGENCIES_TO_REPLACE

            #endregion

            #region CASINOS_TO_REPLACE

            #endregion

            InitializeBlips();

            InitializeNPCs();
        }
    }
}