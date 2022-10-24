using System;
using UnityEngine;

namespace Puzzles
{
    public class PipeTile : CellHelper
    {
        void Awake()
        {
            Debug.Log($"Pipe Placed at Position: {CellCoordinate}");
        }

        private void OnDestroy()
        {
            Debug.Log($"Pipe removed from Position: {CellCoordinate}");
        }
    }
}