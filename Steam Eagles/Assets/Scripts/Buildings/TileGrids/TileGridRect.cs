using System;
using System.Collections.Generic;
using Buildings.Rooms;
using UnityEngine;

namespace Buildings.TileGrids
{
    [Serializable]
    public class TileGridRect
    {
        public readonly BuildingLayers layer;
        public readonly BoundsInt bounds;

        public int Area => bounds.x * bounds.y;
        public TileGridRect(BuildingLayers layer, BoundsInt bounds)
        {
            this.layer = layer;
            this.bounds = bounds;
        }
    }
    
    
    public static class BuildingUtils
    {
        public static BuildingLayers[] SavedLayers = new[] {
            BuildingLayers.SOLID,
            BuildingLayers.PIPE,
            BuildingLayers.WALL,
            BuildingLayers.WIRES,
            BuildingLayers.LADDERS,
            BuildingLayers.PLATFORM,
        };
        public static TileGridRect GetGridRect(this Room room, BuildingLayers layers)
        {
            var building = room.Building;
            var map = building.Map;
            var bounds = map.GetCellsForRoom(room, layers);
            return new TileGridRect(layers, bounds);
        }
        public static TileGridRect[] GetGridRect(this Room room)
        {
            var rects = new TileGridRect[SavedLayers.Length];
            for (int i = 0; i < SavedLayers.Length; i++)
            {
                rects[i] = room.GetGridRect(SavedLayers[i]);
            }
            return rects;
        }
    }
}