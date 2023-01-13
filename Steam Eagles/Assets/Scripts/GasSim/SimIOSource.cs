using System;
using UnityEngine;
using UnityEngine.Events;

namespace GasSim
{
    public class SimIOSource : SimIOPoint, IGasSource
    {
    
        [Range(0, GasSimulator.PRESSURE_MAX)]
        public int flowRate = 0;
        
        public UnityEvent<int> onGasAddedToSim;

        public int RatePerCell => 1;

        protected override Color GizmoColor => Color.green;

        public int SourceFlowRate
        {
            get => enabled ? flowRate : 0;
            set => flowRate = Mathf.Clamp(value, 0, GasSimulator.PRESSURE_MAX);
        }

    
        public void OnGasAddedToSim(int amountAdded)
        {
            onGasAddedToSim?.Invoke(amountAdded);
        }

        private void OnEnable()
        {
            
        }
    }
}
