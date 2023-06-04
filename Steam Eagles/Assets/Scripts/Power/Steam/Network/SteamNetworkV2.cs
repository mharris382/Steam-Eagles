using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

namespace Power.Steam.Network
{
    
    public class SteamSystems<T>
    {
        private Dictionary<Vector2Int, T> _systems = new();
        
        
        private Subject<Vector2Int> _onSystemAdded  = new();
        private Subject<Vector2Int> _onSystemRemoved = new();
        private NodeRegistry _nodeRegistry;

        public IObservable<(Vector2Int, T)> OnSystemAdded => _onSystemAdded.Select(pos => (pos, _systems[pos]));
        public IObservable<(Vector2Int, T)> OnSystemRemoved => _onSystemRemoved.Select(pos => (pos, _systems[pos]));

        [Inject]
        private void Inject(NodeRegistry nodeRegistry)
        {
            _nodeRegistry = nodeRegistry;
            //whenever a node is removed, check if it was a connection
        
        }

        public bool TryGetConnection(Vector2Int vector2Int, out GridNode node)
        {
            node = null;
            if (_nodeRegistry.HasValue(vector2Int))
            {
                node = _nodeRegistry.GetValue(vector2Int);
            }
            return false;
        }

        public void AddSystem(Vector2Int pos, T system)
        {
            Debug.Assert(_nodeRegistry != null);
            if (_systems.ContainsKey(pos) || system == null)
                return;
            _systems.Add(pos, system);
            _onSystemAdded.OnNext(pos);
        }
        
        public void RemoveSystem(Vector2Int pos)
        {
            Debug.Assert(_nodeRegistry != null);
            if (_systems.ContainsKey(pos))
            {
                _onSystemRemoved.OnNext(pos);
                _systems.Remove(pos);
            }
        }
        
        
        public int Count => _systems.Count;

        public IEnumerable<(Vector2Int cell, T value)> GetValues()
        {
            foreach (var system in _systems)
            {
                yield return (system.Key, system.Value);
            }
        }
        public T GetSystem(Vector2Int pos)
        {
            if(!_systems.ContainsKey(pos))
                throw new Exception($"No system at {pos}");
            return _systems[pos];
        }
        public T GetSystem(Vector3Int pos) => GetSystem((Vector2Int) pos);
        
        
        
    }
    public class SteamProducers : SteamSystems<ISteamProducer>{}

    public class SteamNetwork : INetwork, ITickable
    {
        private readonly NodeHandle.Factory _nodeHandleFactory;
        private readonly NodeRegistry _nodeRegistry;
        private readonly GridGraph<NodeHandle> _gridGraph;
        private readonly INetworkTopology _networkTopology;
        private readonly ISteamProcessing _steamProcessing;
        private readonly ISteamEventHandling _steamEventHandling;
        private readonly SteamProducers _producers;
        private readonly SteamConsumers _consumers;
        private readonly CoroutineCaller _coroutineCaller;


        public SteamNetwork(
            NodeHandle.Factory nodeHandleFactory, 
            NodeRegistry nodeRegistry,
            GridGraph<NodeHandle> gridGraph,
            INetworkTopology networkTopology,
            ISteamProcessing steamProcessing, 
            ISteamEventHandling steamEventHandling, 
            SteamProducers producers,
            SteamConsumers consumers, CoroutineCaller coroutineCaller)
        {
            this._nodeHandleFactory = nodeHandleFactory;
            this._nodeRegistry = nodeRegistry;
            _gridGraph = gridGraph;
            _networkTopology = networkTopology;
            _steamProcessing = steamProcessing;
            _steamEventHandling = steamEventHandling;
            _producers = producers;
            _consumers = consumers;
            _coroutineCaller = coroutineCaller;
            _coroutineCaller.StartCoroutine(TickSelf());
        }

        IEnumerator TickSelf()
        {
            while (true)
            {
                Tick();
                yield return null;
            }
        }

