using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Buildables
{
    public class ExhaustVent : Machine<ExhaustVent>
    {
        [Required,ChildGameObjectsOnly] public MachineCell cell;
        public UnityEvent<float> onSteamConsumed;
        private ExhaustVentController _controller;
        
        
        [ShowInInspector, ReadOnly, BoxGroup("Debugging"), HideInEditorMode]
        public int ConsumptionCount
        {
            get;
            set;
        }

        [ShowInInspector, ReadOnly, BoxGroup("Debugging"), HideInEditorMode]
        public float ConsumptionAmountTotal
        {
            get;
            set;
        }
        
        
        //
        // [Inject]
        // public void InjectMe(ExhaustVentController.Factory controllerFactory)
        // {
        //     _controller = controllerFactory.Create(this);
        // }

        // private void OnDestroy()
        // {
        //     _controller.Dispose();
        // }
    }
}