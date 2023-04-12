using FSM;
using Spine.Unity;
using UnityEngine;

namespace Characters.Animations
{
    public abstract class ToolAnimationStateBase : StateBase
    {
        private readonly Character _character;
        private readonly ToolState _characterToolState;
        private readonly SkeletonAnimation _skeletonAnimation;
        private readonly SkinController _skinController;
        private readonly Transform _aimTarget;

        public SkinController SkinController => _skinController;
        public SkeletonAnimation SkeletonAnimation => _skeletonAnimation;
        public Character Character => _character;
        public ToolState CharacterToolState => _characterToolState;
        public Transform AimTarget => _aimTarget;
        
        public ToolAnimationStateBase(Character character, 
            ToolState characterToolState, 
            SkeletonAnimation skeletonAnimation, 
            SkinController skinController,
            Transform aimTarget,
            bool b) : base(b)
        {
            _character = character;
            _characterToolState = characterToolState;
            _skeletonAnimation = skeletonAnimation;
            _skinController = skinController;
            _aimTarget = aimTarget;
        }

        public sealed override void OnEnter()
        {
            OnEquipTool();
            base.OnEnter();
        }

        public sealed override void OnLogic()
        {
            OnToolUpdate();
            base.OnLogic();
        }

        public sealed override void OnExit()
        {
            OnUnEquipTool();
            base.OnExit();
        }

        public abstract void OnEquipTool();


        public abstract void OnToolUpdate();
        
        public abstract void OnUnEquipTool();
    }
}