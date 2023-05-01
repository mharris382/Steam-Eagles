using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.MaximumFlow;
using UnityEngine;

namespace Power.Steam
{
    public class SteamNetworkController : NetworkController<SteamNetworkController, SteamNode, SteamFlow>
    {
        public override float GetUpdateInterval() => PowerNetworkConfigs.Instance.steamNetworkConfig.updateRate;

        private HashSet<SteamNode> _visitedNodes = new HashSet<SteamNode>();
        private Dictionary<SteamNode, int> components;

        private Dictionary<SteamNode, TryFunc<SteamNode, IEnumerable<SteamFlow>>> _bfsTrees =
            new Dictionary<SteamNode, TryFunc<SteamNode, IEnumerable<SteamFlow>>>();
        
        FlowFound[] flows;
        
        private bool IsNetworkDirty()
        {
            //TODO: implement this so that rebuilding the network is not necessary every time we update
            return true;
        }
        public override IEnumerator UpdateNetwork()
        {
            var graph = SteamNetwork.Graph;
            var concreteNetwork = (INetwork<SteamNode, SteamFlow, ConsumerNode, SupplierNode>) SteamNetwork;
            yield break;
            //TODO: improve this to also consider which parts of the graph have changed so that we only need to update certain components
            if (IsNetworkDirty())
            {
                //find strongly connected components
                components = new Dictionary<SteamNode, int>();
                _bfsTrees.Clear();
                int componentCount = graph.StronglyConnectedComponents(components);
                flows = new FlowFound[componentCount];
                for (int i = 0; i < componentCount; i++) flows[0].Component = i;
                
                //find flows within components
                foreach (var consumer in concreteNetwork.GetConsumerNodes())
                {
                    // var component = components[consumer];
                    // flows[component].AddSink(consumer);
                    _bfsTrees.Add(consumer, graph.TreeBreadthFirstSearch(consumer));
                }
                foreach (var supplier in concreteNetwork.GetSupplierNodes())
                {
                    // var component = components[supplier];
                    // flows[component].AddSource(supplier);
                    _bfsTrees.Add(supplier, graph.TreeBreadthFirstSearch(supplier));
                }
            }
            
            foreach (var flow in flows)
            {
                if(flow.IsFlowValid())
                    DoFlow(flow, graph);
            }

            _visitedNodes.Clear();
            foreach (var supplierNode in concreteNetwork.GetSupplierNodes())
            {
                float targetSupply = supplierNode.ProductionTarget;
                if (_bfsTrees[supplierNode](supplierNode, out var path))
                {
                    foreach (var steamFlow in path)
                    {
                        if(targetSupply <= 0)
                            break;
                        var space = steamFlow.Target.AvailableSpace;
                        if(_visitedNodes.Contains(steamFlow.Target))
                            continue;
                        _visitedNodes.Add(steamFlow.Target);
                        if (space > targetSupply)
                        {
                            targetSupply -= space;
                            steamFlow.Target.Pressure += space;
                        }
                        else
                        {
                            steamFlow.Target.Pressure += targetSupply;
                            targetSupply = 0;
                        }
                    }
                }
            }
            _visitedNodes.Clear();
            foreach (var consumerNode in concreteNetwork.GetConsumerNodes())
            {
                if(_visitedNodes.Contains(consumerNode))
                    continue;
                _visitedNodes.Add(consumerNode);
                float targetConsumption = consumerNode.ConsumptionTarget;
                if (_bfsTrees[consumerNode](consumerNode, out var path))
                {
                    if(targetConsumption <= 0)
                        continue;
                    if(_visitedNodes.Contains(consumerNode))
                        continue;
                    _visitedNodes.Add(consumerNode);
                    foreach (var flow in path)
                    {
                        if(targetConsumption <= 0)
                            break;
                        if (flow.Source == consumerNode)
                        {
                            var srcPressure = flow.Target.Pressure;
                            if (srcPressure > targetConsumption)
                            {
                                targetConsumption -= srcPressure;
                                flow.Source.Pressure -= srcPressure;
                            }
                            else
                            {
                                flow.Source.Pressure -= targetConsumption;
                                targetConsumption = 0;
                            }
                        }
                    }
                }
            }
            yield return null;
        }
        
        private struct FlowFound
        {
            public int Component { get; set; }
            public SteamNode Sink {
                get; 
                private set;
                
            }

            public SteamNode Source
            {
                get;
                private set;
            }
            
            public bool IsFlowValid() => Sink != null && Source != null;
            
            public void AddSink(ConsumerNode sink)
            {
                if (Sink == null)
                {
                    Sink = sink;
                }
                else
                {
                    Debug.LogError("Sink already set");
                }
            }

            public void AddSource(SupplierNode source)
            {
                if(Source == null)
                {
                    Source = source;
                }
                else
                {
                    Debug.LogError("Source already set");
                }
            }
        }

        
        private void DoFlow(FlowFound flow, AdjacencyGraph<SteamNode, SteamFlow> adjacencyGraph)
        {
            SteamFlow CreateFlow(SteamNode from, SteamNode to)
            {
                return new SteamFlow(from, to);
            }
            var sink = flow.Sink;
            var src = flow.Source;
            double EdgeCapacities(SteamFlow edge)
            {
                var p1 = edge.Source.Pressure;
                var availableSpace = edge.Target.AvailableSpace;
                return Mathf.Min(p1, availableSpace);
            }
            var result = adjacencyGraph.MaximumFlow(EdgeCapacities, src, sink,
                out var flowPredecessors,
                CreateFlow,
                new ReversedEdgeAugmentorAlgorithm<SteamNode, SteamFlow>(new AdjacencyGraph<SteamNode, SteamFlow>(false), CreateFlow));
            
        }
        private void PushGasAwayFromSuppliers(INetwork<SteamNode, SteamFlow, ConsumerNode, SupplierNode> concreteNetwork, AdjacencyGraph<SteamNode, SteamFlow> graph)
        {
            foreach (var steamNode in concreteNetwork.GetSupplierNodes())
            {
                float supplierTarget = steamNode.ProductionTarget;
                PushSteamAwayFromSupplier(graph, steamNode, supplierTarget);
            }
        }

        private static void PushSteamAwayFromSupplier(AdjacencyGraph<SteamNode, SteamFlow> graph, SupplierNode steamNode, float supplierTarget)
        {
            if (graph.TreeBreadthFirstSearch(steamNode)(steamNode, out var result))
            {
                foreach (var steamFlow in result)
                {
                    if (supplierTarget <= 0) break;
                    throw new NotImplementedException();
                }
            }
        }

        private static void PushGasTowardsConsumers(INetwork<SteamNode, SteamFlow, ConsumerNode, SupplierNode> concreteNetwork, AdjacencyGraph<SteamNode, SteamFlow> graph)
        {
            foreach (var consumerNode in concreteNetwork.GetConsumerNodes())
            {
                float consumptionTarget = consumerNode.ConsumptionTarget;
                PushSteamTowardsConsumer(graph, consumerNode, consumptionTarget);
            }
        }

        private static void PushSteamTowardsConsumer(AdjacencyGraph<SteamNode, SteamFlow> graph, ConsumerNode consumerNode, float consumptionTarget)
        {
            if (graph.TreeBreadthFirstSearch(consumerNode)(consumerNode, out var result))
            {
                foreach (var steamFlow in result)
                {
                    if (consumptionTarget <= 0) break;
                    throw new NotImplementedException();
                }
            }
        }
    }
}