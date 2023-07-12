using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.VFX;

namespace _EXP.PhysicsFun.ComputeFluid
{
    [RequireComponent(typeof(VisualEffect))]
    public class StovetopEffect : MonoBehaviour
    {
        public List<CollisionDetector> colliders;

        public VisualEffect effect;
        [Serializable]
        public class CollisionDetector
        {
            [Required, SceneObjectsOnly] public Collider2D target;
            public Transform debugSphere;
            public string boolHitDetectedProperty;
            public string hitPositionProperty;
            public string hitRadiusProperty;

            private bool useTrigger;
            private Predicate<Collider2D> triggerFilter;
            private Func<Collider2D, (Vector3, float)> triggerPositionGetter;
            private Predicate<Collision2D> collisionFilter;
            private Func<Collision2D, (Vector3, float)> collisionPositionGetter;
            List<Collider2D> colliders = new List<Collider2D>();
            private VisualEffect _effect;
            public IDisposable Setup(VisualEffect visualEffect, Predicate<Collision2D> collisionFilter, Func<Collision2D,(Vector3, float)> positionGetter)
            {
                CompositeDisposable cd = new();
                _effect = visualEffect;
                this.collisionFilter = collisionFilter;
                this.collisionPositionGetter = positionGetter;
                useTrigger = false;
                return cd;
            }
            public IDisposable Setup(VisualEffect visualEffect, Predicate<Collider2D> collisionFilter, Func<Collider2D, (Vector3, float)> positionGetter)
            {
                CompositeDisposable cd = new();
                _effect = visualEffect;
                this.triggerFilter = collisionFilter;
                this.triggerPositionGetter = positionGetter;
                useTrigger = true;
                target.OnTriggerEnter2DAsObservable().Subscribe(OnAdd).AddTo(cd);
                target.OnTriggerExit2DAsObservable().Subscribe(OnRemove).AddTo(cd);
                target.OnTriggerStay2DAsObservable().Subscribe(OnTriggerStay).AddTo(cd);
                return cd;
            }
            void OnCollisionStay(Collision2D collision2D)
            {
                bool hit = collisionFilter(collision2D);
                
            }
            void OnTriggerStay(Collider2D collision2D)
            {
                bool hit = triggerFilter(collision2D);
                if (hit)
                {
                    var tup = triggerPositionGetter(collision2D);
                    var position = tup.Item1;
                    var radius = tup.Item2;
                    _effect.SetVector3(hitPositionProperty, position);
                    _effect.SetFloat(hitRadiusProperty, radius);
                    if(debugSphere)
                    {
                        debugSphere.gameObject.SetActive(true);
                        debugSphere.position = position;
                    }
                }
                if(debugSphere)
                    debugSphere.gameObject.SetActive(false);
                _effect.SetBool(boolHitDetectedProperty, hit);
            }

            void OnAdd(Collider2D collider2D)
            {
                int currentIndex = colliders.Count;
                if (colliders.Count == 0)
                {
                    //first collider enable effect
                    SetEffectEnabled(true);
                }
            }

            void OnRemove(Collider2D collider2D)
            {
                if (colliders.Count == 1)
                {
                    //last collider disable effect
                    SetEffectEnabled(false);
                }
            }

            private void SetEffectEnabled(bool b)
            {
               
            }
        }

        private void Awake()
        {
            Subject<(Collision2D other, int hitIndex)> onHit = new Subject<(Collision2D other, int hitIndex)>();
            for (int i = 0; i < colliders.Count; i++)
            {
                var collider = colliders[i];
                var i1 = i;
                collider.Setup(effect, TriggerFilter, GetSphereFromTrigger).AddTo(this);
            }
            
        }

        (Vector3, float) GetSphereFromTrigger(Collider2D collider2D)
        {
            var collEffect = collider2D.GetComponent<EffectCollider>();
            return (collEffect.EffectPosition.position, collEffect.effectRadius);
        }

        bool TriggerFilter(Collider2D coll)
        {
            if (coll == null) return false;
            var collEffect = coll.GetComponent<EffectCollider>();
            return collEffect != null;
        }
    }
    
}