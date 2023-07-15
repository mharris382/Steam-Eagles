using System;
using CoreLib.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

[InlineProperty, Serializable]
public class CapturedTextures
{
    [FoldoutGroup("Debugging"), ShowInInspector, LabelWidth(45), PreviewField(150, ObjectFieldAlignment.Center), LabelText("IO"), HorizontalGroup("Debugging/h1", width:0.33f)] public RenderTexture IOTexture;
    [FoldoutGroup("Debugging"), ShowInInspector, LabelWidth(45), PreviewField(150, ObjectFieldAlignment.Center), LabelText("Wall"), HorizontalGroup("Debugging/h1", width:0.33f)] public RenderTexture WallTexture;
    [FoldoutGroup("Debugging"), ShowInInspector, LabelWidth(45), PreviewField(150, ObjectFieldAlignment.Center), LabelText("Boundary"), HorizontalGroup("Debugging/h1", width:0.33f)] public RenderTexture BoundaryTexture;
}

[RequireComponent(typeof(RoomCamera), typeof(GasTexture))]
public class CapturedRoomTexture : MonoBehaviour
{



    
    
    private RoomCamera _roomCamera;
    [ShowInInspector, HideLabel] private CapturedTextures capturedTextures;
    public CapturedTextures CapturedTextures => capturedTextures ??= new();
    private RoomCamera RoomCamera => _roomCamera ? _roomCamera : _roomCamera = GetComponent<RoomCamera>();

    public RenderTexture WallTexture
    {
        get => CapturedTextures.WallTexture;
       private set => CapturedTextures.WallTexture = value;
    }
    public RenderTexture BoundaryTexture
    {
        get => CapturedTextures.BoundaryTexture;
       private set => CapturedTextures.BoundaryTexture = value;
    }
    public RenderTexture IOTexture
    {
        get => CapturedTextures.IOTexture;
       private set => CapturedTextures.IOTexture = value;
    }
    
    
    

    public bool IsReady => RoomCamera.IsReady;



    public RenderTexture CaptureLayer(CaptureLayers captureLayers,int width, int height)
    {
        if (!IsReady) return null;
        if(width <= 0 || height <= 0) return null;
        if (!TryGetTexture(captureLayers, out var texture) || !texture.SizeMatches(width, height))
        {
            if(texture!=null)texture.Release();
            texture = new RenderTexture(width, height, 1);
            texture.enableRandomWrite = true;
            texture.Create();
            SetTexture(captureLayers, texture);
        }
        RoomCamera.CaptureRoom(texture, captureLayers.GetLayerMask());
        return texture;
    }

    private bool TryGetTexture(CaptureLayers captureLayers, out RenderTexture texture)
    {
        texture = null;
        switch (captureLayers)
        {
            case CaptureLayers.WALL:
                texture = WallTexture;
                break;
            case CaptureLayers.BOUNDARY:
                texture = BoundaryTexture;
                break;
            case CaptureLayers.INPUT:
                texture = IOTexture;
                break;
        }
        return texture != null;
    }

    private bool SetTexture(CaptureLayers captureLayers, RenderTexture texture)
    {
        if (texture == null) return false;
        switch (captureLayers)
        {
            case CaptureLayers.WALL:
                WallTexture = texture;
                break;
            case CaptureLayers.BOUNDARY:
                BoundaryTexture = texture;
                break;
            case CaptureLayers.INPUT:
                IOTexture = texture;
                break;
            default:
                return false;
        }
        return true;
    }
}

public enum CaptureLayers
{
    WALL,
    BOUNDARY,
    INPUT
}


public static class CaptureLayerMasks
{
    private static LayerMask _inputLayers;
    private static LayerMask _wallLayers;
    private static LayerMask _boundaryLayers;

    public static LayerMask InputLayers => _inputLayers;
    public static LayerMask WallLayers => _wallLayers;
    public static LayerMask BoundaryLayers => _boundaryLayers;

    public static LayerMask GetLayerMask(this CaptureLayers captureLayers)
    {
        switch (captureLayers)
        {
            case CaptureLayers.WALL:
                return WallLayers;
                break;
            case CaptureLayers.BOUNDARY:
                return BoundaryLayers;
                break;
            case CaptureLayers.INPUT:
                return InputLayers;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(captureLayers), captureLayers, null);
        }
    }
    static CaptureLayerMasks()
    {
        _inputLayers = LayerMask.GetMask("Fluid");
        _wallLayers = LayerMask.GetMask("Air");
        _boundaryLayers = LayerMask.GetMask("Solids", "Ground");
    }
}