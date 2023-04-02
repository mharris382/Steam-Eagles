using System;
using CoreLib;
using UniRx;
using UnityEngine;

namespace Spaces
{
    public class TilemapDynamicBlockSpawner : MonoBehaviour
    {
        private void Awake()
        {
            MessageBroker.Default.Receive<DisconnectActionInfo>().Subscribe(disconnectAction =>
            {
                var location = disconnectAction.cellPosition;
                var tilemap = disconnectAction.tilemap;
                var tile = tilemap.GetTile(location);
                 
            });
        }
    }
}