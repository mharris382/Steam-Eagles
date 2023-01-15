using System.Collections.Generic;
using Players;
using UnityEngine;

namespace Characters.Actions.Selectors
{
    public class InputSelector : SelectorBase
    {
        public Player player;
        
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
            var msPos = TargetCamera.ScreenToWorldPoint(Input.mousePosition);
            var vpPos = TargetCamera.ScreenToViewportPoint(Input.mousePosition);
            var viewRect = new Rect(0, 0, 1, 1);
            if (!viewRect.Contains(vpPos))
            {
                yield break;
            }
            var cellPos = TargetTilemap.WorldToCell(msPos);
            var wsPos = TargetTilemap.GetCellCenterWorld(cellPos);
            cellPos.z = 0;
            wsPos.z = 0;
            yield return (cellPos, wsPos); 
        }
    }
}