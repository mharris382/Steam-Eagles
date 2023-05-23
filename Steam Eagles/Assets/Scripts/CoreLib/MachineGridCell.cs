using UnityEngine;

namespace CoreLib
{
    public struct MachineGridCell : IMachineGridCell
    {
        public int MachineID { get; }
        public MachineLayers MachineLayer { get; }
        public Vector3Int Position { get; }

        public MachineGridCell(int machineID, MachineLayers layers, Vector3Int position)
        {
            MachineID = machineID;
            MachineLayer = layers;
            Position = position;
        }
        public MachineGridCell(int machineID, MachineLayers layers, Vector2Int position)
        {
            MachineID = machineID;
            MachineLayer = layers;
            Position = (Vector3Int)position;
        }
    public MachineGridCell(MachineTileInfo cell, MachineLayers layers)
        {
            MachineID = cell.MachineID;
            MachineLayer = layers;
            Position = cell.Cell;
        }
        public void OnCellAddedToTilemap(MachineTileMap machineTileMap)
        {
            throw new System.NotImplementedException();
        }

        public void OnCellRemovedFromTilemap(MachineTileMap machineTileMap)
        {
            throw new System.NotImplementedException();
        }
    }
}