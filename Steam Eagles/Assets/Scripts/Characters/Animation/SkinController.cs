using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace Characters.Animations
{
    public class SkinController : MonoBehaviour
    {
        [SpineAttachment()]
        public string[] removeAttachmentsForBodySkin;
        [Serializable]
        public class StateNameToSkin
        {
            public string stateName;

            [SpineSkin()]public string handSkin;
            
            public bool overrideBodySkin = false;
            
            [ShowIf(nameof(overrideBodySkin)),SpineSkin()]
            
            public string bodySkin;

            public string GetBodySkinName(SkinController controller) => overrideBodySkin ? bodySkin : controller.bodySkinName;

            [SpineAttachment()]
            public string removeAttachment1;
            [SpineAttachment()]
            public string removeAttachment2;
            public Skin Skin {get; private set; }

            public void InitSkin(SkinController controller)
            {
                var bodySkin = controller._skeletonData.FindSkin(GetBodySkinName(controller));
                var handSkin = controller._skeletonData.FindSkin(this.handSkin);
                
                Debug.Assert(bodySkin != null, $"Missing Body skin named {bodySkin} for Skin Controller {controller.name}", controller);
                Debug.Assert(handSkin != null, $"Missing Hand skin named {handSkin} for Skin Controller {controller.name}", controller);
                
                var skin = new Skin($"Skin {stateName}");
                skin.AddSkin(bodySkin);
                foreach (var s in controller.removeAttachmentsForBodySkin)
                {
                    skin.RemoveAttachment(0, s);
                }
                skin.AddSkin(handSkin);
                this.Skin = skin;
            }
            
        }
        public void UpdateState(string stateName)
        {
            if(_stateNameToSkins.TryGetValue(stateName, out var stateSkin))
            {
                _skeleton.SetSkin(stateSkin.Skin);
                _skeleton.SetSlotsToSetupPose();
                _skeletonAnimation.Skeleton.Skin = stateSkin.Skin;
                RefreshSkeletonAttachments();
            }
        }
        void RefreshSkeletonAttachments () {
            _skeletonAnimation.Skeleton.SetSlotsToSetupPose();
            _skeletonAnimation.AnimationState.Apply(_skeletonAnimation.Skeleton); //skeletonAnimation.Update(0);
        }
        
        [SpineSkin()]public string bodySkinName;
        [SerializeField] private List<StateNameToSkin> stateNameToSkins;
        
        
        private Spine.Skeleton _skeleton;
        private Spine.SkeletonData _skeletonData;
        private Spine.Skin closedHands;
        private Spine.Skin jumpHands;

        private SkeletonAnimation _skeletonAnimation;

        private Dictionary<string, StateNameToSkin> _stateNameToSkins;

        private void Awake()
        {
            _skeletonAnimation = GetComponent<SkeletonAnimation>();
            _skeleton = _skeletonAnimation.Skeleton;
            _skeletonData = _skeleton.Data;
            _stateNameToSkins = new Dictionary<string, StateNameToSkin>();
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
            foreach (var stateNameToSkin in stateNameToSkins)
            {
                stateNameToSkin.InitSkin(this);
                _stateNameToSkins.Add(stateNameToSkin.stateName, stateNameToSkin);
            }
            // Skin bodySkin = _skeletonData.FindSkin(bodySkinName);
            // closedHands = new Skin("closed-hands");
            // closedHands.CopySkin(_skeletonData.FindSkin(closedHandsSkin));
            // closedHands.CopySkin(bodySkin);
            //
            // jumpHands = new Skin("jump-hands");
            // jumpHands.CopySkin(bodySkin);
            // jumpHands.AddSkin(_skeletonData.FindSkin(jumpHandsSkin));
        }
        
        
    }
}
