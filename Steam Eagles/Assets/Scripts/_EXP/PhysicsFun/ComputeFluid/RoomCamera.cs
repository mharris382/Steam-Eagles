using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using Buildings;
using Buildings.Rooms;
using Buildings.Rooms.Tracking;
using Buildings.TileGrids;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;
using TextureSet = RoomSimTextures.TextureSet;
// public struct SimRect
// {
//     private Vector2Int _gridPosition;
//     public int Width { get; }
//     public int Height { get; }
//     public SimRect(BoundsInt roomArea, SimConfig config)
//     {
//         
//         var size = (Vector2Int)roomArea.size;
//         Width = size.x * config.resolution;
//         Height = size.y * config.resolution;
//     }
//
//     public override string ToString()
//     {
//         return base.ToString();
//     }
// }

public class RoomSim : MonoBehaviour
{
    public RoomTextures roomTextures;
    public VisualEffectHelper visualEffectHelper;

}
public class RoomCamera : MonoBehaviour
{
    [Required] public Camera camera;
    public float zOffset = 10;
    public int resolution = 1;

    #region [Private Variables]

    private Room _room;
    private Building _building;
    private RoomTextures _roomTextures;
    private RoomSimTextures _simTextures;
    private RenderTexture _renderTexture;
    private BoundsLookup _boundsLookup;

    #endregion

    #region [Properties]

    public RoomTextures RoomTextures => _roomTextures ? _roomTextures : _roomTextures = room.GetComponent<RoomTextures>();
    public Building Building => _building ? _building : _building = room.GetComponentInParent<Building>();
    public Room room => _room ? _room : _room = GetComponent<Room>();
    public RoomSimTextures SimTextures => _simTextures ? _simTextures : _simTextures = GetComponent<RoomSimTextures>();
    public TextureSet TextureSet => SimTextures.textureSet;
    public BoundsLookup BoundsLookup => _boundsLookup ??= new BoundsLookup(room);

    public bool IsReady => camera != null &&
                           RoomTextures.SolidTexture != null &&
                           RoomTextures.FoundationTexture != null &&
                           RoomTextures.WallTexture != null;

    #endregion

    public void CaptureRoom(RenderTexture target, LayerMask layers)
    {
        camera.targetTexture = target;
        camera.cullingMask = layers;
        var bounds = room.WorldSpaceBounds;
        var size = bounds.size;
        var orthoSize = Mathf.Min(size.x, size.y) * 0.5f;
        camera.orthographicSize = orthoSize;
        camera.transform.position = bounds.center + (Vector3.back * zOffset);
        camera.Render();
    }
}


[Serializable]
public class VisualEffectHelper
{
    public string effectParameterName = "Texture";
    public VisualEffect effect;

    public void Bind(RenderTexture simTexture, Room room)
    {
        if(string.IsNullOrEmpty(effectParameterName))return;
        if(effect==null)return;
        var bounds = room.WorldSpaceBounds;
        effect.SetTexture(effectParameterName, simTexture);
        effect.SetVector3("BoundsCenter", bounds.center);
        effect.SetVector3("BoundsSize", bounds.size);
    }
}