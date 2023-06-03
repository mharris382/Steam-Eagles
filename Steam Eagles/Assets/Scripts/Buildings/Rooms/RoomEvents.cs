using System;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.Rooms
{
    public class RoomEvents : MonoBehaviour
    {
        private Subject<(BuildingCell cell, TileBase tile)> _onTilemapCellChanged = new Subject<(BuildingCell cell, TileBase tile)>();
        private IObservable<BuildingCell> _onTileCleared;
        private IObservable<(BuildingCell cell, TileBase tile)> _onTileSet;

        /// <summary> emits whenever a tile is changed </summary>
        public IObservable<(BuildingCell cell, TileBase tile)> OnBuildingCellTileChanged => _onTilemapCellChanged;
        /// <summary> emits whenever a tile is set </summary>
        public IObservable<(BuildingCell cell, TileBase tile)> OnBuildingCellTileSet => _onTileSet;
        /// <summary> emits whenever a tile is cleared </summary>
        public IObservable<BuildingCell> OnBuildingCellTileCleared => _onTileCleared;
        
        
        private void Awake()
        {
            var onTileSet = new Subject<(BuildingCell cell, TileBase tile)>();
            _onTilemapCellChanged.Where(t => t.tile != null).Subscribe(onTileSet);
            _onTileSet = onTileSet;
            onTileSet.AddTo(this);
            
            var onTileCleared = new Subject<BuildingCell>();
            _onTilemapCellChanged.Where(t => t.tile == null).Select(t => t.cell).Subscribe(onTileCleared);
            _onTileCleared = onTileCleared;
            onTileCleared.AddTo(this);
        }

        public void OnTileSet(Vector3Int cell, BuildingLayers layers, TileBase tile)
        {
            _onTilemapCellChanged.OnNext((new BuildingCell(cell, layers), tile));
        }
        
        
        
        public IObservable<(Vector3Int cell, TileBase tile)> OnTileSet(BuildingLayers layers) => OnBuildingCellTileSet.Where(t => t.cell.layers == layers).Select(t => (t.cell.cell, t.tile));

        public IObservable<Vector3Int> OnTileCleared(BuildingLayers layers) => OnBuildingCellTileCleared.Where(t => t.layers == layers).Select(t => t.cell);
    }
}