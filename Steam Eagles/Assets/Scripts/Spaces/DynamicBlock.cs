using System;
using System.Collections.Generic;
using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Spaces
{
    [CreateAssetMenu(menuName = "Steam Eagles/Dynamic Block")]
    public class DynamicBlock : ScriptableObject, IBlockID
    {
        public string BlockName => blockName;
        
        public string blockName = "";
        
        
        [HideInInspector, Obsolete("Unnecessary coupling between systems")]
        public StaticBlock linkedStaticBlock;
    
        [Header("Rendering")]
        public Sprite[] overrideSprites;

        public Color color = Color.clear;
    
        [Header("Physics")]
        public float mass = 1;
        public float gravityScale = 1;
    
    
        public float bounce;
        public float friction =0.4f;
        public bool sharePhysicsMaterials;

        public bool useContinuousCollision;
    
        [Header("Box Collider Stuff")]
        public bool useBoxCollider;
        public Vector2 sizeOffset;
        public float edgeOffset = 0.1f;
        public float roundness;
    
        [Header("On Spawned Event")]
        public UnityEvent<DynamicBlockInstance> onInstanceSpawned;


        private PhysicsMaterial2D _physicMaterial;

        public PhysicsMaterial2D PhysicMaterial2D
        {
            get
            {
                if (_physicMaterial == null)
                {
                    _physicMaterial = new PhysicsMaterial2D()
                    {
                        friction = this.friction,
                        bounciness = this.bounce
                    };
                    _physicMaterial.name = $"{name} Physics Material {(sharePhysicsMaterials ? "(Shared)" : "")}";
                }

                if (_physicMaterial.friction != this.friction || _physicMaterial.bounciness != this.bounce)
                {
                    if (!sharePhysicsMaterials)
                    {
                        _physicMaterial = new PhysicsMaterial2D()
                        {
                            friction = this.friction,
                            bounciness = this.bounce
                        };
                        _physicMaterial.name = $"{name} Physics Material {(sharePhysicsMaterials ? "(Shared)" : "")}";
                    }
                    else
                    {
                        _physicMaterial.friction = this.friction;
                        _physicMaterial.bounciness = this.bounce;
                    }
                }

            
                return _physicMaterial;
            }
        }
    
        /// <summary>
        /// why am i doing all this here instead of on the object itself? because the object should only ever exist at runtime. Additionally I still
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public DynamicBlockInstance SpawnDynamicBlockInstance(Vector3 position, float rotation)
        {
            var inst = new GameObject($"{name} (Instance)", typeof(Rigidbody2D), typeof(SpriteRenderer)).AddComponent<DynamicBlockInstance>();
            var sr = inst.GetComponent<SpriteRenderer>();
            sr.transform.position = position;
            SetupRenderer(sr);
            AddCollision(sr);
        
            SetupPhysics(inst.GetComponent<Rigidbody2D>(), position, rotation);
        
        
            inst.Block = this;
            MessageBroker.Default.Publish(new SpawnedDynamicObjectInfo(this, inst.gameObject));
            return inst;
        }

        public Color GetDynamicBlockColor()
        {
            if (linkedStaticBlock == null)
            {
                var c = color;
                c.a = 1;
                return c;
            }
            return color == Color.clear ? linkedStaticBlock.color : color;
        }
    
        public Sprite GetDynamicBlockSprite()
        {
            if (overrideSprites != null && overrideSprites.Length > 0)
            {
                var index =UnityEngine.Random.Range(0, overrideSprites.Length - 1);
                return overrideSprites[index];
            }
        
            return linkedStaticBlock.sprite;
        }

        private void SetupPhysics(Rigidbody2D rb,Vector3 position, float rotation)
        {
            rb.isKinematic = true;
            rb.position = position;
            rb.mass = mass;
            rb.gravityScale = gravityScale;
            rb.isKinematic = false;
            rb.sharedMaterial = PhysicMaterial2D;
            rb.gameObject.layer = LayerMask.NameToLayer("DynamicBlocks");
            if (useContinuousCollision)
            {
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
            rb.SetRotation(rotation);
        
        }

        private void SetupRenderer(SpriteRenderer spriteRenderer)
        {
            spriteRenderer.color = GetDynamicBlockColor();
            spriteRenderer.sprite = GetDynamicBlockSprite();
        
        }
    
        private  void AddCollision(SpriteRenderer spriteRenderer)
        {
            var sprite = spriteRenderer.sprite;
        
            if (!useBoxCollider && sprite.GetPhysicsShapeCount() > 0)
            {
                var pointCount = sprite.GetPhysicsShapePointCount(0);
                var points = new List<Vector2>(pointCount);
                sprite.GetPhysicsShape(0, points);
            
                var polygonCollider = spriteRenderer.gameObject.AddComponent<PolygonCollider2D>();
                polygonCollider.points = points.ToArray();
            
            }
            else
            {
                var box = spriteRenderer.gameObject.AddComponent<BoxCollider2D>();
                box.edgeRadius = roundness/2f;
                box.size = ((Vector2)sprite.bounds.size) - sizeOffset - (Vector2.one * (box.edgeRadius+edgeOffset)) ;
            }   
        }

        
    }
}