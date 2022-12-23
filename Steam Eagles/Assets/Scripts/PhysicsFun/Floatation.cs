using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsFun
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Floatation : MonoBehaviour
    {
        [SerializeField] private float waterLevel;

        public float floatForce = 1f;
        public float forceFallOff = 1;
        
        public Vector2[] buoyancyPoints;

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        IEnumerable<Vector2> GetBuoyancyPoints()
        {
            if (buoyancyPoints == null || buoyancyPoints.Length == 0) yield return transform.position;
            else
            {
                foreach (var buoyancyPoint in buoyancyPoints)
                {
                    yield return transform.TransformPoint(buoyancyPoint);
                }
            }
        }

        private void FixedUpdate()
        {
            foreach (var buoyancyPoint in GetBuoyancyPoints())
            {
                if(buoyancyPoint.y < waterLevel)
                {
                    var force = floatForce * (waterLevel - buoyancyPoint.y);
                    force = Mathf.Pow(force, forceFallOff);
                    _rb.AddForceAtPosition(Vector2.up * force, buoyancyPoint);
                }
            }
        }

        #region [GIZMOS]

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.Lerp(Color.green, Color.blue, 0.5f);
            void DrawWaterLevel()
            {
                var pos = transform.position;
                pos.y = waterLevel;
                var offset = Vector3.right * 50;
                var p0 = pos - offset;
                var p1 = pos + offset;
                Gizmos.DrawLine(p0, p1);
            }

            void DrawBuoyancyPoints()
            {
                const float r = 0.25f;
                foreach (var buoyancyPoint in GetBuoyancyPoints())
                {
                    Gizmos.DrawWireSphere(buoyancyPoint, r);
                    Gizmos.DrawRay(buoyancyPoint, Vector3.up);
                }
            }
            DrawWaterLevel();
            DrawBuoyancyPoints();
            
        }
#endif

        #endregion
    }
}