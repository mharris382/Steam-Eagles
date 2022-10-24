using CoreLib;
using UniRx;
using UnityEngine;

namespace Puzzles
{
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
            go.AddComponent<DynamicBlock>();
            go.tag = "DynamicBlock";
        }
    }
}