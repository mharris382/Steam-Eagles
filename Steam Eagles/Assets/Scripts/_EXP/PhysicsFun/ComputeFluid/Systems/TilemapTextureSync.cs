using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using Buildings.Rooms;
using Buildings.Tiles;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace _EXP.PhysicsFun.ComputeFluid
{
    public class TilemapTextureSync : IInitializable, IDisposable
    {
        private readonly RoomGasSimConfig _config;
        private readonly Room _room;
        private readonly RoomSimTextures _simTextures;
        private readonly BoundsLookup _boundsLookup;
        private readonly RoomTextureCreator _roomTextureCreator;
        private readonly GasTexture _gasTexture;
        private CompositeDisposable _cd = new();
        
   
        public TilemapTextureSync(RoomGasSimConfig config, Room room, RoomSimTextures simTextures, BoundsLookup boundsLookup, RoomTextureCreator roomTextureCreator, GasTexture gasTexture)
        {
            _config = config;
            _room = room;
            _simTextures = simTextures;
            _boundsLookup = boundsLookup;
            _roomTextureCreator = roomTextureCreator;
            _gasTexture = gasTexture;
        }

        private RenderTexture CompositeTexture => _simTextures.SolidTexture;
        private RenderTexture WallTexture => _simTextures.WallTexture;
        private RenderTexture GasInputTexture => _simTextures.GasInputTexture;
        public void Initialize()
        {
            var solidChangedStream = _room.Building.Map.OnTileChanged(BuildingLayers.SOLID, _room).Select(t => (new BuildingCell(t.cell, BuildingLayers.SOLID), GetValueForSolid(t.tile)));
            var wallChangedStream = _room.Building.Map.OnTileChanged(BuildingLayers.WALL, _room).Select(t => (new BuildingCell(t.cell, BuildingLayers.WALL), GetValueForWall(t.tile)));
            var gasChangedStream = _room.Building.Map.OnTileChanged(BuildingLayers.GAS, _room).Select(t => (new BuildingCell(t.cell, BuildingLayers.GAS), GetValueForGas(t.tile)));
            var rate = _config.syncRate;
            solidChangedStream.Buffer(TimeSpan.FromSeconds(rate)).Where(t => t.Count > 0).Subscribe(UpdateSolid).AddTo(_cd);
            wallChangedStream.Buffer(TimeSpan.FromSeconds(rate)).Where(t => t.Count > 0).Subscribe(UpdateWall).AddTo(_cd);
            gasChangedStream.Buffer(TimeSpan.FromSeconds(rate)).Where(t => t.Count > 0).Subscribe(UpdateGas).AddTo(_cd);
        }

        private void UpdateSolid(IList<(BuildingCell, float)> valueTuples) => Update(valueTuples, BuildingLayers.SOLID);
        private void UpdateGas(IList<(BuildingCell, float)> valueTuples) => Update(valueTuples, BuildingLayers.GAS);
        private void UpdateWall(IList<(BuildingCell, float)> valueTuples) => Update(valueTuples, BuildingLayers.WALL);

        float GetValueForGas(TileBase gasTile) => gasTile == null ? 0 : 1;
        float GetValueForSolid(TileBase solidTile) => solidTile == null ? 0 : 1;
        float GetValueForWall(TileBase wallTile)
        {
            if (wallTile == null || wallTile is DamagedWallTile)
            {
                return 1;
            }
            return 0;
        }

        public void Dispose()
        {
            _cd.Dispose();
        }

        RenderTexture GetTextureFor(BuildingLayers layers)
        {
            switch (layers)
            {
                case BuildingLayers.SOLID:
                    return CompositeTexture;
                case BuildingLayers.WALL:
                    return WallTexture;
                case BuildingLayers.GAS:
                    return GasInputTexture;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layers), layers, null);
            }
        }

        void Update(IEnumerable<(BuildingCell, float)> changes, BuildingLayers layers)
        {
            var valueTuples = changes as (BuildingCell, float)[] ?? changes.ToArray();
            var rt = GetTextureFor(layers);
            _roomTextureCreator.WriteArbitraryData(rt, valueTuples.Select(t => t.Item1), layers);

        }


        Vector2Int GetTexel(BuildingCell cell)
        {
            var bounds = _boundsLookup.GetBounds(cell.layers);
            var pos = cell.cell2D - (Vector2Int) bounds.min;
            return pos;
        }
    }
}