using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Experimental
{
    public class TestGearNetwork : MonoBehaviour
    {
        public Gear testStartGear;

        private GearGraph _graph;
        
        [Button]
        public void TestBuildGraph()
        {
            if (testStartGear == null) return;
            _graph= new GearGraph(testStartGear);
               
        }
        [Button]
        public void LogGearGraphInfo()
        {
            if (_graph == null) return;
            Debug.Log($"Graph has {_graph.GearNetwork.VertexCount} gears");
        }
        

        public void OnDrawGizmos()
        {
            if (_graph == null) return;
            _graph.DrawGizmos();
            
        }
    }
}