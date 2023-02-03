using System;
using Unity.Collections;
using UnityEngine;

namespace PhysicsFun
{
    public class DistanceJointLineRenderer : MonoBehaviour
    {
        
        public LineRenderer lineRenderer;
        public DistanceJoint2D distanceJoint2D;

        private NativeArray<Vector3> _positions;

        private void Awake()
        {
            _positions = new NativeArray<Vector3>(2, Allocator.Persistent);
        }

        private void OnDestroy()
        {
            _positions.Dispose();
        }

        bool HasResources()
        {
            return lineRenderer != null && distanceJoint2D != null;
        }

        private void Update()
        {
            if (!HasResources()) return;
            var anchor1 =distanceJoint2D.anchor;
            var anchor2 = distanceJoint2D.connectedAnchor;
            _positions[0] = distanceJoint2D.transform.TransformPoint(anchor1);
            _positions[1] = distanceJoint2D.connectedBody.transform.TransformPoint(anchor2);
            lineRenderer.SetPositions(_positions);
        }
    }
    
    [RequireComponent (typeof(LineRenderer))]
    public class Ellipse : MonoBehaviour {
 
        public float a = 5;
        public float b = 3;
        public float h = 1;
        public float k = 1;
        public float theta = 45;
        public int resolution = 1000;
 
        private Vector3[] positions;
     
        void Start () {
            positions = CreateEllipse(a,b,h,k,theta,resolution);
            LineRenderer lr = GetComponent<LineRenderer>();
            lr.SetVertexCount (resolution+1);
            for (int i = 0; i <= resolution; i++) {
                lr.SetPosition(i, positions[i]);
            }
        }
 
        Vector3[] CreateEllipse(float a, float b, float h, float k, float theta, int resolution) {
 
            positions = new Vector3[resolution+1];
            Quaternion q = Quaternion.AngleAxis (theta, Vector3.forward);
            Vector3 center = new Vector3(h,k,0.0f);
 
            for (int i = 0; i <= resolution; i++) {
                float angle = (float)i / (float)resolution * 2.0f * Mathf.PI;
                positions[i] = new Vector3(a * Mathf.Cos (angle), b * Mathf.Sin (angle), 0.0f);
                positions[i] = q * positions[i] + center;
            }
 
            return positions;
        }
    }
}