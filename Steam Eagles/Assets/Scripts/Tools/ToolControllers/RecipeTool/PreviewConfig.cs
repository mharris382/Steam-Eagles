using System;
using UnityEngine;

namespace Tools.RecipeTool
{
    [Serializable]
    public class PreviewConfig
    {
        public Color validColor = new Color(0, 1, 0, 0.5f);
        public Color invalidColor = new Color(1, 1, 0, 0.5f);
        public int sortingOrder = 0;
        public string sortingLayer = "Default";
        
        private SpriteRenderer _previewSprite;

        public SpriteRenderer GetPreviewSprite(Transform parent  = null)
        {
            if (_previewSprite == null)
            {
                _previewSprite = new GameObject("PreviewSprite").AddComponent<SpriteRenderer>();
                
                _previewSprite.sortingOrder = sortingOrder;
                _previewSprite.sortingLayerName = sortingLayer;
            }
            if(parent != null)
                _previewSprite.transform.SetParent(parent);
            return _previewSprite;
        }
    }
}