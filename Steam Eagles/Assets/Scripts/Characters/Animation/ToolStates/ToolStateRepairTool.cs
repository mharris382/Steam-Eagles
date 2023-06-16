using FSM;
using Spine.Unity;
using SteamEagles.Characters;
using UnityEngine;

namespace Characters.Animations
{
    public class ToolStateRepairTool : ToolAnimationStateBase
    {
        public ToolStateRepairTool(CharacterState characterState, ToolState characterToolState, SkeletonAnimation skeletonAnimation, SkinController skinController, Transform aimTarget, StateMachine defaultFSM, bool b) : base(characterState, characterToolState, skeletonAnimation, skinController, aimTarget,defaultFSM, b)
        {
        }

        public override void OnEquipTool()
        {
           // throw new System.NotImplementedException();
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