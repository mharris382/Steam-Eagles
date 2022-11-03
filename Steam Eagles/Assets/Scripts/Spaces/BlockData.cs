using System;
using UnityEngine;

namespace Spaces
{
    [Obsolete("use Spaces.BlockDatabase")]
    
    public class BlockData : ScriptableObject
    {
        [SerializeField] internal StaticBlock staticBlock;
        [SerializeField] internal DynamicBlock dynamicBlock;

        public Sprite BlockSprite => staticBlock.sprite;
        public Color StaticBlockColor => staticBlock.color;

    }



}