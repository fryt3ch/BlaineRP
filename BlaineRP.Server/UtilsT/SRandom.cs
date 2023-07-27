using System;
using System.Security.Cryptography;

namespace BlaineRP.Server.UtilsT
{
    public static class SRandom
    {
        private static Random random = new Random();
        private static RandomNumberGenerator secureRandom = RandomNumberGenerator.Create();

        private static object secureRandomLock = new object();
        private static object randomLock = new object();

        public static int NextInt32(int max) => NextInt32(0, max);

        public static int NextInt32(int min, int max)
        {
            lock (randomLock)
            {
                return random.Next(min, max);
            }
        }

        public static double NextDouble()
        {
            lock (randomLock)
            {
                return random.NextDouble();
            }
        }

        public static void NextBytes(byte[] buffer)
        {
            lock (randomLock)
            {
                random.NextBytes(buffer);
            }
        }

        public static int GetInt32(int max = int.MaxValue) => GetInt32(0, max);

        public static int GetInt32(int min, int max)
        {
            return (new Random()).Next(min, max);
        }

        public static double GetDouble()
        {
            return (new Random()).NextDouble();
        }

        public static int NextInt32S(int min, int max)
        {
            lock (secureRandomLock)
            {
                var bytes = new byte[4];

                secureRandom.GetBytes(bytes);

                var res = BitConverter.ToUInt32(bytes, 0);

                return (int)(min + (max - min) * (res / (uint.MaxValue + 1.0)));
            }
        }

        public static double NextDoubleS()
        {
            lock (secureRandomLock)
            {
                var bytes = new byte[8];

                secureRandom.GetBytes(bytes);

                var res = BitConverter.ToUInt64(bytes, 0) / (1 << 11);

                return res / (double)(1UL << 53);
            }
        }

        public static void NextBytesS(byte[] buffer)
        {
            lock (secureRandomLock)
            {
                secureRandom.GetBytes(buffer);
            }
        }

        public static int GetInt32S(int min, int max)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];

                rng.GetBytes(bytes);

                var res = BitConverter.ToUInt32(bytes, 0);

                return (int)(min + (max - min) * (res / (uint.MaxValue + 1.0)));
            }
        }

        public static double GetDoubleS()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[8];

                rng.GetBytes(bytes);

                var res = BitConverter.ToUInt64(bytes, 0) / (1 << 11);

                return res / (double)(1UL << 53);
            }
        }
    }
}
