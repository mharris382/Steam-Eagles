using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using UnityEngine;
#if UNITY_EDITOR
                
using UnityEditor;
#endif
namespace Characters.Animations
{
    public class SkinController : MonoBehaviour
    {
        
        [Serializable]
        public class StateNameToSkin
        {
            [InlineButton(nameof(ApplySkinInEditor))]
            public string stateName;

            [SpineSkin()]public string[] handSkin;
            
            public bool overrideBodySkin = false;
            
            
            [ShowIf(nameof(overrideBodySkin)),SpineSkin()]
            public string bodySkin;

            public string GetBodySkinName(SkinController controller) => overrideBodySkin ? bodySkin : controller.bodySkinName;
            
            public Skin Skin {get; private set; }

            public void InitSkin(SkinController controller)
            {
                var bodySkin = controller.SkeletonData.FindSkin(GetBodySkinName(controller));
                
                Debug.Assert(bodySkin != null, $"Missing Body skin named {bodySkin} for Skin Controller {controller.name}", controller);
                Debug.Assert(handSkin != null, $"Missing Hand skin named {handSkin} for Skin Controller {controller.name}", controller);
                
                var skin = new Skin($"Skin {stateName}");
                skin.AddSkin(bodySkin);
                foreach (var skinName in this.handSkin)
                {
                    skin.AddSkin(controller.SkeletonData.FindSkin(skinName));
                }
                this.Skin = skin;
            }

            [Button]
            public void ApplySkinInEditor()
            {
#if UNITY_EDITOR
                var selectedGO = Selection.activeGameObject;
                if (selectedGO == null) return;
                var controller = selectedGO.GetComponent<SkinController>();
                InitSkin(controller);
                controller.UpdateState(this.stateName);
#endif
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

        public SkeletonData SkeletonData
        {
            get
            {
                if(_skeletonData == null) Init();
                return _skeletonData;
            }
        }
        
        public Skeleton Skeleton
        {
            get
            {
                if(_skeleton == null) Init();
                return _skeleton;
            }
        }

        private void Init()
        {
            _skeletonAnimation = GetComponent<SkeletonAnimation>();
            _skeleton = _skeletonAnimation.Skeleton;
            _skeletonData = _skeleton.Data;
            _stateNameToSkins = new Dictionary<string, StateNameToSkin>();
            CreateSkins();
        }
        private void Awake()
        {
           Init();
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