        public NodeHandle AddNode(Vector2Int position, NodeType nodeType) => _networkTopology.AddNode(position, nodeType);

        public void RemoveNode(Vector2Int position) => _networkTopology.RemoveNode(position);

        public void ConnectNodes(Vector2Int positionA, Vector2Int positionB) => _networkTopology.ConnectNodes(positionA, positionB);

        public void DisconnectNodes(Vector2Int positionA, Vector2Int positionB) => _networkTopology.DisconnectNodes(positionA, positionB);
        public IEnumerable<Vector2Int> GetUsedPositions()
        {
            return _networkTopology.GetUsedPositions();
        }

        public void UpdateSteamState(float deltaTime) => _steamProcessing.UpdateSteamState(deltaTime);
        public bool HasPosition(Vector2Int position) => _steamProcessing.HasPosition(position);
        public float GetSteamFlowRate(Vector2Int p1, Vector2Int p2) => _steamProcessing.GetSteamFlowRate(p1, p2);
        public float GetPressureLevel(Vector2Int position) => _steamProcessing.GetPressureLevel(position);
        public float GetTemperature(Vector2Int position) => _steamProcessing.GetTemperature(position);
        public bool IsBlocked(Vector2Int position) => _steamProcessing.IsBlocked(position);
        public void LoadSteamStateForTexture(BoundsInt cellBounds, Texture2D saveTexture)
        {
            _steamProcessing.LoadSteamStateForTexture(cellBounds, saveTexture);
        }

        public IObservable<GasConsumedEventData> GasConsumedObservable => _steamEventHandling.GasConsumedObservable;

        public IObservable<GasProducedEventData> GasProducedObservable => _steamEventHandling.GasProducedObservable;
        public void AddConsumer(Vector2Int position, ISteamConsumer consumer)
        {
            _consumers.AddSystem(position, consumer);
        }

        public void RemoveConsumer(Vector2Int position)
        {
            _consumers.RemoveSystem(position);
        }

        public void AddProducer(Vector2Int position, ISteamProducer producer)
        {
            _producers.AddSystem(position, producer);
        }

        public void RemoveProducer(Vector2Int position)
        {
            _producers.RemoveSystem(position);
        }

        public void Tick()
        {
            int producerCount = _producers.Count;
            int consumerCount = _consumers.Count;
            int nodeCount = _nodeRegistry.Count;
            int connectedComponentCount = _nodeRegistry.CountConnectedComponents();
            Debug.Log($"Ticking SteamNetwork BITCHES!\tProducer Count= {producerCount}\tConsumer Count= {consumerCount}\tNode Count= {nodeCount}\nConnected Component Count= {connectedComponentCount}");

            int connectedConsumers = 0;
            foreach (var tup in _consumers.GetValues())
            {
                var (pos, consumer) = tup;
                var adjacentComponents = _nodeRegistry.GetAdjacentComponents(pos).ToArray();;
                if (_gridGraph.HasNode(pos) || adjacentComponents.Length > 0)
                {
                    connectedConsumers++;
                }                    
            }
            int connectedProducers = 0;
            foreach (var tup in _producers.GetValues())
            {
                var (pos, producer) = tup;
                var adjacentComponents = _nodeRegistry.GetAdjacentComponents(pos).ToArray();
                if (_gridGraph.HasNode(pos) || adjacentComponents.Length > 0)
                {
                    connectedProducers++;
                }
            }
            Debug.Log($"Connected Consumers= {connectedConsumers}\tConnected Producers= {connectedProducers}\n" +
                      $"Unconnected Consumers= {consumerCount - connectedConsumers}\tUnconnected Producers= {producerCount - connectedProducers}");
        }
    }


    public interface INetwork : ISteamProcessing, INetworkTopology, ISteamEventHandling
    {

        void AddConsumer(Vector2Int position, ISteamConsumer consumer);
        void RemoveConsumer(Vector2Int position);

        void AddProducer(Vector2Int position, ISteamProducer producer);
        void RemoveProducer(Vector2Int position);
    }


}