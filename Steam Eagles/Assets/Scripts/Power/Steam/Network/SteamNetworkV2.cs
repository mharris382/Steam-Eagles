using System;
using System.Collections;
using System.Collections.Generic;
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

        public IObservable<(Vector2Int, T)> OnSystemAdded => _onSystemAdded.Select(pos => (pos, _systems[pos]));
        public IObservable<(Vector2Int, T)> OnSystemRemoved => _onSystemRemoved.Select(pos => (pos, _systems[pos]));

        public void AddSystem(Vector2Int pos, T system)
        {
            if (_systems.ContainsKey(pos) || system == null)
                return;
            _systems.Add(pos, system);
            _onSystemAdded.OnNext(pos);
        }

        public void RemoveSystem(Vector2Int pos)
        {
            if (_systems.ContainsKey(pos))
            {
                _onSystemRemoved.OnNext(pos);
                _systems.Remove(pos);
            }
        }
        public int Count => _systems.Count;
    }
    public class SteamProducers : SteamSystems<ISteamProducer>{}
    public class SteamConsumers : SteamSystems<ISteamConsumer>{}
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



        public bool HasPosition(Vector2Int position)
        {
            return _steamProcessing.HasPosition(position);
        }

        public float GetSteamFlowRate(Vector2Int p1, Vector2Int p2)
        {
            return _steamProcessing.GetSteamFlowRate(p1, p2);
        }

        public float GetPressureLevel(Vector2Int position) => _steamProcessing.GetPressureLevel(position);
        public float GetTemperature(Vector2Int position)
        {
            throw new NotImplementedException();
        }

        public bool IsBlocked(Vector2Int position) => _steamProcessing.IsBlocked(position);

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
            Debug.Log($"Ticking SteamNetwork BITCHES!\tProducer Count= {producerCount}\tConsumer Count= {consumerCount}");
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