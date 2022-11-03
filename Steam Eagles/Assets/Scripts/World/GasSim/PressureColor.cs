using System;
using UnityEngine;

namespace GasSim
{
    /// <summary>
    /// defines the mapping between a gas particle's density and color (specifically alpha)
    /// </summary>
    [Serializable]
    internal class PressureColor
    {
        [Range(0,1)]
        [SerializeField] private float alphaMinPressure = 0.2f; 
        [Range(0,1)]
        [SerializeField] private float alphaMaxPressure = 0.8f; 
        
        
        public Color PressureToColor(int pressure, Color current)
        {
            current.a = PressureToAlpha(pressure);
            return current;
        }
        public Color PressureToColor(int pressure)
        {
            var current = Color.white;
            current.a = PressureToAlpha(pressure);
            return current;
        }

        public int ColorToPressure(Color color)
        {
            return AlphaToPressure(color.a);
        }

        private float PressureToAlpha(int pressure)
        {
            if (pressure <= 0) return 0f;
            int mult = (int)Mathf.Sign(pressure);
            float t = pressure / 16f;
            return Mathf.Lerp(alphaMinPressure, alphaMaxPressure, t) *  mult;
        }

        private int AlphaToPressure(float alpha)
        {
            int mult = (int)Mathf.Sign(alpha);

            int p = Mathf.RoundToInt(Mathf.InverseLerp(alphaMinPressure, alphaMaxPressure, alpha) * 16);
            return p * mult;
        }
    }
}