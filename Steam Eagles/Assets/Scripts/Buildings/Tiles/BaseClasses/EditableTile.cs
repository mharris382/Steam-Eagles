//using PhysicsFun.DynamicBlocks;

using System;
using System.ComponentModel;
using System.Linq;
using Buildings.Rooms;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.Tiles
{
    public interface IEditableTile
    {
        // ReSharper disable once InconsistentNaming
        string name { get; }
        bool CanTileBePlacedInRoom(Room room);

        bool CheckIsPlacementValid(Vector3Int cell, BuildingMap buildingMap, ref string error);
        BuildingLayers GetLayer();
    }
    public abstract class EditableTile : PuzzleTile, IEditableTile
    {
        public string recipeName;
        public bool useTileRulesAsPlacementRules = true;

        public abstract bool CanTileBePlacedInRoom(Room room);
        public override bool CanTileBeDisconnected()
        {
            //Debug.Assert(dynamicBlock!=null, $"Tile {name} is missing a DynamicBlock!", this);
            return true;
        }
        
        
        public abstract BuildingLayers GetLayer();

        public bool CheckIsPlacementValid(Vector3Int cell, BuildingMap buildingMap, ref string error)
        {
            
            var tilemap = buildingMap.GetTilemap(GetLayer());
            if (tilemap == null)
            {
                error = "Tilemap is null";
                return false;
            }

            if (useTileRulesAsPlacementRules && !RuleMatches(cell, tilemap))
            {
                error = "That is not a valid shape for this tile";
                return false;
            }
            if (!IsPlacementValid(cell, buildingMap))
            {
                error = "Invalid Tile placement";
                return false;
            }
            return true;
        }

        bool RuleMatches(Vector3Int cell, Tilemap tm)
        {
            Matrix4x4 transform = Matrix4x4.identity;
            foreach (var mTilingRule in m_TilingRules)
            {
                if (RuleMatches(mTilingRule, cell, tm, ref transform))
                    return true;
            }
            return false;
        }
        
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

        public virtual string GetSaveKey()
        {
            switch (GetLayer())
            {
                case BuildingLayers.NONE:
                case BuildingLayers.FOUNDATION:
                case BuildingLayers.COVER:
                case BuildingLayers.DECOR:
                case BuildingLayers.REQUIRED:
                    throw new InvalidEnumArgumentException();
                    break;
                case BuildingLayers.WALL:
                    return "Wall";
                    break;
                    break;
                case BuildingLayers.SOLID:
                    return "Solid";
                    break;
                case BuildingLayers.PIPE:
                    return "Pipe";
                case BuildingLayers.PLATFORM:
                case BuildingLayers.LADDERS:
                    return "Ladder";
                case BuildingLayers.WIRES:
                    return "Wire";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}