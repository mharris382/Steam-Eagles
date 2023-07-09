using System;
using System.Linq;
using UnityEngine;

namespace Buildables.Debugging
{
    [Debugger, Preview]
    public class MachineCellsDebugger : MonoBehaviour
    {
        public CellDebugger debugger;

        public BuildableMachineBase machine;


        private void Update()
        {
            debugger.Debug(machine.GetCells().Select(t => (Vector2Int)t));
        }
    }
}