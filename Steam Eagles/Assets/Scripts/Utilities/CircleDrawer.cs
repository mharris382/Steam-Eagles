using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    [ExecuteAlways]
    [RequireComponent(typeof(LineRenderer))]
    public class CircleDrawer : LineUtility
    {
        [Min(2)]
        public int segments = 16;
        public float radius = 10;
        
        private void Update()
        {
            UpdatePoints(GetCirclePoints());
        }

        IEnumerable<Vector3> GetCirclePoints()
        {
            Vector3 center = transform.position;
            Vector3 right = Vector3.right;
            float degPerSeg = 360f/(segments-1);
            for (int i = 0; i < segments; i++)
            {
                float angle = degPerSeg * i;
                float angleRad = angle * Mathf.Deg2Rad;
                float x = (transform.lossyScale.x * radius) * Mathf.Cos(angleRad);
                float y = (transform.lossyScale.x * radius) * Mathf.Sin(angleRad);
                Vector2 offset = new Vector2(x, y);
                yield return (Vector2)center + offset;
            }
        }
    }
}