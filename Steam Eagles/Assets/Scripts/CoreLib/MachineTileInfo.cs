using System;
using UnityEngine;

namespace CoreLib
{
    
    /// <summary>
    /// this structure is used to keep track of which machine is on which cell position on the Grid. machine cells
    /// will need to be aligned with <see cref="Buildings.BuildingLayers"/> depending on which type of building tilemap
    /// the machine needs to interface with.     
    /// </summary>
    public struct MachineTileInfo : IEquatable<MachineTileInfo>
    {
        public MachineVec Cell { get; }
        public int MachineID { get; set; }
        
        /// <summary>
        /// TODO: since certain machine cells will have special properties, such as pipe inputs and pipe outputs
        /// idea is that this tile index can be used to access the tile properties of an array of tiles that are
        /// owned by this machine.  This will make it easier to implement multi-threaded operations on machine tiles
        /// </summary>
        public int MachineTileIndex { get; }

        public bool IsEmpty => MachineID == -1;
        
        public MachineTileInfo(Vector3Int cell, int machineID, int machineTileIndex)
        {
            this.Cell = cell;
            MachineID = machineID;
            MachineTileIndex = machineTileIndex;
        }

        public MachineTileInfo(Vector3Int cell)
        {
            Cell = cell;
            MachineID = -1;
            MachineTileIndex = -1;
        }
        public bool Equals(MachineTileInfo other) => Cell.Equals(other.Cell);

        public override bool Equals(object obj) => obj is MachineTileInfo other && Equals(other);
       
        public override int GetHashCode() => Cell.GetHashCode();
        
        
    }
}