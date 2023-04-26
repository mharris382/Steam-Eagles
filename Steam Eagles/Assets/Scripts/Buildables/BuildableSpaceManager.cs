using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using UnityEngine;

namespace Buildables
{
    

    public static class BuildableSpaceManager
    {
        private static Dictionary<Building, BuildingSpace> _buildingSpaces = new Dictionary<Building, BuildingSpace>();
        public static void AddMachine(BuildableMachineBase machine, bool withSafetyCheck = true)
        {
            var building = machine.Building;
            Debug.Assert(building != null, "Machines must be placed inside a building", machine);
            var space = GetBuildingSpace(building);
            if (withSafetyCheck)
            {
                foreach (var cell in machine.GetCells())
                {
                    if (building.IsCellOverlappingMachine(cell))
                        throw new MachineOverlapException(machine, building.GetMachineAtPosition(cell));
                }
            }
            space.AddMachine(machine);
        }
        public static void RemoveMachine(BuildableMachineBase machine, bool withSafetyCheck = true)
        {
            var building = machine.Building;
            Debug.Assert(building != null, "Machines must be placed inside a building", machine);
            var space = GetBuildingSpace(building);
            if (withSafetyCheck)
            {
                foreach (var cell in machine.GetCells())
                {
                    var machineAtPosition = building.GetMachineAtPosition(cell);
                    if (machineAtPosition != machine)
                        throw new MachineOverlapException(machine, machineAtPosition);
                }
            }
            space.RemoveMachine(machine);
        }

        public static bool IsCellOverlappingMachine(this Building building, Vector3Int cell)
        {
            var space = GetBuildingSpace(building);
            return space.IsCellOverlappingMachine(cell);
        }
        public static BuildableMachineBase GetMachineAtPosition(this Building building, Vector3Int cell)
        {
            var space = GetBuildingSpace(building);
            return space.GetMachine(cell);
        }
        private static BuildingSpace GetBuildingSpace(Building building)
        {
            if (!_buildingSpaces.TryGetValue(building, out var space))
            {
                space = new BuildingSpace(building);
                _buildingSpaces.Add(building, space);
            }
            return space;
        }
        private class BuildingSpace
        {
            private readonly Building _building;
            private Dictionary<Vector3Int, BuildableMachineBase> _machines; 
            public BuildingSpace(Building building)
            {
                _building = building;
                _machines = new Dictionary<Vector3Int, BuildableMachineBase>(); 
            }
            public void AddMachine(BuildableMachineBase machine)
            {
                foreach (var cell in machine.GetCells())
                {
                    _machines.Add(cell, machine);
                }
            }
            
            public void RemoveMachine(BuildableMachineBase machine)
            {
                foreach (var cell in machine.GetCells())
                {
                    _machines.Remove(cell);
                }
            }
            
            public bool IsCellOverlappingMachine(Vector3Int cell) => _machines.ContainsKey(cell);

            public BuildableMachineBase GetMachine(Vector3Int cell)
            {
                if (_machines.ContainsKey(cell))
                {
                    return _machines[cell];
                }
                return null;
            }
        }
    }
}