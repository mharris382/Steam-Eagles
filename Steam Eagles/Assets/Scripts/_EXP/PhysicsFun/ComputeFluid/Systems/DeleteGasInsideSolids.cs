using System;
using System.Collections;
using Buildings.Rooms;
using UniRx;
using UnityEngine;
using Zenject;

namespace _EXP.PhysicsFun.ComputeFluid
{
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
}