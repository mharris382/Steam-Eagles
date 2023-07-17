using System;
using UniRx;
using UnityEngine;

namespace Buildings
{
    
    [Serializable]
    public class HyperGenerator
    {
           
        public PowerStorageUnit internalPowerStorageUnit;
        public OverridablePowerSupplier[] outputPipes;
        public OverridablePowerConsumer[] inputPipes;

        [Range(0, 1), SerializeField] private float capacityNeededToJumpstart = 1;
        [SerializeField] private float timeWithoutInputBeforeShutdown = 5;

        private Subject<float> _onConsume = new();
        private ReactiveProperty<bool> _isStartedUp = new();
        private float _timeLastInput;
        public float BuildupNeededForStartup => internalPowerStorageUnit.capacity * capacityNeededToJumpstart;
        
        float TimeSinceInput => Time.time - _timeLastInput;
        private bool WillShutdown => IsStartedUp && TimeSinceInput > timeWithoutInputBeforeShutdown;
        public void Initialize()
        {
            foreach (var overridablePowerSupplier in outputPipes)
            {
                overridablePowerSupplier.SetOverride(GetProductionRate, Produce);
            }
            foreach (var overridablePowerConsumer in inputPipes)
            {
                overridablePowerConsumer.SetOverride(GetConsumptionRate, Consume);
            }
        }
        public float StoredAmount
        {
            get => internalPowerStorageUnit.currentStored;
            set => internalPowerStorageUnit.currentStored = value;
        }
        public float Capacity => internalPowerStorageUnit.capacity;
        public bool IsStartedUp
        {
            get => _isStartedUp.Value;
            set => _isStartedUp.Value = value;
        }
        
        public IReadOnlyPowerStorageUnit PowerStorageUnit => internalPowerStorageUnit;
        public IObservable<bool> StartUpShutdownStream() => _isStartedUp;
        
        public IObservable<float> OnConsumption() => _onConsume;
        
        public float GetConsumptionRate() => internalPowerStorageUnit.MaxCanAddRaw;

        void Consume(float amount)
        {
            internalPowerStorageUnit.currentStored += amount;
            if(StoredAmount >= BuildupNeededForStartup) IsStartedUp = true;
            ResetShutdownTimer();
            _onConsume.OnNext(amount);
        }

        private void ResetShutdownTimer()
        {
            _timeLastInput = Time.time;
        }

        public float GetProductionRate() => IsStartedUp ? internalPowerStorageUnit.MaxCanRemoveRaw : 0;

        float Produce(float amount)
        {
            if (WillShutdown)
            {
                IsStartedUp = false;
            }
            internalPowerStorageUnit.currentStored-=amount;
            return amount;
        }

        public void Tick(float deltaTime)
        {
            
        }

        public void Dispose()
        {
            internalPowerStorageUnit.Dispose();
        }
    }
}