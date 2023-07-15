using System;
using Buildings;
using CoreLib;
using CoreLib.Extensions;
using CoreLib.Interfaces;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Characters
{

     
    public class ColliderClimbableFactory : IClimbableFactory
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


        public void Dispose()
        {
            _cd = new CompositeDisposable();
        }

        public bool TryGetClimbable(Vector2 climberPosition, out IClimbable climbable, float maxDistance)
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

    }

    //
   [Serializable]
   public class TilemapClimbableValues
   {
       public float ladderDetectTime = 0.5f;
      [ReadOnly] public float timeLastHasClimbable;
  
      public float TimeSinceClimbable => Time.time - timeLastHasClimbable;
      public bool HasClimbable() => TimeSinceClimbable <= ladderDetectTime;
  
      // [FoldoutGroup("Debugging"),ReadOnly] public bool hasRoom;
      // [FoldoutGroup("Debugging"),ReadOnly] public LadderTilemap ladderTilemap;
      // [FoldoutGroup("Debugging"),ReadOnly] public Building building;
      // [FoldoutGroup("Debugging"),ReadOnly] public bool checkingForLadders;
      // [FoldoutGroup("Debugging/Update"),ReadOnly] public float timeLastUpdated;
   }
    public class TilemapClimbableFactory : IClimbableFactory, IDisposable
    {
        // private readonly EntityRoomState _state;
        private readonly TilemapClimbableValues _tilemapClimbableValues;
        private Rigidbody2D _buildingBody;
        private ILadderTilemap _ladderTilemap;
        private IDisposable _disposable;
        private CompositeDisposable _cd = new();
        private GameObject _rigidbodyGameObject;
        private Transform _lastParent;


        private IReadOnlyReactiveProperty<bool> _hasRoom;
        // private ReadOnlyReactiveProperty<LadderTilemap> _currentLadderTilemap;
        // private ReadOnlyReactiveProperty<Building> _currentBuilding;

        private float _timeSinceClimbableDetected;
        // private ReadOnlyReactiveProperty<BuildingCell> _lastClimbableCell;
        private readonly ReadOnlyReactiveProperty<bool> _checkForLadders;
        private readonly Collider2D[] _colliderCache;
        private readonly int _climbableLayerMask;


        // private struct PotentialClimbable
        // {
        //     public Rigidbody2D rigidbody;
        //     public Vector2 start;
        //     public Vector2 top;
        //     public Vector2 bottom;
        //     public PotentialClimbable(Building building, Vector3 start, Vector3 top, Vector3 bottom)
        //     {
        //         rigidbody = building.GetComponent<Rigidbody2D>();
        //         this.start = start;
        //         this.top = top;
        //         this.bottom = bottom;
        //     }
        //
        //     public IClimbable GetClimable()
        //     {
        //         return new TilemapClimbableInstance(rigidbody, top, bottom);
        //     }
        // }
        
        // private ReadOnlyReactiveProperty<PotentialClimbable> _potentialClimbable;

        public TilemapClimbableFactory(
            //EntityRoomState state,
            IObservable<Rigidbody2D> buildingBody,
            TilemapClimbableValues tilemapClimbableValues)
        {
            _climbableLayerMask = LayerMask.GetMask("Triggers");
            _colliderCache = new Collider2D[10];
            // _state = state;
            _tilemapClimbableValues = tilemapClimbableValues;
           //  _hasRoom = state.CurrentRoom.WithCurrent().Select(t => t != null).ToReadOnlyReactiveProperty();
           //  _hasRoom.SubscribeWithCurrent(t => tilemapClimbableValues.hasRoom = t).AddTo(_cd);
           //  
           //  _currentBuilding = state.CurrentBuilding; 
           //  _currentBuilding.SubscribeWithCurrent(t => tilemapClimbableValues.building = t).AddTo(_cd);
           //  
           //  _currentLadderTilemap = _currentBuilding.Where(t => t != null).Select(t => t.LadderTilemap).ToReadOnlyReactiveProperty();
           //  _currentLadderTilemap.SubscribeWithCurrent(t => tilemapClimbableValues.ladderTilemap = t).AddTo(_cd);
           //  _currentLadderTilemap.AddTo(_cd);
           //  
           // _checkForLadders = _hasRoom.WithCurrent().ZipLatest(_currentBuilding.WithCurrent(), _currentLadderTilemap.WithCurrent(),
           //      (b, building, ladderTm) => b && building != null && ladderTm != null).ToReadOnlyReactiveProperty();
           //
           // _checkForLadders.SubscribeWithCurrent(t => tilemapClimbableValues.checkingForLadders = t).AddTo(_cd);
           //  
           //  var hasClimbableSubject = new Subject<PotentialClimbable>();
           //  _potentialClimbable = hasClimbableSubject.ToReadOnlyReactiveProperty();
           //  
           //  hasClimbableSubject.Subscribe(_ => _tilemapClimbableValues.timeLastHasClimbable = Time.time).AddTo(_cd);
           //
           //  _checkForLadders.Select(t => t ? Observable.EveryUpdate() : Observable.Never<long>()).Switch()
           //      .Do(t => tilemapClimbableValues.timeLastUpdated = Time.time)
           //      .Select(_ => _currentBuilding.Value)
           //      .Subscribe(building =>
           //      {
           //          _tilemapClimbableValues.timeLastUpdated = Time.time;
           //          var cell = building.Map.WorldToBCell(state.transform.position, BuildingLayers.LADDERS);
           //          if (CheckForPotentialClimablesOnCell(building, cell, out var climbable))
           //              hasClimbableSubject.OnNext(climbable);
           //      }).AddTo(_cd);

            

          
               // _hasRoom
               // .Select(t => t ? Observable.EveryUpdate() : Observable.Never<long>())
               // .Switch()
               // .Select(_ => state.CurrentBuilding)
               // .Switch()
               // .ToReadOnlyReactiveProperty();
            
            
            
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
        // bool CheckForPotentialClimablesOnCell(Building building, BuildingCell cell, out PotentialClimbable climbable)
        // {
        //     var lt = building.Map.GetTile<LadderTile>(cell);
        //     climbable = default;
        //     if (lt == null)
        //     {
        //         return false;
        //     }
        //
        //   
        //     
        //     BuildingCell FindLowestCell()
        //     {
        //         var current = cell;
        //         Vector3Int direction = Vector3Int.down;
        //         
        //         while(true)
        //         {
        //             current += direction;
        //             var t = building.Map.GetTile(current);
        //             if (t == null)
        //             {
        //                 return new BuildingCell(current.cell + Vector3Int.up, BuildingLayers.LADDERS);
        //             }
        //         }
        //     }
        //     BuildingCell FindHighestCell()
        //     {
        //         var current = cell;
        //         Vector3Int direction = Vector3Int.up;
        //         while(true)
        //         {
        //             current += direction;
        //             var t = building.Map.GetTile(current);
        //             if (t == null)
        //             {
        //                 return new BuildingCell(current.cell + Vector3Int.down, BuildingLayers.LADDERS);
        //             }
        //         }
        //     }
        //
        //     //var cellSize = building.Map.GetCellSize(BuildingLayers.LADDERS);
        //     
        //     var top = FindHighestCell();
        //     var bottom = FindLowestCell();
        //     var topLs = building.Map.CellToLocalCentered(top);
        //     var bottomLs = building.Map.CellToLocalCentered(bottom);
        //     climbable = new PotentialClimbable(building, topLs, topLs, bottomLs);
        //     return true;
        // }
        // bool CheckForClimablesOnCell(Building building, BuildingCell cell, out IClimbable climbable)
        // {
        //     var lt = building.Map.GetTile<LadderTile>(cell);
        //     climbable = null;
        //     if (lt == null)
        //     {
        //         return false;
        //     }
        //
        //   
        //     
        //     BuildingCell FindLowestCell()
        //     {
        //         var current = cell;
        //         Vector3Int direction = Vector3Int.down;
        //         
        //         while(true)
        //         {
        //             current += direction;
        //             var t = building.Map.GetTile(current);
        //             if (t == null)
        //             {
        //                 return new BuildingCell(current.cell + Vector3Int.up, BuildingLayers.LADDERS);
        //             }
        //         }
        //     }
        //     BuildingCell FindHighestCell()
        //     {
        //         var current = cell;
        //         Vector3Int direction = Vector3Int.up;
        //         while(true)
        //         {
        //             current += direction;
        //             var t = building.Map.GetTile(current);
        //             if (t == null)
        //             {
        //                 return new BuildingCell(current.cell + Vector3Int.down, BuildingLayers.LADDERS);
        //             }
        //         }
        //     }
        //
        //     //var cellSize = building.Map.GetCellSize(BuildingLayers.LADDERS);
        //     
        //     var top = FindHighestCell();
        //     var bottom = FindLowestCell();
        //
        //     var topLs = building.Map.CellToLocalCentered(top);
        //     var bottomLs = building.Map.CellToLocalCentered(bottom);
        //
        //     climbable = new TilemapClimbableInstance(_buildingBody.GetComponent<Rigidbody2D>(), bottomLs, topLs);
        //     return true;
        // }
        public bool TryGetClimbable(Vector2 climberPosition, out IClimbable climbable, float maxDistance = 0.5f)
        {
            var prev = Physics2D.queriesHitTriggers;
            Physics2D.queriesHitTriggers = true;
            int hits = Physics2D.OverlapCircleNonAlloc(climberPosition, 0.2f, _colliderCache, _climbableLayerMask);
            Physics2D.queriesHitTriggers = prev;
            if (hits == 0)
            {
                climbable = null;
                return false;
            }

            for (int i = 0; i < hits; i++)
            {
                var hit = _colliderCache[i];
                if (hit.CompareTag("Ladder") == false) continue;
                var ltm = hit.GetComponent<ILadderTilemap>();
                if (ltm == null) continue;
                _ladderTilemap = ltm;
                break;
            }
            
            // if (_currentBuilding.Value != null)
            // {
            //     if (_tilemapClimbableValues.HasClimbable())
            //     {
            //         climbable = _potentialClimbable.Value.GetClimable();
            //         
            //     }
            // }
            climbable = null;
            if (_ladderTilemap == null)
                return false;
            var cell = _ladderTilemap.Tilemap.WorldToCell(new Vector3(climberPosition.x, climberPosition.y));
            cell.z = 0;
            var zMin = _ladderTilemap.Tilemap.cellBounds.zMin;
            var zMax = _ladderTilemap.Tilemap.cellBounds.zMax;
            cell.z = Mathf.Clamp(cell.z, zMin, zMax);
            Debug.DrawLine(climberPosition, _ladderTilemap.Tilemap.GetCellCenterWorld(cell), Color.red, 0.2f);
            var tile = _ladderTilemap.Tilemap.GetTile(cell);
            if (tile == null) return false;
            
            
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
                        return current - Vector3Int.down;
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
                        return current - Vector3Int.up;
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
            _cd?.Dispose();
        }
    }
}