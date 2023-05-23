using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using UniRx;
using UnityEngine;

namespace CoreLib
{
    public class MachineTileMap
    {
        private Component _building;

        private Dictionary<int, Dictionary<MachineLayers, HashSet<IMachineGridCell>>> _allMachineGridCells = new();
        private Dictionary<int, List<MachineGridArea>> _allMachineGridAreas = new();
        private Dictionary<MachineLayers, Dictionary<Vector3Int, IMachineGridCell>> _allGridCells = new();
        private Dictionary<int, IDisposable> _destroyMachineDisposables = new();

        private int _nextMachineID = 0;
        public MachineTileMap(Component building)
        {
            _building = building;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="machineTiles"></param>
        /// <returns>handle that can be used to destruct the machine and safely remove all machine tiles for that machine</returns>
        public IDisposable AddMachine(IMachineTileProvider machineTiles)
        {
            var id =machineTiles.MachineID = _nextMachineID++;
            var cd = new CompositeDisposable();
            
            if (!_allMachineGridAreas.ContainsKey(id)) 
                _allMachineGridAreas.Add(id, null);
            
            if(!_allMachineGridCells.ContainsKey(id))
                _allMachineGridCells.Add(id, new Dictionary<MachineLayers, HashSet<IMachineGridCell>>());
            
            _allMachineGridAreas[id] = machineTiles.GetMachineGridAreas().ToList();
            cd.Add(Disposable.Create(() => {
                _allMachineGridAreas[id].Clear();
                _allMachineGridAreas.Remove(id);
            }));


            var cellLayersLookup = _allMachineGridCells[id];
            foreach (var gridArea in _allMachineGridAreas[id])
            {
                if(!cellLayersLookup.ContainsKey(gridArea.MachineLayer))
                    cellLayersLookup.Add(gridArea.MachineLayer, new HashSet<IMachineGridCell>());
                
                if(!_allGridCells.ContainsKey(gridArea.MachineLayer)) 
                    _allGridCells.Add(gridArea.MachineLayer, new Dictionary<Vector3Int, IMachineGridCell>());
                
                var globalCells = _allGridCells[gridArea.MachineLayer];
                var layerCells = cellLayersLookup[gridArea.MachineLayer];
                
                foreach (var machineGridCell in gridArea.Cells)
                {
                    var layer = machineGridCell.MachineLayer;
                    
                    Debug.Assert(layer == gridArea.MachineLayer, $"Layer Missmatch Cell:{layer} and Grid:{gridArea.MachineLayer}");

                    if (layerCells.Contains(machineGridCell))
                        Debug.LogWarning($"Machine Cell {machineGridCell} already exists in local cell map");

                    //add to global lookup
                    var buildingGridCell = MachineCellToBuildingCell(machineTiles,
                        machineGridCell.Position,
                        machineGridCell.MachineLayer);
                    Debug.Log($"Converted Machine Cell {machineGridCell.Position} to {buildingGridCell} on layer {layer}");
                    
                    if (!globalCells.ContainsKey(buildingGridCell))
                    {
                        Debug.Log($"Adding Machine Cell {buildingGridCell} to Global Cell Map");
                        globalCells.Add(buildingGridCell, machineGridCell);
                    }
                    else
                    {
                        Debug.LogWarning($"Machine Cell {buildingGridCell} already exists in global cell map");
                    }
                    
                    //remove from global lookup when the machine is destroyed
                    cd.Add(Disposable.Create(() =>
                    {
                        machineGridCell.OnCellRemovedFromTilemap(this);
                        globalCells.Remove(buildingGridCell);
                    }));
                    
                    //add to machine local lookup
                    layerCells.Add(machineGridCell);
                    machineGridCell.OnCellAddedToTilemap(this);
                    
                    //remove from local lookup when the machine is destroyed
                    cd.Add(Disposable.Create(() =>
                    {
                        layerCells.Remove(machineGridCell);
                        machineGridCell.OnCellRemovedFromTilemap(this);
                    }));
                }
            }

            //remove from global lookup when the machine is destroyed
            if (!_destroyMachineDisposables.ContainsKey(machineTiles.MachineID))
            { 
                _destroyMachineDisposables.Add(machineTiles.MachineID, cd);
            }
            else
            {
                _destroyMachineDisposables[machineTiles.MachineID]?.Dispose();
                _destroyMachineDisposables[machineTiles.MachineID] = cd;
            }
            
            
            return cd;
        }

        private Vector3Int MachineCellToBuildingCell(IMachineTileProvider machine, Vector3Int machineCell, MachineLayers layers)
        {
            switch (layers)
            {
                case MachineLayers.SOLID:
                case MachineLayers.PIPE:
                    //no conversion needed for these layers
                    return machineCell + machine.CellPosition;
                case MachineLayers.WIRES:
                    throw new NotImplementedException();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layers), layers, null);
            }
        }
        public void RemoveMachine(IMachineTileProvider machineTileProvider)
        {
            if (_destroyMachineDisposables.ContainsKey(machineTileProvider.MachineID))
            {
                _destroyMachineDisposables[machineTileProvider.MachineID]?.Dispose();
                _destroyMachineDisposables.Remove(machineTileProvider.MachineID);
            }
            else
            {
                Debug.LogWarning("Machine Tile Map does not contain machine with id: " + machineTileProvider.MachineID);
            }
        }
      
    }
}