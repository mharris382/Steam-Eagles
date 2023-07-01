using CoreLib;
using Puzzles;
using UniRx;
using UnityEngine;

namespace PhysicsFun.DynamicBlocks
{
    [System.Obsolete("Entire DynamicBlocks system is deprecated, it will be phased out and replaced with the item pickup system")]
    public class DynamicBlockManager : MonoBehaviour
    {
        private void Awake()
        {
            MessageBroker.Default.Receive<SpawnedDynamicObjectInfo>()
                .TakeUntilDestroy(this)
                .Subscribe(OnDynamicObjectSpawned);
                
        }

        private void OnDynamicObjectSpawned(SpawnedDynamicObjectInfo spawnedDynamicObjectInfo)
        {
            var go = spawnedDynamicObjectInfo.dynamicBlock;
            var dynamicBlock = go.AddComponent<DynamicBlock>();
            dynamicBlock.blockID = spawnedDynamicObjectInfo.blockID;
            go.tag = "DynamicBlock";
        }
    }
}