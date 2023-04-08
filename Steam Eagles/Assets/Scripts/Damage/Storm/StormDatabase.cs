using Sirenix.OdinInspector;
using UnityEngine;

namespace Damage
{
    [CreateAssetMenu(menuName = "Steam Eagles/Storm Database")]
    public class StormDatabase : ScriptableObject
    {
        [TableList(AlwaysExpanded = true)]
        [SerializeField] private Storm[] storms;
        
        
        private static StormDatabase _instance;
        public static StormDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<StormDatabase>("StormDatabase");
                }
                return _instance;
            }
        }

        public Storm GetStorm(int intensity)
        {
            intensity = Mathf.Clamp(intensity, 0, storms.Length-1);
            return storms[intensity];
        }
    }
}