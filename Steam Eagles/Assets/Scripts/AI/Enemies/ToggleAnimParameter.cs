using UnityEngine;

namespace AI.Enemies
{
    public class ToggleAnimParameter: StateMachineBehaviour
    {
        public string parameterName;
        public bool value;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool(parameterName, value);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool(parameterName, !value);
        }
    }
}