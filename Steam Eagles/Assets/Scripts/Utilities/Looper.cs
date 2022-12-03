using System;
using System.Linq;
using DG.DemiEditor;
using UnityEngine;

namespace Utilities
{
    public class Looper : MonoBehaviour
    {
        public RectInt loopBounds = new RectInt(10, 10, 100, 100);
        
        public Transform[] targets;

        public Vector3 TeleportPosition
        {
            set
            {
                
            }
        }
        private void Update()
        {
            if (transform.position.x < loopBounds.xMin)
            {
                TeleportPosition = new Vector3(loopBounds.xMax, transform.position.y, transform.position.z);
            }
            else if (transform.position.x > loopBounds.xMax)
            {
                TeleportPosition = new Vector3(loopBounds.xMin, transform.position.y, transform.position.z);
            }

            if (transform.position.y < loopBounds.yMin)
            {
                TeleportPosition = new Vector3(transform.position.x, loopBounds.yMax, transform.position.z);
            }
            else if (transform.position.y > loopBounds.yMax)
            {
                TeleportPosition = new Vector3(transform.position.x, loopBounds.yMin, transform.position.z);
            }
        }

        private void Move(Vector3 move)
        {
            foreach (var target in targets.Where(t => t != null))
            {
                var moveOffset = move - transform.position;
                
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow.SetAlpha(0.5f);
            Gizmos.DrawWireCube(loopBounds.center, new Vector3(loopBounds.size.x, loopBounds.size.y, 0));
        }
    }
}