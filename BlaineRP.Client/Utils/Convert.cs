namespace BlaineRP.Client.Utils
{
    internal static class Convert
    {
        public static double ToDouble(object obj)
        {
            return System.Convert.ToDouble(obj);
        }

        public static byte ToByte(object obj)
        {
            return System.Convert.ToByte(obj);
        }

        public static decimal ToDecimal(object obj)
        {
            return System.Convert.ToDecimal(obj);
        }

        public static int ToInt32(object obj)
        {
            return System.Convert.ToInt32(obj);
        }

        public static long ToInt64(object obj)
        {
            return System.Convert.ToInt64(obj);
        }

        public static float ToSingle(object obj)
        {
            return System.Convert.ToSingle(obj);
        }

        public static ushort ToUInt16(object obj)
        {
            return System.Convert.ToUInt16(obj);
        }

        public static uint ToUInt32(object obj)
        {
            var value = System.Convert.ToDecimal(obj);

            unchecked
            {
                if (value < 0)
                {
                    return (uint)(int)value;
                }

                return (uint)value;
            }
        }

        public static ulong ToUInt64(object obj)
        {
            var value = System.Convert.ToDecimal(obj);

            unchecked
            {
                if (value < 0)
                {
                    return (ulong)(long)value;
                }

                return (ulong)value;
            }
        }
    }
}
