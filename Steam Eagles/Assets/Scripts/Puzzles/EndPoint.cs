using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Puzzles
{
    public class EndPoint : CellHelper
    {
        public static Dictionary<Vector3Int, EndPoint> registeredEndpoints = new Dictionary<Vector3Int, EndPoint>();

        private List<StartPoint> connectedStartPoints = new List<StartPoint>();
        private Vector3Int coord;
        
        void Awake()
        {
            onDisconnected?.Invoke();
        }
        private void Start()
        {
            registeredEndpoints.Add(CellCoordinate, this);
            coord = CellCoordinate;
        }

        private void OnDestroy()
        {
            registeredEndpoints.Remove(coord);
        }

        public UnityEvent onConnected;
        public UnityEvent onDisconnected;


        public void NotifyConnectedTo(StartPoint startPoint)
        {
            if (connectedStartPoints.Contains(startPoint))
            {
                return;
            }

            
            connectedStartPoints.Add(startPoint);
            onConnected?.Invoke();
            
        }

        public void NotifyDisconnectedFrom(StartPoint startPoint)
        {
            if (!connectedStartPoints.Contains(startPoint))
            {
                return;
            }

            connectedStartPoints.Remove(startPoint);
            onDisconnected?.Invoke();
        }
        
    }
}