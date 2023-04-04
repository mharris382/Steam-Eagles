using Sirenix.OdinInspector;
using UnityEngine;

namespace AI.Context
{
    //TODO: come back to this when status effects are implemented
    public class AIContextWrapper : MonoBehaviour
    {
        [ShowInInspector]
        public AIContext Context
        {
            get;
            set;
        }
    }

    public struct AIContext
    {
        public Vector2 position;
        public Vector2 velocity;
        public short health;
        public short maxHealth;
        public short energy;
        public short maxEnergy;
        public float HealthAsPercentage => (float) health / maxHealth;
        public float EnergyAsPercentage => (float) energy / maxEnergy;
 }
    
}