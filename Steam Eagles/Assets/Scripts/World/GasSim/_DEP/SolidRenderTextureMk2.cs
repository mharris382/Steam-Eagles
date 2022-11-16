using System;
using GasSim;
using UnityEngine;

public class SolidRenderTextureMk2 : MonoBehaviour
{
    public Camera camera;
    public RenderTexture currentRenderTexture;
    public RectInt renderArea;
    public RectInt offsetAndPadding;
    public GameObject gasSim;
    private IGasSim _gasSim;
    private Grid _grid;
    public LayerMask layerMask;
    private IGasSim GasSim{
        get
        {
            if (_gasSim == null)
            {   
                if (gasSim == null)
                {
                    gasSim = GameObject.FindWithTag("GasSim");
                    Debug.Assert(gasSim!=null, "No gas Sim in scene!", this);
                }
                _gasSim = gasSim.GetComponent<IGasSim>();
            }
            return _gasSim;
        }
    }

    private Grid Grid => _grid == null ? (_grid = GasSim.Grid) : _grid;
    public float scale = 1f;
    private void Awake()
    {
        if (gasSim == null)
        {
            gasSim = GameObject.FindWithTag("GasSim");
            Debug.Assert(gasSim!=null, "No gas Sim in scene!", this);
        }
        _gasSim = gasSim.GetComponent<IGasSim>();
        renderArea = _gasSim.SimulationRect;
        var r = _gasSim.SimulationRect;
        r.max = new Vector2Int(
            Mathf.RoundToInt((r.max.x * Grid.cellSize.x) * scale),
            Mathf.RoundToInt((r.max.y * Grid.cellSize.y)* scale));
        r.min += offsetAndPadding.min;
        r.max += offsetAndPadding.max;
        renderArea = r;
        _grid = _gasSim.Grid;
    }

    private void Start()
    {
        SetupRenderTexture();
        Vector3 pos = renderArea.center;
        pos.z = -10;
        SetupCamera(currentRenderTexture,pos );
    }

    void SetupRenderTexture()
    {
        currentRenderTexture = new RenderTexture(
            GasSim.SimulationRect.width,
            GasSim.SimulationRect.height, 24);
        currentRenderTexture.enableRandomWrite = true;
        currentRenderTexture.filterMode = FilterMode.Point;
        currentRenderTexture.Create();
    }

    void SetupCamera(RenderTexture rt, Vector3 pos)
    {
        if (camera == null)
        {
            var go = new GameObject("Solid Tilemap Render Camera", typeof(Camera));
            camera = go.GetComponent<Camera>();
            

        }
        
        camera.targetTexture = currentRenderTexture;
        camera.transform.position = pos;
        camera.orthographic = true;
        camera.orthographicSize = Mathf.Min(renderArea.width, renderArea.height) / 2f;
        camera.backgroundColor = Color.white;
            
        camera.cullingMask = layerMask;
        camera.targetTexture = rt;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(renderArea.center,  new Vector3(renderArea.width,renderArea.height));
    }
}