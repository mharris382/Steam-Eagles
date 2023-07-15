using Buildings;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Crafting
{
    public class CraftingAimHanding
    {
        private readonly AimHandlingMode _gamepadMode;
        private readonly AimHandlingMode _keyboardMode;
        private ReactiveProperty<Vector3> _aimPositionWS = new();
        
        
        public IReadOnlyReactiveProperty<Vector3> AimWorldSpace => _aimPositionWS;
        public BuildingCell cell { get; private set;  }
        public bool centerOnGrid = true;
        public CraftingAimHanding() : this(new KeyboardMouseAimHandlingMode(), new ControllerAimHandlingMode()){}
        public CraftingAimHanding(AimHandlingMode keyboardMode, AimHandlingMode gamepadMode)
        {
            _keyboardMode = keyboardMode;
            _gamepadMode = gamepadMode;
        }
        public void ProcessAim(PlayerInput playerInput, GameObject character, Camera camera)
        {
            var isKeyboard = playerInput.currentControlScheme.Contains("Keyboard");
            _aimPositionWS.Value = isKeyboard ? 
                _keyboardMode.GetAimPosition(playerInput, character, camera) :
                _gamepadMode.GetAimPosition(playerInput, character, camera);
        }

        public BuildingCell ProcessGridAim(
            PlayerInput playerInput, 
            GameObject character,
            Camera camera, 
            Building building, BuildingLayers layers)
        {
            ProcessAim(playerInput, character, camera);
            var aimWs = _aimPositionWS.Value;
            var cell = building.Map.WorldToCell(aimWs, layers);
            cell.z = 0;
            if (centerOnGrid) _aimPositionWS.Value = building.Map.CellToWorldCentered(cell, layers);
            this.cell = new BuildingCell(cell,layers);
            return this.cell;
        }
        
        
        public abstract class AimHandlingMode
        {
            ReactiveProperty<Vector3> _aimPositionWs = new();
            public IReadOnlyReactiveProperty<Vector3> AimWorldSpace => _aimPositionWs;
            public Vector3 GetAimPosition(PlayerInput playerInput, GameObject character, Camera camera)
            {
                var aim = _aimPositionWs.Value;
                if (playerInput == null || character == null || camera == null)
                    return aim;

                aim = ReadAimPosition(playerInput, character, camera);
                return _aimPositionWs.Value = aim;
            }

            public abstract Vector3 ReadAimPosition(PlayerInput playerInput, GameObject character, Camera camera);
        }

        public class KeyboardMouseAimHandlingMode : AimHandlingMode
        {
            public override Vector3 ReadAimPosition(PlayerInput playerInput, GameObject character, Camera camera)
            {
                var mousePos = Mouse.current.position.ReadValue();
                var aim = camera.ScreenToWorldPoint(mousePos);
                aim.z = character.transform.position.z;
                return aim;
            }
        }
    
        public class ControllerAimHandlingMode : AimHandlingMode
        {
            public override Vector3 ReadAimPosition(PlayerInput playerInput, GameObject character, Camera camera)
            {
                throw new System.NotImplementedException();
            }
        }
    }


   
}