using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Characters.Actions.Selectors
{
    [System.Obsolete("Ability system prototypes will be phased out and replaced by the tool system")]
    public abstract class SelectorBase : MonoBehaviour
    {
        public Grid TargetGrid { get; set; }
        public Camera TargetCamera { get; set; }
        public Tilemap TargetTilemap { get; set; }

        public abstract bool CanSelectCells();
        public abstract IEnumerable<(Vector3Int cellPos, Vector3 wsPos)> GetSelectableCells();

        public virtual Vector3 GetSelectedWorldSpacePosition()
        {
            var selectableCells = GetSelectableCells();
            if(selectableCells == null)return Vector3.zero;
            return selectableCells.First().wsPos;
        }
    }
}