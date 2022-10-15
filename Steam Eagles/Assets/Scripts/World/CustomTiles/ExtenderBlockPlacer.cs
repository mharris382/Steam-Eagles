using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using World;

public class ExtenderBlockPlacer : MonoBehaviour
{
    public ExtenderBlockBase extenderBlock;
    public SharedTilemap solidTilemap;
    public Vector3Int extenderDirection = Vector3Int.right;
    public int maxSegments = 100;
    [FormerlySerializedAs("timePerSecond")] public float timePerSegment = 0.5f;
    
    private void Update()
    {
        var mp = solidTilemap.Value.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        var tle = solidTilemap.Value.GetTile(mp);
        if (tle != null) return;
        PlaceExtender(solidTilemap.Value, mp);
    }
    
    

    private void PlaceExtender(Tilemap solidTilemapValue, Vector3Int cellPosition)
    {
        var blockBase = Instantiate(extenderBlock);
        
    }
    
    
    
}