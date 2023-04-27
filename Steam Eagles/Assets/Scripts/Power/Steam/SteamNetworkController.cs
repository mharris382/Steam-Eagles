using System.Collections.Generic;
using System.Linq;
using QuikGraph.Algorithms;

namespace Power.Steam
{
    public class SteamNetworkController : NetworkController<SteamNetworkController, SteamNode, SteamFlow>
    {
        public override float GetUpdateInterval() => PowerNetworkConfigs.Instance.steamNetworkConfig.updateRate;

        private HashSet<SteamNode> _visitedNodes = new HashSet<SteamNode>();
        public override void UpdateNetwork()
        {
            var suppliers = Network.GetSupplierNodes();
            var graph = Network.Network;
            _visitedNodes.Clear();
            foreach (var steamNode in suppliers)
            {
                TraverseAndTransferSteam(steamNode as INetworkSupplier, steamNode);
            }
        }

        Stack<SteamNode> _stack = new Stack<SteamNode>();
        void TraverseAndTransferSteam(INetworkSupplier supplier, SteamNode startNode)
        {
            var graph = Network.Network;
            float transferAmount = supplier.TryGetSupply(PowerNetworkConfigs.Instance.steamNetworkConfig.maxPressure);
            _stack.Clear();
            //var bfs = graph.TreeBreadthFirstSearch(startNode);
            RecursivelyFindSteamPath(startNode, ref transferAmount);
        }
        
        void RecursivelyFindSteamPath(SteamNode node, ref float remainingAmount)
        {
            if (_visitedNodes.Contains(node)) return;
            _visitedNodes.Add(node);
            
            if (node.AvailableSpace > 0)
            {
                if (node.AvailableSpace > remainingAmount)
                {
                    node.Pressure += remainingAmount;
                    remainingAmount = 0;
                    return;
                }
                node.Pressure = node.Capacity;
                remainingAmount -= node.AvailableSpace;
            }
            
            var graph = Network.Network;
            
            if (graph.TryGetOutEdges(node, out var edges))
            {
                foreach (var e in edges)
                {
                    if(_visitedNodes.Contains(e.Target))continue;//need this to prevent infinite loops in case of cycles
                    var target = e.Target;
                    var space = target.AvailableSpace;
                    if (space > remainingAmount)
                    {
                        target.Pressure += remainingAmount;
                        remainingAmount = 0;
                        return;
                    }
                    target.Pressure = target.Capacity;
                    remainingAmount -= space;
                    _stack.Push(target);
                }
            }

            while (_stack.Count > 0)
            {
                RecursivelyFindSteamPath(_stack.Pop(), ref remainingAmount);
            }
        }
        
        
    }
}