using System;
using UnityEngine;

namespace PhysicsFun
{
    public class AtmosphereVisual : MonoBehaviour
    {
        public Atmosphere atmosphere;

        bool HasResources() => atmosphere != null;
        private void Awake()
        {
            if (!HasResources()) return;
        }

        private void OnDrawGizmosSelected()
        {
            if (!HasResources()) return;
            atmosphere.DrawAtmosphere();
        }
    }
}