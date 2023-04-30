using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using QuikGraph;
using UniRx;
using UnityEngine;

namespace Power.Steam
{
    public static class SteamNetworks
    {
        private static Dictionary<Building, SteamNetwork> _steamNetworks = new Dictionary<Building, SteamNetwork>();

        public static SteamNetwork GetSteamNetwork(this Building building)
        {
            if (_steamNetworks.TryGetValue(building, out var network) == false)
            {
                network = new SteamNetwork(building);
                _steamNetworks.Add(building, network);
            }
            return network;
        }



        public class SteamNetwork : IDisposable,
            INetwork<SteamNode, SteamFlow>,
            INetwork<SteamNode, SteamFlow, ConsumerNode, SupplierNode>
        {
            private readonly Building _building;
            private SteamNetworkController _controller;
            private readonly IDisposable _disposable;
            private readonly Dictionary<Vector3Int, SupplierNode> _steamSuppliers;
            private readonly Dictionary<Vector3Int, ConsumerNode> _steamConsumers;
            private readonly Dictionary<Vector3Int, PipeNode> _steamNodes;

            private readonly AdjacencyGraph<SteamNode, SteamFlow> _steamNetwork;
            private readonly Queue<Vector3Int> _removeConsumerQueue;
            private readonly Queue<Vector3Int> _removeSupplierQueue;
            private readonly Queue<Vector3Int> _removePipeQueue;

            public AdjacencyGraph<SteamNode, SteamFlow> Graph => _steamNetwork;
            
            public IEnumerable<SteamNode> GetSupplierNodes() => _steamSuppliers.Values;
            IEnumerable<ConsumerNode> INetwork<SteamNode, SteamFlow, ConsumerNode, SupplierNode>.GetConsumerNodes() => _steamConsumers.Values;

            IEnumerable<SupplierNode> INetwork<SteamNode, SteamFlow, ConsumerNode, SupplierNode>.GetSupplierNodes() => _steamSuppliers.Values;

            public IEnumerable<SteamNode> GetConsumerNodes() => _steamConsumers.Values;

            public SteamNetwork(Building building)
            {
                _building = building;
                _controller = SteamNetworkController.CreateControllerForBuilding(building);
                _steamNetwork = new AdjacencyGraph<SteamNode, SteamFlow>();
                _steamSuppliers = new Dictionary<Vector3Int, SupplierNode>();
                _steamConsumers = new Dictionary<Vector3Int, ConsumerNode>();
                _removeConsumerQueue = new Queue<Vector3Int>();
                _removeSupplierQueue = new Queue<Vector3Int>();
                _removePipeQueue = new Queue<Vector3Int>();
                
                _steamNodes = new Dictionary<Vector3Int, PipeNode>(building.Map.GetAllNonEmptyCells(BuildingLayers.PIPE)
                    .Select(t => new KeyValuePair<Vector3Int, PipeNode>(t, new PipeNode(t))));
                
                
                Debug.Log($"Found {_steamNodes.Count} pipe nodes initially in {building.name}");

            
                var cd = new CompositeDisposable();
                
                var pipeTilemapChanged = building.TilemapChanged.OnLayer(BuildingLayers.PIPE);
                pipeTilemapChanged.WhereTileWasRemoved().Subscribe(OnPipeTileRemoved).AddTo(cd);
                pipeTilemapChanged.WhereTileWasAdded().Subscribe(OnPipeTileAdded).AddTo(cd);
                
                _disposable = cd;
                _controller.AssignNetwork(this);
                
                
                
                void OnPipeTileRemoved(BuildingTilemapChangedInfo info)
                {
                    if (TryGetSteamNode(info.Cell, out var node))
                    {
                        RemoveNode(node);
                    }
                }
                
                void OnPipeTileAdded(BuildingTilemapChangedInfo info)
                {
                    if (!TryGetSteamNode(info.Cell, out var _))
                    {
                        AddPipeNodeAt(info.Cell);
                    }
                }
            }

          
            
            public void NetworkUpdated()
            {
                RemoveConsumers();
                RemoveSuppliers();
                RemovePipes();
                
                void RemoveSuppliers()
                {
                    while (_removeSupplierQueue.Count > 0)
                    {
                        var supplier = _steamSuppliers[_removeSupplierQueue.Dequeue()];
                        _steamSuppliers.Remove(supplier.Cell);
                    }
                }
                void RemoveConsumers()
                {
                    while (_removeConsumerQueue.Count > 0)
                    {
                        var consumer = _steamConsumers[_removeConsumerQueue.Dequeue()];
                        _steamConsumers.Remove(consumer.Cell);
                    }
                }
                void RemovePipes()
                {
                    while (_removePipeQueue.Count > 0)
                    {
                        var pipe = _steamNodes[_removePipeQueue.Dequeue()];
                        _steamNodes.Remove(pipe.Cell);
                    }
                }
            }


            private void BuildNetworkFrom(SupplierNode root)
            {
                SteamNode current = root;
                foreach (var cell in current.GetNeighbors())
                {
                    if (TryGetSteamNode(cell, out var node))
                    {
                        var edge = new SteamFlow(current, node);
                        _steamNetwork.AddVerticesAndEdge(edge);
                        BuildNetworkRecursively(node, current);
                        return;
                    }
                }
            }

            private void BuildNetworkRecursively(SteamNode node, SteamNode parent)
            {
                foreach (var cell in node.GetNeighbors())
                {
                    if (TryGetSteamNode(cell, out var neighbor) && neighbor != parent &&
                        _steamNetwork.ContainsEdge(node, neighbor) == false)
                    {
                        if (neighbor is SupplierNode)
                            continue;
                        
                        var edge = new SteamFlow(node, neighbor);
                        _steamNetwork.AddVerticesAndEdge(edge);
                        if(neighbor is PipeNode)
                            BuildNetworkRecursively(neighbor, node);
                    }
                }
            }


            private bool TryGetSteamNode(Vector3Int cell, out SteamNode node)
            {
                if (_steamNodes.TryGetValue(cell, out var pipeNode))
                {
                    node = pipeNode;
                    return true;
                }
                if (_steamConsumers.TryGetValue(cell, out var consumerNode))
                {
                    node = consumerNode;
                    return true;
                }
                if (_steamSuppliers.TryGetValue(cell, out var supplierNode))
                {
                    node = supplierNode;
                    return true;
                }
                node = null;
                return false;
            }

            public SupplierNode AddSupplierAt(Vector3Int cell)
            {
                var supplier = new SupplierNode(cell);
                if (_steamSuppliers.ContainsKey(cell))
                {
                    if (_steamSuppliers[cell] == null)
                    {
                        _steamSuppliers.Remove(cell);
                    }
                    else
                    {
                        return _steamSuppliers[cell];
                    }
                }
                _steamSuppliers.Add(cell, supplier);
                BuildNetworkFrom(supplier);
                return supplier;
            }

            public ConsumerNode AddConsumerAt(Vector3Int cell)
            {
                var consumer = new ConsumerNode(cell);
                if (_steamConsumers.ContainsKey(cell))
                {
                    if (_steamConsumers[cell] == null)
                    {
                        _steamConsumers.Remove(cell);
                    }
                    else
                    {
                        return _steamConsumers[cell];
                    }
                }
                _steamConsumers.Add(cell, consumer);
                foreach (var neighbor in consumer.GetNeighbors())
                {
                    if (TryGetSteamNode(neighbor, out var neighborNode))
                    {
                        if (neighborNode is PipeNode)
                        {
                            var edge = new SteamFlow(neighborNode, consumer);
                            _steamNetwork.AddVerticesAndEdge(edge);
                        }
                    }
                }
                return consumer;
            }

            private void AddPipeNodeAt(Vector3Int infoCell)
            {
                var pipeNode = new PipeNode(infoCell);
                _steamNodes.Add(infoCell, pipeNode);
                
                foreach (var neighbor in pipeNode.GetNeighbors())
                {
                    if (IsCellPartOfNetwork(neighbor) && TryGetSteamNode(neighbor, out var neighborNode))
                    {
                        var edge = new SteamFlow(neighborNode, pipeNode);
                        _steamNetwork.AddVerticesAndEdge(edge);
                        BuildNetworkRecursively(pipeNode, neighborNode);
                        return;
                    }
                }
            }

            private bool IsCellPartOfNetwork(Vector3Int neighbor)
            {
                //TODO: check if cell is a vertex in the pipe graph
                if (TryGetSteamNode(neighbor, out var node))
                {
                    return Graph.ContainsVertex(node);
                }
                return false;
            }

            private void RemoveNode(SteamNode steamNode)
            {
                //TODO: CHECK IF WE NEED TO UPDATE NETWORK TOPOLOGY
                if (Graph.ContainsVertex(steamNode))
                {
                    Graph.RemoveVertex(steamNode);
                }
            }

            public void Dispose()
            {
                //TODO: save network state?
                _disposable.Dispose();
            }

            public void RemoveSupplierAt(Vector3Int cell)
            {
                if (_steamSuppliers.TryGetValue(cell, out var supplier))
                {
                    _removeSupplierQueue.Enqueue(cell);
                }
            }
            
            public void RemoveConsumerAt(Vector3Int cell)
            {
                if (_steamConsumers.TryGetValue(cell, out var consumer))
                {
                    _removeConsumerQueue.Enqueue(cell);
                }
            }
            
            public void RemovePipeAt(Vector3Int cell)
            {
                if (_steamNodes.TryGetValue(cell, out var pipeNode))
                {
                    _removePipeQueue.Enqueue(cell);
                }
            }
        }
    }

    public static class TilemapStreamExtensions
    {
        public static IObservable<BuildingTilemapChangedInfo> WhereTileWasRemoved(this IObservable<BuildingTilemapChangedInfo> stream) => stream.Where(t => t.Tile == null);
        public static IObservable<BuildingTilemapChangedInfo> WhereTileWasAdded(this IObservable<BuildingTilemapChangedInfo> stream) => stream.Where(t => t.Tile != null);
        public static IObservable<BuildingTilemapChangedInfo> OnLayer(this IObservable<BuildingTilemapChangedInfo> stream, BuildingLayers layer) => stream.Where(t => t.layer == layer);
    }


    
}