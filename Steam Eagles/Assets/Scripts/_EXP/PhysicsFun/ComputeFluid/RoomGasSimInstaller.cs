using System;
using System.Collections;
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
            Container.BindInterfacesTo<DeleteGasInsideSolids>().AsSingle().NonLazy();
        }
    }

    public class DeleteGasInsideSolids : IInitializable, IDisposable
    {
        private readonly RoomGasSimConfig _config;
        private readonly Room _room;
        private readonly RoomSimTextures _simTextures;
        private readonly BoundsLookup _boundsLookup;
        private readonly RoomTextureCreator _roomTextureCreator;
        private readonly GasTexture _gasTexture;
        private readonly CoroutineCaller _coroutineCaller;
        private CompositeDisposable _cd = new();
        private Coroutine _coroutine;

        private float _timeLastUpdate;
        private RenderTexture CompositeTexture => _simTextures.SolidTexture;
        private RenderTexture GasTexture => _gasTexture.RenderTexture;
   
        public DeleteGasInsideSolids(
            RoomGasSimConfig config,
            Room room, 
            RoomSimTextures simTextures,
            BoundsLookup boundsLookup,
            RoomTextureCreator roomTextureCreator,
            GasTexture gasTexture, CoroutineCaller _coroutineCaller)
        {
            _config = config;
            _room = room;
            _simTextures = simTextures;
            _boundsLookup = boundsLookup;
            _roomTextureCreator = roomTextureCreator;
            _gasTexture = gasTexture;
            this._coroutineCaller = _coroutineCaller;
            _timeLastUpdate = Time.time;
        }

        public void Tick()
        {
            if (Time.time - _timeLastUpdate > _config.deleteGasInSolidRate)
            {
                _timeLastUpdate = Time.time;
                DoUpdate();
            }
        }

        private void DoUpdate()
        {
            if(GasTexture == null || CompositeTexture == null) return;
            var gasTexture = _gasTexture.RenderTexture;
            var compositeTexture = _simTextures.SolidTexture;
            DynamicGasIOCompute.ExecuteDeleteGasInBoundary(gasTexture, compositeTexture);
        }

        public void Initialize()
        {
            _coroutine = _coroutineCaller.StartCoroutine(DoUpdateRoutine());
        }

        IEnumerator DoUpdateRoutine()
        {
            
            while (true)
            {
                if (GasTexture != null && CompositeTexture != null)
                {
                    var gasTexture = _gasTexture.RenderTexture;
                    var compositeTexture = _simTextures.SolidTexture;
                    DynamicGasIOCompute.ExecuteDeleteGasInBoundary(gasTexture, compositeTexture);
                }
                yield return new WaitForSeconds(_config.deleteGasInSolidRate);
            }
        }
        public void Dispose()
        {
            if (_coroutineCaller != null && _coroutine != null)
            {
                _coroutineCaller.StopCoroutine(_coroutine);
            }
        }
    }

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