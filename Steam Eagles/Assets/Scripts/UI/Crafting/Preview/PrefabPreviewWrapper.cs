
using System.Collections.Generic;
using Buildables;
using Buildings;
using Sirenix.Utilities;
using Unity.Linq;
using UnityEngine;
using Zenject;

namespace UI.Crafting
{
    public class PrefabPreviewWrapper
    {
        private readonly GameObject _preview;

        
        private List<SpriteRenderer> _spriteRenderers = new List<SpriteRenderer>();
        public GameObject Preview => _preview;

        public Color Color
        {
            set
            {
                foreach (var spriteRenderer in _spriteRenderers)
                {
                    spriteRenderer.color = value;
                }
            }
        }
        public bool Active
        {
            set => Preview.gameObject.SetActive(value);
        }

        public bool Flipped
        {
            set
            {
                if (Preview == null) return;
                var scale = Preview.transform.localScale;
                var scaleAbs = Mathf.Abs(scale.x);
                var newScale = new Vector3(value ? -scaleAbs : scaleAbs, scale.y, scale.z);
                Preview.transform.localScale = newScale;
            }
        }
        
        public PrefabPreviewWrapper( GameObject loadedObject)
        {
            var autoInjector = loadedObject.GetComponent<ZenAutoInjecter>();
            if (autoInjector != null) autoInjector.enabled = false;
            
            Dictionary<string, Transform> transformCopies = new Dictionary<string, Transform>();
            Dictionary<string, Matrix4x4> localMatrices = new Dictionary<string, Matrix4x4>();
            Dictionary<string, BuildableVariant> variants = new();

            var copy = new GameObject(loadedObject.name).transform; //GameObject.Instantiate(loadedObject);
            transformCopies.Add(loadedObject.name, copy);
            
            if (autoInjector != null) autoInjector.enabled = true;

            Queue<(Transform, string, Transform)> toProcess = new();
            loadedObject.Descendants(t => t.GetComponentsInChildren<SpriteRenderer>().Length > 0)
                .ForEach(t => {
                    if (transformCopies.ContainsKey(t.name))
                    {
                        Debug.LogError($"Duplicate name found: {t.name}, this is not allowed for sprites on buildables", loadedObject);
                        return;
                    }


                    var childCopy = new GameObject(t.name).transform;
                    transformCopies.Add(t.name, childCopy);
                    localMatrices.Add(t.name, t.transform.worldToLocalMatrix);
                    var parentName = t.transform.parent.name;
                    toProcess.Enqueue((childCopy, parentName, t.transform));
            
                });

            while (toProcess.Count > 0)
            {
                var (childCopy, parentName, original) = toProcess.Dequeue();
                if (transformCopies.ContainsKey(parentName))
                {
                    childCopy.parent = transformCopies[parentName];
                }
                else
                {
                    Debug.LogWarning("Couldn't find parent: " + parentName, loadedObject);
                    Object.Destroy(childCopy.gameObject);
                    continue;
                }
                Process(childCopy, original);
               

                // var variant = original.GetComponent<BuildableVariant>();
                // if(variant != null) variants.Add(original.name, variant);
            }

            void Process(Transform copyT, Transform originalT)
            {
                var parentName = originalT.parent.name;
                var copyParent = transformCopies[parentName];
                
                copyT.parent = copyParent;
                copyT.localPosition = originalT.localPosition;
                copyT.localRotation = originalT.localRotation;
                copyT.localScale = originalT.localScale;
                var sr =originalT.GetComponent<SpriteRenderer>();
                if (sr != null && sr.enabled && sr.sprite != null)
                {
                    var srCopy = copyT.gameObject.AddComponent<SpriteRenderer>();
                    MatchOriginal(srCopy, sr, true);
                }
            }
            //
            // var components = copy.GetComponentsInChildren<Component>();
            // foreach (var component in components)
            // {
            //     if (component is Transform) continue;
            //     if (component is SpriteRenderer sr)
            //     {
            //         _spriteRenderers.Add(sr);
            //     }
            //     else
            //     {
            //         var type = component.GetType();
            //         if (type.InheritsFrom<MonoInstaller>() || type == typeof(GameObjectContext))
            //         {
            //             continue;
            //         }
            //
            //         var previewAttribute = type.GetCustomAttribute<PreviewAttribute>();
            //         if (previewAttribute == null) 
            //             Object.Destroy(component);
            //         else
            //         {
            //             if(!previewAttribute.isPreview) Object.Destroy(component);
            //         }
            //     }
            // }
            //
            // var originalSprites = loadedObject.GetComponentsInChildren<SpriteRenderer>();
            // var copySprites = copy.GetComponentsInChildren<SpriteRenderer>();
            // for (int i = 0; i < originalSprites.Length; i++)
            // {
            //     var originalSprite = originalSprites[i];
            //     var copySprite = copySprites[i];
            //     MatchOriginal(copySprite, originalSprite, copySprite.gameObject != copy);
            // }
            // var newGo = new GameObject($"{loadedObject.name} Preview");
            // var originalSprite = loadedObject.GetComponent<SpriteRenderer>();
            // if (originalSprite != null)
            // {
            //     var sr = newGo .AddComponent<SpriteRenderer>();
            //     MatchOriginal(sr, originalSprite);
            // }
            // for (int i = 0; i < loadedObject.transform.childCount; i++)
            // {
            //     var child = loadedObject.transform.GetChild(i);
            //    
            //     var childSr = child.GetComponent<SpriteRenderer>();
            //     Transform newChildTransform = null;
            //     if (childSr != null)
            //     {
            //         var newChild = new GameObject($"{childSr.name} Preview");
            //         newChildTransform = newChild.transform;
            //         newChild.transform.parent = newGo.transform;
            //         var newChildSr = newChild.AddComponent<SpriteRenderer>();
            //         MatchOriginal(newChildSr, childSr, true);                    
            //     }
            //     for (int j = 0; j < child.childCount; j++)
            //     {
            //         var deepChild = child.GetChild(j);
            //         var deepChildSr = deepChild.GetComponent<SpriteRenderer>();
            //         if (deepChildSr != null)
            //         {
            //             if (newChildTransform == null)
            //             {
            //                 newChildTransform = new GameObject("Deep Child").transform;
            //             }
            //             newChildTransform.parent = newGo.transform;
            //         }
            //     }
            // }
            _preview = copy.gameObject;
        }

        public void Update(Building building, BuildingCell aimedPosition)
        {
            Preview.transform.parent = building.transform;
            Preview.transform.position= building.Map.CellToWorld(aimedPosition.cell, aimedPosition.layers);
        }
        void MatchOriginal(SpriteRenderer previewSr, SpriteRenderer originalSr, bool isChild = false)
        {
            _spriteRenderers.Add(previewSr);
            previewSr.sprite = originalSr.sprite;
            previewSr.size = originalSr.size;
            previewSr.drawMode = originalSr.drawMode;
            previewSr.enabled = originalSr.enabled;
            previewSr.sortingOrder = originalSr.sortingOrder;
            previewSr.sortingLayerID = originalSr.sortingLayerID;
            if (isChild)
            {
                var previewSrTransform = previewSr.transform;
                var originalSrTransform = originalSr.transform;
                previewSrTransform.localPosition = originalSrTransform.localPosition;
                previewSrTransform.localRotation = originalSrTransform.localRotation;
                previewSrTransform.localScale = originalSrTransform.localScale;
            }
        }
    }
}