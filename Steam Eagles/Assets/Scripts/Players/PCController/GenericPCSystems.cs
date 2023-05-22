using System;
using System.Collections.Generic;
using Buildings.Rooms.Tracking;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Players.PCController
{
    public class TestPcSystem : PCSystem, IInitializable, ITickable, ILateTickable
    {
        public TestPcSystem(PC pc) : base(pc){}

        public void Initialize()
        {
            Debug.Log($"Ticking TestPcSystem for PC # {Pc.PlayerNumber}");
        }

        public void Tick()
        {
            Debug.Log($"Ticking TestPcSystem for PC # {Pc.PlayerNumber}");
        }

        public void LateTick()
        {
            Debug.Log($"Late Ticking TestPcSystem for  PC # {Pc.PlayerNumber}");
        }

        public override void Dispose()
        {
            Debug.Log($"Ticking TestPcSystem for PC # {Pc.PlayerNumber}");
        }
    }
    public class GenericPCSystems : PCSystems<GenericPCSystem>
    {
        public GenericPCSystems(PCTracker pcTracker, PC.Factory pcFactory, 
            ISystemFactory<GenericPCSystem> factory, 
            List< IFactory<PC, IPCSystem>> pcSystemFactories,
            List<IFactory<PC, Scene, IPCSceneSystem>> sceneSystemFactories) :
            base(pcTracker, pcFactory, factory)
        {
        }

        public override GenericPCSystem CreateSystemFor(PC pc) => this.Factory.Create(pc);
        
        
        
    }

    

    public interface IPCSystem
    {
    }

    public interface IPCSceneSystem
    {
    }

    

    
    
    public class GenericPCSystem : PCSystem, IInitializable, ITickable, ILateTickable
    {
        private List<IPCSystem> _pcSystems = new List<IPCSystem>();
        private Dictionary<Scene, List<IPCSceneSystem>> _sceneSystems = new Dictionary<Scene, List<IPCSceneSystem>>();
        
        private Subject<IPCSystem> _pcSystemAdded = new Subject<IPCSystem>();
        private Subject<IPCSystem> _pcSystemRemoved = new Subject<IPCSystem>();

        public GenericPCSystem(PC pc) : base(pc)
        {
        }


        public void AddPCSystem(IPCSystem system)
        {
            if (system == null)
            {
                Debug.LogError("tried to add a null system to the PCSystem");
                return;
            }

            if (_pcSystems.Contains(system))
                return;
            _pcSystems.Add(system);
            _pcSystemAdded.OnNext(system);
        }

        public void RemovePCSystem(IPCSystem system)
        {
            if (system == null)
            {
                Debug.LogError("tried to remove a null system to the PCSystem");
                return;
            }

            if (!_pcSystems.Contains(system))
                return;
            _pcSystems.Remove(system);
            _pcSystemRemoved.OnNext(system);
        }

        public void AddPCSystems(IEnumerable<IPCSystem> system)
        {
            foreach (var pcSystem in system)
            {
                AddPCSystem(pcSystem);
            }
        }

        public void AddSceneSystem(Scene scene, IPCSceneSystem sceneSystem)
        {
            throw new NotImplementedException();
        }

        public void AddSceneSystem(Scene scene, IEnumerable<IPCSceneSystem> sceneSystem)
        {
            foreach (var system in sceneSystem)
                AddSceneSystem(scene, system);
        }

        public void Initialize()
        {
        }

        public void Tick()
        {
        }

        public void LateTick()
        {
        }

        private List<IPCSceneSystem> GetSceneSystems(Scene scene)
        {
            if (!_sceneSystems.ContainsKey(scene))
            {
                _sceneSystems.Add(scene, new List<IPCSceneSystem>());
            }

            return _sceneSystems[scene];
        }

        public class Factory : PlaceholderFactory<PC, GenericPCSystem>, ISystemFactory<GenericPCSystem>
        {
        }
    }
}