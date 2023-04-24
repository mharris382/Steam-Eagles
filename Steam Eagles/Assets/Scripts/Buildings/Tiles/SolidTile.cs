using System.Linq;
using Buildings.Rooms;
using CoreLib;
using UnityEngine;

namespace Buildings.Tiles
{
    public class SolidTile : EditableTile
    {
        public bool allowPlacementOnDamagedWall = false;
        public bool allowPlacementOnPipe = false;

        public override bool CanTileBePlacedInRoom(Room room)
        {
            return room.buildLevel != BuildLevel.NONE;
        }

        public override bool CanTileBeDisconnected()
        {
            return true;
        }

        public override BuildingLayers GetLayer() => BuildingLayers.SOLID;

        
        /// <summary>
        /// rules for placement of solid tiles:
        /// 1. must be placed in an engineering room
        /// 2. cannot be placed on top of damaged wall
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="buildingMap"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override bool IsPlacementValid(Vector3Int cell, BuildingMap buildingMap)
        {
            if (!allowPlacementOnDamagedWall)
            {
                if (CheckIfOtherCellExistsOnMap<DamagedWallTile>(cell, buildingMap, BuildingLayers.WALL))
                    return false;
            }

            if (!allowPlacementOnPipe)
            {
                if (CheckIfOtherCellExistsOnMap(cell, buildingMap, BuildingLayers.PIPE))
                    return false;
            }

            if (CheckIfOtherCellIsFoundation(cell, buildingMap))
            {
                return false;
            }


            if (!buildingMap.CellIsInARoom(cell, GetLayer())) 
                return false;
            var room = buildingMap.GetRoom(cell, GetLayer());
            if (room.buildLevel == BuildLevel.NONE)
                return false;
            
            return true;
        }
        private bool CheckIfSolidCellHasPipe(Vector3Int cell, BuildingMap buildingMap)
        {
            var pipeCells = buildingMap.ConvertBetweenLayers(BuildingLayers.SOLID, BuildingLayers.PIPE, cell)
                .ToArray();
            Debug.Assert(pipeCells.Length == 1, "Expected exactly one pipe cell to be returned", this);
            var pipeCell = pipeCells[0];
            var pipeTile = buildingMap.GetTile<PipeTile>(pipeCell, BuildingLayers.PIPE);
            if (pipeTile != null)
            {
                if (debug)
                    Debug.Log($"Cannot place solid tile at position {cell} because there is a pipe there", this);
                return false;
            }

            return true;
        }
        private bool CheckIfSolidCellHasDamagedWall(Vector3Int cell, BuildingMap buildingMap)
        {
            var wallCells = buildingMap.ConvertBetweenLayers(BuildingLayers.SOLID, BuildingLayers.WALL, cell).ToArray();
            Debug.Assert(wallCells.Length == 1, "Expected exactly one wall cell to be returned", this);
            var wallCell = wallCells[0];
            var damagedTile = buildingMap.GetTile<DamagedWallTile>(wallCell, BuildingLayers.WALL);
            if (damagedTile != null)
            {
                if (debug)
                    Debug.Log($"Cannot place solid tile at position {cell} because there is a damaged wall there",
                        this);
                return true;
            }

            return false;
        }
    }
}