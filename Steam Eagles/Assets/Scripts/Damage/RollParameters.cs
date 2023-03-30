using UnityEngine;

namespace Damage
{
    public struct RollParameters
    {
        public const int MAX_CHANCES = 100;
        public float Percent { get; }
        public int Chances { get; }

        public RollParameters(float percent, int chances)
        {
            Percent = percent;
            Chances = Mathf.Clamp(chances, 1, MAX_CHANCES);
        }
    }
}