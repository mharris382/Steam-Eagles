
using System.Collections.Generic;
using Buildings;
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
            var copy = GameObject.Instantiate(loadedObject);
            if (autoInjector != null) autoInjector.enabled = true;
            
            var components = copy.GetComponentsInChildren<Component>();
            foreach (var component in components)
            {
                if (component is Transform) continue;
                if (component is SpriteRenderer sr)
                {
                    _spriteRenderers.Add(sr);
                }
                else
                {
                    Object.Destroy(component);
                }
            }

            var originalSprites = loadedObject.GetComponentsInChildren<SpriteRenderer>();
            var copySprites = copy.GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < originalSprites.Length; i++)
            {
                var originalSprite = originalSprites[i];
                var copySprite = copySprites[i];
                MatchOriginal(copySprite, originalSprite, copySprite.gameObject != copy);
            }
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
            _preview = copy;
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