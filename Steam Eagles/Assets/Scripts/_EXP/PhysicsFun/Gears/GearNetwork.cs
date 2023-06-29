using System.Collections.Generic;
using CoreLib;
using QuikGraph;
using UnityEngine;

namespace Experimental
{
    public class GearNetwork : AdjacencyGraph<IGear2D, GearConnection> { }
    
    

    public class GearGraph
    {
        public GearNetwork GearNetwork { get; }

        public void DrawGizmos()
        {
            Color gearColor = Color.red.SetAlpha(0.5f);
            Color connectionColor = Color.blue.SetAlpha(0.5f);
            
            Gizmos.color = gearColor;
            foreach (var gear in GearNetwork.Vertices)
            {
                Gizmos.DrawSphere(gear.Center, gear.Radius);
            }

            Gizmos.color = connectionColor;
            foreach (var gearNetworkEdge in GearNetwork.Edges)
            {
                var srcPoint = gearNetworkEdge.Source.Center;
                var srcRadius = gearNetworkEdge.Source.Radius;
                var dstPoint = gearNetworkEdge.Target.Center;
                var dstRadius = gearNetworkEdge.Target.Radius;
                var centerPoint = (srcPoint + dstPoint) / 2;
                var srcDir = (dstPoint - srcPoint).normalized;
                var dstDir = (srcPoint - dstPoint).normalized;
                const float drawRadius = 0.1f;
                var drawDst = centerPoint + srcDir * drawRadius * 2;
                var drawSrc = centerPoint + dstDir * drawRadius * 2;
                Gizmos.DrawWireSphere(drawDst, drawRadius);
                Gizmos.DrawSphere(drawSrc, drawRadius);
                Gizmos.DrawLine(gearNetworkEdge.Source.Center, gearNetworkEdge.Target.Center);                
            }
        }
        
        public GearGraph(IGear2D startGear)
        {
            GearNetwork = CreateGearGraph(startGear);
        }

        public GearNetwork CreateGearGraph(IGear2D startGear2D)
        {
            GearConnection GetGearConnection(IGear2D gear1, IGear2D gear2) => 
                new GearConnection(gear1, gear2, gear2.IsAxelConnection);
            
            var gearNetwork = new GearNetwork();
            var layerMask = LayerMask.GetMask("Gears");
            var colls = new Collider2D[16];
            var unsearchedGears = new Stack<IGear2D>();
            
            unsearchedGears.Push(startGear2D);
            
            //each time find a gear that is nearby to another gear, add to the list of possible connections for that gear
            Dictionary<IGear2D, List<IGear2D>> possibleConnections = new Dictionary<IGear2D, List<IGear2D>>();
            
            

            //first find all gears in network 
            while (unsearchedGears.Count > 0)
            {
                //get next gear to search
                var current = unsearchedGears.Pop();
                //add to network indicating that it has been searched
                gearNetwork.AddVertex(current);
                
                possibleConnections.Add(current, new List<IGear2D>());
                
                //find all gear colliders that are nearby
                int cnt = Physics2D.OverlapCircleNonAlloc(current.Center, current.Radius + 0.1f, colls);
                
                for (int i = 0; i < cnt; i++)
                {
                    //check if it is a gear
                    var gear = colls[i].GetComponent<IGear2D>();
                    
                    Debug.Assert(gear != null, "Collider on gear layer without gear component!", gear.Rb);
                    if (gear == null) continue;
                    
                    //check if it is already in the network
                    if (gearNetwork.ContainsVertex(gear)) continue;
                    
                    
                    if(!possibleConnections[current].Contains(gear))
                        possibleConnections[current].Add(gear);
                    
                    unsearchedGears.Push(gear);
                }
            }
            Debug.Log($"Built Graph: {gearNetwork.VertexCount} vertices, {gearNetwork.EdgeCount} edges");
            return gearNetwork;
        }
    }

    public struct GearConnection : IEdge<IGear2D>
    {
        public IGear2D driver;
        public IGear2D driven;
        public bool axelConnection;
        
        public GearConnection(IGear2D driver, IGear2D driven, bool axelConnection)
        {
            this.driver = driver;
            this.driven = driven;
            this.axelConnection = axelConnection;
        }
        
        public float GetRatio() => axelConnection ? driver.TeethCount : driver.TeethCount / (float)driven.TeethCount;

        public IGear2D Source => driver;
        public IGear2D Target => driven;
    }

    
    public interface IGear2D : IRotationalMechanism2D
    {
        public bool IsAxelConnection { get; }
        
        public int TeethCount { get;  }
        
        public float Radius { get; }
    }
    

    public interface IRotationalMechanism2D
    {
        public Rigidbody2D Rb { get; }
        
        public Vector3 Center { get; }
        
        public float Rotation { get; set; }
        
        public float AngularVelocity { get; set; }
    }
    
}