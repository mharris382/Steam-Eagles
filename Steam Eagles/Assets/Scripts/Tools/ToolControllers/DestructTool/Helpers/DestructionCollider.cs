using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using CoreLib.Interfaces;
using UniRx;
using UnityEngine;

namespace Tools.DestructTool.Helpers
{
    public interface ITilemapDestructionLogic
    {
        public IEnumerable<BuildingCell> GetEffectedCells(BuildingMap buildingMap);
    }
    
    [RequireComponent(typeof(Collider2D))]
    public abstract class DestructionCollider : MonoBehaviour, ITilemapDestructionLogic
    {
        private Collider2D _collider;
        private Dictionary<Collider2D, IDestruct> _seen = new();
        private ReactiveCollection<IDestruct> _destructing = new();
        
        public Collider2D Collider => _collider ? _collider : _collider = GetComponent<Collider2D>();

        public IReadOnlyReactiveCollection<IDestruct> Destructing => _destructing;
        

        public virtual float PlacementRadius => 0;
        
        public IEnumerable<BuildingCell> GetEffectedCells(BuildingMap buildingMap)
        {
            var bounds = Collider.bounds;
            foreach (var layer in GetEffectedLayers())
            {
                var size = buildingMap.GetCellSize(layer);
                var startGridPosition = buildingMap.WorldToCell(bounds.min, layer);
                var endGridPosition = buildingMap.WorldToCell(bounds.max, layer);
                for (int x = startGridPosition.x; x < endGridPosition.x; x++)
                {
                    for (int y = endGridPosition.y; y < endGridPosition.y; y++)
                    {
                        var gridPos = new BuildingCell(new Vector2Int(x, y), layer);
                        if(buildingMap.HasCell(gridPos) && IsCellEffected(gridPos, buildingMap))
                            yield return gridPos;
                    }
                }
            }
        }

        protected virtual IEnumerable<BuildingLayers> GetEffectedLayers()
        {
            yield return BuildingLayers.SOLID;
            yield return BuildingLayers.PIPE;
        }
        
        protected virtual bool IsCellEffected(BuildingCell cell, BuildingMap buildingMap)
        {
            return true;
        }

        private void Awake()
        {
            Collider.isTrigger = true;
        }

        private void OnEnable()
        {
            Collider.enabled = true;
        }

        private void OnDisable()
        {
            Collider.enabled = false;
        }
        

        private void OnTriggerEnter2D(Collider2D col)
        {
            IDestruct destruct;
            if (_seen.TryGetValue(col, out destruct) || col.gameObject.TryGetComponent<IDestruct>(out destruct))
            {
                _seen.TryAdd(col, destruct);
                if(_destructing.Contains(destruct) == false)
                    _destructing.Add(destruct);
            }
        }   

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_seen.TryGetValue(other, out var destruct))
            {
                _destructing.Remove(destruct);
            }
        }
    }
}