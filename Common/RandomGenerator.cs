using System;
using System.Security.Cryptography;

namespace Common
{
    public static class RandomGenerator
    {
        public static int GetIntWithNDigits(int digits)
        {
            if (digits < 1 || digits > 9) throw new ArgumentOutOfRangeException(nameof(digits));

            int minValue = 1;
            for (int i = 0; i < digits - 1; i++)
            {
                minValue *= 10;
            }

            int maxValue = minValue * 10;

            return GetInt(minValue, maxValue);
        }

        public static int GetInt(int minValue, int maxValue)
        {
            using (RNGCryptoServiceProvider randomProvider = new RNGCryptoServiceProvider())
            {
                byte[] randomBytes = new byte[4];
                randomProvider.GetBytes(randomBytes);
                int seed = randomBytes[3] << 24 | randomBytes[0] << 16 | randomBytes[2] << 8 | randomBytes[1];
                Random random = new Random(seed);
                return random.Next(minValue, maxValue);
            }
        }
    }
}
