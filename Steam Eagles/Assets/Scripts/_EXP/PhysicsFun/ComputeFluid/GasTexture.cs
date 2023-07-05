using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class GasTexture : MonoBehaviour
{
    [SerializeField, Range(1, 5)] private int resolution = 1;
    [SerializeField, ReadOnly] private Vector2Int sizeRaw;
    [SerializeField, ReadOnly, PreviewField]
    private RenderTexture _renderTexture;
    
    private Vector2Int SizeActual => sizeRaw * resolution;

    public int Resolution => resolution;
    
    public bool HasTexture => _renderTexture != null;
    
    public RenderTexture RenderTexture => _renderTexture ? _renderTexture : _renderTexture = GetGasTexture(sizeRaw.x, sizeRaw.y);

    private void ResetTexture()
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
        return _renderTexture;
    }

    public bool TryGetTexture(out RenderTexture texture)
    {
        texture = null;
        if (HasTexture)
        {
            texture = _renderTexture;
            return true;
        }

        return false;
    }

    void CreateTexture(int w, int h)
    {
        _renderTexture = new RenderTexture(w, h, 1);
        _renderTexture.enableRandomWrite = true;
        _renderTexture.Create();
    }
    
    private bool IsTextureSizeMismatch()
    {
        var sizeActual = SizeActual;
        if (HasTexture == false) return false;
        return _renderTexture.width != sizeActual.x || _renderTexture.height != sizeActual.y;
    }
}