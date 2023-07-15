using System;
using System.Collections.Generic;
using System.Linq;
using Buildables;
using Buildables.Interfaces;
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
            
            var machine = bMachine.Machine;
            var listeners = machine.GetComponentsInChildren<IMachineListener>();
            foreach (var machineListener in listeners) machineListener.OnMachineRemoved(machine);

            _onMachineRemoved.OnNext(bMachine);
            bMachine.Dispose();
            return true;
        }

        public bool PlaceMachine(BuildableMachineBase machine, Vector2Int position, bool flipped)
        {
            if (!CanPlaceMachine(machine, position, flipped))
            {
                return false;
            }
            var bMachine = _machineFactory.Create(machine, position);
            foreach (var cell in GetMachineCells(machine, position)) _usedCells.Add(cell, bMachine);
            
            var listeners = machine.GetComponentsInChildren<IMachineListener>();
            foreach (var machineListener in listeners)
            {
                machineListener.OnMachineBuilt(machine);
            }
            
            _onMachinePlaced.OnNext(bMachine);
            
            return true;
        }


        public bool PlaceMachine(BuildableMachineBase machine, Vector2Int position) => PlaceMachine(machine, position, machine.IsFlipped);

        public BuildableMachineBase GetMachine(Vector2Int position)
        {
            if (!IsCellOverlappingMachine(position)) return null;
            var bMachine = _usedCells[position];
            return bMachine.Machine;
        }

        public BuildableMachineBase GetMachine(Vector3Int position) =>
            GetMachine(new Vector2Int(position.x, position.y));

        public bool TryGetMachine(Vector3Int position, out BuildableMachineBase machineBase)
        {
            machineBase = GetMachine(position);
            return machineBase != null;
        }
        public IEnumerable<Vector2Int> GetAllValidCells(BuildableMachineBase machine, Vector2Int placement)
        {
            return GetAllValidCells(machine, placement, machine.IsFlipped);
        }
        public IEnumerable<Vector2Int> GetAllValidCells(BuildableMachineBase machine, Vector2Int placement, bool flipped)
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
        public bool CanPlaceMachine(BuildableMachineBase machine, Vector2Int placement, ref string reason) => CanPlaceMachine(machine, placement, machine.IsFlipped, ref reason);
        
        public bool CanPlaceMachine(BuildableMachineBase machine, Vector2Int placement, bool flipped, ref string reason)
        {
            if(_config.debugCells) _debugger.Debug(GetAllValidCells(machine, placement, flipped));
            var allValidCells = GetAllValidCells(machine, placement, flipped).ToArray();
            var neededSpace = machine.MachineGridSize.x * machine.MachineGridSize.y;
            if (allValidCells.Length != neededSpace)
            {
                reason = "Placement is not valid.";
                return false;
            }
            foreach (var cell in GetMachineCells(machine, placement, flipped))
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
                return _helper.IsPlacementOnFloor(machine, placement, flipped);
            }
            return true;
    }
        public bool CanPlaceMachine(BuildableMachineBase machine, Vector2Int placement) => CanPlaceMachine(machine, placement, machine.IsFlipped);

        public bool CanPlaceMachine(BuildableMachineBase machine, Vector2Int placement, bool flipped)
        {
               var reason = "";
                return CanPlaceMachine(machine, placement, flipped, ref reason);
        }

        private IEnumerable<Vector2Int> GetMachineCells(BuildableMachineBase machine, Vector2Int placement) =>
            GetMachineCells(machine, placement, machine.IsFlipped);
        private IEnumerable<Vector2Int> GetMachineCells(BuildableMachineBase machine, Vector2Int placement, bool flipped) => _helper.GetMachineCells(machine, placement, flipped);

        private bool IsCellOverlappingMachine(Vector2Int cell) => _usedCells.ContainsKey(cell);


        private bool IsCellOverlappingTile(Vector2Int cell)
        {
            var solidCell = new BuildingCell(cell, BuildingLayers.SOLID);
            var foundationCell = new BuildingCell(cell, BuildingLayers.FOUNDATION);
            return _building.Map.HasCell(solidCell) || _building.Map.HasCell(foundationCell);
        }
    }
}