using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings
{
    [RequireComponent(typeof(TilemapRenderer))]
    public class CoverTilemap : BuildingTilemap
    {
        private TilemapRenderer _tilemapRenderer;
        public TilemapRenderer tilemapRenderer => _tilemapRenderer ? _tilemapRenderer : _tilemapRenderer = GetComponent<TilemapRenderer>();
        
        public override TilemapType TilemapType => TilemapType.COVER;
        public Color Color
        {
            get => tilemap.color;
            set => tilemap.color = value;
        }

        public float GetOpacity() => tilemap.color.a;

        public void SetOpacity(float opacity)
        {
            var c = Color;
            c.a = opacity;
            Color = c;
        }
    }
}