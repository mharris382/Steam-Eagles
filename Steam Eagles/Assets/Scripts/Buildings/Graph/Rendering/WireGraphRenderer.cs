using System;
using Buildings.BuildingTilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PhysicsFun.Buildings.Graph.Rendering
{
    public class WireGraphRenderer : MonoBehaviour
    {
        public Camera targetCamera;

        public BuildingTilemap targetTilemap;

        private void Awake()
        {
            var graph = new WireGraph(targetTilemap);
            if (graph.VertexCount == 0)
            {
                Debug.LogError($"Cannot render wires because there are none on {targetTilemap.name}", this);
                enabled = false;
                return;
            }
            
            int count = graph.GetStronglyConnectedComponents(out var components);
            Debug.Log($"Found {count} strongly connected components in {targetTilemap.name}",this);
        }
    }
}