using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using Buildings.Rooms;
using CoreLib;
using CoreLib.Interfaces;
using Items;
using Sirenix.OdinInspector;
using Tools.BuildTool;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using ToolControllerBase = Tools.BuildTool.ToolControllerBase;

namespace Tools.DestructTool
{
    
  
    public partial class DestructToolController : ToolControllerBase
    {
        [SerializeField] private DestructToolTileVisualizer tileVisualizer;
        [Min(0)] [SerializeField, HideInInspector] private float minDistance = 0.5f;
        [Min(0)] [SerializeField, HideInInspector] private float maxDistance = 3f;

      //  [FoldoutGroup("Aiming Settings"),SerializeField] private Vector2 originOffset = new Vector2(0, 1);
      //  [FormerlySerializedAs("aimSpeed")] [FoldoutGroup("Aiming Settings"),SerializeField] private float maxAngleDelta = 3f;
      //  [FoldoutGroup("Aiming Settings"),SerializeField] private float aimSmoothing = 0.1f;
      //  [FoldoutGroup("Aiming Settings"), Range(-1, 1)] [SerializeField] private float aimSnapThreshold = 0.1f;
        
        [SerializeField] private DestructionConfig config;
        [SerializeField] private DestructionToolFeedbacks feedbacks;

        private Subject<DestructParams> _onDestruct = new Subject<DestructParams>();
        public IObservable<DestructParams> OnDestruct => _onDestruct;


        public Transform aimPoint;
        private Vector2 _actualAimPositionLs;
        private Vector2 _aimVelocity;
        private float _speed;
        private float _velAngle;
        private RaycastHit2D[] _cache = new RaycastHit2D[30];
        private Dictionary<Component, IDestruct> _seenDestructables = new Dictionary<Component, IDestruct>();
        private Dictionary<IDestruct, Destructor> _destructors = new Dictionary<IDestruct, Destructor>();
        private Vector2 _direction;
        private Subject<(IDestruct, BuildingToolAimInfo, DestructParams)> _aimInfoSubject = new Subject<(IDestruct, BuildingToolAimInfo, DestructParams)>();

        public IObservable<(IDestruct target, BuildingToolAimInfo aimInfo, DestructParams)> OnDestructToolAimInfo => _aimInfoSubject;

        #region [Properties]

        [FoldoutGroup("Aiming Settings"),PropertyOrder(-1),ShowInInspector, MinMaxSlider(0, 10)]
        public Vector2 ToolRange
        {
            get => new Vector2(minDistance, maxDistance);
            set
            {
                minDistance = Mathf.Min(value.x, value.y);
                maxDistance = Mathf.Max(value.x, value.y);
            }
        }

        public Vector2 AimDirection => AimHandler.AimDirection;

        public float CastDistance => maxDistance - minDistance;

        #endregion

        private IDestruct _currentDestructable;
        private BuildingToolAimInfo _currentAimInfo;
        private DestructParams _currentDestructParams;
        private BuildingLayers _targetLayer;
        private string _mode;
        
        private Dictionary<string, DestructToolModeConfig> _modesToTargetedLayers = new Dictionary<string, DestructToolModeConfig>();
        private DestructToolModeConfig _fallbackMode;
        
        private class DestructToolModeConfig
        {
            public readonly BuildingLayers[] SearchLayers;
            public readonly BuildingLayers SearchLayerMask;
            public readonly bool PerformRaycastSearch;
            public readonly bool PerformTilemapSearch;
            public DestructToolModeConfig(bool performRaycastSearch, params BuildingLayers[] searchLayers)
            {
                SearchLayerMask = BuildingLayers.NONE;
                foreach (var sl in searchLayers) SearchLayerMask |= sl;
                PerformTilemapSearch = SearchLayerMask != BuildingLayers.NONE;
                PerformRaycastSearch = performRaycastSearch;
                SearchLayers = searchLayers;
            }
        }
        private const string SOLID_MODE = "Solids";
        private const string PIPE_MODE = "Pipes";
        private const string WIRE_MODE = "Wires";
        

        private List<string> _modes;
        
        
        protected override bool ToolUsesModes(out List<string> modes)
        {
            if (_modes != null)
            {
                modes = _modes;
                return true;
            }
            
            modes = _modes = new List<string>();
            
            modes.Add(SOLID_MODE);
            _modesToTargetedLayers.Add(SOLID_MODE, _fallbackMode = new DestructToolModeConfig(true, BuildingLayers.SOLID, BuildingLayers.LADDERS, BuildingLayers.PLATFORM));
            
            modes.Add(PIPE_MODE);
            _modesToTargetedLayers.Add(PIPE_MODE, new DestructToolModeConfig(false, BuildingLayers.PIPE ));
            
            modes.Add(WIRE_MODE);
            _modesToTargetedLayers.Add(WIRE_MODE, new DestructToolModeConfig(false, BuildingLayers.WIRES ));
            
            return true;
        }

