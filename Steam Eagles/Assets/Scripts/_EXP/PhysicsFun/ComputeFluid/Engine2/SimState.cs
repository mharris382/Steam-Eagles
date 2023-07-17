using System;
using UniRx;
using UnityEngine;

namespace _EXP.PhysicsFun.ComputeFluid.Engine2
{
    [Serializable]
    public class SimState
    {
        [SerializeField] BoolReactiveProperty isRunning = new BoolReactiveProperty(false);
        public bool IsRunning
        {
            get => isRunning.Value;
            set => isRunning.Value = value;
        }
        
        public IObservable<Unit> OnDeactivate => isRunning.Where(x => !x).AsUnitObservable();
        public IObservable<Unit> OnActivate => isRunning.Where(x => x).AsUnitObservable();
    }
}