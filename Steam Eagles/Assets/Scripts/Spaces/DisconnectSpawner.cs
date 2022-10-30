using System;
using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;

namespace Spaces
{
    public class DisconnectSpawner : MonoBehaviour
    {
        public void Awake()
        {
            MessageBroker.Default.Receive<DisconnectActionInfo>().TakeUntilDestroy(this).Subscribe(OnDisconnectAction);
        }

        private void OnDisconnectAction(DisconnectActionInfo disconnectActionInfo)
        {
            
        }


        private bool TryGetDynamicBlock(TileBase tile, out DynamicBlock dynamicBlock)
        {
            if (BlockManager.Instance.IsLoaded)
            {
                
            }
            dynamicBlock = null;
            return false;
        }
    }
}