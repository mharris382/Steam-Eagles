using System;
using Buildings.BuildingTilemaps;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhysicsFun.Buildings.Graph
{
    public class WireGraphTester : MonoBehaviour
    {
        [Required]
        public WireTilemap wireTilemap;

        private WireGraph _wireGraph;
        private void Start()
        {
            _wireGraph = new WireGraph(wireTilemap);
        }
    }
}