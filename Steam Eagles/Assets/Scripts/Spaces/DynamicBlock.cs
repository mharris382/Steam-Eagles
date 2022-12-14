using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using NaughtyAttributes;
using UniRx;
using Rand = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace Spaces
{
    #if UNITY_EDITOR
    
    public class DynamicBlockEditorWindow : EditorWindow
    {
        public List<DynamicBlock> blocks;
        private const string DYNAMIC_BLOCK_PATH = "Assets/Tiles/Dynamic Block/";
        
        [MenuItem("Tools/Block Editor")]
        public static void OpenBlockEditorWindow()
        {
            var window = GetWindow<DynamicBlockEditorWindow>();
            var guids = AssetDatabase.FindAssets("t:DynamicBlock");
            if (guids.Length == 0)
            {
                Debug.LogWarning("Search for dynamic blocks returned no results");
            }

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var block = AssetDatabase.LoadAssetAtPath<DynamicBlock>(path);
                window.blocks.Add(block);
            }
            window.Show();
        }

        private void OnGUI()
        {
            var so = new SerializedObject(this);
            var prop = so.FindProperty("blocks");
            EditorGUILayout.PropertyField(prop, true);
            so.ApplyModifiedProperties();
        }
    }
    
    #endif
    [CreateAssetMenu(menuName = "Steam Eagles/Dynamic Block")]
    public class DynamicBlock : ScriptableObject, IBlockID
    {
        public string BlockName => blockName;
        
        public string blockName = "";
    
        [Required]
        public RuleTile tile;

        [Header("Rendering")] public Sprite[] overrideSprites;
        public string sortingLayerName = "Dynamic";
        public int sortingOrder = 10;
        public Color color = Color.clear;
        public ParticleSystem.MinMaxGradient spawnColor;
    
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
        
        
        [Header("Spawn Prefabs")]
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
            SpawnPrefabs(inst.gameObject);
            SetupPhysics(inst.GetComponent<Rigidbody2D>(), position, rotation);
        
        
            inst.Block = this;
            MessageBroker.Default.Publish(new SpawnedDynamicObjectInfo(this, inst.gameObject));
            return inst;
        }

        public Color GetDynamicBlockColor() => spawnColor.Evaluate(Rand.value);
        
        public Sprite GetDynamicBlockSprite()
        {
            if (overrideSprites != null && overrideSprites.Length > 0)
            {
                var index =UnityEngine.Random.Range(0, overrideSprites.Length - 1);
                return overrideSprites[index];
            }
            return tile.m_DefaultSprite;
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
            spriteRenderer.sortingLayerName = sortingLayerName;
            spriteRenderer.sortingOrder = sortingOrder;
        }
        
        private void SpawnPrefabs(GameObject parent)
        {
            foreach (var spawnPrefab in spawnPrefabs)
            {
                spawnPrefab.Spawn(parent);
            }
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