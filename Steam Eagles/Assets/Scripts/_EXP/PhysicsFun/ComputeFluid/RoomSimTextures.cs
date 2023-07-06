using System;
using Buildings;
using Buildings.Rooms;
using Buildings.Rooms.Tracking;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public interface ISimIOTextures
{
    public RenderTexture SimInputTexture { get; }
    public RenderTexture SimOutputTexture { get; }
}
[RequireComponent(typeof(RoomState),typeof(Room), typeof(GasTexture))]
public class RoomSimTextures : MonoBehaviour, ISimIOTextures
{
    public TextureSet textureSet;
    public DebugHelper debugHelper;
    public Vector2Int roomSize;
    
    
    private RoomState _roomState;
    private GasTexture _gasTexture;
    private BoundsLookup _boundsLookup;
    private RoomTextureCreator _roomTextureCreator;
    public RoomState RoomState => _roomState ? _roomState : _roomState = GetComponent<RoomState>();
    private BoundsLookup BoundsLookup => _boundsLookup ??= new BoundsLookup(RoomState.Room);
    private RoomTextureCreator RoomTextureCreator => _roomTextureCreator ??= new RoomTextureCreator(RoomState);
    public GasTexture GasTexture => _gasTexture ? _gasTexture : _gasTexture = GetComponent<GasTexture>();
    
    
   [ShowInInspector, BoxGroup("Areas")] public Vector2Int SolidTextureSize => (Vector2Int)BoundsLookup.GetBounds(BuildingLayers.SOLID).size;
   [ShowInInspector, BoxGroup("Areas")] public Vector2Int WallTextureSize => (Vector2Int)BoundsLookup.GetBounds(BuildingLayers.WALL).size;
   [ShowInInspector, BoxGroup("Areas")] public Vector2Int FoundationTextureSize => (Vector2Int)BoundsLookup.GetBounds(BuildingLayers.FOUNDATION).size;
   public Texture SolidTexture => textureSet.solidTexture.texture;



   [Serializable]
   public class TextureSet
   {
       public TilemapTexture foundationTexture;
       public TilemapTexture solidTexture;
       public TilemapTexture compositeBoundariesTexture;
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
       public RawImage compositeBoundariesTexture;
       public RawImage gasOutputTexture;
       public void Update(TextureSet textureSet, GasTexture gasTexture)
       {
          if(gridLayoutGroup) gridLayoutGroup.cellSize = new Vector2(textureSet.solidTexture.gridSize.x * scale, textureSet.solidTexture.gridSize.y * scale);
           if(solidTexture) solidTexture.texture = textureSet.solidTexture.texture;
            if(wallTexture) wallTexture.texture = textureSet.wallTexture.texture;
           if(foundationTexture) foundationTexture.texture = textureSet.foundationTexture.texture;
           if(gasInputTexture) gasInputTexture.texture = textureSet.gasInputTexture.texture;
           if(compositeBoundariesTexture) compositeBoundariesTexture.texture = textureSet.compositeBoundariesTexture.texture;
           if(gasOutputTexture)gasOutputTexture.texture = gasTexture.RenderTexture;
       }
   }

   [Button()]
   public void Init()
   {
       roomSize = SolidTextureSize;
       InitTilemapTextures();
       InitGasTexture();
       debugHelper.Update(textureSet, GasTexture);
         
   }

   private void InitTilemapTextures()
   {
       textureSet.foundationTexture = new TilemapTexture(FoundationTextureSize, RoomState.RoomTextures.FoundationTexture ? RoomState.RoomTextures.FoundationTexture : RoomTextureCreator.CopyFromTilemap(BuildingLayers.FOUNDATION, true));
       textureSet.solidTexture = new TilemapTexture(SolidTextureSize, RoomState.RoomTextures.SolidTexture ? RoomState.RoomTextures.SolidTexture : RoomTextureCreator.CopyFromTilemap(BuildingLayers.SOLID, true));
       textureSet.wallTexture = new TilemapTexture(WallTextureSize, RoomState.RoomTextures.WallTexture ? RoomState.RoomTextures.WallTexture : RoomTextureCreator.CopyFromTilemap(BuildingLayers.WALL, true));
       textureSet.gasInputTexture = new TilemapTexture(WallTextureSize, RoomState.RoomTextures.GasInputTexture ? RoomState.RoomTextures.GasInputTexture : RoomTextureCreator.CopyFromTilemap(BuildingLayers.GAS, true));
       textureSet.compositeBoundariesTexture = new TilemapTexture(SolidTextureSize, RoomTextureCreator.CopyFromMultipleTilemaps(true,BuildingLayers.SOLID, BuildingLayers.FOUNDATION));
   }

   private void InitGasTexture()
   {
       var gasTex = GasTexture;
       gasTex.GetGasTexture(roomSize.x, roomSize.y);
   }

 

   public RenderTexture SimInputTexture => textureSet.gasInputTexture.texture as RenderTexture;
   public RenderTexture SimOutputTexture => textureSet.wallTexture.texture as RenderTexture;
   
}