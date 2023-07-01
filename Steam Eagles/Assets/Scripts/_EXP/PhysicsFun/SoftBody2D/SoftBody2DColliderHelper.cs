using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhysicsFun.SoftBody2D
{
    public class SoftBody2DColliderHelper : MonoBehaviour
    {
        [OnValueChanged(nameof(OnValueChanged))]
        public float radius = 0.5f;
        [SerializeField] private Density density;

        

        class SoftBodyHelperProperty {
            public virtual void Update(SoftBody2DCollider collider) { }
        }
        
        [Toggle("enable")]
        [System.Serializable]
        class Density : SoftBodyHelperProperty
        {
            public bool enable;
            public bool autoMass = true;
            
            [ShowIf(nameof(autoMass))]
            public float density = 1;

            [HideIf(nameof(autoMass))]
            public float mass = 1;
            public override void Update(SoftBody2DCollider collider)
            {
                if (enable)
                {
                    collider.rigidbody.useAutoMass = autoMass;
                    if (autoMass)
                    {
                        collider.circleCollider2D.density = density;    
                    }
                    else
                    {
                        collider.rigidbody.mass = mass;
                    }
                }
            }
        }

        IEnumerable<SoftBodyHelperProperty> GetHelperProps()
        {
            yield return density;
        }

        void OnValueChanged(float r)
        {
            var colls = GetComponentsInChildren<SoftBody2DCollider>();
            foreach (var softBody2DCollider in colls)
            {
                softBody2DCollider.circleCollider2D.radius = r;
                foreach (var softBodyHelperProperty in GetHelperProps())
                {
                    softBodyHelperProperty.Update(softBody2DCollider);
                }
            }
        }
    }
}