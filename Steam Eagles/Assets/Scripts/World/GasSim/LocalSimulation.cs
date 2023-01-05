using System;
using CoreLib;
using UnityEngine;

namespace GasSim
{
    public class LocalSimulation : MonoBehaviour
    {
        
        [SerializeField] private float maxUpdateRate = 0.5f;
        [SerializeField] private Rect simBounds = new Rect(0, 0, 1, 1);
        

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            simBounds.DrawGizmos();
        }
    }

    

    public struct Cell
    {
        public Vector2 Position;
        public float Density;
        public float Pressure;
        public float Temperature;
        public float Velocity;
    }
}