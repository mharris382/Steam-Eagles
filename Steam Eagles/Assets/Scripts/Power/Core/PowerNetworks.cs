using System.Collections;
using System.Collections.Generic;
using CoreLib;
using QuikGraph;

namespace Power
{
    public class PowerNetworks<TNode, TEdge> : Registry<TNode>
        where TNode : IPowerNetworkNode
        where TEdge : IPowerNetworkEdge, IEdge<TNode>
    {
        private AdjacencyGraph<TNode, TEdge> _adjacencyGraph = new AdjacencyGraph<TNode, TEdge>();
        public AdjacencyGraph<TNode, TEdge> AdjacencyGraph => _adjacencyGraph;

        protected override void AddValue(TNode value)
        {
            _adjacencyGraph.RemoveVertex(value);
        }

        protected override void RemoveValue(TNode value)
        {
            _adjacencyGraph.RemoveVertex(value);
        }
    }
}