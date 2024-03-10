using System;
using UnityEngine;
using System.Collections;

namespace Core
{
    public static class MathUtils
    {
        public static float GetImageFillValue(int targetIndex, int currentIndex)
        {
            if (targetIndex == 0) return 0;

            double percentage = ((currentIndex * 100) / targetIndex);
            float floatPercentage = (float)percentage;
            return floatPercentage / 100;
        }
    }
}