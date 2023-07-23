using BlaineRP.Client.Extensions.RAGE.Elements;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Utils.Game
{
    internal static class Vehicles
    {
        public static Vehicle GetClosestVehicle(Vector3 position, float radius)
        {
            float minDistance = radius;

            Vehicle vehicle = null;

            for (int i = 0; i < Entities.Vehicles.Streamed.Count; i++)
            {
                var veh = Entities.Vehicles.Streamed[i];

                if (veh == null)
                    continue;

                float distance = Vector3.Distance(position, veh.Position);

                if (distance <= radius && minDistance >= distance)
                {
                    vehicle = veh;
                    minDistance = distance;
                }
            }

            return vehicle;
        }

        public static Vehicle GetClosestVehicleToSeatIn(Vector3 position, float radius, int seatId = -1)
        {
            float minDistance = radius;

            Vehicle vehicle = null;

            seatId--;

            for (int i = 0; i < Entities.Vehicles.Streamed.Count; i++)
            {
                var veh = Entities.Vehicles.Streamed[i];

                if (veh == null)
                    continue;

                float distance = Vector3.Distance(position, veh.Position);

                if (distance <= radius && minDistance >= distance)
                {
                    if (veh.IsDead(0))
                        continue;

                    if (seatId < 0)
                    {
                        if (!veh.AreAnySeatsFree())
                            continue;
                    }
                    else
                    {
                        if (!veh.IsSeatFree(seatId, 0))
                            continue;
                    }

                    vehicle = veh;
                    minDistance = distance;
                }
            }

            return vehicle;
        }

        public static string GetVehicleName(Vehicle veh, byte type = 0)
        {
            var data = Data.Vehicles.GetByModel(veh.Model);

            if (type == 0)
            {
                return data?.Name ?? string.Empty;
            }
            else if (type == 1)
            {
                var np = veh.GetNumberplateText();

                return $"{data?.Name ?? string.Empty} [{(np == null || np.Length == 0 ? Locale.Get("VEHICLE_NP_NONE") : np)}]";
            }

            return string.Empty;
        }

        public static bool PlayerInFrontOfVehicle(Vehicle vehicle, float radius = 2f)
        {
            var leftFront = vehicle.GetWorldPositionOfBone(vehicle.GetBoneIndexByName("suspension_lf"));
            var rightFront = vehicle.GetWorldPositionOfBone(vehicle.GetBoneIndexByName("suspension_rf"));

            var leftBack = vehicle.GetWorldPositionOfBone(vehicle.GetBoneIndexByName("suspension_lr"));
            var rightBack = vehicle.GetWorldPositionOfBone(vehicle.GetBoneIndexByName("suspension_rr"));

            var playerPos = Player.LocalPlayer.Position;

            if (Vector3.Distance(playerPos, leftFront) <= radius || Vector3.Distance(playerPos, rightFront) <= radius)
                return true;
            else if (Vector3.Distance(playerPos, leftBack) <= radius || Vector3.Distance(playerPos, rightBack) <= radius)
                return false;

            return false;
        }
    }
}
