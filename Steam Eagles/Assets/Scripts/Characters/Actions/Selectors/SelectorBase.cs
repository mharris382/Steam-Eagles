using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Characters.Actions.Selectors
{
    public abstract class SelectorBase : MonoBehaviour
    {
        public Grid TargetGrid { get; set; }
        public Camera TargetCamera { get; set; }
        public Tilemap TargetTilemap { get; set; }

        public abstract bool CanSelectCells();
        public abstract IEnumerable<(Vector3Int cellPos, Vector3 wsPos)> GetSelectableCells();
    }
}