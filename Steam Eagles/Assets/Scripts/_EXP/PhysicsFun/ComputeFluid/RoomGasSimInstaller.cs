using System;
using _EXP.PhysicsFun.ComputeFluid.Utilities;
using Buildings.Rooms;
using Buildings.Rooms.Tracking;
using Sirenix.OdinInspector;
using UnityEngine;
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


            Container.Bind<SamplePoints>().AsSingle();
            Container.BindInterfacesAndSelfTo<SamplePointsCompute>().AsSingle().NonLazy();
            Container.Bind<TextureMap>().AsSingle();

            Container.Bind<DynamicIObjects>().AsSingle();
            Container.BindInterfacesAndSelfTo<DynamicGasIO>().AsSingle().NonLazy();
        }
    }
}