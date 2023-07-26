using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Client.Game.Data.Vehicles
{
    [Script(int.MaxValue)]
    public class Core
    {
        public static readonly Dictionary<string, Vehicle> All = new Dictionary<string, Vehicle>();

        public Core()
        {
            #region TO_REPLACE

            #endregion

            /*            var newModels = new List<string>
                        {
                            "boor", "brickade2", "broadway", "cargoplane2", "entity3", "eudora", "everon2", "issi8", "journey2", "manchez3", "panthere", "powersurge", "r300", "surfer3", "tahoma", "tulip2", "virtue",
                        };

                        foreach (var x in newModels)
                        {
                            var model = RAGE.Util.Joaat.Hash(x);

                            JObject data = new JObject();

                            var name = RAGE.Game.Ui.GetLabelText(RAGE.Game.Vehicle.GetDisplayNameFromVehicleModel(model));
                            var brand = RAGE.Game.Ui.GetLabelText(RAGE.Game.Invoker.Invoke<string>(0xF7AF4F159FF99F97, (int)model));

                            if (name == "NULL")
                                name = x;

                            if (brand != "NULL")
                                name = $"{brand} {name}";

                            data.Add("DisplayName", name);

                            data.Add("MaxSpeed", RAGE.Game.Vehicle.GetVehicleModelMaxSpeed(model));
                            data.Add("MaxBraking", RAGE.Game.Vehicle.GetVehicleModelMaxBraking(model));
                            data.Add("MaxTraction", RAGE.Game.Vehicle.GetVehicleModelMaxTraction(model));
                            data.Add("MaxAcceleration", RAGE.Game.Vehicle.GetVehicleModelAcceleration(model));

                            data.Add("_0xBFBA3BA79CFF7EBF", RAGE.Game.Invoker.Invoke<float>(RAGE.Game.Natives._0xBFBA3BA79CFF7EBF, (int)model));
                            data.Add("_0x53409B5163D5B846", RAGE.Game.Invoker.Invoke<float>(RAGE.Game.Natives._0x53409B5163D5B846, (int)model));
                            data.Add("_0xC6AD107DDC9054CC", RAGE.Game.Invoker.Invoke<float>(RAGE.Game.Natives._0xC6AD107DDC9054CC, (int)model));
                            data.Add("_0x5AA3F878A178C4FC", RAGE.Game.Invoker.Invoke<float>(RAGE.Game.Natives._0x5AA3F878A178C4FC, (int)model));

                            var seats = RAGE.Game.Vehicle.GetVehicleModelNumberOfSeats(model);

                            if (seats < 0)
                                seats = 0;

                            data.Add("MaxNumberOfPassengers", seats == 0 ? 0 : seats - 1);
                            data.Add("MaxOccupants", seats);
                            data.Add("VehicleClass", RAGE.Game.Vehicle.GetVehicleClassFromName(model));

                            Events.CallRemote("vehicle_data_p", model.ToString(), JsonConvert.SerializeObject(data));
                        }

                        Events.CallRemote("vehicle_data_f", newModels.Count);*/
        }

        public static Vehicle GetById(string id)
        {
            return id == null ? null : All.GetValueOrDefault(id);
        }

        public static Vehicle GetByModel(uint model)
        {
            return All.Where(x => x.Value.Model == model).Select(x => x.Value).FirstOrDefault();
        }
    }
}