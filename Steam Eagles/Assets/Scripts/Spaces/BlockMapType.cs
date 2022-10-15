using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using World;

[CreateAssetMenu(menuName = "Steam Eagles/Block Map Type", fileName = "new Block Map Type")]
public class BlockMapType : ScriptableObject
{
    public SharedTilemap activeTilemapReference;

    
    public string mapLayer = "Ground";

    public int GetMapLayer => LayerMask.NameToLayer(mapLayer);
    
    public void ActivateMap(BlockMap blockMap)
    {
        if (blockMap == null) return;
        activeTilemapReference.Value = blockMap.Tilemap;
    }

    public UnityEvent<Vector3Int, TileBase> onTilePlaced;
    public UnityEvent<Vector3Int, TileBase> onTileRemoved;
    


}