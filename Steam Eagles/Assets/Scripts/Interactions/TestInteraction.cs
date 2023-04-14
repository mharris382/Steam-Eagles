using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Interactions
{
    public class TestInteraction : Interactable
    {
        public float delay = 1f;
        public override async UniTask<bool> Interact(InteractionAgent agent)
        {
            Debug.Log($"{agent.name} Interacted with test interaction",this);
            await UniTask.Delay((int) (delay * 1000));
            return true;
        }
    }
}