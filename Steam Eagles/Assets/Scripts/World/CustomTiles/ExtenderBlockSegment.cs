using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/Extender/Base", fileName = "New ExtenderBlock")]
public class ExtenderBlockSegment : ExtenderBlockBase
{
    public Vector3Int extendDirection = Vector3Int.right;
    public override void OnPlacement(ExtenderBlockPlacer placer, Vector3Int position)
    {
        _placer = placer;
        _timePlaced = Time.time;
    }
    private ExtenderBlockPlacer _placer;
    private float _timePlaced;

    
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        if (!IsValid()) return;
        float duration = _placer.timePerSegment;
        float endTime = _timePlaced + _placer.timePerSegment;
        base.GetTileData(position, tilemap, ref tileData);
    }
}