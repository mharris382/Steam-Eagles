using System.Collections.Generic;
using UnityEngine;

namespace Characters.Actions.Selectors
{
    [System.Obsolete("Ability system prototypes will be phased out and replaced by the tool system")]
    public interface ICellSelector
    {
        IEnumerable<Vector3Int> GetSelectedCells();
    }
}