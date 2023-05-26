using System.Linq;
using Buildings.Rooms;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.Tiles
{
    public abstract class NonRuleEditableTile : TileBase, IEditableTile
    {
        protected bool CheckIfOtherCellExistsOnMap(Vector3Int cell, BuildingMap buildingMap,
            BuildingLayers otherLayer, bool expectOnly1 = false)
        {
            var otherCells = buildingMap.ConvertBetweenLayers(GetLayer(), otherLayer, cell);
            if (expectOnly1)
            {
                return buildingMap.GetTile( otherCells.FirstOrDefault(), otherLayer) != null;
            }
            foreach (var otherCell in otherCells)
            {
                var tile = buildingMap.GetTile(otherCell, otherLayer);
                if (tile != null)
                    return true;
            }
            
            return false;
        }
        protected bool CheckIfOtherCellExistsOnMap<T>(Vector3Int cell, BuildingMap buildingMap,
            BuildingLayers otherLayer, bool expectOnly1 = false) where T : TileBase
        {
            var otherCells = buildingMap.ConvertBetweenLayers(GetLayer(), otherLayer, cell);
            if (expectOnly1)
            {
                return buildingMap.GetTile<T>( otherCells.FirstOrDefault(), otherLayer) != null;
            }
            foreach (var otherCell in otherCells)
            {
                if (buildingMap.GetTile<T>(otherCell, otherLayer) != null)
                    return true;
            }
            
            return false;
        }

        public bool CanTileBePlacedInRoom(Room room) => room.buildLevel != BuildLevel.NONE;

        public bool CheckIsPlacementValid(Vector3Int cell, BuildingMap buildingMap, ref string error)
        {
            return true;
        }

        public abstract BuildingLayers GetLayer();
        private bool CheckIfPipeCellHasSolid(Vector3Int cell, BuildingMap buildingMap) => CheckIfOtherCellExistsOnMap(cell, buildingMap, BuildingLayers.SOLID);
    }
}