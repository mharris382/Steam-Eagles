using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CoreLib.Pickups
{
    [CreateAssetMenu(fileName = "New Pickup", menuName = "Steam Eagles/New Pickup", order = 0)]
    public class Pickup : ScriptableObject
    {
        [ValidateInput(nameof(ValidateKey))]
        public string key;

        [SerializeField]
        private bool usePrefab;
        [AssetsOnly, ShowIf(nameof(usePrefab))]
        public PickupBody prefab;

        public TransformOptions transformOptions;
        public PhysicsOptions physicsOptions;
        public RenderingOptions renderingOptions;
        public FeedbackOptions feedbackOptions;

        [ShowInInspector, ReadOnly, PreviewField(ObjectFieldAlignment.Right, Height = 200)]
        public Sprite DefaultSprite
        {
            get
            {
                if (prefab == null) return null;
                return prefab.SpriteRenderer.sprite;
            }
        }
        
        [PreviewField(ObjectFieldAlignment.Right, Height = 150)]
        public Sprite[] overrideSprites;

        protected internal Sprite GetSprite()
        {
            if (overrideSprites.Length == 0)
            {
                Debug.LogError($"No Sprite Assigned to Pickup {name}! Please assign at least one sprite", this);
                return null;
            }
            if(overrideSprites.Length == 1)
                return overrideSprites[0];
            else
                return overrideSprites[UnityEngine.Random.Range(0, overrideSprites.Length)];
        }
        
        public PickupBody SpawnPickup(Vector3 position)
        {
            if (!usePrefab)
            {
                var inst2 = new GameObject($"{key} Pickup (Instance)", typeof(Rigidbody2D), typeof(SpriteRenderer), physicsOptions.GetColliderType(), typeof(PickupBody))
                    .AddComponent<PickupInstance>();
                inst2.gameObject.layer = LayerMask.NameToLayer("DynamicBlocks");
                inst2.transform.position = position;
                inst2.InjectPickup(this);

                renderingOptions.SetupRenderer(inst2, this);
                physicsOptions.SetupPhysics(inst2);
                feedbackOptions.SpawnPrefabsOnPickupSpawned(inst2);
                transformOptions.SetupTransform(inst2.transform);

                return inst2.PickupBody;
            }
            var inst = Instantiate(prefab, position, Quaternion.identity);
            var pickupID = inst.gameObject.AddComponent<PickupID>();
            pickupID.Key = key;
            return inst;
        }
        
        

        private bool ValidateKey(string key)
        {
            return !String.IsNullOrEmpty(key);
        }
        
        [Serializable]
        public class TransformOptions
        {
            public ParticleSystem.MinMaxCurve rotation;
            public ParticleSystem.MinMaxCurve scale = new ParticleSystem.MinMaxCurve(1);

            public void SetupTransform(Transform instance)
            {
                var instanceRotation = rotation.Evaluate(UnityEngine.Random.Range(0, 1));
                var instanceScale = this.scale.Evaluate(UnityEngine.Random.Range(0, 1));
                instance.rotation = Quaternion.Euler(0, 0, instanceRotation);
                instance.localScale = Vector3.one * instanceScale;
            }
        }
        
        [Serializable]
        public class PhysicsOptions
        {
            private PhysicsMaterial2D _physicsMaterial2D;
            public PhysicsMaterial2D PhysicMaterial2D
            {
                get
                {
                    if (_physicsMaterial2D == null || 
                        Math.Abs(_physicsMaterial2D.bounciness - bounce) > Mathf.Epsilon || 
                        Math.Abs(_physicsMaterial2D.friction - friction) > Mathf.Epsilon)
                    {
                        _physicsMaterial2D = new PhysicsMaterial2D()
                        {
                            friction = friction,
                            bounciness = bounce
                        };
                    }
                    return _physicsMaterial2D;
                }
            }
           
            public enum Shape
            {
                BOX,
                CIRCLE,
                POLYGON
            }

            
           [TabGroup("Basic")] public float mass = 1;
           [TabGroup("Basic")] public float gravityScale = 1;
           [TabGroup("Basic")] public float linearDrag;
           [TabGroup("Basic")] public float angularDrag = 0.2f;

          
            
            [TabGroup("Collider")]
            public Shape shape;
            
            [TabGroup("Collider")] [Tooltip("By default the collider will be sized to encompass the entire sprite, this offset will be added to the size of the collider")]
            public float colliderOffset;
            [TabGroup("Collider"),ShowIf("@shape==Shape.BOX")]
            public Vector2 sizeOffset;
            [TabGroup("Collider")] [ShowIf("@shape==Shape.BOX")]
            public float edgeRadius = 0;
            public float edgeOffset = 0.1f;
            
            [TabGroup("Physics Material")]
            public float bounce;
            [TabGroup("Physics Material")]
            public float friction = .25f;
            public Type GetColliderType()
            {
                switch (shape)
                {
                    case Shape.BOX:
                        return typeof(BoxCollider2D);
                        break;
                    case Shape.CIRCLE:
                        return typeof(CircleCollider2D);
                        break;
                    case Shape.POLYGON:
                        return typeof(PolygonCollider2D);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            
            public void SetupPhysics(PickupInstance instance)
            {
                SetupCorePhysics(instance.PickupBody.Rigidbody2D);
                SetupColliderPhysics(instance.PickupBody.Collider2D);

                void SetupCorePhysics(Rigidbody2D rigidbody2D)
                {
                    
                    rigidbody2D.mass = mass;
                    rigidbody2D.gravityScale = gravityScale;
                    rigidbody2D.drag = linearDrag;
                    rigidbody2D.angularDrag = angularDrag;
                    rigidbody2D.sharedMaterial = PhysicMaterial2D;
                }

                void SetupBoxCollider(BoxCollider2D boxCollider)
                {
                    
                    boxCollider.edgeRadius = edgeRadius/2f;
                    boxCollider.size -= Vector2.one * colliderOffset;
                    var boundsSize = ((Vector2)instance.PickupBody.SpriteRenderer.sprite.bounds.size);
                    var boundsOffset = new Vector2(boundsSize.x * sizeOffset.x, boundsSize.y * sizeOffset.y);
                    boxCollider.size = boundsSize - boundsOffset - (Vector2.one * (boxCollider.edgeRadius+edgeOffset)) ;
                }

                void SetupPolygonCollider(PolygonCollider2D polygonCollider2D, Sprite sprite)
                {
                    int shapeCount = sprite.GetPhysicsShapeCount();
                    if (shapeCount == 0)
                    {
                        Debug.LogError($"No Physics Shape found on sprite.  Cannot generate polygon for pickup {instance.name}", sprite);
                        return;
                    }

                    int pointCount = sprite.GetPhysicsShapePointCount(0);
                    var points = new List<Vector2>(pointCount);
                    sprite.GetPhysicsShape(0, points);
                    polygonCollider2D.points = points.ToArray();
                }
                void SetupColliderPhysics(Collider2D collider)
                {
                    switch (shape)
                    {
                        case Shape.BOX:
                            SetupBoxCollider(collider as BoxCollider2D);
                            break;
                        case Shape.CIRCLE:
                            var circleCollider = collider as CircleCollider2D;
                            circleCollider.radius -= colliderOffset;
                            break;
                        case Shape.POLYGON:
                            SetupPolygonCollider(collider as PolygonCollider2D, instance.PickupBody.SpriteRenderer.sprite);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                }
            }
        }
        
        [Serializable]
        public class RenderingOptions
        {
            [SerializeField]
            ParticleSystem.MinMaxGradient spawnColor = new ParticleSystem.MinMaxGradient(Color.white);
            
            public string sortingLayerName = "Dynamic";

            [MinMaxSlider(-100, 100)]
            public Vector2Int sortingOrderRange = new Vector2Int(-10, 10);

            public Color GetSpawnColor() => spawnColor.Evaluate(UnityEngine.Random.value);


            public void SetupRenderer(PickupInstance pickupInstance, Pickup pickup)
            {
                var sr = pickupInstance.PickupBody.SpriteRenderer;
                sr.sortingLayerName = sortingLayerName;
                sr.sortingOrder = UnityEngine.Random.Range(sortingOrderRange.x, sortingOrderRange.y);
                sr.color = GetSpawnColor();
                sr.sprite = pickup.GetSprite();
            }
        }
        
        
        [Serializable]
        public class FeedbackOptions
        {
            [SerializeField] private SpawnPrefab[] spawnPrefabs;
        
        
            [Serializable] private class SpawnPrefab
            {
                public GameObject prefab;
                public bool spawnAsChild = true;

                public void Spawn(GameObject parent)
                {
                    var instance = Instantiate(prefab, parent.transform);
                    instance.transform.position = parent.transform.position;
                    instance.transform.rotation = parent.transform.rotation;
                    if (!spawnAsChild)
                    {
                        instance.transform.SetParent(null);
                    }
                }
            }


            public void SpawnPrefabsOnPickupSpawned(PickupInstance obj)
            {
                foreach (var spawnPrefab in spawnPrefabs)
                {
                    if (spawnPrefab == null) continue;
                    spawnPrefab.Spawn(obj.gameObject);
                }
            }
        }
    }
}