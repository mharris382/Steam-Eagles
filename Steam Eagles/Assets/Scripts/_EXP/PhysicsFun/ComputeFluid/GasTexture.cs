using System;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GasTexture : MonoBehaviour
{
    [SerializeField, Range(1, 5)] private int resolution = 1;
    [SerializeField, ReadOnly] private Vector2Int sizeRaw;

    [FoldoutGroup("Debugging"), ShowInInspector, PreviewField(150, ObjectFieldAlignment.Center), LabelText("Gas t"), LabelWidth(42),HorizontalGroup("Debugging/h1",  width:0.25f)] private RenderTexture _pressureTexture;
    [FoldoutGroup("Debugging"), ShowInInspector, PreviewField(150, ObjectFieldAlignment.Center), LabelText("Gas t-1"), LabelWidth(42),HorizontalGroup("Debugging/h1",  width:0.25f)] private RenderTexture _pressureTexture2;
    [FoldoutGroup("Debugging"), ShowInInspector, PreviewField(150, ObjectFieldAlignment.Center), LabelText("Dye"), LabelWidth(42),HorizontalGroup("Debugging/h1",  width:0.25f)] private RenderTexture _dyeTexture;
    [FoldoutGroup("Debugging"), ShowInInspector, PreviewField(150, ObjectFieldAlignment.Center), LabelText("m/s"), LabelWidth(42),HorizontalGroup("Debugging/h1",  width:0.25f)] private RenderTexture _velocityTexture;
    
    public DebugImages debugImages;
    public RawImage image;


    private Subject<Unit> _onTextureWillBeReleased = new();
    private Subject<Unit> _onTexturesReleased = new();
    
    
    public IObservable<Unit> OnTextureWillBeReleased => _onTextureWillBeReleased;
    public IObservable<Unit> OnTexturesReleased => _onTexturesReleased;
    
    

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
    
    public RenderTexture PressurePrevious => _pressureTexture2 ? _pressureTexture2 : _pressureTexture2 = GetGasTexture(sizeRaw.x, sizeRaw.y);

    [Button()]
    public void ResetTexture()
    {
        ReleaseTextures();
        _pressureTexture = null;
        _dyeTexture = null;
        _velocityTexture = null;
    }

    public void ReleaseTextures()
    {
        _onTextureWillBeReleased.OnNext(Unit.Default);
        if(_pressureTexture!=null) _pressureTexture.Release();
        if(_velocityTexture !=null)_velocityTexture.Release();
        if(_dyeTexture != null)_dyeTexture.Release();
        _onTexturesReleased.OnNext(Unit.Default);
    }

    public void SetSize(int w, int h)
    {
        sizeRaw = new Vector2Int(w, h);
    }

    public void SwapTextures()
    {
        (_pressureTexture2, _pressureTexture) = (_pressureTexture, _pressureTexture2);
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
        if(_pressureTexture)_pressureTexture.Release();
        if(_pressureTexture2)_pressureTexture2.Release();
        if(_dyeTexture)_dyeTexture.Release();
        if(_velocityTexture)_velocityTexture.Release();
        
        _pressureTexture = new RenderTexture(w, h, 0);
        _pressureTexture.enableRandomWrite = true;
        _pressureTexture.Create();
        
        _pressureTexture2 = new RenderTexture(w, h, 0);
        _pressureTexture2.enableRandomWrite = true;
        _pressureTexture2.Create();
        
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