﻿using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.DestructTool.Helpers
{
    
    [InfoBox("Parameters: Tool In Use {bool}\tHit Count {int}\tOn Hit {trigger}")]
    [RequireComponent(typeof(Animator))]
    public class DestructionFeedbacks : MonoBehaviour
    {
        private Animator _animator;
        private static readonly int InUse = Animator.StringToHash("Tool In Use");
        private static readonly int HitCountProperty = Animator.StringToHash("Hit Count");
        private static readonly int OnHitProperty = Animator.StringToHash("On Hit");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        
        
        public void SetToolInUseState(bool state)
        {
            _animator.SetBool(InUse, state);
        }
        
        public void SetHitCount(int tiles)
        {
            _animator.SetInteger(HitCountProperty, tiles);
        }

        public void SetHitTrigger()
        {
            _animator.SetTrigger(OnHitProperty);
        }
    }
}