        private DestructToolModeConfig GetCurrentMode()
        {
            if (_modesToTargetedLayers.TryGetValue(ToolMode, out var modeConfig))
            {
                return modeConfig;
            }
            Debug.LogWarning($"No Destruct Mode found for {ToolMode} falling back to Solid",this);
            return _fallbackMode;
        }

        public override string ToolMode
        {
            get => !string.IsNullOrEmpty(_mode) ? _mode : SOLID_MODE;
            set
            {
                _mode = value;
                
            }
        }

        protected override void OnAwake()
        {
            AimHandler.ToolAimInfo.Subscribe(info =>
            {
                if (TryGetDestructable(ref info, out var destruct, out var destructParams))
                {
                    _aimInfoSubject.OnNext((_currentDestructable=destruct, _currentAimInfo=info, _currentDestructParams=destructParams));
                }
                else
                {
                    _aimInfoSubject.OnNext((_currentDestructable=null, _currentAimInfo=info, default));
                }
            }).AddTo(this);
            AimHandler.ToolAimInfo.Subscribe(info =>
            {

            });
            base.OnAwake();
        }

        private void OnEnable()
        {
            tileVisualizer.OnEnable(this);
            config.OnEnable(this);
            AimHandler.AimDecorator = config.AimDecorator;
            
        }

        private void OnDisable()
        {
            tileVisualizer.OnDisable();
            AimHandler.AimDecorator = null;
        }

       

        public override void OnToolEquipped()
        {
            Activator.IsEquipped = true;
            
            base.OnToolEquipped();
        }

        public override void OnToolUnEquipped()
        {
            Activator.IsEquipped = false;
            base.OnToolUnEquipped();
        }

        protected override void OnStart()
        {
            aimPoint.parent = transform;
            
        }

        public override BuildingLayers GetTargetLayer() => BuildingLayers.SOLID;

        protected override void OnRoomChanged(Room room)
        {
            HasRoom = room != null && room.buildLevel == BuildLevel.FULL;
        }

        public override ToolStates GetToolState()
        {
            return ToolStates.Destruct;
        }

        public override bool UsesRecipes(out List<Recipe> recipes)
        {
            recipes = null;
            return false;
        }

        private void Update()
        {
            if (!HasResources())
            {
                Debug.Log("No resources", this);
                return;
            }
       
            UpdateAim(Time.deltaTime);
            if (ToolState.Inputs.SamplePressed)
            {
               if(TrySample())
                   return;
            }
            
            Activator.IsInUse = ToolState.Inputs.UseHeld;
            
            if (ToolState.Inputs.UseHeld && _currentDestructable != null)
            {
                NotifyHitDestructable(_currentDestructable, _currentDestructParams, Time.deltaTime);
            }
        }

        /// <summary>
        /// samples the environment for a destructable object and switches to that tool mode if found
        /// </summary>
        /// <returns></returns>
        private bool TrySample()
        {
            foreach (var kvp in _modesToTargetedLayers)
            {
                if(TryGetDestructable(kvp.Value, ref _currentAimInfo, out var destruct, out var destructParams))
                {
                    ToolMode = kvp.Key;
                    return true;
                }
            }
            return false;
        }

        private void UpdateAim(float dt)
        {
            AimHandler.UpdateAimPosition(BuildingLayers.SOLID);
        }
        private bool TryGetDestructable(DestructToolModeConfig mode, ref BuildingToolAimInfo aimInfo, out IDestruct destruct, out DestructParams destructParams)
        {
            if (mode == null)
            {
                Debug.LogError("Null Mode", this);
                destruct = null;
                destructParams = default;
                return false;
            }
            if (mode.PerformTilemapSearch && Building != null)
            {
                if (DoTilemapSearch(ref aimInfo, out destruct, out destructParams))
                {
                    Debug.Log($"Tilemap Search Success on {(destruct as Component)?.name}", this);
                    return true;
                }
            }

            if (mode.PerformRaycastSearch)
            {
                if (DoRaycastSearch(out destruct, out destructParams))
                {
                    Debug.Log($"Raycast Search Success on {(destruct as Component)?.name}", this);
                    return true;
                }
            }
            destruct = null;
            destructParams = default;
            return false;
        }
        private bool TryGetDestructable(ref BuildingToolAimInfo aimInfo, out IDestruct destruct, out DestructParams destructParams) => TryGetDestructable(GetCurrentMode(), ref aimInfo, out destruct, out destructParams);

