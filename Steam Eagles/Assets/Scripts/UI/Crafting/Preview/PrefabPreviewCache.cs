using System.Collections.Generic;
using Buildings;
using Items;
using UnityEngine;

namespace UI.Crafting
{
    public class PrefabPreviewCache
    {
        private Dictionary<GameObject, PrefabPreviewWrapper> _previewCache = new Dictionary<GameObject, PrefabPreviewWrapper>();
        
        public PrefabPreviewWrapper GetPreview(Recipe recipe, GameObject loadedObject, Building building, BuildingCell aimedPosition)
        {
            if (_previewCache.ContainsKey(loadedObject))
            {
                var go = _previewCache[loadedObject];
                if (go == null || go.Preview == null) 
                {
                    _previewCache.Remove(loadedObject);
                }
                else
                {
                    go.Update(building, aimedPosition);
                    return go;
                }
            }

            var wrapper = new PrefabPreviewWrapper(loadedObject);
            _previewCache.Add(loadedObject, wrapper);
            wrapper.Update(building, aimedPosition);
            return wrapper;
        }
    }
}