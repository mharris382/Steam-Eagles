using UnityEngine;

namespace CoreLib
{
    /// <summary>
    /// signal called when a dynamic object is spawned in the scene
    /// </summary>
    public struct SpawnedDynamicObjectInfo
    {
        public readonly ScriptableObject blockID;
        public GameObject dynamicBlock;

        public SpawnedDynamicObjectInfo(ScriptableObject blockID, GameObject dynamicBlock)
        {
            this.blockID = blockID;
            this.dynamicBlock = dynamicBlock;
        }
    }
}