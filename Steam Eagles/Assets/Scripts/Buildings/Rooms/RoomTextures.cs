using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;
using UniRx;

namespace Buildings.Rooms
{
    [RequireComponent(typeof(Room), typeof(RoomEvents))]
    public class RoomTextures : MonoBehaviour
    {
        private Room _room;
        public Room Room => _room ? _room : _room = GetComponent<Room>();
        
        
        private Dictionary<BuildingLayers, Texture2D> _textures = new Dictionary<BuildingLayers, Texture2D>();
        private BoundsLookup _boundsLookup;
        private ITileColorPicker _tileColorPicker;
        private Subject<BuildingTile> _onTileSet = new();

        public IObservable<BuildingTile> OnTileSetStream => _onTileSet;

        [ShowInInspector, PreviewField, ReadOnly, BoxGroup("Textures")]
        public Texture2D SolidTexture
        {
            get => _textures.ContainsKey(BuildingLayers.SOLID) ? _textures[BuildingLayers.SOLID] : null;
            set => AssignTexture(BuildingLayers.SOLID, value);
        }
        [ShowInInspector, PreviewField, ReadOnly, BoxGroup("Textures")]
        public Texture2D PipeTexture
        {
            get => GetTexture(BuildingLayers.PIPE);
            set => AssignTexture(BuildingLayers.PIPE, value);
        }
        [ShowInInspector, PreviewField, ReadOnly, BoxGroup("Textures")]
        public Texture2D WallTexture
        {
            get => GetTexture(BuildingLayers.WALL);
            set => AssignTexture(BuildingLayers.WALL, value);
        }

        [ShowInInspector, PreviewField, ReadOnly, BoxGroup("Textures")]
        public Texture2D FoundationTexture
        {
            get => GetTexture(BuildingLayers.FOUNDATION);
            set => AssignTexture(BuildingLayers.FOUNDATION, value);
        }

        [ShowInInspector, PreviewField, ReadOnly, BoxGroup("Textures")]
        public Texture2D GasInputTexture
        {
            get => GetTexture(BuildingLayers.GAS);
            set => AssignTexture(BuildingLayers.GAS, value);
        }

        public void InitAllTextures()
        {
            
        }
        

        private Texture2D GetTexture(BuildingLayers layers) => _textures.ContainsKey(layers) ? _textures[layers] : null;
        public void AssignTexture(BuildingLayers layers, Texture2D texture2D)
        {
            if(_textures.ContainsKey(layers))
                _textures.Remove(layers);
            var expectedLayerBounds = Room.Building.Map.GetCellsForRoom(Room, layers);
            var expectedLayerBoundsSize = expectedLayerBounds.size;
            Debug.Assert(texture2D.width == expectedLayerBoundsSize.x);
            Debug.Assert(texture2D.height == expectedLayerBoundsSize.y);
            _textures.Add(layers, texture2D);
        }


        
        void Inject(RoomEvents roomEvents, BoundsLookup boundsLookup, ITileColorPicker tileColorPicker)
        {
            this._boundsLookup = boundsLookup;
            _tileColorPicker = tileColorPicker;
            roomEvents.OnBuildingCellTileChanged.Subscribe(t => OnTileSet(t.cell.cell, t.cell.layers, t.tile)).AddTo(this);
        }


        void OnTileSet(Vector3Int cell, BuildingLayers layers, TileBase tile)
        {
            _onTileSet.OnNext(new BuildingTile()
            {
                cell = new BuildingCell(cell, layers),
                tile = tile
            });
            var texture = GetTexture(layers);
            if (texture == null) return;
            if(_boundsLookup == null)_boundsLookup = new BoundsLookup(Room);
            var bounds = _boundsLookup.GetBounds(layers);
            var index = (cell.x - bounds.xMin) + (cell.y - bounds.yMin) * bounds.size.x;
            var prevColor = texture.GetPixel(index % bounds.size.x, index / bounds.size.x);
            var newColor = _tileColorPicker.GetColorForTile(cell, layers, tile, prevColor);
            texture.SetPixel(index % bounds.size.x, index / bounds.size.x, newColor);
        }

       void OnTileCleared(Vector3Int cell, BuildingLayers layers)
        {
            OnTileSet(cell, layers, null);
        }
    }
}