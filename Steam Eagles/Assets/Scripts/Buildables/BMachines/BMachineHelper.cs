using System;
using System.Collections.Generic;
using Buildables;
using Buildings;
using UnityEngine;

namespace Buildables
{
    public class BMachineHelper
    {
        private readonly Building _building;
        private readonly CellDebugger _debugger;
        private readonly BMachineConfig _config;

        private BMachineHelper(Building building, CellDebugger debugger, BMachineConfig config)
        {
            _building = building;
            _debugger = debugger;
            _config = config;
        }
        public IEnumerable<Vector2Int> GetMachineCells(BuildableMachineBase prefab, Vector2Int placement)
        {
            foreach (var cell in prefab.GetCells(placement))
            {
                yield return (Vector2Int)cell;
            }
        }

        public IEnumerable<Vector2Int> GetFloorCells(BuildableMachineBase prefab, Vector2Int placement)
        {
            foreach (var cell in prefab.GetBottomCells(placement))
            {
                yield return (Vector2Int)cell + Vector2Int.down;
            }
        }

        public bool IsPlacementOnFloor(BuildableMachineBase prefab, Vector2Int placement)
        {
            if (_config.debugFloor)
            {
                _debugger.Debug(GetFloorCells(prefab, placement));
            }
            foreach (var floorCell in GetFloorCells(prefab, placement))
            {
                var solidCell = new BuildingCell(floorCell, BuildingLayers.SOLID);
                var foundationCell = new BuildingCell(floorCell, BuildingLayers.FOUNDATION);
                if (!_building.Map.HasCell(solidCell) && !_building.Map.HasCell(foundationCell))
                {
                    return false;
                }
            }

            return true;
        }
    }
}