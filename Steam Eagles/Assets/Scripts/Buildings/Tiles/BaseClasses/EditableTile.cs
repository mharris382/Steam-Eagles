//using PhysicsFun.DynamicBlocks;

using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.Tiles
{
    public abstract class EditableTile : PuzzleTile
    {
        
        public override bool CanTileBeDisconnected()
        {
            //Debug.Assert(dynamicBlock!=null, $"Tile {name} is missing a DynamicBlock!", this);
            return true;
        }
        
        
        public abstract BuildingLayers GetLayer();
        public abstract bool IsPlacementValid(Vector3Int cell, BuildingMap buildingMap);
        
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

        protected bool CheckIfOtherCellIsFoundation(Vector3Int cell, BuildingMap buildingMap) =>
            CheckIfOtherCellExistsOnMap(cell, buildingMap, BuildingLayers.FOUNDATION);
    }
}