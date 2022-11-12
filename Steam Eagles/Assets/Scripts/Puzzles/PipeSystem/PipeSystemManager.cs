using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Puzzles.PipeSystem
{
    public class PipeSystemManager : MonoBehaviour
    {
        
        
    }


    public class GasFlow
    {
        public StartPoint startPoint;
        public EndPoint endPoint;
    }

    public class HyperGasReactor : MonoBehaviour
    {
        
        private List<StartPoint> _startPoints;
        private EndPoint _endPoint;

        private void Awake()
        {
            _startPoints = GetComponentsInChildren<StartPoint>().ToList();
            
            Debug.Assert(_startPoints.Count == 2, "HyperGas Reactor must have exactly 2 start points");
            
            _endPoint = GetComponentInChildren<EndPoint>();
            Debug.Assert(_endPoint!=null, "HyperGas Reactor must have exactly 1 end point");
        }
    }
}