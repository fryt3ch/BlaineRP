using System.Collections.Generic;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Management.Phone
{
    public static class Numbers
    {
        private static readonly HashSet<uint> _usedPhoneNumbers = new HashSet<uint>();

        public static uint GenerateNewPhoneNumber()
        {
            while (true)
            {
                var num = (uint)SRandom.NextInt32(100_000, 999_999_999);

                if (_usedPhoneNumbers.Add(num))
                {
                    return num;
                }
            }
        }

        public static bool SetNumberAsUsed(uint number) => _usedPhoneNumbers.Add(number);
    }
}