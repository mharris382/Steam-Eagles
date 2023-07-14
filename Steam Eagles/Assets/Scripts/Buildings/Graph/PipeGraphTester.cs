using Buildings;
using Buildings.BuildingTilemaps;
using Buildings.Graph;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR

using UnityEditor;
#endif
namespace PhysicsFun.Buildings.Graph
{
    public class PipeGraphTester : MonoBehaviour
    {
        private PipeTilemapGraph PipeTilemapGraph;
        private WireTilemapGraph WireTilemapGraph;


        [ShowInInspector, HideIf("@pipeDebugInfo==null")]
        public GraphDebugInfo pipeDebugInfo;
        [ShowInInspector, HideIf("@wireDebugInfo==null")]
        public GraphDebugInfo wireDebugInfo;
        
        

        [DisableInEditorMode]
        [Button, EnableIf("@PipeTilemapGraph != null && pipeDebugInfo==null")]
        void OpenPipeDebugWindow()
        {
            pipeDebugInfo = new GraphDebugInfo("Pipe", PipeTilemapGraph);
            wireDebugInfo = null;
        }
        [DisableInEditorMode]
        [Button, EnableIf("@PipeTilemapGraph != null && wireDebugInfo==null")]
        void OpenWireDebugWindow()
        {
            pipeDebugInfo = null;
            wireDebugInfo = new GraphDebugInfo("Pipe", WireTilemapGraph);
        }


        private BuildingPowerGrid _powerGrid;
    }



    public class GraphDebugInfo
    {
        [ShowInInspector, HideLabel, DisplayAsString]
        public string GraphName =>  _graph == null ? ($"{_name} (NULL)") : _graph.IsDisposed ? $"{_name} (DISPOSED)" : _name;


        private readonly string _name;
        private readonly BuildingTilemapGraph _graph;

        [ShowInInspector, HorizontalGroup("h1", LabelWidth = 50), LabelText("Nodes")]
        public int NodeCount => _graph.NodeCount;
        [ShowInInspector, HorizontalGroup("h1", LabelWidth = 50), LabelText("Nodes")]
        public int EdgeCount => _graph.EdgeCount;
        
        
        public GraphDebugInfo(string graphName, BuildingTilemapGraph graph)
        {
            _name = graphName;
            _graph = graph;
        }
    }
}