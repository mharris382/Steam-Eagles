using UnityEngine;

namespace World
{
    [CreateAssetMenu(fileName = "BlockDatabase", menuName = "BlockDatabase", order = 0)]
    public class BlockDatabase : ScriptableObject
    {
        
        public BlockData[] blockData;
    }
}