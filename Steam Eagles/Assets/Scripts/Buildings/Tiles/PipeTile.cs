using System.Linq;
using Buildings.Rooms;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.Tiles
{
    public class PipeTile : DamageableTile
    {
        public Vector4 scale = new Vector4(1, 1, 1, 1);
        public bool allowPlacementOnFoundations = false;
        public bool allowPlacementOnSolid = true;

        public DamagedPipeTile damagedTileVariant;
        
        public override bool CanTileBePlacedInRoom(Room room)
        {
            return room.buildLevel != BuildLevel.NONE;
        }
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);
            tileData.transform.SetTRS(tileData.transform.GetPosition(), tileData.transform.rotation, scale);
        }

        public override BuildingLayers GetLayer() => BuildingLayers.PIPE;

        public override bool IsPlacementValid(Vector3Int cell, BuildingMap buildingMap)
        {
            if (!allowPlacementOnFoundations && CheckIfOtherCellIsFoundation(cell, buildingMap))
            {
                return false;
            }
            
            if (!allowPlacementOnSolid )
            {
                if (CheckIfPipeCellHasSolid(cell, buildingMap))
                {
                    return false;
                }
            }
            
            return buildingMap.CellIsInARoom(cell, GetLayer());
        }

        public override RepairableTile GetDamagedTileVersion() => damagedTileVariant;

        private bool CheckIfPipeCellHasSolid(Vector3Int cell, BuildingMap buildingMap) => CheckIfOtherCellExistsOnMap(cell, buildingMap, BuildingLayers.SOLID);
        private bool CheckIfPipeCellHasDamagedWall(Vector3Int cell, BuildingMap buildingMap) =>
            CheckIfOtherCellExistsOnMap<DamagedWallTile>(cell, buildingMap, BuildingLayers.WALL);

        protected override bool TileIsMatch(TileBase other)
        {
            if(other == damagedTileVariant)
                return true;
            return base.TileIsMatch(other);
        }
    }
}