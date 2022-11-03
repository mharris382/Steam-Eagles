using CoreLib;
using GasSim;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SolidGrid : MonoBehaviour
{
    private IGasSim _gasSim;
    public Tilemap solidTilemap;


    private IGasSim GasSim => _gasSim == null ? (_gasSim = GetComponent<IGasSim>()) : _gasSim;
    private Grid Grid => _gasSim.Grid;
    
    private void Awake()
    {
        _gasSim = GetComponent<IGasSim>();
    }

    private Vector2 GasCellsPerSolid => new(solidTilemap.cellSize.x / Grid.cellSize.x, solidTilemap.cellSize.y / Grid.cellSize.y );

    public Vector2Int GasCellsPerSolidCell =>
        new Vector2Int(Mathf.RoundToInt(GasCellsPerSolid.x * solidTilemap.transform.lossyScale.x), Mathf.RoundToInt(GasCellsPerSolid.y * solidTilemap.transform.lossyScale.y)) ;


    private void Start()
    {
        var rect = GasSim.SimulationRect;
        var gasCellsPerSolidCell = GasCellsPerSolidCell;
        for (int x = 0; x < rect.size.x / gasCellsPerSolidCell.x; x++)
        {
            for (int y = 0; y < rect.size.y; y++)
            {
                var solidCoord = new Vector3Int(x, y);
                if (solidTilemap.HasTile(solidCoord))
                {
                    var gasCoordTL = new Vector3Int(x * gasCellsPerSolidCell.x, y * gasCellsPerSolidCell.y);
                    for (int gx = 0; gx < gasCellsPerSolidCell.x; gx++)
                    {
                        for (int gy = 0; gy < gasCellsPerSolidCell.y; gy++)
                        {
                            var gasCoord = gasCoordTL + new Vector3Int(gx, gy);
                            GasSim.SetStateOfMatter( (Vector2Int)gasCoord, StateOfMatter.SOLID);
                        }
                    }
                }
            }
        }

        MessageBroker.Default.Receive<BuildActionInfo>().TakeUntilDestroy(this).Subscribe(OnBuildAction);
        MessageBroker.Default.Receive<DisconnectActionInfo>().TakeUntilDestroy(this).Subscribe(OnDisconnectAction);
    }

    private void OnBuildAction(BuildActionInfo buildActionInfo)
    {
        if (solidTilemap != buildActionInfo.tilemap) return;
        var gasCellsPerSolidCell = GasCellsPerSolidCell;
        var solidCoord = buildActionInfo.cellPosition;
        var gasCoordTL = new Vector3Int(solidCoord.x * gasCellsPerSolidCell.x, solidCoord.y * gasCellsPerSolidCell.y);
        for (int gx = 0; gx < gasCellsPerSolidCell.x; gx++)
        {
            for (int gy = 0; gy < gasCellsPerSolidCell.y; gy++)
            {
                var gasCoord = gasCoordTL + new Vector3Int(gx, gy);
                GasSim.SetStateOfMatter( (Vector2Int)gasCoord, StateOfMatter.SOLID);
            }
        }
    }
    
    private void OnDisconnectAction(DisconnectActionInfo buildActionInfo)
    {
        if (solidTilemap != buildActionInfo.tilemap) return;
        var gasCellsPerSolidCell = GasCellsPerSolidCell;
        var solidCoord = buildActionInfo.cellPosition;
        var gasCoordTL = new Vector3Int(solidCoord.x * gasCellsPerSolidCell.x, solidCoord.y * gasCellsPerSolidCell.y);
        for (int gx = 0; gx < gasCellsPerSolidCell.x; gx++)
        {
            for (int gy = 0; gy < gasCellsPerSolidCell.y; gy++)
            {
                var gasCoord = gasCoordTL + new Vector3Int(gx, gy);
                GasSim.SetStateOfMatter( (Vector2Int)gasCoord, StateOfMatter.AIR);
            }
        }
    }
}