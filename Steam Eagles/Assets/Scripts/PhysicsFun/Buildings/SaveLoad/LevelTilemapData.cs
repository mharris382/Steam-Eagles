using System;
using System.Collections.Generic;
using Buildings.BuildingTilemaps;
using Spaces;
using UnityEngine;

namespace PhysicsFun.Buildings
{
    [Serializable]
    public class LevelTilemapData
    {
        
        public List<PuzzleTile> tiles = new List<PuzzleTile>();
        public List<Vector3Int> positions = new List<Vector3Int>();
        
        public LevelTilemapData(BuildingTilemap tmp)
        {
            var tilemap = tmp;
            BoundsInt bounds = tilemap.Tilemap.cellBounds;
            for (int x = bounds.min.x; x < bounds.max.x; x++)
            {
                for (int y = bounds.min.y; y < bounds.max.y; y++)
                {
                    for(int z = bounds.min.z; z < bounds.max.z; z++)
                    {
                        Vector3Int position = new Vector3Int(x, y, z);
                        PuzzleTile tile = tilemap.Tilemap.GetTile(position) as PuzzleTile;
                        if (tile != null)
                        {
                            tiles.Add(tile);
                            positions.Add(position);
                        }
                    }
                }   
            }
        }
    }
}