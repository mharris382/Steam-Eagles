﻿using Unity.Jobs;
using UnityEngine;

[ExecuteAlways]
public class Rope : MonoBehaviour
{


    public RopeData ropeData;

    public Transform anchor1, anchor2;

    public LineRenderer lineRenderer;

    public int nodeCount = 10;
    public float nodeSpacing = 0.5f;
    public float gravity = -0.5f;
    public Vector3 windForce = Vector3.zero;
    public float friction = 0.02f;

    public bool drawGizmo = false;

    bool HasResources() => anchor1 != null && anchor2 != null && lineRenderer != null && ropeData != null;

    void Start()
    {
        
       lineRenderer = lineRenderer ? lineRenderer : GetComponent<LineRenderer>();
        ropeData = new RopeData(nodeCount);
        ropeData.SetAnchor(0, true);
        ropeData.SetAnchor(nodeCount - 1, true);
        lineRenderer.positionCount = nodeCount;
    }

    void Update()
    {
        if(!HasResources())
        {
            return;
        }
        MoveRopeEndpoints();    //Main Thread writes external changes into the rope data
        UpdateRope();           //Main Thread performs the physics calculations and writes the results into the rope data
        UpdateLine();           //Main Thread passes the result of the physics calculations to the line renderer
    }


    void MoveRopeEndpoints()
    {
        if (ropeData == null) return;
        
        ropeData.MoveNode(anchor1.position, 0);
        ropeData.MoveNode(anchor2.position, nodeCount - 1);
    }

    void UpdateRope() => ropeData.UpdateNodes(nodeSpacing, gravity, friction, windForce);

    void UpdateLine()
    {
        for (int i = 0; i < nodeCount; i++)
        {
            lineRenderer.SetPosition(i, ropeData.nodes[i].position());
        }
    }
    
    void OnDrawGizmos()
    {
        if (Application.isPlaying && drawGizmo)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < ropeData.nodes.Length - 1; i++)
            {
                Gizmos.DrawLine(ropeData.nodes[i].position(), ropeData.nodes[i + 1].position());
            }
        }
    }

    #region Rope

    public class NativeRopeData
    {
        private RopeData _ropeData;
        private int _nodeCount;
        public NativeRopeData(int nodeCount)
        {
            _nodeCount = nodeCount;
            _ropeData = new RopeData(nodeCount);
        }

        public void RopeUpdate()
        {
            
        }

        public void RopeLateUpdate()
        {
            
        }

        public struct RopeJob : IJob
        {
            public void Execute()
            {
                
            }
        }
    
    }
    public class RopeData
    {
        int nodeCount;
        float nodeSpacing;
        float gravity;
        float friction;
        Vector3 windForce;

        int previousNodeIndex;

        public Node[] nodes;

        public RopeData(int _nodeCount)
        {
            nodeCount = _nodeCount;
            nodes = new Node[nodeCount];
        }

        public void MoveNode(Vector3 _position, int _nodeIndex)
        {
            nodes[_nodeIndex].x = _position.x;
            nodes[_nodeIndex].y = _position.y;
            nodes[_nodeIndex].z = _position.z;
        }

        public void SetAnchor(int _nodeIndex, bool _isAnchor)
        {
            nodes[_nodeIndex].isAnchor = _isAnchor;
        }

        public void UpdateNodes(float _nodeSpacing, float _gravity, float _friction, Vector3 _windForce)
        {
            nodeSpacing = _nodeSpacing;
            gravity = _gravity;
            friction = _friction;
            windForce = _windForce;

            previousNodeIndex = 0;

            for (int i = 0; i < nodeCount; i++)
            {
                if (!nodes[i].isAnchor)
                {
                    ProcessNodes(i);
                }
            }

            //reverse
            previousNodeIndex = nodeCount - 1;

            for (int i = nodeCount - 1; i >= 0; i--)
            {
                if (!nodes[i].isAnchor)
                {
                    ProcessNodes(i);
                }
            }
        }

        void ProcessNodes(int i)
        {
            float px = 0f;
            float py = 0f;
            float pz = 0f;

            nodes[i].x += nodes[i].vx;
            nodes[i].y += nodes[i].vy;
            nodes[i].z += nodes[i].vz;

            //much more understandable, but when using a vector the rope doesn't unfold when origin is at (0,0,0) 
            Vector3 dir =
                new Vector3(nodes[previousNodeIndex].x, nodes[previousNodeIndex].y, nodes[previousNodeIndex].z) -
                new Vector3(nodes[i].x, nodes[i].y, nodes[i].z);
            dir.Normalize();
            px = nodes[i].x + dir.x * nodeSpacing;
            py = nodes[i].y + dir.y * nodeSpacing;
            pz = nodes[i].z + dir.z * nodeSpacing;

            nodes[i].x = nodes[previousNodeIndex].x - (px - nodes[i].x);
            nodes[i].y = nodes[previousNodeIndex].y - (py - nodes[i].y);
            nodes[i].z = nodes[previousNodeIndex].z - (pz - nodes[i].z);

            nodes[i].vx = nodes[i].x - nodes[i].ox;
            nodes[i].vy = nodes[i].y - nodes[i].oy;
            nodes[i].vz = nodes[i].z - nodes[i].oz;

            nodes[i].vx *= friction * (1 - friction);
            nodes[i].vy *= friction * (1 - friction);
            nodes[i].vz *= friction * (1 - friction);

            nodes[i].vx += windForce.x;
            nodes[i].vy += gravity + windForce.y;
            nodes[i].vz += windForce.z;

            nodes[i].ox = nodes[i].x;
            nodes[i].oy = nodes[i].y;
            nodes[i].oz = nodes[i].z;

            previousNodeIndex = i;
        }
    }

    #endregion

    #region Node

    public struct Node
    {
        public float x;
        public float y;
        public float z;

        public float ox;
        public float oy;
        public float oz;

        public float vx;
        public float vy;
        public float vz;

        public bool isAnchor;

        public Vector3 position()
        {
            return new Vector3(x, y, z);
        }
    }

    #endregion

}