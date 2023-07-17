using System;
using Buildings;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UniRx;
using Zenject;

namespace Buildables
{
    
    public class ExhaustVent : Machine<ExhaustVent>
    {
         public float steamConsumptionRate = 1;
        
        

        [Required, ChildGameObjectsOnly()]
        public OverridablePowerConsumer steamInput;
        

        public UnityEvent<float> onSteamConsumed;
        
        
        [ShowInInspector, ReadOnly, FoldoutGroup("Debugging"), HideInEditorMode] public int ConsumptionCount
        {
            get;
            set;
        }
        [ShowInInspector, ReadOnly, FoldoutGroup("Debugging"), HideInEditorMode] public float ConsumptionAmountTotal
        {
            get;
            set;
        }
        
        
   
        
        
        private void Awake()
        {
            steamInput.SetOverride(GetConsumptionRate, OnConsume);
        }
        
        float GetConsumptionRate() => steamConsumptionRate;
        void OnConsume(float consume)
        {
            if (consume <= 0) return;
            ConsumptionCount++;
            ConsumptionAmountTotal += consume;
            onSteamConsumed.Invoke(consume);
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