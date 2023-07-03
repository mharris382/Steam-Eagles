using System;
using Buildings;
using Buildings.Rooms;
using Buildings.Rooms.Tracking;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RoomState),typeof(Room))]
public class RoomSimTextures : MonoBehaviour
{
    private RoomState _roomState;
    public RoomState RoomState => _roomState ? _roomState : _roomState = GetComponent<RoomState>();
    
    private BoundsLookup _boundsLookup;
    private BoundsLookup BoundsLookup => _boundsLookup ??= new BoundsLookup(RoomState.Room);
    
    private RoomTextureCreator _roomTextureCreator;
    private RoomTextureCreator RoomTextureCreator => _roomTextureCreator ??= new RoomTextureCreator(RoomState);
    
    
   [ShowInInspector, BoxGroup("Areas")] public Vector2Int SolidTextureSize => (Vector2Int)BoundsLookup.GetBounds(BuildingLayers.SOLID).size;
   [ShowInInspector, BoxGroup("Areas")] public Vector2Int WallTextureSize => (Vector2Int)BoundsLookup.GetBounds(BuildingLayers.WALL).size;
   [ShowInInspector, BoxGroup("Areas")] public Vector2Int FoundationTextureSize => (Vector2Int)BoundsLookup.GetBounds(BuildingLayers.FOUNDATION).size;

   public TextureSet textureSet;
   public DebugHelper debugHelper;
   public Vector2Int roomSize;

   [Serializable]
   public class TextureSet
   {
       public TilemapTexture foundationTexture;
       public TilemapTexture solidTexture;
       public TilemapTexture wallTexture;
       public TilemapTexture gasInputTexture;
   }
   [Serializable]
   public class TilemapTexture
   {
       public Vector2Int gridSize;
       [PreviewField(100)]
       public Texture texture;
       public TilemapTexture(Vector2Int gridSize, Texture texture)
       {
           int x = (int)(1/gridSize.x);
           int y = (int)(1/gridSize.y);
           this.gridSize = gridSize;
           this.texture = texture;
       }
   }
   
   [Serializable]
   public class DebugHelper
   {
       public float scale = 1;
       public GridLayoutGroup gridLayoutGroup;
       
       public RawImage solidTexture;
       public RawImage wallTexture;
       public RawImage foundationTexture;
       public RawImage gasInputTexture;
       public void Update(TextureSet textureSet)
       {
           gridLayoutGroup.cellSize = new Vector2(textureSet.solidTexture.gridSize.x * scale, textureSet.solidTexture.gridSize.y * scale);
           solidTexture.texture = textureSet.solidTexture.texture;
            wallTexture.texture = textureSet.wallTexture.texture;
            foundationTexture.texture = textureSet.foundationTexture.texture;
            gasInputTexture.texture = textureSet.gasInputTexture.texture;
       }
   }
   public Texture SolidTexture => textureSet.solidTexture.texture;
   [Button()]
   public void Init()
   {
       roomSize = SolidTextureSize;
       textureSet.foundationTexture = new TilemapTexture(FoundationTextureSize,
           RoomState.RoomTextures.FoundationTexture
               ? RoomState.RoomTextures.FoundationTexture
               : RoomTextureCreator.CopyFromTilemap(BuildingLayers.FOUNDATION, true));
       textureSet.solidTexture = new TilemapTexture(SolidTextureSize,
           RoomState.RoomTextures.SolidTexture
               ? RoomState.RoomTextures.SolidTexture
               : RoomTextureCreator.CopyFromTilemap(BuildingLayers.SOLID, true));
         textureSet.wallTexture = new TilemapTexture(WallTextureSize,
           RoomState.RoomTextures.WallTexture
               ? RoomState.RoomTextures.WallTexture
               : RoomTextureCreator.CopyFromTilemap(BuildingLayers.WALL, true));
         
         textureSet.gasInputTexture = new TilemapTexture(WallTextureSize,
             RoomState.RoomTextures.GasInputTexture
                 ? RoomState.RoomTextures.GasInputTexture
                 : RoomTextureCreator.CopyFromTilemap(BuildingLayers.GAS, true));
         debugHelper.Update(textureSet);
         
   }
}