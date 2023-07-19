using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

namespace Buildings
{
    internal interface ITickPower
    {
        void Update(float deltaTime);
    }
    public class BuildingPowerGrid : IInitializable, ILateTickable, IDisposable
    {
        private List<ITickPower> _changesCheck = new();
        private Dictionary<BuildingCell, IPowerSupplier> _suppliers = new();
        private Dictionary<BuildingCell, IPowerConsumer> _consumers = new();
        
        private Subject<(BuildingCell, IPowerSupplier)> _onSupplierAdded = new();
        private Subject<(BuildingCell, IPowerConsumer)> _onConsumerAdded = new();
        private Subject<(BuildingCell, IPowerSupplier)> _onSupplierRemoved = new();
        private Subject<(BuildingCell, IPowerConsumer)> _onConsumerRemoved = new();
        
        
        public IObservable<(BuildingCell, IPowerSupplier)> OnSupplierAdded => _onSupplierAdded;
        public IObservable<(BuildingCell, IPowerSupplier)> OnSupplierRemoved => _onSupplierAdded;
        public IObservable<(BuildingCell, IPowerConsumer)> OnConsumerAdded => _onConsumerAdded;
        public IObservable<(BuildingCell, IPowerConsumer)> OnConsumerRemoved => _onConsumerRemoved;


        public IEnumerable<(BuildingCell cell, IPowerSupplier supplier)> GetSuppliers() => _suppliers.Select(t => (t.Key, t.Value));
        public IEnumerable<(BuildingCell cell, IPowerConsumer consumer)> GetConsumers() => _consumers.Select(t => (t.Key, t.Value));

        public IPowerSupplier GetSupplier(BuildingCell cell) => _suppliers[cell];
        public IPowerConsumer GetConsumer(BuildingCell cell) => _consumers[cell];
        
        public bool TryGetConsumer(BuildingCell cell, out IPowerConsumer consumer) => _consumers.TryGetValue(cell, out consumer);
        public bool TryGetSupplier(BuildingCell cell, out IPowerSupplier supplier) => _suppliers.TryGetValue(cell, out supplier);
        public bool HasSupplier(BuildingCell cell) => _suppliers.ContainsKey(cell);
        public bool HasConsumer(BuildingCell cell) => _consumers.ContainsKey(cell);


        public bool HasCell(BuildingCell cell) => HasSupplier(cell) || HasConsumer(cell);
        
        public bool AddSupplier(BuildingCell cell, IPowerSupplier supplier)
        {
            if (HasSupplier(cell)) return false;
            AddSupplier_Internal(cell, supplier);
            return true;
        }

        public bool AddConsumer(BuildingCell cell, IPowerConsumer consumer)
        {
            if (HasConsumer(cell)) return false;
            AddConsumer_Internal(cell, consumer);
            return true;
        }

        public bool AddSupplier(BuildingCell cell, Func<float> supplyRateGetter, Func<float, float> supply)
        {
            if (HasSupplier(cell)) return false;
            var delegateSupplier = new DelegateProducer(supplyRateGetter, supply);
            AddSupplier_Internal(cell, delegateSupplier);
            return true;
        }

        public bool AddConsumer(BuildingCell cell, Func<float> consumptionRateGetter, Action<float> consume)
        {
            if (HasConsumer(cell)) return false;
            var delegateConsumer = new DelegateConsumer(consumptionRateGetter, consume);
            AddConsumer_Internal(cell, delegateConsumer);
            return true;
        }
        
        public void RemoveSupplier(BuildingCell cell) => RemoveSupplier_Internal(cell);

        public void RemoveConsumer(BuildingCell cell) => RemoveConsumer_Internal(cell);

        private void AddSupplier_Internal(BuildingCell cell, IPowerSupplier supplier)
        {
            _suppliers.Add(cell, supplier);
            _onSupplierAdded.OnNext((cell, supplier));
        }
        
        private void AddConsumer_Internal(BuildingCell cell, IPowerConsumer consumer)
        {
            _consumers.Add(cell, consumer);
            _onConsumerAdded.OnNext((cell, consumer));
        }

        private void RemoveSupplier_Internal(BuildingCell cell)
        {
            if (!_suppliers.ContainsKey(cell)) return;
            var supplier = _suppliers[cell];
            _suppliers.Remove(cell);
            _onSupplierRemoved.OnNext((cell, supplier));
        }
        
        private void RemoveConsumer_Internal(BuildingCell cell)
        {
            if(!_consumers.ContainsKey(cell))return;
            IPowerConsumer consumer = _consumers[cell];
            _consumers.Remove(cell);
            _onConsumerRemoved.OnNext((cell, consumer));
        }

      
        private struct DelegateConsumer : IPowerConsumer, ITickPower, IDisposable
        {
            internal ReactiveProperty<float> _consumptionRate;
            private readonly Func<float> _consumptionRateGetter;
            private readonly Action<float> _consume;
            

            public DelegateConsumer(Func<float> consumptionRateGetter, Action<float> consume)
            {
                _consumptionRate = new();
                _consumptionRateGetter = consumptionRateGetter;
                _consume = consume;
            }
            public IObservable<float> GetConsumptionRateChanges() => _consumptionRate;

            public float GetConsumptionRate() => _consumptionRateGetter();
            public void Consume(float amount) => _consume(amount);

            public void Update(float deltaTime)
            {
                _consumptionRate.Value = _consumptionRateGetter();
            }

            public void Dispose()
            {
                _consumptionRate.Dispose();
            }
        }
        private struct DelegateProducer : IPowerSupplier, ITickPower, IDisposable
        {
            internal ReactiveProperty<float> _productionRate;
            private readonly Func<float> _supplyRateGetter;
            private readonly Func<float, float> _supply;
            
            public DelegateProducer(Func<float> supplyRateGetter, Func<float, float> supply)
            {
                _productionRate = new();
                _supplyRateGetter = supplyRateGetter;
                _supply = supply;
            }

            public IObservable<float> GetSupplyRateChanges() => _productionRate;

            public float GetSupplyRate() => _supplyRateGetter();
            public float Supply(float supply) => _supply(supply);

            public void Update(float deltaTime)
            {
                _productionRate.Value = _supplyRateGetter();
            }

            public void Dispose()
            {
                _productionRate.Dispose();
            }
        }

        public void Initialize()
        {
            Debug.Log("Inited BuildingPowerGrid");   
        }

        public void Dispose()
        {
            _onConsumerAdded.Dispose();
            _onConsumerRemoved.Dispose();
            _onSupplierAdded.Dispose();
            _onSupplierRemoved.Dispose();
            Debug.Log("Disposed BuildingPowerGrid");   
        }

        public void LateTick()
        {
            foreach (var tickPower in _changesCheck)
            {
                tickPower.Update(Time.deltaTime);
            }
        }
    }
}