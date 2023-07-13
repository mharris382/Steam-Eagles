using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GasTexture : MonoBehaviour
{
    [SerializeField, Range(1, 5)] private int resolution = 1;
    [SerializeField, ReadOnly] private Vector2Int sizeRaw;
    [FoldoutGroup("Debugging"), ShowInInspector, PreviewField] private RenderTexture _pressureTexture;
    [FoldoutGroup("Debugging"), ShowInInspector, PreviewField] private RenderTexture _dyeTexture;
    [FoldoutGroup("Debugging"), ShowInInspector, PreviewField] private RenderTexture _velocityTexture;
    
    public DebugImages debugImages;
    public RawImage image;
    
    
    
    
    [Serializable]
    public class DebugImages
    {
        public RawImage pressureImage;
        public RawImage dyeImage;
        public RawImage velocityImage;
        
        
        public void Debug(RenderTexture pressure, RenderTexture dye, RenderTexture velocity)
        {
            if(pressureImage) pressureImage.texture = pressure;
            if(dyeImage) dyeImage.texture = dye;
            if(velocityImage) velocityImage.texture = velocity;
        }
    }
    
    
   [FoldoutGroup("Debugging")] [ShowInInspector] private Vector2Int SizeActual => sizeRaw * (resolution * resolution);
   [FoldoutGroup("Debugging")] [ShowInInspector] public Vector2Int ImageSize
    {
        get
        {
            if (!HasTexture)
            {
                ResetTexture();
                GetComponent<RoomSimTextures>().Init();
            }
            if(HasTexture) return new Vector2Int(_pressureTexture.width, _pressureTexture.height);
            return Vector2Int.zero;
        }
    }
    public int Resolution => resolution;
    
    public bool HasTexture => _pressureTexture != null && _velocityTexture != null && _dyeTexture != null;
    
    public RenderTexture RenderTexture => _pressureTexture ? _pressureTexture : _pressureTexture = GetGasTexture(sizeRaw.x, sizeRaw.y);
    public RenderTexture Velocity
    {
        get
        {
            if(_velocityTexture == null) CreateTexture(sizeRaw.x, sizeRaw.y);
            return _velocityTexture;
        }
    }

    public RenderTexture Dye
    {
        get
        {
            if(_dyeTexture == null) CreateTexture(sizeRaw.x, sizeRaw.y);
            return _dyeTexture;// ? _dyeTexture : _dyeTexture = GetGasTexture(sizeRaw.x, sizeRaw.y);
        }
    }

    [Button()]
    public void ResetTexture()
    {
        _pressureTexture = null;
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
        
        if(image) image.texture = _pressureTexture;
        return _pressureTexture;
    }

    public bool TryGetTexture(out RenderTexture texture)
    {
        texture = null;
        if (HasTexture)
        {
            texture = _pressureTexture;
            if(image) image.texture = _pressureTexture;
            return true;
        }

        return false;
    }

    void CreateTexture(int w, int h)
    {
        if (HasTexture)
        {
            _pressureTexture?.Release();
            _dyeTexture?.Release();
            _velocityTexture?.Release();
        }
        
        _pressureTexture = new RenderTexture(w, h, 0);
        _pressureTexture.enableRandomWrite = true;
        _pressureTexture.Create();
        
        _dyeTexture = new RenderTexture(w, h, 0);
        _dyeTexture.enableRandomWrite = true;
        _dyeTexture.Create();
        
        _velocityTexture = new RenderTexture(w, h, 0);
        _velocityTexture.enableRandomWrite = true;
        _velocityTexture.Create();
        
        if(image) image.texture = _pressureTexture;
        debugImages.Debug(_pressureTexture, _dyeTexture, _velocityTexture);
        
    }
    
    private bool IsTextureSizeMismatch()
    {
        var sizeActual = SizeActual;
        if (HasTexture == false) return false;
        return _pressureTexture.width != sizeActual.x || _pressureTexture.height != sizeActual.y;
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