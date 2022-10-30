using UnityEngine;

namespace World
{
    public class BlockManager : MonoBehaviour
    {
        private const string DATABASE_ADDRESS = "BlockDatabase"; 
        
        public BlockDatabase blockDatabase; 
        private bool _blocksLoaded;
        private static BlockManager _instance;
        internal static BlockManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("BLOCK MANAGER", typeof(BlockManager)).GetComponent<BlockManager>();
                }
                return _instance;
            }
        }


        private void Awake()
        {
            if (_instance == null) _instance = this;
        }
    }
}