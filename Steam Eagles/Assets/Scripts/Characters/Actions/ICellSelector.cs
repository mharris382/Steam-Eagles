using System.Collections.Generic;
using UnityEngine;

namespace Characters.Actions
{
    public interface ICellSelector
    {
        IEnumerable<Vector3Int> GetSelectedCells();
    }
}