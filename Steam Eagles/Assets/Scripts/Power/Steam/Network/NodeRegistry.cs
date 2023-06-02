using CoreLib;
using UnityEngine;

namespace Power.Steam.Network
{
    public class NodeRegistry : Registry<NodeHandle>
    {
        private readonly GridGraph<NodeHandle> _graph;
        public int nextGUID = 0;

        public int GetNextGUID() => nextGUID++;

        public NodeRegistry(GridGraph<NodeHandle> graph) : base()
        {
            _graph = graph;
        }

        protected override void AddValue(NodeHandle value)
        {
            if (!_graph.AddNode(value.Position))
            {
                Unregister(value);
                Debug.LogError($"Failed to add node at {value.Position}");
                return;
            }
            base.AddValue(value);
        }
        protected override void RemoveValue(NodeHandle value)
        {
            _graph.RemoveNode(value.Position);
            base.RemoveValue(value);
        }
    }
}