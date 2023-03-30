using Sirenix.OdinInspector;
using UnityEngine;

namespace Damage
{
    public class DamageController : MonoBehaviour
    {
        
        [Range(0,1)] public float baselineProbability = 0.5f;
        [LabelWidth(75)] public DamageRate lowerRate = new DamageRate(1, 3);
        [LabelWidth(75)] public DamageRate upperRate = new DamageRate(20, 1);
       
       [Min(1)] public int checksPerTurn = 4;
    }
}