using System;
using UnityEngine;

namespace Spaces
{
    [CreateAssetMenu(menuName = "Steam Eagles/Block Data")]
    public class BlockData : ScriptableObject
    {
        [SerializeField] internal StaticBlock staticBlock;
        [SerializeField] internal DynamicBlock dynamicBlock;

        public Sprite BlockSprite => staticBlock.sprite;
        public Color StaticBlockColor => staticBlock.color;

    }



}