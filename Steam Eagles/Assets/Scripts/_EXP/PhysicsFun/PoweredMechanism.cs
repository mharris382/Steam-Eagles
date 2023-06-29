using System;
using CoreLib.Interfaces;
using UniRx;
using UnityEngine;

namespace PhysicsFun
{
    [Obsolete("This will be replaced with a dedicated assembly for power and other production/consumption heavy systems")]
    /// <summary>
    /// powered mechanism is base class for machines which perform work and consume energy from a power source
    /// </summary>
    [RequireComponent(typeof(IPowerSource))]
    public abstract class PoweredMechanism : MonoBehaviour
    {
        [SerializeField] private BoolReactiveProperty isPowered;
        
        public IReadOnlyReactiveProperty<bool> IsPowered => isPowered;

        private IPowerSource _powerSource;
        public IPowerSource PowerSource => _powerSource!=null ? _powerSource : _powerSource = GetComponentInParent<IPowerSource>();
        
        public abstract float GetWork();

        protected virtual void Update()
        {
            isPowered.Value = PowerSource.AvailablePower > 0;
             
        }
    }

}