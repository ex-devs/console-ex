using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtendedConsole
{
    public static class Color
    {
        // Stolen from .Net source code
        public static float GetBrightness(byte R, byte G, byte B)
        {
            int max;
            int min;

            if (R > G)
            {
                max = (int)R;
                min = (int)G;
            }
            else
            {
                max = (int)G;
                min = (int)R;
            }

            if (B > max)
            {
                max = (int)B;
            }

            if (B < min)
            {
                min = (int)B;
            }

            return (max + min) / (byte.MaxValue * 2f);
        }
    }
}