using System;
using UnityEngine;

namespace PhysicsFun
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Balloon : MonoBehaviour
    {
        private Rigidbody2D _rb;
        public Rigidbody2D rb => _rb ? _rb : _rb = GetComponent<Rigidbody2D>();


        public Atmosphere atmosphere;


        private void OnDrawGizmos()
        {
            if (atmosphere)
            {
                atmosphere.DrawAtmosphere();
            }
        }
    }
}