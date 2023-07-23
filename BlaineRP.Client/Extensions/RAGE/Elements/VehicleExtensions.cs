using RAGE.Elements;

namespace BlaineRP.Client.Extensions.RAGE.Elements
{
    public static class VehicleExtensions
    {
        public static float GetSpeedKm(this Vehicle vehicle) => vehicle.GetSpeed() * 3.6f;

        public static int GetColourType(this Vehicle veh)
        {
            int t = 0, a = 0;

            veh.GetModColor1(ref t, ref a, ref a);

            return t;
        }

        public static int GetFirstFreeSeatId(this Vehicle veh, int startIdx = 0)
        {
            for (int i = startIdx; i < global::RAGE.Game.Vehicle.GetVehicleModelNumberOfSeats(veh.Model); i++)
                if (veh.IsSeatFree(i - 1, 0))
                    return i;

            return -1;
        }

        public static Utils.Colour GetNeonColour(this Vehicle veh)
        {
            int r = 0, g = 0, b = 0;

            veh.GetNeonLightsColour(ref r, ref g, ref b);

            return new Utils.Colour((byte)r, (byte)g, (byte)b);
        }

        public static string GetNumberplateText(this Vehicle veh) => veh.GetNumberPlateText()?.Replace(" ", "");

        public static int? GetPearlColour(this Vehicle veh)
        {
            int t = 0, a = 0;

            veh.GetExtraColours(ref t, ref a);

            return t == 0 ? null : (int?)t;
        }

        public static Utils.Colour GetPrimaryColour(this Vehicle veh)
        {
            int r = 0, g = 0, b = 0;

            veh.GetCustomPrimaryColour(ref r, ref g, ref b);

            return new Utils.Colour((byte)r, (byte)g, (byte)b);
        }

        public static Utils.Colour GetSecondaryColour(this Vehicle veh)
        {
            int r = 0, g = 0, b = 0;

            veh.GetCustomSecondaryColour(ref r, ref g, ref b);

            return new Utils.Colour((byte)r, (byte)g, (byte)b);
        }

        public static int GetTrailerVehicle(this Vehicle veh)
        {
            int res = -1;

            veh.GetTrailerVehicle(ref res);

            return res;
        }

        public static Utils.Colour GetTyreSmokeColour(this Vehicle veh)
        {
            int r = 0, g = 0, b = 0;

            veh.GetTyreSmokeColor(ref r, ref g, ref b);

            return new Utils.Colour((byte)r, (byte)g, (byte)b);
        }

        public static int? GetWheelsColour(this Vehicle veh)
        {
            int t = 0, a = 0;

            veh.GetExtraColours(ref a, ref t);

            return t == 0 ? null : (int?)t;
        }

        public static int? GetXenonColour(this Vehicle veh)
        {
            if (!veh.IsToggleModOn(22))
                return null;

            var colour = global::RAGE.Game.Invoker.Invoke<int>(0x3DFF319A831E0CDB, veh.Handle);

            if (colour == 255)
                return -1;

            return colour;
        }

        public static bool IsAttachedToTrailer(this Vehicle veh, int trailerHandle) => veh.GetTrailerVehicle() == trailerHandle;

        public static void SetColourType(this Vehicle veh, int type)
        {
            int t = 0, a = 0;

            veh.GetExtraColours(ref t, ref a);

            veh.SetModColor1(type, 0, 0);

            veh.SetModColor2(type, 0);

            veh.SetExtraColours(t, a);
        }

        public static void SetNeonEnabled(this Vehicle veh, bool state)
        {
            for (int i = 0; i < 4; i++)
                veh.SetNeonLightEnabled(i, state);
        }

        public static void SetPearlColour(this Vehicle veh, int colour) => veh.SetExtraColours(colour, veh.GetWheelsColour() ?? 0);

        public static void SetWheels(this Vehicle veh, int type, int num, bool front = true)
        {
            veh.SetWheelType(type);

            veh.SetMod(front ? 23 : 25, num, false);
        }

        public static void SetWheelsColour(this Vehicle veh, int colour) => veh.SetExtraColours(veh.GetPearlColour() ?? 0, colour);

        public static void SetXenonColour(this Vehicle veh, int? colour)
        {
            if (colour is int col)
            {
                veh.ToggleMod(22, true);

                global::RAGE.Game.Invoker.Invoke(0xE41033B25D003A07, veh.Handle, col);
            }
            else
            {
                veh.ToggleMod(22, false);
            }
        }

        public static void TaskTempAction(this Vehicle veh, int action, int time)
        {
            var driverPed = veh.GetPedInSeat(-1, 0);

            if (driverPed < 0)
                return;

            global::RAGE.Game.Invoker.Invoke(global::RAGE.Game.Natives.TaskVehicleTempAction, driverPed, veh.Handle, action, time);
        }
    }
}