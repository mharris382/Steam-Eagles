using System.Collections.Generic;
using UnityEngine;

namespace GasSim
{
    public interface IGasIO
    {
        IEnumerable<(Vector2Int coord, int amount)> GetSourceCells();
        
    }
}