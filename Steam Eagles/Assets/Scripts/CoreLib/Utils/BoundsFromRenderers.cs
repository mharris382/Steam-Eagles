using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CoreLib
{
 
    /// <summary>
    /// utility class to generate colliders to use as Cinemachine 
    /// </summary>
    [ExecuteInEditMode]
    public class BoundsFromRenderers : MonoBehaviour
    {
        private const float MAX_RECT_WIDTH = 10000f;
        private const float MAX_RECT_HEIGHT = 10000f;
        private const float MAX_RECT_X = -1000;
        private const float MAX_RECT_Y = -1000;
        
        public List<Renderer> renderers;
        

        private PolygonCollider2D _polygonCollider2D;

        private PolygonCollider2D PolygonCollider2D => _polygonCollider2D != null
            ? _polygonCollider2D
            : (_polygonCollider2D = GetComponent<PolygonCollider2D>());


        private void Update()
        {
            if (Application.isPlaying)
            {
                enabled = false;
                return;
            }
            PolygonCollider2D.SetPath(0, DetermineRect().GetPoints().ToArray());
        }

        Rect DetermineRect()
        {
            if (renderers.Count == 0)
            {
                return new Rect(MAX_RECT_X, MAX_RECT_Y, MAX_RECT_WIDTH, MAX_RECT_HEIGHT);
            }

            Vector2 minPosition = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 size = new Vector2(0, 0);
            foreach (var renderer1 in renderers)
            {
                TilemapRenderer tmr;
                
                var bounds = renderer1.bounds;
                if (renderer1 is TilemapRenderer tilemapRenderer)
                {
                    bounds = tilemapRenderer.bounds ;
                    var tm = tilemapRenderer.GetComponent<Tilemap>();
                    var cellBounds = tm.cellBounds;
                    var min = tm.CellToWorld(cellBounds.min);
                    var max = tm.CellToWorld(cellBounds.max);
                    bounds.SetMinMax(min, max);
                }
                else if (renderer1 is SpriteRenderer spriteRenderer)
                {
                    bounds = spriteRenderer.bounds;
                }
                var w = bounds.size.x;
                var h = bounds.size.y;
                var x = bounds.min.x;
                var y = bounds.min.y;
                if (size.x < w) size.x = w;

                if (size.y < h) size.y = h;

                if (minPosition.x > x) minPosition.x = x;

                if (minPosition.y > y) minPosition.y = y;
            }

            return new Rect(minPosition, size);
        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            DetermineRect().DrawGizmos();
        }
    }
}