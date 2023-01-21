using System.Collections.Generic;
using Players;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters.Actions.Selectors
{
    public class InputSelector : SelectorBase
    {
        public Player player;
        [SerializeField] private float controllerAimSensitivity = 0.5f;
        [SerializeField] private float maxAimDistance = 10;
        public Transform aimTransform;
        private PlayerInput PlayerInput => player.CharacterInput.PlayerInput;
        private PlayerCharacterInput PlayerCharacterInput => player.CharacterInput;
        
        
        Vector2 controllerAimOffset = Vector2.zero;
        
        public override bool CanSelectCells()
        {
            if (player == null) return false;
            if (player.CharacterInput == null)
            {
                Debug.LogWarning($"Player {player.name} has no input",this);
                return false;
            }
            if (player.CharacterInput.PlayerInput.camera == null)
            {
                player.CharacterInput.PlayerInput.camera = TargetCamera;
            }
            else if (TargetCamera != null && player.CharacterInput.PlayerInput.camera != TargetCamera)
            {
                Debug.LogWarning($"Player input camera ({player.CharacterInput.PlayerInput.camera.name}) is not the same as the selector camera ({TargetCamera.name})!",this);
                player.CharacterInput.PlayerInput.camera = TargetCamera;
            }
            return TargetCamera != null && TargetGrid != null;
        }

        public override IEnumerable<(Vector3Int cellPos, Vector3 wsPos)> GetSelectableCells()
        {
            if(PlayerInput.devices.Count == 0) yield break;
            Debug.Log($"Selector has input control scheme: {PlayerInput.currentControlScheme}",this);
            var scheme = PlayerInput.currentControlScheme;
            bool isControllerScheme = scheme.ToLower().Contains("controller");
            Vector2 selectorPos = Vector2.zero;
            if (isControllerScheme)
            {
                var characterPos = player.characterTransform.Value.position;
                //get aim position from controller
                var aimVector = PlayerInput.actions["Aim"].ReadValue<Vector2>();
                controllerAimOffset+= (aimVector * (controllerAimSensitivity * Time.deltaTime));
                controllerAimOffset = Vector2.ClampMagnitude(controllerAimOffset, maxAimDistance);
                selectorPos = characterPos + new Vector3(controllerAimOffset.x,controllerAimOffset.y);
                UpdateAimTransformForJoystick(selectorPos);
            }
            else
            {
                UpdateAimTransformForMouse();
                var msPos = TargetCamera.ScreenToWorldPoint(Input.mousePosition);
                var vpPos = TargetCamera.ScreenToViewportPoint(Input.mousePosition);
                var viewRect = new Rect(0, 0, 1, 1);
                if (!viewRect.Contains(vpPos))
                {
                    yield break;
                }
                selectorPos = msPos;
                
            }
            var cellPos = TargetTilemap.WorldToCell(selectorPos);
            var wsPos = TargetTilemap.GetCellCenterWorld(cellPos);
            cellPos.z = 0;
            wsPos.z = 0;
            yield return (cellPos, wsPos); 
        }

        void UpdateAimTransformForJoystick(Vector2 vec)
        {
            if (aimTransform != null)
            {
                if(!aimTransform.gameObject.activeSelf)
                    aimTransform.gameObject.SetActive(true);
                aimTransform.parent = null;
                aimTransform.position = vec;
            }
        }

        void UpdateAimTransformForMouse()
        {
            if(aimTransform != null)
                aimTransform.gameObject.SetActive(false);
        }
    }
}