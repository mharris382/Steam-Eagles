using System;
using CoreLib;
using UniRx;
using UnityEngine;

namespace Characters
{
    public interface IClimbableFactory
    {
        bool TryGetClimbable(Vector2 climberPosition, out IClimbable climbable, float maxDistance = 0.5f);
    }


    public class TilemapClimbableFactory : IClimbableFactory, IDisposable
    {
        private Rigidbody2D _buildingBody;
        private ILadderTilemap _ladderTilemap;
        private IDisposable _disposable;
        public TilemapClimbableFactory(IObservable<Rigidbody2D> buildingBody)
        {
            _disposable = buildingBody.Subscribe(body => {
                if (body == null)
                {
                    _buildingBody = null;
                    _ladderTilemap = null;
                    return;
                }
                _buildingBody = body;
                _ladderTilemap = _buildingBody.GetComponentInChildren<ILadderTilemap>();
                if (_ladderTilemap == null)
                {
                    _buildingBody = null;
                    _ladderTilemap = null;
                }
            });
        }
        public bool TryGetClimbable(Vector2 climberPosition, out IClimbable climbable, float maxDistance = 0.5f)
        {
            climbable = null;
            if (_ladderTilemap == null)
                return false;
            var cell = _ladderTilemap.Tilemap.WorldToCell(new Vector3(climberPosition.x, climberPosition.y));
            var tile = _ladderTilemap.Tilemap.GetTile(cell);
            if (tile == null)
                return false;
            Vector3Int highestCell = FindHighestCell(), lowestCell = FindLowestCell();

            Vector3 maxPosition = _ladderTilemap.Tilemap.CellToLocal(highestCell);
            Vector3 minPosition = _ladderTilemap.Tilemap.CellToLocal(lowestCell);
            
            //offset to center
            var cellSize = _ladderTilemap.Tilemap.cellSize;
            maxPosition.x += cellSize.x / 2f;
            minPosition.x += cellSize.x / 2f;
            
            climbable = new TilemapClimbableInstance(_ladderTilemap.Tilemap.GetComponentInParent<Rigidbody2D>(), minPosition, maxPosition);
            return true;
            
            Vector3Int FindLowestCell()
            {
                Vector3Int current = cell;
                Vector3Int direction = Vector3Int.down;
                
                while(true)
                {
                    current += direction;
                    var t = _ladderTilemap.Tilemap.GetTile(current);
                    if (t == null)
                    {
                        return current;
                    }
                }
            }
            Vector3Int FindHighestCell()
            {
                Vector3Int current = cell;
                Vector3Int direction = Vector3Int.up;
                while(true)
                {
                    current += direction;
                    var t = _ladderTilemap.Tilemap.GetTile(current);
                    if (t == null)
                    {
                        return current;
                    }
                }
            }
            
        }
        
        
        private struct TilemapClimbableInstance : IClimbable
        {
            public TilemapClimbableInstance(Rigidbody2D rigidbody, Vector2 minClimbPosition, Vector2 maxClimbPosition)
            {
                Rigidbody = rigidbody;
                MinClimbLocalPosition = minClimbPosition;
                MaxClimbLocalPosition = maxClimbPosition;
            }

            public Rigidbody2D Rigidbody { get; }
            public Vector2 MinClimbLocalPosition { get; }
            public Vector2 MaxClimbLocalPosition { get; }
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }
}