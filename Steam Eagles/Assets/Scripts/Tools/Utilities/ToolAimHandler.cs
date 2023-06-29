using System;
using Buildings;
using Characters;
using CoreLib;
using SteamEagles.Characters;
using UniRx;
using UnityEngine;

namespace Tools.BuildTool
{
    public class AimDistanceLimiter : IToolAimDecorator
    {
        private readonly Transform _aimOrigin;
        private readonly Func<float> _distanceLimit;

        public AimDistanceLimiter(Transform aimOrigin, float distanceLimit) : this(aimOrigin, () => distanceLimit)
        {
            Debug.Assert(distanceLimit > 0);
        }

        public AimDistanceLimiter(Transform aimOrigin, Func<float> distanceLimit)
        {
            _aimOrigin = aimOrigin;
            _distanceLimit = distanceLimit;
        }

        public Vector3 GetAimPosition(Vector3 aimPosition)
        {
            var limit = _distanceLimit();
            var diff = _aimOrigin.position - aimPosition;
            if (diff.sqrMagnitude > limit * limit)
            {
                diff = diff.normalized * limit;
                return _aimOrigin.position - diff;
            }
            return aimPosition;
        }
    }
    class NullAimDecorator : IToolAimDecorator
    {
        public Vector3 GetAimPosition(Vector3 aimPosition) => aimPosition;
    }
    
    /// <summary>
    /// internally manages the logic of converting raw aim input into cell selection inside buildings
    /// <para>NOTE: must at some point call UpdateAimPosition for this to work <see cref="UpdateAimPosition"/></para> 
    /// </summary>
    public class ToolAimHandler
    {
        private Camera _camera;
        private readonly MonoBehaviour _owner;
        private ToolState _toolState;
        private ReactiveProperty<Vector3Int> _hoveredPosition = new ReactiveProperty<Vector3Int>();
        private Subject<BuildingToolAimInfo> _toolAimInfoSubject = new Subject<BuildingToolAimInfo>();
        private Vector2 _mousePosition;
        
        private IToolAimDecorator _aimDecorator;
        private IToolAimDecorator _nullAimDecorator = new NullAimDecorator();
        
        public IToolAimDecorator AimDecorator
        {
            get => _aimDecorator ?? _nullAimDecorator;
            set => _aimDecorator = value;
        }

        public IReadOnlyReactiveProperty<Vector3Int> HoveredPosition => _hoveredPosition;
        public Vector2 AimDirection { get; private set; }
        
        public float AimAngle => Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;

        /// <summary>
        /// used for mouse input
        /// </summary>
        public Camera Camera
        {
            get
            {
                if (_toolState != null)
                {
                    var characterState = _toolState.GetComponent<CharacterState>();
                    return _camera = characterState.AssignedPlayerCamera;
                }
                return _camera;
            }
        }

        private Building Building => (_owner as ToolControllerBase)?.Building;
        private bool UseMousePosition => _toolState.Inputs.CurrentInputMode == InputMode.KeyboardMouse;
        
        public IObservable<BuildingToolAimInfo> ToolAimInfo => _toolAimInfoSubject;
        
        public ToolAimHandler(MonoBehaviour owner, ToolState toolState)
        {
            _owner = owner;
            _toolState = toolState;
            _camera = toolState.GetComponent<CharacterState>().AssignedPlayerCamera;
        }

        public bool HasResourcesForGridAim() => HasPlayerResources() && Building != null;

        public bool HasPlayerResources() => _toolState != null && Camera != null;

        /// <summary>
        /// must be called to update the aim position
        /// </summary>
        /// <param name="targetLayer"></param>
        public void UpdateAimPosition(BuildingLayers targetLayer)
        {
            HandleMousePosition();
            HandleGridAiming(targetLayer);
            HandleDirectionAiming();
        }


        public void UpdateAimDirection()
        {
            HandleMousePosition();
            HandleDirectionAiming();
        }

        public void HandleDirectionAiming()
        {
            if (UseMousePosition)
            {
                Cursor.visible = true;
                _mousePosition = Camera.ScreenToWorldPoint(Input.mousePosition);
                var aimOrigin = (Vector2)_toolState.transform.position + _toolState.aimOriginOffset;
                var aim = (_mousePosition - aimOrigin);
                if (aim.sqrMagnitude > 0) AimDirection = aim.normalized;
            }
            else
            {
                AimDirection = _toolState.Inputs.AimInputRaw;
            }
        }

        private void HandleGridAiming(BuildingLayers targetLayer)
        {
            if (!HasResourcesForGridAim())
            {
                _hoveredPosition.Value = Vector3Int.zero;
                return;
            }

            if (_toolState.Inputs.CurrentInputMode == InputMode.KeyboardMouse)
            {
                UpdateAimForMousePosition(targetLayer);
            }
            else
            {
                UpdateAimForController(targetLayer);
            }

            UpdateAimVisual(_hoveredPosition.Value, targetLayer, Building);
        }

        private void HandleMousePosition()
        {
            _mousePosition = Camera.ScreenToWorldPoint(Input.mousePosition);
        }


        private void UpdateAimForMousePosition(BuildingLayers targetLayer) => 
            UpdateAim(_mousePosition, targetLayer);

        private void UpdateAimForController(BuildingLayers targetLayer) =>
            UpdateAim(_toolState.AimPositionWorld, targetLayer);
        
        
        private void UpdateAim(Vector3 aimPosition, BuildingLayers targetLayer)
        {
            aimPosition = AimDecorator.GetAimPosition(aimPosition);
            if(Building == null) return;
            if (Building.Map == null) return;
            var cell = Building.Map.WorldToCell(aimPosition, targetLayer);
            _hoveredPosition.Value = cell;
        }

        private void UpdateAimVisual(Vector3Int hoveredPositionValue, BuildingLayers targetLayer, Building building)
        {
            var cell = new BuildingCell(hoveredPositionValue, targetLayer);
            _toolAimInfoSubject.OnNext(new BuildingToolAimInfo(cell, building.Map.CellToWorld(cell), building));
        }
    }

    public struct BuildingToolAimInfo
    {
        public BuildingCell Cell { get; private set; }
        public Vector3 AimPositionWs { get; }
        public Building Building { get; }

        public BuildingLayers Layer
        {
            get => Cell.layers;
            set => Cell = new BuildingCell(Cell.cell, value);
        }

        public BuildingToolAimInfo(BuildingCell cell, Vector3 aimPositionWs, Building building)
        {
            Cell = cell;
            AimPositionWs = aimPositionWs;
            Building = building;
        }
    }
}