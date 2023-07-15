using Sirenix.OdinInspector;
using UnityEngine;

namespace Buildings.Graph
{
    public class GraphInfo : MonoBehaviour
    {
        private WireTilemapGraph _wireTilemapGraph;
        private PipeTilemapGraph _pipeTilemapGraph;
        
        
        public bool HasResources => _wireTilemapGraph != null && _pipeTilemapGraph != null;



       [ShowInInspector, ReadOnly, BoxGroup("Debugging/Pipes")]  public int PipeNodeCount
        {
            get => HasResources ? _pipeTilemapGraph.NodeCount : -1;
        }
        [ShowInInspector, ReadOnly, BoxGroup("Debugging/Pipes")] public int PipeEdgeCount
        {
            get => HasResources ? _pipeTilemapGraph.EdgeCount : -1;
        }
       [ShowInInspector, ReadOnly, BoxGroup("Debugging/Wires")]  public int WireNodeCount
        {
            get => HasResources ? _wireTilemapGraph.NodeCount : -1;
        }
       [ShowInInspector, ReadOnly, BoxGroup("Debugging/Wires")]  public int WireEdgeCount
        {
            get => HasResources ? _wireTilemapGraph.EdgeCount : -1;
        }

        public void Install(PipeTilemapGraph pipeTilemapGraph, WireTilemapGraph wireTilemapGraph)
        {
            _wireTilemapGraph = wireTilemapGraph;
            _pipeTilemapGraph = pipeTilemapGraph;
        }
    }
}