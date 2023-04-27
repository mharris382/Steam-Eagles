using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings
{
    public struct BuildingTilemapChangedInfo
    {
        public readonly Building building;
        public readonly Tilemap tilemap;
        public readonly Vector3Int Cell;
        public readonly BuildingLayers layer;

        public TileBase Tile => tilemap.GetTile(Cell);
        public bool HasTile => Tile != null;

        public BuildingTilemapChangedInfo(Building building, Tilemap tm, Vector3Int cell, BuildingLayers layer)
        {
            this.building = building;
            this.Cell = cell;
            this.layer = layer;
            this.tilemap = tm;
        }
    }
}