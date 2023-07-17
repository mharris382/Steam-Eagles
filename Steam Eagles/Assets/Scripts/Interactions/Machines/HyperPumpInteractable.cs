using Buildables;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Interactions.Machines
{
    public class HyperPumpInteractable : Interactable
    {
        [Required]
        public HyperPump hyperPump;

        public float waitTime = 1;
        [Required]
        public Animator animator;

        public string animationName;

        public override async UniTask<bool> Interact(InteractionAgent agent)
        {
            hyperPump.Interact();
            if(animator.enabled)
            {
                animator.Play(animationName);
            }
            float t = Time.time;
            await UniTask.WaitUntil(() => Time.time - t > waitTime);
            return true;
        }
    }
}