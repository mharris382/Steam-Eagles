using System;
using System.Collections;
using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;

namespace Puzzles.GasNetwork
{
    [RequireComponent(typeof(GasNetworkManager))]
    public class GasNetworkTilemapAdapter : MonoBehaviour
    {
        public SharedTilemap pipeTilemap;
        private Tilemap _pipeTilemap;
        private IPipeGraph _pipeNetwork;

        private Tilemap PipeTilemap
        {
            get
            {
                if (_pipeTilemap == null)
                {
                    if (pipeTilemap != null && pipeTilemap.HasValue)
                        _pipeTilemap = pipeTilemap.Value;
                    else
                    {
                        var pipeTilemapGO = GameObject.FindGameObjectWithTag("PipeTilemap");
                        _pipeTilemap = pipeTilemapGO.GetComponent<Tilemap>();
                    }

                    Debug.Assert(_pipeTilemap != null, this);
                }

                return _pipeTilemap;
            }
        }

        private Grid PipeGrid => PipeTilemap.layoutGrid;

        private IPipeGraph PipeGraph => _pipeNetwork == null
            ? (_pipeNetwork = GetComponent<IPipeGraph>())
            : _pipeNetwork;

        private void Awake()
        {
            Debug.Assert(PipeTilemap != null);
            _pipeNetwork = GetComponent<GasNetworkManager>();
            MessageBroker.Default.Receive<BuildActionInfo>()
                .Where(t => t.tilemapType == TilemapTypes.PIPE)
                .Subscribe(OnPipeTileAdded).AddTo(this);

            MessageBroker.Default.Receive<DisconnectActionInfo>()
                .Where(t => t.tilemapType == TilemapTypes.PIPE)
                .Subscribe(OnPipeTileRemoved).AddTo(this);
        }

        private void Start()
        {
            GetComponent<GasNetworkManager>().Grid = this.PipeGrid;
            PipeTilemap.CompressBounds();
            var bounds = PipeTilemap.cellBounds;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    var pipeTile = PipeTilemap.GetTile(new Vector3Int(x, y, 0));
                    
                    if(pipeTile!=null)PipeGraph.AddNode(new Vector3Int(x, y));
                }
            }
        }


    void OnPipeTileAdded(BuildActionInfo info)
        {
            var tile = PipeTilemap.GetTile(info.cellPosition);
            if (tile != null)
            {
                PipeGraph.AddNode(info.cellPosition);
            }
        }

        void OnPipeTileRemoved(DisconnectActionInfo info)
        {
            PipeGraph.RemoveNode(info.cellPosition);
        }
    }
}