using CoreLib;
using Puzzles;
using Spaces;
using UnityEngine;

namespace Characters.Actions.Inventory
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Steam Eagles/New Item Tile")]
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