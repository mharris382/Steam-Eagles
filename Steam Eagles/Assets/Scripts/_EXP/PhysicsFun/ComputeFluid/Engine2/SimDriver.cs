using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace _EXP.PhysicsFun.ComputeFluid.Engine2
{
    public class SimResources
    {
        private readonly int _width;
        private readonly int _height;

        private RenderTexture _ping;
        private RenderTexture _pong;
        private RenderTexture _pressure;
        private RenderTexture _velocity;
        private RenderTexture _divergence;
        
        private ComputeBuffer _directionsLUTPressure;
        private ComputeBuffer _directionsLUTVelocity;
        
        private RenderTexture _boundaryOffsetPressure;
        private RenderTexture _boundaryOffsetVelocity;

        public SimResources(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public void Initialize()
        {
            _velocity = new RenderTexture(_width, _height, 1);
            _ping = new RenderTexture(_width, _height, 1);
            _pong = new RenderTexture(_width, _height, 1);
            _pressure = new RenderTexture(_width, _height, 1);
            _divergence = new RenderTexture(_width, _height, 1);

            _velocity.enableRandomWrite = true;
            _ping.enableRandomWrite = true;
            _pong.enableRandomWrite = true;
            _pressure.enableRandomWrite = true;
            _divergence.enableRandomWrite = true;
            
            _pong.Create();
            _ping.Create();
            _velocity.Create();
            _pressure.Create();
            _divergence.Create();
            
            _directionsLUTPressure = new ComputeBuffer(32, sizeof(int) * 4);
            _directionsLUTVelocity = new ComputeBuffer(32, sizeof(int) * 4);
            
            _directionsLUTPressure.SetData(ArbitaryBoundaryLUTGenerator.GetVelocityLUT(), 0, 0, 32);
            _directionsLUTVelocity.SetData(ArbitaryBoundaryLUTGenerator.GetVelocityLUT(), 0, 0, 32);
        }
    }
    public class SimDriver : IInitializable, IDisposable
    {
        private readonly SimState _simState;
        private readonly SimEffect _effect;
        private readonly GasTexture _gasTexture;
        private readonly Simulator _simulator;
        private readonly CapturedRoomTexture _roomTexture;
        private readonly RoomSimTextures _simTextures;
        
        //TODO: these are the same or alternate variations, i'm not sure which one is correct. fix it
        private readonly RoomGasSimConfig _config1;
        private readonly CoroutineCaller _coroutineCaller;
        
        private IDisposable _currentSimLoop;
        private SimResources _simResources;

        private readonly CompositeDisposable _cd = new();

        public SimDriver(SimState simState, SimEffect effect, GasTexture gasTexture,
            Simulator simulator,
           // CapturedRoomTexture roomTexture,
            RoomSimTextures simTextures, RoomGasSimConfig config1,  CoroutineCaller coroutineCaller)
        {
            _simState = simState;
            _effect = effect;
            _gasTexture = gasTexture;
            _simulator = simulator;
            //   _roomTexture = roomTexture;
            _simTextures = simTextures;
            _config1 = config1;
            _coroutineCaller = coroutineCaller;
        }

        public void Initialize()
        {
            if(_simState.IsRunning) StartSimLoop();
            _simState.OnActivate.Subscribe(_ => StartSimLoop()).AddTo(_cd);
            _simState.OnDeactivate.Subscribe(_ => StopSimLoop()).AddTo(_cd);
            
            _simResources = new SimResources(_gasTexture.RenderTexture.width, _gasTexture.RenderTexture.height);
            _simResources.Initialize();
        }

        public void Dispose()
        {
            _cd.Dispose();   
        }

        void StartSimLoop()
        {
            StopSimLoop();
            _currentSimLoop = Observable.FromCoroutine<float>(SimLoop).Subscribe(UpdateSim);
        }
      
        
        void StopSimLoop()
        {
            if (_currentSimLoop != null)
            {
                _currentSimLoop.Dispose();
                _currentSimLoop = null;
            }
        }

        IEnumerator SimLoop(IObserver<float> o, CancellationToken ct)
        {
            float elapsedTime = 0;
            while (_simState.IsRunning)
            {
                if (ct.IsCancellationRequested)
                    yield break;
                
                var loopTime = _config1.syncRate - elapsedTime;
                yield return new WaitForSeconds(Mathf.Max(0, loopTime ));
                Debug.Log("Sim Loop");
                if (ct.IsCancellationRequested)
                    yield break;
                
                var t = Time.time;
                o.OnNext(loopTime);
                elapsedTime = Time.time - t;
            }
        }

       
        
        
        void UpdateSim(float timeDelta)
        {
            _simulator.Simulate(_simResources, timeDelta);
        }
    }
}