        private bool DoTilemapSearch(ref BuildingToolAimInfo aimInfo, out IDestruct destruct, out DestructParams destructParams)
        {
            foreach (var cell in GetDestructableCells(aimInfo.Cell.cell))
            {
                var tile = this.Building.Map.GetTile(cell);
                if (tile == null) continue;
                var tilemap = Building.Map.GetTilemap(cell.layers);
                var destructable = GetDestructable(tilemap);
                if (destructable == null)
                {
                    Debug.LogWarning(
                        "Building Tilemap was Searched by destruct tool but the tilemap has no IDestruct component attached to it",
                        tilemap);
                    continue;
                }

                aimInfo = new BuildingToolAimInfo(cell, Building.Map.CellToWorld(cell), Building);
                destruct = destructable;
                destructParams = new DestructParams(aimInfo.Cell.cell);
                return true;
            }

            destruct = null;
            destructParams = default;
            return false;
        }
        private bool DoRaycastSearch(out IDestruct destruct, out DestructParams destructParams)
        {
            using (new Physics2DQueryScope(hitTriggers: true, startInColliders: true))
            {
                var castStart = ToolState.aimOriginOffset + (Vector2)ToolState.transform.position;
                //int hits = Physics2D.OverlapCircleNonAlloc(position, config.radius, _cache, config.destructibleLayers);
                int hits = Physics2D.CircleCastNonAlloc(castStart, config.radius, AimDirection, _cache,
                    CastDistance, config.destructibleLayers);

                Debug.DrawRay(castStart, AimDirection.normalized * CastDistance, hits > 0 ? Color.red : Color.red.Lighten(0.5f),
                    0.1f);

                for (int i = 0; i < hits; i++)
                {
                    var destructable = GetDestructable(_cache[i].collider);
                    if (destructable == null)
                        continue;
                    destruct = destructable;
                    destructParams = _cache[i];
                    return true;
                }
            }

            destruct = null;
            destructParams = default;
            return false;
        }

        [System.Obsolete("prefer TryGetDestructable or otherwise OnDestructAim instead")]
        private void CheckForDestructables(float dt)
        {
            var aimPosition = AimHandler.HoveredPosition.Value;
            foreach (var cell in GetDestructableCells(aimPosition))
            {
                
                var tile = this.Building.Map.GetTile(cell);
                if (tile != null)
                {
                    var tilemap = Building.Map.GetTilemap(cell.layers);
                    var destructable = GetDestructable(tilemap);
                    if (destructable == null)
                    {
                        Debug.LogWarning("Building Tilemap was Searched by destruct tool but the tilemap has no IDestruct component attached to it", tilemap);
                        continue;
                    }
                    NotifyHitDestructable(destructable, new DestructParams(cell.cell), dt);
                    return;
                }
            }
            using (new Physics2DQueryScope(hitTriggers: true, startInColliders: true))
            {
                var castStart = ToolState.aimOriginOffset + (Vector2)ToolState.transform.position;
                //int hits = Physics2D.OverlapCircleNonAlloc(position, config.radius, _cache, config.destructibleLayers);
                int hits = Physics2D.CircleCastNonAlloc(castStart, config.radius, AimDirection, _cache,
                    CastDistance, config.destructibleLayers);
                
                Debug.DrawRay(castStart, AimDirection.normalized * CastDistance, hits > 0 ? Color.red : Color.red.Lighten(0.5f), 0.1f);
               
                for (int i = 0; i < hits; i++)
                {
                    var destructable = GetDestructable(_cache[i].collider);
                    if (destructable == null)
                        continue;
                    NotifyHitDestructable(destructable, _cache[i], dt);
                }
            }
        }

        IEnumerable<BuildingCell> GetDestructableCells(Vector3Int aimPosition)
        {
            var mode = GetCurrentMode();
            var solidCell =new BuildingCell(aimPosition, BuildingLayers.SOLID);
            foreach (var layer in mode.SearchLayers)
            {
                foreach (var cell in Building.Map.ConvertBetweenLayers(solidCell,layer))
                    yield return cell;
            }
        }

        private IDestruct GetDestructable(Component coll)
        {
            if (coll == null)
                return null;

            if (_seenDestructables.TryGetValue(coll, out var destructable))
                return destructable;

            destructable = coll.GetComponent<IDestruct>();
            if (destructable != null)
                _seenDestructables.Add(coll, destructable);

            return destructable;
        }

        private void NotifyHitDestructable(IDestruct destructable, DestructParams destructParams, float dt)
        {
            if (destructable == null)
                return;
            if (!_destructors.TryGetValue(destructable, out var destructor))
            {
                destructor = new Destructor(destructable, this);
                
                destructor.OnDestruct.Subscribe(_onDestruct).AddTo(this);
                _destructors.Add(destructable, destructor);
            }

            destructor.OnHit(dt, destructParams);
        }


        #region [Editor Gizmos]



        #endregion
    }
    
    
    
}