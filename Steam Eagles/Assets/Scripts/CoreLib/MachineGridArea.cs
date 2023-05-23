using System.Collections;
using UnityEngine;

namespace CoreLib
{
    /// <summary>
    /// used to create a 2D area composed of IMachineGridCells on a specific layer 
    /// </summary>
    public struct MachineGridArea
    {
        public readonly MachineLayers MachineLayer;
        public readonly int MachineID;
        public Vector2Int MachineSize { get; }
        public readonly IMachineGridCell[] Cells;

        public IMachineGridCell this[int x, int y]
        {
            get => Cells[x + y * MachineSize.x];
            set => Cells[x + y * MachineSize.x] = value;
        }

        public IMachineGridCell this[int i] => this[i / MachineSize.x, i % MachineSize.x];

        public int Length => Cells.Length;
        public MachineGridArea(int machineID, Vector2Int machineSize, MachineLayers machineLayer)
        {
            MachineLayer = machineLayer;
            MachineSize = machineSize;
            Cells = new IMachineGridCell[machineSize.x * machineSize.y];
            MachineID = machineID;
            for (int i = 0; i < machineSize.x; i++)
            {
                for (int j = 0; j < machineSize.y; j++)
                {
                    this[i, j] = new MachineGridCell(machineID, machineLayer, new Vector2Int(i, j));
                }
            }
        }
    }
}