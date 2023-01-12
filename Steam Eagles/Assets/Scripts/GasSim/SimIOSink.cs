using UnityEngine;
using UnityEngine.Events;

namespace GasSim
{
    public class SimIOSink : SimIOPoint, global::GasSim.IGasSink
    {
        [Range(0, GasSimulator.PRESSURE_MAX)] public int flowRate = 0;
        public UnityEvent<int> onGasRemovedFromSim;


        protected override Color GizmoColor => Color.red;

        public int SinkFlowRate
        {
            get => flowRate;
            set => flowRate = Mathf.Clamp(value, 0, GasSimulator.PRESSURE_MAX);
        }
    
        public void OnGasRemovedFromSim(int amountRemoved)
        {
            onGasRemovedFromSim?.Invoke(amountRemoved);
        }
    }
}
