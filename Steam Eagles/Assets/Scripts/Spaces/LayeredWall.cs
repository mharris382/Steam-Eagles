using System;
using UnityEngine;

namespace Spaces
{
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class LayeredWall : MonoBehaviour
    {
        private SpriteRenderer _sr;
        private SpriteRenderer sr => _sr == null ? (_sr = GetComponent<SpriteRenderer>()) : _sr;

        public Vector3 positionOffset = new Vector3(0, 0, -1);
        public int orderOffset = 1;

        public bool copySortingLayer = false;
        public bool copyRectSize;
        
        
        public Color colorOffset = Color.white;
        [SerializeField] private ColorOffsetMode colorOffsetMode;
        enum ColorOffsetMode
        {
            MULTIPLY,
            ADD,
            SUBTRACT,
            LERP
        }
        
        private void OnDrawGizmos()
        {
            var t = transform;
            
            var order = sr.sortingOrder + orderOffset;
            var pos = sr.transform.position + positionOffset;
            Func<Color, float, Color> offsetColorFunc = null;
            var color = sr.color;
            switch (colorOffsetMode)
            {
                case ColorOffsetMode.MULTIPLY:
                    offsetColorFunc = (color1, t) => color1 * colorOffset;
                    break;
                case ColorOffsetMode.ADD:
                    offsetColorFunc = (color1, t) => color1 + colorOffset;
                    break;
                case ColorOffsetMode.LERP:
                    offsetColorFunc = (color1, t) => Color.Lerp(sr.color, this.colorOffset, t);
                    break;
                case ColorOffsetMode.SUBTRACT:
                    offsetColorFunc = (color1, t) => color1 - colorOffset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            color = offsetColorFunc(color, 0);
            for (int i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i);
                child.transform.position = pos;
                pos += positionOffset;
                
                if(child.gameObject.TryGetComponent<SpriteRenderer>(out var childSr))
                {
                    childSr.color = color;
                    childSr.sortingOrder = order;
                    if (copySortingLayer) childSr.sortingLayerID = sr.sortingLayerID;
                    if (copyRectSize && sr.drawMode != SpriteDrawMode.Simple)
                    {
                        childSr.drawMode = sr.drawMode;
                        childSr.transform.localScale = Vector3.one;
                        childSr.tileMode = SpriteTileMode.Continuous;
                        childSr.size = sr.size;
                    }
                    order += orderOffset;
                    color = offsetColorFunc(color, i/(float)t.childCount);
                }
            }
        }
    }
}