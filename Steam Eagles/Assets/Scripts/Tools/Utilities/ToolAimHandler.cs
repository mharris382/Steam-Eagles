using Buildings;
using Characters;
using CoreLib;
using SteamEagles.Characters;
using UniRx;
using UnityEngine;

namespace Tools.BuildTool
{
    /// <summary>
    /// internally manages the logic of converting raw aim input into cell selection inside buildings
    /// <para>NOTE: must at some point call UpdateAimPosition for this to work <see cref="UpdateAimPosition"/></para> 
    /// </summary>
    public class ToolAimHandler
    {
        private Camera _camera;
        private readonly MonoBehaviour _owner;
        private ToolState _toolState;
        private Building _building;
        private ReactiveProperty<Vector3Int> _hoveredPosition = new ReactiveProperty<Vector3Int>();
        private Vector2 _mousePosition;

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
                    _camera = characterState.AssignedPlayerCamera;
                }
                return _camera;
            }
        }
        
        private Building Building => _building != null ? _building : (_building = _owner.GetComponentInParent<Building>());
        private bool UseMousePosition => _toolState.Inputs.CurrentInputMode == InputMode.KeyboardMouse;
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
        
        

        private void HandleDirectionAiming()
        {
            if (UseMousePosition)
            {
                Cursor.visible = true;
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
        }

        private void UpdateAimForMousePosition(BuildingLayers targetLayer) => _hoveredPosition.Value = Building.Map.WorldToCell(_mousePosition, targetLayer);

        private void HandleMousePosition()
        {
            _mousePosition = Camera.ScreenToWorldPoint(Input.mousePosition);
        }

        private void UpdateAimForController(BuildingLayers targetLayer) => _hoveredPosition.Value = Building.Map.WorldToCell(_toolState.AimPositionWorld, targetLayer);
    }
}