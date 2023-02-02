using System;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace Characters.Animations
{
    public class SkinController : MonoBehaviour
    {
        private Spine.Skeleton _skeleton;
        private Spine.SkeletonData _skeletonData;
        private Spine.Skin closedHands;
        private Spine.Skin jumpHands;

        [SpineSkin()]public string bodySkinName;
        [SpineSkin()]public string closedHandsSkin;
        [SpineSkin()]public string jumpHandsSkin;
        private SkeletonAnimation _skeletonAnimation;

        private void Start()
        {
            _skeletonAnimation = GetComponent<SkeletonAnimation>();
            _skeleton = _skeletonAnimation.Skeleton;
            _skeletonData = _skeleton.Data;

            CreateSkins();
            _skeleton.SetSkin(closedHands);
        }
        

        public void UpdateState(CharacterSkeletonAnimator.States newState)
        {
//            Debug.Log(newState);
            if(newState == CharacterSkeletonAnimator.States.JUMP)
                _skeleton.SetSkin(jumpHands);
            else
            {
                _skeleton.SetSkin(closedHands);
            }
            _skeleton.SetSlotsToSetupPose();
        }

        private void CreateSkins()
        {
            Skin bodySkin = _skeletonData.FindSkin(bodySkinName);
            closedHands = new Skin("closed-hands");
            closedHands.CopySkin(_skeletonData.FindSkin(closedHandsSkin));
            closedHands.CopySkin(bodySkin);
            
            jumpHands = new Skin("jump-hands");
            jumpHands.CopySkin(bodySkin);
            jumpHands.AddSkin(_skeletonData.FindSkin(jumpHandsSkin));
        }
        
        
    }
}
