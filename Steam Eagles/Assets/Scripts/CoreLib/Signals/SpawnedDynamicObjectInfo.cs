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
    
    public struct SpawnedPickupInfo
    {
        public readonly ScriptableObject pickupID;
        public GameObject pickupInstance;

        public SpawnedPickupInfo(ScriptableObject pickupID, GameObject pickupInstance)
        {
            this.pickupID = pickupID;
            this.pickupInstance = pickupInstance;
        }
    }
}