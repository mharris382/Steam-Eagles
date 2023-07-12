using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Interactions.Lights
{
    public class SwitchInteractable : Interactable
    {
        public UnityEvent<bool> onSwitched;
        public AnimatorModule animatorModule;
        public BoolReactiveProperty isOn;
        [Serializable] public class AnimatorModule
        {
            public bool enabled;
            [Required] public Animator animator;
            public string onAnimationName = "On";
            public string offAnimationName = "Off";
            // public string onBoolName = "On";
            //
            // public void Setup(SwitchInteractable switchInteractable)
            // {
            //     if(!enabled)
            //         return;
            //     switchInteractable.onSwitched.AddListener(t => animator.SetBool(onBoolName, t));
            // }
            //
            //
            public async UniTask<bool> Interact(InteractionAgent agent, Func<bool>isOn, Action<bool> onSwitched)
            {
                if (!enabled)
                    return true;
                var isOnValue = isOn();
                animator.Play(isOnValue ? onAnimationName : offAnimationName);
                //wait until animation starts
                await UniTask.WaitUntil(() => animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == (isOnValue ? onAnimationName : offAnimationName));
                //wait until animation finishes
                await UniTask.WaitUntil(() => animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != (isOnValue ? onAnimationName : offAnimationName));
                onSwitched(!isOnValue);
                return true;
            }
        }
        public override UniTask<bool> Interact(InteractionAgent agent)
        {
            return animatorModule.Interact(agent, () => isOn.Value, t => isOn.Value = t);
        }
    }
}