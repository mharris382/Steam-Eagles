using Buildings.BuildingTilemaps;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhysicsFun.Buildings.Graph
{
    public class PipeGraphTester : MonoBehaviour
    {
        [Required]
        public PipeTilemap wireTilemap;

        private PipeGraph _wireGraph;
        private void Start()
        {
            _wireGraph = new PipeGraph(wireTilemap);
        }
    }
}