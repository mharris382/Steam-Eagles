using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class GasTexture : MonoBehaviour
{
    [SerializeField, Range(1, 5)] private int resolution = 1;
    [SerializeField, ReadOnly] private Vector2Int sizeRaw;
    [SerializeField, ReadOnly, PreviewField]
    private RenderTexture _renderTexture;
    public RawImage image;
    [ShowInInspector]
    private Vector2Int SizeActual => sizeRaw * (resolution * resolution);
    [ShowInInspector]
    public Vector2Int ImageSize
    {
        get
        {
            if(HasTexture) return new Vector2Int(_renderTexture.width, _renderTexture.height);
            return Vector2Int.zero;
        }
    }
    public int Resolution => resolution;
    
    public bool HasTexture => _renderTexture != null;
    
    public RenderTexture RenderTexture => _renderTexture ? _renderTexture : _renderTexture = GetGasTexture(sizeRaw.x, sizeRaw.y);

    [Button()]
    public void ResetTexture()
    {
        _renderTexture = null;
    }

    public void SetSize(int w, int h)
    {
        sizeRaw = new Vector2Int(w, h);
    }
    public RenderTexture GetGasTexture(int w, int h)
    {
        if (w <= 0 || h <= 0)
        {
            Debug.LogError($"Invalid size: {w},{h}",this);
            throw new InvalidOperationException();
        }
        SetSize(w,h);
        if (!HasTexture || IsTextureSizeMismatch())
        {
            var sizeActual = SizeActual;
            CreateTexture(sizeActual.x, sizeActual.y);
        }
        
        if(image) image.texture = _renderTexture;
        return _renderTexture;
    }

    public bool TryGetTexture(out RenderTexture texture)
    {
        texture = null;
        if (HasTexture)
        {
            texture = _renderTexture;
            if(image) image.texture = _renderTexture;
            return true;
        }

        return false;
    }

    void CreateTexture(int w, int h)
    {
        if (HasTexture) _renderTexture.Release();
        _renderTexture = new RenderTexture(w, h, 0);
        _renderTexture.enableRandomWrite = true;
        _renderTexture.Create();
        if(image) image.texture = _renderTexture;
        
    }
    
    private bool IsTextureSizeMismatch()
    {
        var sizeActual = SizeActual;
        if (HasTexture == false) return false;
        return _renderTexture.width != sizeActual.x || _renderTexture.height != sizeActual.y;
    }
    
    
    public Vector2Int GetTexelFromWorldPos(Vector3 worldPos)
    {
        var size = new Vector2(1/(float)resolution, 1/(float)resolution);
        var pos = worldPos - transform.position;
        var x = Mathf.FloorToInt(pos.x / size.x);
        var y = Mathf.FloorToInt(pos.y / size.y);
        
        return new Vector2Int(x, y);
    }

    public Vector3 GetWorldPosFromTexel(Vector2Int dataTexel)
    {
        var size = new Vector2(1/(float)resolution, 1/(float)resolution);
        var pos = new Vector3(dataTexel.x * size.x, dataTexel.y * size.y, 0);
        return pos + transform.position;
    }
}