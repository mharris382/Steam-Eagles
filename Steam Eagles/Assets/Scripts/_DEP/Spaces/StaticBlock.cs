using System;
using System.Collections;
using CoreLib;
using Spaces;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using World;


[CreateAssetMenu(menuName = "Steam Eagles/Static Block")]
public class StaticBlock : ScriptableObject
{
    
    public SharedTilemap sharedTilemap;
    public SharedTilemap[] blockingTilemaps;
    
    [Header("Dynamic Block")]
    public DynamicBlock dynamicBlock;
    
    [Header("Static Block")]
    public TileBase tile;
    public Color color;
    public Sprite sprite;


    [Tooltip("How many blocks can currently be placed?  If null, blocks can always be placed")]
    public SharedInt remainingBlocks;


    private Tilemap GetBuildingTargetTilemap()
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// places a static block tile into the world (provided there is a valid active tilemap)
    /// </summary>
    /// <param name="cellCoord">tilemap cell space coordinate at which to place the block</param>
    /// <exception cref="NotImplementedException"></exception>
    public IEnumerator PlaceBlock(Vector3Int cellCoord)
    {
        var tm = GetBuildingTargetTilemap();
        
        throw new NotImplementedException();
    }

    
    /// <summary>
    /// detaches a block from the world.  Removes a static block and spawns the corresponding dynamic block
    /// </summary>
    /// <param name="cellCoord"></param>
    /// <exception cref="NotImplementedException"></exception>
    public IEnumerator RemoveBlock(Vector3Int cellCoord)
    {
        throw new NotImplementedException();
    }
    
    
}