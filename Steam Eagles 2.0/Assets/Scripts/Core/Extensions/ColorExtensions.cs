using UnityEngine;

namespace Core
{
    public static class ColorExtensions
    {
        public static Color SetRed(this Color color, float red)
        {
            color.r = red;
            return color;
        }
        
        public static Color SetGreen(this Color color, float green)
        {
            color.g = green;
            return color;
        }
        
        public static Color SetBlue(this Color color, float blue)
        {
            color.b = blue;
            return color;
        }
        public static Color SetAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}