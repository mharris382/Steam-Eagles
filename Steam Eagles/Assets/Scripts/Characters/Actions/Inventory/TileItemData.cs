using Buildings.Tiles;
using CoreLib;
using PhysicsFun.DynamicBlocks;
using Puzzles;
using UnityEngine;

namespace Characters.Actions.Inventory
{
    [System.Obsolete("all inventory code is now the responsibility of the items assembly")]
    public class TileItemData : ItemData
    {
        public PuzzleTile staticBlock;
        public DynamicBlock dynamicBlock;
        public TilemapTypes targetTilemap;
        public override T GetItemData<T>()
        {
            switch (typeof(T))
            {
                case var _ when typeof(PuzzleTile) == typeof(T):
                    return staticBlock as T;
                case var _ when typeof(DynamicBlock) == typeof(T):
                    return dynamicBlock as T;
                case var _ when typeof(TilemapTypes) == typeof(T):
                    return targetTilemap as T;
                default:
                    return base.GetItemData<T>();
            }
        }
    }
}