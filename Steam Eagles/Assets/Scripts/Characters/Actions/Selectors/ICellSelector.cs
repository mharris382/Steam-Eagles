using System.Collections.Generic;
using UnityEngine;

namespace Characters.Actions.Selectors
{
    public interface ICellSelector
    {
        IEnumerable<Vector3Int> GetSelectedCells();
    }
}