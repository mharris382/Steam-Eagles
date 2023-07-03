using System.Collections;
using System.Collections.Generic;
using Buildings;
using Buildings.Rooms;
using Buildings.TileGrids;
using UnityEngine;

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

public class RoomCamera : MonoBehaviour
{
    private Room _room;
    private Building _building;
    private RoomTextures _roomTextures;
    
    public RoomTextures RoomTextures => _roomTextures ? _roomTextures : _roomTextures = room.GetComponent<RoomTextures>();
    public Building Building => _building ? _building : _building = room.GetComponentInParent<Building>();
    public Room room => _room ? _room : _room = GetComponent<Room>();

    public bool IsReady => RoomTextures.SolidTexture != null &&
                           RoomTextures.FoundationTexture != null &&
                           RoomTextures.WallTexture != null;
}