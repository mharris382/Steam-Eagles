using UnityEngine;

namespace World.GasSim
{
    [CreateAssetMenu(fileName = "Simulation Config", menuName = "Gas Sim/Simulation Config", order = 0)]
    public class SimulationConfig : ScriptableObject
    {
        public float updateRate = 0.125f;
        
        [Range(1, 32)]
        public int maxGasDensity = 16;

        public Gradient gasColorGradient = new Gradient()
        {
            alphaKeys = new[] { new GradientAlphaKey(0.2f, 0), new GradientAlphaKey(0.8f, 1) },
            colorKeys = new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }
        };
    }
}