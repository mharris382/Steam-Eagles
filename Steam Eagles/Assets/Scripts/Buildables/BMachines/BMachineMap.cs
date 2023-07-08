using System;
using System.Collections.Generic;
using System.Linq;
using Buildables;
using Buildings;
using UniRx;
using UnityEngine;

namespace Buildables
{
    public class BMachineMap
    {
        private readonly Building _building;
        private readonly BMachineHelper _helper;
        private readonly BMachine.Factory _machineFactory;
        private readonly BMachineConfig _config;
        private readonly CellDebugger _debugger;
        private readonly Dictionary<Vector2Int, BMachine> _usedCells = new();
        private Dictionary<BuildableMachineBase, List<Vector2Int>> _machineCells = new();
        private Subject<BMachine> _onMachinePlaced = new Subject<BMachine>();
        private Subject<BMachine> _onMachineRemoved = new Subject<BMachine>();
        
        public IObservable<BMachine> OnMachinePlaced => _onMachinePlaced;
        public IObservable<BMachine> OnMachineRemoved => _onMachineRemoved;

        public BMachineMap(Building building, BMachineHelper helper, BMachine.Factory machineFactory, BMachineConfig config, CellDebugger debugger)
        {
            _building = building;
            _helper = helper;
            _machineFactory = machineFactory;
            _config = config;
            _debugger = debugger;
        }

        
        public bool  RemoveMachineAt(Vector2Int position)
        {
            if(IsCellOverlappingMachine( position)==false)
                return false;
            var bMachine = _usedCells[position];
            foreach (var bMachineCell in bMachine.Cells) _usedCells.Remove(bMachineCell);
            _onMachineRemoved.OnNext(bMachine);
            bMachine.Dispose();
            return true;
        }

      

        public bool PlaceMachine(BuildableMachineBase machine, Vector2Int position)
        {
            if (!CanPlaceMachine(machine, position))
            {
                return false;
            }
            var bMachine = _machineFactory.Create(machine, position);
            foreach (var cell in GetMachineCells(machine, position)) _usedCells.Add(cell, bMachine);
            _onMachinePlaced.OnNext(bMachine);
            //machine.Build((Vector3Int)position, building);
            return true;
        }
        
        public BuildableMachineBase GetMachine(Vector2Int position)
        {
            if (!IsCellOverlappingMachine(position)) return null;
            var bMachine = _usedCells[position];
            return bMachine.Machine;
        }

        public IEnumerable<Vector2Int> GetAllValidCells(BuildableMachineBase machine, Vector2Int placement)
        {
            foreach (var cell in GetMachineCells(machine, placement))
            {
                if (IsCellOverlappingMachine(cell))
                {
                    continue;
                }

                if (IsCellOverlappingTile(cell))
                {
                    continue;
                }
                yield return cell;
            }
        }

        public bool CanPlaceMachine(BuildableMachineBase machine, Vector2Int placement, ref string reason)
        {
            if(_config.debugCells) _debugger.Debug(GetAllValidCells(machine, placement));
            var allValidCells = GetAllValidCells(machine, placement).ToArray();
            var neededSpace = machine.MachineGridSize.x * machine.MachineGridSize.y;
            if (allValidCells.Length != neededSpace)
            {
                reason = "Placement is not valid.";
                return false;
            }
            foreach (var cell in GetMachineCells(machine, placement))
            {
                if (IsCellOverlappingMachine(cell))
                {
                    reason = "Position is overlapping another machine";
                    return false;
                }

                if (IsCellOverlappingTile(cell))
                {
                    reason = "Position is overlapping a tile";
                    return false;
                }
            }
            if (machine.snapsToGround)
            {
                reason = "Machine must be placed on floor";
                return _helper.IsPlacementOnFloor(machine, placement);
            }
            return true;
    }
        public bool CanPlaceMachine(BuildableMachineBase machine, Vector2Int placement)
        {
            foreach (var cell in GetMachineCells(machine, placement))
            {
                if (IsCellOverlappingMachine(cell))
                {
                    return false;
                }

                if (IsCellOverlappingTile(cell))
                {
                    return false;
                }
            }
            if (machine.snapsToGround)
            {
                return _helper.IsPlacementOnFloor(machine, placement);
            }
            return true;
        }

        private IEnumerable<Vector2Int> GetMachineCells(BuildableMachineBase machine, Vector2Int placement) => _helper.GetMachineCells(machine, placement);

        private bool IsCellOverlappingMachine(Vector2Int cell) => _usedCells.ContainsKey(cell);


        private bool IsCellOverlappingTile(Vector2Int cell)
        {
            var solidCell = new BuildingCell(cell, BuildingLayers.SOLID);
            var foundationCell = new BuildingCell(cell, BuildingLayers.FOUNDATION);
            return _building.Map.HasCell(solidCell) || _building.Map.HasCell(foundationCell);
        }
    }
}