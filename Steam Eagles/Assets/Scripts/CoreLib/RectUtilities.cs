using System;
using UnityEngine;

namespace CoreLib
{
    public static class RectUtilities
    {
        public static Rect Lerp(Rect rectMin, Rect rectMax, Rect t)
        {
            var minX = Mathf.Lerp(rectMin.x, rectMax.x, t.x);
            var minY = Mathf.Lerp(rectMin.y, rectMax.y, t.y);
            var width = Mathf.Lerp(rectMin.width, rectMax.width, t.width);
            var height =  Mathf.Lerp(rectMin.height, rectMax.height, t.height);
            return new Rect(minX, minY, width, height);
        }
        public static Rect Lerp(Rect rectMin, Rect rectMax, float t) => Lerp(rectMin, rectMax, new Rect(t, t, t, t));

        public static Rect InverseLerp(Rect rectMin, Rect rectMax, Rect rectValue)
        {
            var minX = Mathf.InverseLerp(rectMin.x, rectMax.x, rectValue.x);
            var minY = Mathf.InverseLerp(rectMin.y, rectMax.y, rectValue.y);
            var width = Mathf.InverseLerp(rectMin.width, rectMax.width, rectValue.width);
            var height =  Mathf.InverseLerp(rectMin.height, rectMax.height, rectValue.height);
            return new Rect(minX, minY, width, height);
        }

        public static int GetClosestRectInSize(Rect actual, params Rect[] options)
        {
            if (options == null || options.Length == 0)
            {
                return -1;
            }

            if (options.Length == 1)
                return 0;
            Vector2 closestSize = new Vector2(float.MaxValue, float.MaxValue);
            float minDist = float.MaxValue;
            int best = -1;
            for (int i = 0; i < options.Length; i++)
            {
                var size = options[i].size;
                var dif = Vector2.Distance(size, actual.size);
                if (dif < minDist)
                {
                    minDist = dif;
                    best = i;
                }
            }
            return best;
        }
        public static int GetClosestRectInMinPosition(Rect actual, params Rect[] options)
        {
            if (options == null || options.Length == 0)
            {
                return -1;
            }

            if (options.Length == 1)
                return 0;
            Vector2 closestSize = new Vector2(float.MaxValue, float.MaxValue);
            float minDist = float.MaxValue;
            int best = -1;
            for (int i = 0; i < options.Length; i++)
            {
                var pos = options[i].min;
                var dif = Vector2.Distance(pos, actual.min);
                if (dif < minDist)
                {
                    minDist = dif;
                    best = i;
                }
            }
            return best;
        }
    }
}