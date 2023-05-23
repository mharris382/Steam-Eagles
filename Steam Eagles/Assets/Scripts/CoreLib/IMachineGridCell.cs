using UnityEngine;

namespace CoreLib
{
    public interface IMachineGridCell
    {
        /// <summary>
        /// the machine ID of the machine this cell belongs to
        /// </summary>
        int MachineID { get; }
        
        /// <summary>
        /// determines which layer this cell is placed on. only one cell can occupy a given position on a given layer
        /// </summary>
        MachineLayers MachineLayer { get; }
        
        /// <summary>
        /// cell position local to the machine size (converted to layer grid if layer is not standard size)
        /// </summary>
        Vector3Int Position { get; }
        
        /// <summary>
        /// use to implement setup logic when this type of cell is added to the world
        /// </summary>
        /// <param name="machineTileMap"></param>
        void OnCellAddedToTilemap(MachineTileMap machineTileMap);
        
        /// <summary>
        /// use to implement cleanup logic when this type of cell is removed from the world
        /// </summary>
        /// <param name="machineTileMap"></param>
        void OnCellRemovedFromTilemap(MachineTileMap machineTileMap);
    }
}