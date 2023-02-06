using System;
using CoreLib.SharedVariables;
using StateMachine;
using UnityEngine;
using Utilities;

namespace CoreLib
{
    public class GameFxInvoker : MonoBehaviour
    {
        public GameFX fx;
        public bool playOnAwake;
        public SharedTransform owner;
        public SharedTransform target;


        public Transform Owner =>owner==null ? null : owner.Value;
        public Transform Target => target == null || !target.HasValue ? this.transform : target.Value;


        private void Awake()
        {
            if (playOnAwake)
            {
                InvokeGameFx();
            }
        }

        public void InvokeGameFx()
        {
            if (Owner == null)
            {
                return;
            }
            fx.SpawnEffectFrom(Owner, Target);
        }
    }
}