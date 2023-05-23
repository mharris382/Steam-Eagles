using System.Collections.Generic;
using UnityEngine;

namespace CoreLib
{
    /// <summary>
    /// interface which defines the contract that machines must fulfil in order to interact with the machine tile map
    /// </summary>
    public interface IMachineTileProvider
    {
        public int MachineID { set; get; }
        
        public Vector2Int MachineSize { get; }
        
        public Vector3Int CellPosition { get; }
        
        public IEnumerable<MachineGridArea> GetMachineGridAreas();
    }
}