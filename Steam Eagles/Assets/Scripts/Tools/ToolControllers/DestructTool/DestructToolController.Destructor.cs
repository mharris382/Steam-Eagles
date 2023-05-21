using System;
using System.Linq;
using Buildings.BuildingTilemaps;
using CoreLib.Interfaces;
using UniRx;
using UnityEngine;

namespace Tools.DestructTool
{
    public partial class DestructToolController
    {
        public class Destructor 
        {
            private readonly IDestruct _destructable;
            private readonly DestructToolController _tool;

            private float _lastHitTime;
            private float _remainingTimeTillNextDestruct;
            private Subject<DestructParams> _onDestruct = new Subject<DestructParams>();
            private readonly bool _isTilemap;
            private readonly BuildingTilemap _destructableTilemap;
            
            public IObservable<DestructParams> OnDestruct => _onDestruct;

            public Destructor(IDestruct destructable, DestructToolController tool)
            {
                _destructable = destructable;
                _tool = tool;
                _remainingTimeTillNextDestruct = tool.config.rate;
                if (destructable is BuildingTilemap)
                {
                    _isTilemap = true;
                    _destructableTilemap = destructable as BuildingTilemap;
                    
                   // var layer = buildingtm.Layer;
                   // var map = buildingtm.Building.Map;
                   // var cell = map.WorldToCell(tool.transform.position, layer);
                }
                _onDestruct.Buffer(TimeSpan.FromSeconds(tool.config.rate))
                    .Where(t => t.Count > 0)
                    .Select(t=> t.First())
                    .Subscribe(t =>
                    {
                        Debug.Log($"Got hit on {_destructable} with {t}");
                        _destructable.TryToDestruct(t);
                    });
            }

            private static readonly Vector3Int[] neighbors = new Vector3Int[]
            {
                Vector3Int.down, Vector3Int.up, Vector3Int.left, Vector3Int.right,
                Vector3Int.down + Vector3Int.up, Vector3Int.left + Vector3Int.right,
                Vector3Int.down + Vector3Int.left, Vector3Int.down + Vector3Int.right,
            };
            public void OnHit(float dt, DestructParams dparams)
            {
                if (_isTilemap)
                {
                    _onDestruct.OnNext(dparams);
                }
                else
                {
                    _onDestruct.OnNext(dparams);
                }
                // _lastHitTime = Time.realtimeSinceStartup;
                // _remainingTimeTillNextDestruct -= dt;
                // if (_remainingTimeTillNextDestruct > 0)
                //     return;
                //
                // _remainingTimeTillNextDestruct += _tool.config.rate;
                // _destructable.TryToDestruct(dparams);
            }
        }
    }
}