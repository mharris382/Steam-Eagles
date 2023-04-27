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



        public class SteamNetwork : IDisposable, INetwork<SteamNode, SteamFlow>
        {
            private readonly Building _building;
            private SteamNetworkController _controller;
            private readonly IDisposable _disposable;
            private readonly Dictionary<Vector3Int, SupplierNode> _supplierNodes;
            private readonly Dictionary<Vector3Int, ConsumerNode> _steamConsumers;
            private readonly Dictionary<Vector3Int, PipeNode> _steamNodes;

            private readonly AdjacencyGraph<SteamNode, SteamFlow> _steamNetwork;

            public AdjacencyGraph<SteamNode, SteamFlow> Network => _steamNetwork;
            
            public IEnumerable<SteamNode> GetSupplierNodes() => _supplierNodes.Values;
            public IEnumerable<SteamNode> GetConsumerNodes() => _steamConsumers.Values;

            public SteamNetwork(Building building)
            {
                _building = building;
                _controller = SteamNetworkController.CreateControllerForBuilding(building);
                _steamNetwork = new AdjacencyGraph<SteamNode, SteamFlow>();
                _supplierNodes = new Dictionary<Vector3Int, SupplierNode>();
                _steamConsumers = new Dictionary<Vector3Int, ConsumerNode>();
                _steamNodes = new Dictionary<Vector3Int, PipeNode>(building.Map
                    .GetAllNonEmptyCells(BuildingLayers.PIPE).Select(t => new PipeNode(t))
                    .Select(t => new KeyValuePair<Vector3Int, PipeNode>(t.Cell, t)));
                Debug.Log($"Found {_steamNodes.Count} pipe nodes initially in {building.name}");

            
                var cd = new CompositeDisposable();
                building.TilemapChanged.Where(t => t.layer == BuildingLayers.PIPE).Subscribe(info =>
                {
                    if (info.Tile == null && _steamNodes.ContainsKey(info.Cell)) //pipe was removed
                    {
                        RemovePipeNode(_steamNodes[info.Cell]);
                    }
                    else if (info.Tile != null && !_steamNodes.ContainsKey(info.Cell))
                    {
                        AddPipeNodeAt(info.Cell);
                    }
                }).AddTo(cd);
                _disposable = cd;
                _controller.AssignNetwork(this);
            }


            public SupplierNode AddSupplierAt(Vector3Int cell)
            {
                var supplier = new SupplierNode(cell);
                if (_supplierNodes.ContainsKey(cell))
                {
                    if (_supplierNodes[cell] == null)
                    {
                        _supplierNodes.Remove(cell);
                    }
                    else
                    {
                        return _supplierNodes[cell];
                    }
                }
                _supplierNodes.Add(cell, supplier);
                BuildNetworkFrom(supplier);
                return supplier;
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
                if (_supplierNodes.TryGetValue(cell, out var supplierNode))
                {
                    node = supplierNode;
                    return true;
                }
                node = null;
                return false;
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
                            _steamNetwork.AddEdge(edge);
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
                    return Network.ContainsVertex(node);
                }
                return false;
            }

            private void RemovePipeNode(SteamNode steamNode)
            {
                //TODO: CHECK IF WE NEED TO UPDATE NETWORK TOPOLOGY
                if (Network.ContainsVertex(steamNode))
                {
                    Network.RemoveVertex(steamNode);
                }
            }

            public void Dispose()
            {
                //TODO: save network state?
                _disposable.Dispose();
            }
        }
    }
    
    


    
}