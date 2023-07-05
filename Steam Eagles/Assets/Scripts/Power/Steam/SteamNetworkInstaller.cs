using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using Buildings.Rooms;
using Power;
using Power.Steam;
using Power.Steam.Core;
using Power.Steam.Network;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

public class SteamNetworkInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        
        
        Container.BindInterfacesAndSelfTo<SteamNetworkState>().AsSingle().NonLazy();

        //binds Producers and Consumers as well as Producer Factory and Consumer Factory
        SteamIO.Installer.Install(Container);
        Container.BindInterfacesAndSelfTo<PipeGridGraph>().AsSingle().NonLazy();
        Container.Bind(typeof(INetwork), typeof(NodeRegistry), typeof(ISteamProcessing), typeof(GridGraph<NodeHandle>)).FromSubContainerResolve().ByInstaller<Power.Steam.Network.SteamNetworkInstaller>().AsSingle().NonLazy();
        Container.BindFactory<Vector3Int, NodeType, NodeHandle, NodeHandle.Factory>().AsSingle().NonLazy();

        Container.Bind<PowerNetworkConfigs.SteamNetworkConfig>().FromInstance(PowerNetworkConfigs.Instance.steamNetworkConfig).AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<SteamNetworkTilemapBridge>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<SteamNetworkBusinessLogic>().AsSingle().NonLazy();
        Container.BindInterfacesTo<Tester>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<SteamFlowCalculator>().AsSingle().NonLazy();
        
        //Container.BindInterfacesAndSelfTo<BuildingPipeNetwork>().AsSingle().NonLazy();
        
    }
    class Tester : IInitializable
    {
        private readonly INetwork _steamNetwork;
        private readonly Building _building;

        public Tester(INetwork steamNetwork, Building building)
        {
            _steamNetwork = steamNetwork;
            _building = building;
        }

        public void Initialize()
        {
            Debug.Assert(_steamNetwork != null);
            Debug.Log($"Initialized steam network for {_building.name}", _building);
        }
    }
    
    public class SteamNetworkBusinessLogic : IInitializable
    {
        private readonly NodeRegistry _pipes;
        private readonly SteamConsumers _consumers;
        private readonly SteamProducers _producers;
        private readonly CoroutineCaller _coroutineCaller;
        private readonly PowerNetworkConfigs.SteamNetworkConfig _config;

        public SteamNetworkBusinessLogic(NodeRegistry pipes, SteamConsumers consumers, SteamProducers producers, 
            CoroutineCaller coroutineCaller, PowerNetworkConfigs.SteamNetworkConfig config)
        {
            _pipes = pipes;
            _consumers = consumers;
            _producers = producers;
            _coroutineCaller = coroutineCaller;
            _config = config;
        }

        public void Initialize()
        {
            _coroutineCaller.StartCoroutine(SteamNetworkLogic());
        }
        IEnumerator SteamNetworkLogic()
        {
            
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            float timeUpdateStarted = 0;
            int count = 0;
            void StartUpdate()
            {
                Debug.Log("Starting Steam Network Update");
                timeUpdateStarted = Time.time;
                visited.Clear();
                count = 0;
            }

            float GetRemainingTimeTillNextUpdate() => Mathf.Max(0, _config.updateRate - (Time.time - timeUpdateStarted));
            
            
            while (true)
            {
                StartUpdate();
                foreach (var updateConsumer in GetUpdateConsumers(visited))
                {
                    updateConsumer();
                    count++;
                    if (count >= _config.maximumNodesToProcessPerFrame)
                    {
                        yield return null;
                        Debug.Log("Hit Maximum Nodes to Process Per Frame");
                        count = 0;
                    }
                }

                foreach (var updateProducer in GetUpdateProducers(visited))
                {
                    updateProducer();
                    count++;
                    if (count >= _config.maximumNodesToProcessPerFrame)
                    {
                        yield return null;
                        Debug.Log("Hit Maximum Nodes to Process Per Frame");
                        count = 0;
                    }
                }

                foreach (var updateNode in UpdateNodes())
                {
                    updateNode();
                    count++;
                    if (count >= _config.maximumNodesToProcessPerFrame)
                    {
                        yield return null;
                        Debug.Log("Hit Maximum Nodes to Process Per Frame");
                        count = 0;
                    }
                }
                
                
                yield return new WaitForSeconds(GetRemainingTimeTillNextUpdate());
            }
        }
   
        IEnumerable<Action> GetUpdateConsumers( HashSet<Vector2Int> visited)
        {
            int cnt = 0;
            var consumers = _consumers.Where(t => t.value.IsActive).ToArray();
            foreach (var io in consumers)
            {
                if (_consumers.TryGetConnection(io.cell, out var node))
                {
                    _config.Log($"Connected to {node.Position}");
                    cnt++;
                    yield return () =>
                    {
                        _config.Log("Placeholder Consumer Update");
                    };
                }
            }
            _config.Log($"{cnt} Active Consumers");
        }
        IEnumerable<Action> GetUpdateProducers(HashSet<Vector2Int> visited)
        {
            int cnt = 0;
            var producers = _producers.Where(t => t.value.IsActive).ToArray();
            foreach (var io in producers)
            {
                if (_producers.TryGetConnection(io.cell, out var node))
                {
                    _config.Log($"Connected to {node.Position}");
                    cnt++;
                    yield return () =>
                    {
                        _config.Log("Placeholder Producer Update");
                    };
                }
                
            }
            _config.Log($"{cnt} Active Producers");
        }

        IEnumerable<Action> UpdateNodes()
        {
            var values = _pipes.Values.ToArray();
            foreach (var pipesValue in values)
            {
                yield return () =>
                {
                    _config.Log($"Placeholder Pipe Update: {pipesValue.ToString()}",true);
                };
            }
        }

    
    }
    
    public class RoomPipeNetwork
    {
        public class Factory : PlaceholderFactory<Room, RoomPipeNetwork> { }
    }
}


