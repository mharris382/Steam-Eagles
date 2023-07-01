using System;
using NaughtyAttributes;
using UnityEngine;

namespace PhysicsFun.Utilities
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class LayeredSpriteRenderers : MonoBehaviour
    {
        private SpriteRenderer _sr;
        public SpriteRenderer sr => _sr ? _sr : _sr = GetComponent<SpriteRenderer>();



        
        [OnValueChanged(nameof(UpdateChildSpriteSizes))]
        public Vector2 size = Vector2.one;
        public bool forceSamePosition = true;

        [OnValueChanged(nameof(SetChildSpritesVisible))]
        public bool hideChildSpritesInHierarchy = true;
        
        void UpdateChildSpriteSizes(Vector2 size)
        {
            sr.size = size;
            foreach (var child in GetComponentsInChildren<SpriteRenderer>())
            {
                child.drawMode = sr.drawMode;
                child.size = size;
                if(forceSamePosition)
                    child.transform.localPosition = Vector3.zero;
            }
        }

        void SetChildSpritesVisible(bool visibleInHierarchy)
        {
            foreach (var child in GetComponentsInChildren<SpriteRenderer>())
            {
                child.hideFlags = visibleInHierarchy ? HideFlags.None : HideFlags.HideInHierarchy;
            }
        }

        Vector2 _lastSize;
        private void OnDrawGizmosSelected()
        {
            if (_lastSize != sr.size)
            {
                size = sr.size;
                UpdateChildSpriteSizes(size);
                _lastSize = size;
            }
        }
    }
}