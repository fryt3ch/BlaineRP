namespace BlaineRP.Server.Extensions.System
{
    public static class UInt64Extensions
    {
        public static bool TryAdd(this ulong source, ulong value, out ulong result)
        {
            unchecked
            {
                result = source + value;

                return result >= source;
            }
        }

        public static bool TrySubtract(this ulong source, ulong value, out ulong result)
        {
            unchecked
            {
                result = source - value;

                return result <= source;
            }
        }
    }
}