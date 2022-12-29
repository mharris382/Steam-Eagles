using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;

namespace Buildings.BuildingTilemaps
{
    [RequireComponent(typeof(Tilemap))]
    public abstract class BuildingTilemap : MonoBehaviour
    {
        private Tilemap _tm;
        public Tilemap Tilemap => _tm ? _tm : _tm = GetComponent<Tilemap>();
        
        public abstract BuildingLayers Layer { get; }
        
        public virtual void UpdateTilemap(Building building)
        {
            name = $"{building.buildingName} {GetType().Name}";
        }

        public Rect GetWorldBounds()
        {
            var bounds = Tilemap.cellBounds;
            var min = Tilemap.CellToWorld(bounds.min);
            var max = Tilemap.CellToWorld(bounds.max);
            return new Rect(min, max - min);
        }
    }
}