namespace BlaineRP.Server.Extensions.System
{
    public static class UInt32Extensions
    {
        public static bool TryAdd(this uint source, uint value, out uint result)
        {
            unchecked
            {
                result = source + value;

                return result >= source;
            }
        }

        public static bool TrySubtract(this uint source, uint value, out uint result)
        {
            unchecked
            {
                result = source - value;

                return result <= source;
            }
        }
    }
}