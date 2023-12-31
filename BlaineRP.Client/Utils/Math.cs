﻿namespace BlaineRP.Client.Utils
{
    internal static class Math
    {
        public static float GetLimitedValue(float curValue, float minValue, float maxValue)
        {
            return System.Math.Min(maxValue, System.Math.Max(minValue, curValue));
        }
    }
}