using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Damage
{
    [CreateAssetMenu(menuName = "Steam Eagles/Storm Hits")]
    public class StormHitGen : SerializedScriptableObject
    {
    
        [SerializeField, ListDrawerSettings(Expanded = true)]
        List<StormHit> hits = new List<StormHit>();

        
    }
}