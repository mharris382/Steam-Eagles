using Sirenix.OdinInspector;
using UnityEngine;

namespace Damage
{
    [System.Serializable]
    public class Storm
    {
        public int intensity = 1;
        
        [SerializeField, SuffixLabel("min")]
        private float duration = 5;
        
        [SerializeField, SuffixLabel("sec")]
        private float damageCheckInterval = 1;
        
        
        public DamageCalculator damageCalculator;
        
        public float DurationInSeconds => duration * 60;
        
        
        public float DamageCheckIntervalInSeconds => damageCheckInterval;
        
        public float DamageCheckIntervalInPercent => DamageCheckIntervalInSeconds / DurationInSeconds;
    }
}