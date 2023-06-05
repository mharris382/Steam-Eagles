using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Power.Steam.Network
{
    public class SteamFlowCalculator : IInitializable
    {
        private readonly SteamConsumers _consumers;
        private readonly SteamProducers _producers;
        private readonly NodeRegistry _pipes;
        private readonly SteamIO.Connections _connections;
        private readonly CoroutineCaller _coroutineCaller;
        private readonly PowerNetworkConfigs.SteamNetworkConfig _config;


        Dictionary<Cell<ISteamProducer>, Dictionary<Cell<ISteamConsumer>, Flow>> _flowCache = new();

        Dictionary<int, List<Flow>> _flowCacheByComponent = new();
        public SteamFlowCalculator(SteamConsumers consumers, SteamProducers producers, NodeRegistry pipes, SteamIO.Connections connections,
            CoroutineCaller coroutineCaller, PowerNetworkConfigs.SteamNetworkConfig config)
        {
            _consumers = consumers;
            _producers = producers;
            _pipes = pipes;
            _connections = connections;
            _coroutineCaller = coroutineCaller;
            _config = config;
        }

        public void Initialize()
        {
            _coroutineCaller.StartCoroutine(Loop());
        }


        IEnumerator Loop()
        {
            while (true)
            {
                float timeNow = Time.time;
                DoUpdate();
                int flowComponentCount = _flowCacheByComponent.Count;
                Debug.Log($"Steam Flow Calculator Update Complete. {flowComponentCount} flows calculated in {Time.time - timeNow} seconds");
                yield return new WaitForSeconds(Mathf.Max(_config.updateRate - (Time.time - timeNow), 0));
            }
        }

        
        private void DoUpdate()
        {
            HashSet<Cell<ISteamProducer>> producersFound = new HashSet<Cell<ISteamProducer>>();
            HashSet<Cell<ISteamConsumer>> consumersFound = new HashSet<Cell<ISteamConsumer>>();
            foreach (var producer in _producers)
            {
                if (_connections.HasConnection(producer.cell))
                {
                    var connectedPosition = _connections.GetConnectedNode(producer.cell);
                    Debug.Assert(_pipes.HasValue(connectedPosition));
                    var connectedNode = _pipes.GetValue(connectedPosition);
                    var component = _pipes.GetComponentID(connectedNode);
                    var newCell = new Cell<ISteamProducer>((Vector2Int)connectedPosition, producer.value, component, connectedNode);
                    producersFound.Add(newCell);
                }
            }
            foreach (var consumer in _consumers)
            {
                if (_connections.HasConnection(consumer.cell))
                {
                    var connectedPosition = _connections.GetConnectedNode(consumer.cell);
                    Debug.Assert(_pipes.HasValue(connectedPosition));
                    var connectedNode = _pipes.GetValue(connectedPosition);
                    var component = _pipes.GetComponentID(connectedNode);
                    var newCell = new Cell<ISteamConsumer>((Vector2Int)connectedPosition, consumer.value, component, connectedNode);
                    consumersFound.Add(newCell);
                }
            }
            
            Dictionary<int, List<Cell<ISteamProducer>>> producersByComponent = new Dictionary<int, List<Cell<ISteamProducer>>>();
            Dictionary<int, List<Cell<ISteamConsumer>>> consumersByComponent = new Dictionary<int, List<Cell<ISteamConsumer>>>();
            foreach (var producer in producersFound)
            {
                if (!producersByComponent.ContainsKey(producer.ComponentID))
                {
                    producersByComponent.Add(producer.ComponentID, new List<Cell<ISteamProducer>>());
                }
                producersByComponent[producer.ComponentID].Add(producer);
            }
            foreach (var consumer in consumersFound)
            {
                if (!consumersByComponent.ContainsKey(consumer.ComponentID))
                {
                    consumersByComponent.Add(consumer.ComponentID, new List<Cell<ISteamConsumer>>());
                }
                consumersByComponent[consumer.ComponentID].Add(consumer);
            }
            foreach (var producers in producersByComponent)
            {
                //DO NOT NEED TO UPDATE IF COMPONENT IS NOT DIRTY
                if (!_pipes.IsComponentDirty(producers.Key))
                    continue;
                
                if (!consumersByComponent.ContainsKey(producers.Key))
                    continue;
                
                //HAS FLOWS
                if (!_flowCacheByComponent.TryGetValue(producers.Key, out var flows))
                {
                    flows = new List<Flow>();
                    _flowCacheByComponent.Add(producers.Key, flows);
                }
                flows.Clear();
                CreateFlow(producers.Value, consumersByComponent[producers.Key]);
            }
            _pipes.ClearDirtyComponents();
        }

        private void CreateFlow(List<Cell<ISteamProducer>> producersValue, List<Cell<ISteamConsumer>> steamConsumers)
        {
            Debug.Log($"Creating {producersValue.Count * steamConsumers.Count} flows for {producersValue.Count} producers and {steamConsumers.Count} consumers");
            foreach (var steamProducer in producersValue)
            {
                if (!_flowCache.TryGetValue(steamProducer, out var flows))
                {
                    flows = new Dictionary<Cell<ISteamConsumer>, Flow>();
                    _flowCache.Add(steamProducer, flows);
                }
                foreach (var steamConsumer in steamConsumers)
                {
                    if (!flows.ContainsKey(steamConsumer))
                    {
                        try
                        {
                            var path = _pipes.GetPath(steamProducer.RootPosition2D, steamConsumer.RootPosition2D);
                            var flow = new Flow(steamProducer, steamConsumer, path);
                            flows.Add(steamConsumer, flow);
                            if(_flowCacheByComponent.TryGetValue(steamProducer.ComponentID, out var flowsByComponent))
                                flowsByComponent.Add(flow);
                            Debug.Log($"Created flow! {flow}");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                            continue;
                        }
                        //flows.Add(steamConsumer, new Flow(steamProducer, steamConsumer));
                    }
                }
            }
        }

        struct Cell<T> : IEquatable<Cell<T>>
        {
            public Vector2Int cell { get; }
            public T Value { get; }
            public int ComponentID { get; }
            public Vector3Int RootPosition { get; }
            public Vector2Int RootPosition2D => (Vector2Int)RootPosition; 

            public Cell(Vector2Int cell, T value, int componentID, Vector3Int rootPosition)
            {
               this. cell = cell;
                Value = value;
                ComponentID = componentID;
                RootPosition = rootPosition;
            }

            public bool Equals(Cell<T> other)
            {
                return cell.Equals(other.cell);
            }

            public override bool Equals(object obj)
            {
                return obj is Cell<T> other && Equals(other);
            }

            public override int GetHashCode()
            {
                return cell.GetHashCode();
            }

            public override string ToString()
            {
                return $"Cell Type {typeof(T).Name} Cell:{cell}\n Component:{ComponentID} StartAt: {RootPosition}";
            }
        }
        private struct Flow
        {
            public Cell<ISteamProducer> ProducerCell { get; }
            public Cell<ISteamConsumer> ConsumerCell { get; }

            public List<Vector2Int> Path { get; set; }

            public Flow(Cell<ISteamProducer> producerCell, Cell<ISteamConsumer> consumerCell, List<Vector2Int> path)
            {
                ProducerCell = producerCell;
                ConsumerCell = consumerCell;
                Path = path;
            }
            
        }
    }
}