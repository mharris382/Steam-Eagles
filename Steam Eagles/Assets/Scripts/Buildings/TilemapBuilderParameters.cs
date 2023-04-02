using System;
using Buildings.BuildingTilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

public readonly struct TilemapBuilderParameters
{
    public TilemapBuilderParameters(Type tilemapType, 
        bool withRenderer = true, 
        bool withCollider = false,
        bool withPlatformEffector = false,
        bool asTrigger = false,
        int layer= 0,
        string tag ="",
        string sortingLayerName = "Default",
        int sortingOrder = 0
    )
    {
        TilemapType = tilemapType;
        WithRenderer = withRenderer;
        WithCollider = withCollider || withPlatformEffector || asTrigger;
        WithPlatformEffector = withPlatformEffector;
        AsTrigger = asTrigger;
        Layer = layer;
        Tag = tag;
        SortingLayerName = sortingLayerName;
        SortingOrder = sortingOrder;
    }

    public bool WithPlatformEffector { get; }
    public bool AsTrigger { get; }
    public int Layer { get; }
    public string Tag { get; }
    public string SortingLayerName { get; }
    public int SortingOrder { get; }

    public bool WithCollider { get; }

    public bool WithRenderer { get; }

    public Type TilemapType { get; }


    public BuildingTilemap CreateTilemap(Transform parent)
    {
        var tilemap = new GameObject("Test Tilemap", TilemapType, typeof(Tilemap));
        tilemap.transform.SetParent(parent,false);
        tilemap.layer = Layer;
        if (!string.IsNullOrEmpty(Tag)) tilemap.tag = Tag;
           
        if (WithRenderer)
        {
            var tr = tilemap.AddComponent<TilemapRenderer>();
            tr.sortingLayerName = SortingLayerName;
            tr.sortingOrder = SortingOrder;
        }
            
        if (WithCollider)
        {
            var collider = tilemap.AddComponent<TilemapCollider2D>();
            if (WithPlatformEffector)
            {
                var platformEffector2D = tilemap.AddComponent<PlatformEffector2D>();
                collider.usedByEffector = true;
            }

            if (AsTrigger)
            {
                collider.isTrigger = true;
            }
        }
            
        return tilemap.GetComponent(TilemapType) as BuildingTilemap;
    }
    
    
    
}