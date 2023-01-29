using FSM;
using UnityEngine;

namespace Characters.Actions.AbilityStates
{
    public class AbilityStateBase : StateBase
    {
        private readonly MonoBehaviour _abilityStateController;

        public AbilityStateBase(MonoBehaviour abilityStateController, bool needsExitTime) : base(needsExitTime)
        {
            _abilityStateController = abilityStateController;
        }

        public override void OnEnter()
        {
            _abilityStateController.enabled = true;
            base.OnEnter();
        }

        public override void OnExitRequest()
        {
            base.OnExitRequest();
        }

        public override void OnExit()
        {
            _abilityStateController.enabled = false;
            base.OnExit();
        }
    }
}