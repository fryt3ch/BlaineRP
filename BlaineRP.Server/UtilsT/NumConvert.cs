using System;

namespace BlaineRP.Server
{
    public static partial class Utils
    {
        public static decimal ToDecimal(object obj)
        {
            return Convert.ToDecimal(obj);
        }
        public static float ToSingle(object obj)
        {
            return Convert.ToSingle(obj);
        }

        public static byte ToByte(object obj)
        {
            return Convert.ToByte(obj);
        }

        public static ushort ToUInt16(object obj)
        {
            return Convert.ToUInt16(obj);
        }

        public static int ToInt32(object obj)
        {
            return Convert.ToInt32(obj);
        }

        public static long ToInt64(object obj)
        {
            return Convert.ToInt64(obj);
        }

        public static uint ToUInt32(object obj)
        {
            var value = Utils.ToDecimal(obj);

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
            var value = Utils.ToDecimal(obj);

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
