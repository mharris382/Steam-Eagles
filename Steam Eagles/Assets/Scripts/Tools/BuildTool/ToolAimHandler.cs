using Buildings;
using Characters;
using CoreLib;
using SteamEagles.Characters;
using UniRx;
using UnityEngine;

namespace Tools.BuildTool
{
    public class ToolAimHandler
    {
        private Camera _camera;
        private readonly MonoBehaviour _owner;
        private ToolState _toolState;
        private Building _building;
        private ReactiveProperty<Vector3Int> _hoveredPosition = new ReactiveProperty<Vector3Int>();


        public IReadOnlyReactiveProperty<Vector3Int> HoveredPosition => _hoveredPosition;
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
        public ToolAimHandler(MonoBehaviour owner, ToolState toolState)
        {
            _owner = owner;
            _toolState = toolState;
            _camera = toolState.GetComponent<CharacterState>().AssignedPlayerCamera;
        }

        public bool HasResourcesForGridAim() => _toolState != null && Camera != null && Building != null;


        public void UpdateAimPosition(BuildingLayers targetLayer)
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

        private void UpdateAimForMousePosition(BuildingLayers targetLayer)
        {
            var wp = Camera.ScreenToWorldPoint(Input.mousePosition);
            wp.z = 0;
            _hoveredPosition.Value = Building.Map.WorldToCell(wp, targetLayer);
        }

        private void UpdateAimForController(BuildingLayers targetLayer) => _hoveredPosition.Value = Building.Map.WorldToCell(_toolState.AimPositionWorld, targetLayer);
    }
}