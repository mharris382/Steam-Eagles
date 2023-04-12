﻿using Spine.Unity;
using UnityEngine;

namespace Characters.Animations
{
    public class ToolStateDestructTool : ToolAnimationStateBase
    {
        public ToolStateDestructTool(Character character, ToolState characterToolState, SkeletonAnimation skeletonAnimation, SkinController skinController, Transform aimTarget, bool b) : base(character, characterToolState, skeletonAnimation, skinController, aimTarget, b)
        {
        }

        public override void OnEquipTool()
        {
            //throw new System.NotImplementedException();
        }

        public override void OnToolUpdate()
        {
            //throw new System.NotImplementedException();
        }

        public override void OnUnEquipTool()
        {
            //throw new System.NotImplementedException();
        }
    }
}