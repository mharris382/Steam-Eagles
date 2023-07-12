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

     
    public class ColliderClimbableFactory : IClimbableFactory, IDisposable
    {
        private CompositeDisposable _cd;
        private Transform transform;
        private LayerMask _climbableLayerMask;
        private Collider2D[] _colliderCache;
        class BoxColliderClimbable : IClimbable
        {
            private Vector3 _min, _max;
            private BoxCollider2D _boxCollider2D;
            private Vector2 _size;
            public BoxColliderClimbable(BoxCollider2D boxCollider2D2D)
            {
                Rigidbody = boxCollider2D2D.attachedRigidbody;
                _boxCollider2D = boxCollider2D2D;
                _size = boxCollider2D2D.size;
                var offset1 = boxCollider2D2D.offset;
                var wsPos = boxCollider2D2D.transform.position;
                var min = (Vector2)wsPos + offset1 + (Vector2)(_size.y * 0.5f * Vector3.down);
                var max = (Vector2)wsPos + offset1 + (Vector2)((1 + _size.y * 0.5f) * Vector3.up);

                _min = boxCollider2D2D.attachedRigidbody.transform.InverseTransformPoint(min);
                _max = boxCollider2D2D.attachedRigidbody.transform.InverseTransformPoint(max);
                
                RecalculateBounds();
            }

            private void RecalculateBounds()
            {
                var size1 = _boxCollider2D.size;
                var offset1 = _boxCollider2D.offset;
                var wsPos = _boxCollider2D.transform.position;
                var min = (Vector2)wsPos + offset1 + (Vector2)(size1.y * 0.5f * Vector3.down);
                var max = (Vector2)wsPos + offset1 + (Vector2)(size1.y * 0.5f * Vector3.up);

                _min = _boxCollider2D.attachedRigidbody.transform.InverseTransformPoint(min);
                _max = _boxCollider2D.attachedRigidbody.transform.InverseTransformPoint(max);
                _size = size1;
            }

            public Rigidbody2D Rigidbody { get; }
            public Vector2 MinClimbLocalPosition
            {
                get
                {
                    if(_size != _boxCollider2D.size)RecalculateBounds();
                    return _min;
                }
            }

            public Vector2 MaxClimbLocalPosition
            {
                get
                {
                    if(_size != _boxCollider2D.size)RecalculateBounds();
                    return _max;
                }
            }
        }
        public ColliderClimbableFactory()
        {
            _cd = new CompositeDisposable();
            _climbableLayerMask = LayerMask.GetMask("Triggers");
            _colliderCache = new Collider2D[10];
        }

        public bool TryGetClimbable(Vector2 climberPosition, out IClimbable climbable, float maxDistance = 0.5f)
        {
            int hits = Physics2D.OverlapCircleNonAlloc(climberPosition, 0.2f, _colliderCache, _climbableLayerMask);
            if (hits > 0)
            {
                for (int i = 0; i < hits; i++)
                {
                    var c = _colliderCache[i];
                    if (c.CompareTag("Ladder"))
                    {
                        var box = c.GetComponent<BoxCollider2D>();
                        if(box != null)
                        {
                            climbable = new BoxColliderClimbable(box);
                            return true;
                        }
                    }
                }
            }

            climbable = null;
            return false;
        }

        public void Dispose()
        {
            _cd = new CompositeDisposable();
        }
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