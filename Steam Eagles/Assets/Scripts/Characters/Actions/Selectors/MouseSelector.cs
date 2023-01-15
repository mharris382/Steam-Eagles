using System.Collections.Generic;
using UnityEngine;

namespace Characters.Actions.Selectors
{
    public class MouseSelector : SelectorBase
    {
        public override bool CanSelectCells()
        {
            
            return TargetCamera != null && TargetGrid != null;
        }

        public override IEnumerable<(Vector3Int cellPos, Vector3 wsPos)> GetSelectableCells()
        {
            var msPos = TargetCamera.ScreenToWorldPoint(Input.mousePosition);
            var cellPos = TargetTilemap.WorldToCell(msPos);
            var wsPos = TargetTilemap.GetCellCenterWorld(cellPos);
            cellPos.z = 0;
            wsPos.z = 0;
            return new[] { (cellPos, wsPos) };
        }
    }
}