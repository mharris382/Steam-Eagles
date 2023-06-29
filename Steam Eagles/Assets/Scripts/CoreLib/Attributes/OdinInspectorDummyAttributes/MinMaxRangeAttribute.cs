
    using UnityEngine;

    public class MinMaxRangeAttribute : PropertyAttribute
    {
        public MinMaxRangeAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public float min { get; set; }
        public float max { get; set; }
    }
