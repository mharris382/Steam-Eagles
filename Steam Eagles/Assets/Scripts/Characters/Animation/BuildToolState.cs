using FSM;
using Spine.Unity;

namespace Characters.Animations
{
    
    public class BuildToolState : StateBase
    {
        public BuildToolState(bool needsExitTime) : base(needsExitTime)
        {
        }

        public BuildToolState(CharacterState characterState, ToolState characterToolState, SkeletonAnimation skeletonAnimation, SkinController skinController, bool b) : base(b)
        {
            
        }
    }
}