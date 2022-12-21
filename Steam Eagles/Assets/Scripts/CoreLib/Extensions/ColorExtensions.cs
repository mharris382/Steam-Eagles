using UnityEngine;

namespace CoreLib
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

        public static Color Darken(this Color color, float intensity) => Color.Lerp(color, Color.black, intensity);
        public static Color Lighten(this Color color, float intensity) => Color.Lerp(color, Color.white, intensity);

        public static Color Mix(this Color color, Color other, float amount) => Color.Lerp(color, other, amount);
    }
}