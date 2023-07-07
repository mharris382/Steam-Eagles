using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using Buildings.Rooms;
using Buildings.Rooms.Tracking;
using Buildings.Tiles;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;
using TilemapData = Buildings.Rooms.RoomTextureCreator.TilemapData;
namespace _EXP.PhysicsFun.ComputeFluid
{
    [RequireComponent(typeof(Room), typeof(RoomEffect))]
    public class RoomGasSimInstaller : MonoInstaller
    {
        public RoomGasSimConfig config;
        private RoomState _roomState;
        public RoomState RoomState => _roomState ? _roomState : _roomState = GetOrAdd<RoomState>();
        
        private BoundsLookup _boundsLookup;
        private RoomTextureCreator _roomTextureCreator;
        private GasTexture _gasTexture;
        private Room _room;
        private RoomSimTextures _simTextures;
        private RoomEffect _roomEffect;
        private RoomCamera _roomCamera;
        
        private RoomTextureCreator RoomTextureCreator => _roomTextureCreator ??= new RoomTextureCreator(RoomState);
        private BoundsLookup BoundsLookup => _boundsLookup ??= new BoundsLookup(RoomState.Room);
        public GasTexture GasTexture => _gasTexture ? _gasTexture : _gasTexture = GetOrAdd<GasTexture>();
        public Room Room => _room ? _room : _room = GetComponent<Room>();
        public RoomSimTextures SimTextures => _simTextures ? _simTextures : _simTextures = GetOrAdd<RoomSimTextures>();
        public RoomEffect RoomEffect => _roomEffect ? _roomEffect : _roomEffect = GetOrAdd<RoomEffect>();
        public RoomCamera RoomCamera => _roomCamera ? _roomCamera : _roomCamera = GetOrAdd<RoomCamera>(t => t.Init());
        T GetOrAdd<T>() where T : Component => GetOrAdd<T>(_ => { });
        T GetOrAdd<T>(Action<T> onCreated) where T : Component
        {
            var t = GetComponent<T>();
            if(t == null)
            {
                t = gameObject.AddComponent<T>();
                onCreated?.Invoke(t);
            }
            return t;
        }

        public override void InstallBindings()
        {
            
            
            Container.Bind<Room>().FromInstance(Room).AsSingle().IfNotBound();
            Container.Bind<RoomState>().FromInstance(RoomState).AsSingle().IfNotBound();
            Container.Bind<RoomTextureCreator>().FromInstance(RoomTextureCreator).AsSingle().IfNotBound();
            Container.Bind<BoundsLookup>().FromInstance(BoundsLookup).AsSingle().IfNotBound();
            Container.Bind<GasTexture>().FromInstance(GasTexture).AsSingle().IfNotBound();
            Container.Bind<RoomSimTextures>().FromInstance(SimTextures).AsSingle().IfNotBound();
            Container.Bind<RoomEffect>().FromInstance(RoomEffect).AsSingle().IfNotBound();
            Container.Bind<RoomCamera>().FromInstance(RoomCamera).AsSingle().IfNotBound();
            
            
            Container.Bind<RoomGasSimConfig>().FromInstance(config).AsSingle().NonLazy();
            Container.BindInterfacesTo<TilemapTextureSync>().AsSingle().NonLazy();
        }
    }

    

    public class TilemapTextureSync : IInitializable, IDisposable
    {
        private readonly RoomGasSimConfig _config;
        private readonly Room _room;
        private readonly RoomSimTextures _simTextures;
        private CompositeDisposable _cd = new();
        
   
        public TilemapTextureSync(RoomGasSimConfig config, Room room, RoomSimTextures simTextures)
        {
            _config = config;
            _room = room;
            _simTextures = simTextures;
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
        private void UpdateGas(IList<(BuildingCell, float)> valueTuples) => Update(valueTuples, BuildingLayers.SOLID);
        private void UpdateWall(IList<(BuildingCell, float)> valueTuples) => Update(valueTuples, BuildingLayers.SOLID);

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
            var data = changes.Select(t => new TilemapData(t.Item1.cell2D, t.Item2)).ToArray();
            if(data.Length == 0) return;
            var rt = GetTextureFor(layers);
            RoomTextureCreator.WriteTilemapDataToTexture(rt, data);
        }
        
    }
}