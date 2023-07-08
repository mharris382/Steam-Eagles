using System;
using System.Collections.Generic;
using Buildings.Rooms;
using Cysharp.Threading.Tasks;
using Power;
using UniRx;
using UnityEngine;
using Zenject;

namespace Buildables
{
    public class HyperPumpController : IDisposable
    {
        private readonly HypergasEngineConfig _config;
        private FloatReactiveProperty _amountStored;
        private ReadOnlyReactiveProperty<bool> _isProducing;
        private ReadOnlyReactiveProperty<float> _productionRate;
        private IDisposable _disposable;
        private readonly SteamIO.Producer _producer;
        private float StorageCapacity => _config.pumpStorageCapacity;

        private float AmountStored
        {
            get => _amountStored.Value;
            set => _amountStored.Value = Mathf.Clamp(value, 0, StorageCapacity);
        }

        public class Factory : PlaceholderFactory<HyperPump, HyperPumpController> { }

        public HyperPumpController(
            HyperPump pump,
            HypergasEngineConfig config,
            SteamIO.Producer.Factory producerFactory)
        {
            _config = config;
            Vector2Int cell = pump.GetOutputCell();
            var cd = new CompositeDisposable();
            _producer = producerFactory.Create(cell, GetProductionRate, ProduceSteam);
            _amountStored = new();
            _isProducing = _amountStored.Select(amt => amt > 0.01f).ToReadOnlyReactiveProperty().AddTo(cd);
            _isProducing.Subscribe(isProducing => _producer.IsActive = isProducing).AddTo(cd);
            _productionRate = _isProducing.Select(t => t ? Mathf.Min(_amountStored.Value, _config.pumpProductionRate) : 0).ToReadOnlyReactiveProperty();
            _productionRate.Subscribe(t => pump.ProductionRate = t).AddTo(cd);
            _isProducing.Subscribe(t => pump.IsProducing = t).AddTo(cd);
            _amountStored.Subscribe(t => pump.AmountStored = t).AddTo(cd);
            _disposable = cd;
        }

        private float GetProductionRate() => Mathf.Min(_config.pumpProductionRate, AmountStored);
        private void ProduceSteam(float amount) => AmountStored -= amount;

        public void OnInteraction()
        {
            var amountToProduce = _config.pumpProductionRate;
            AmountStored += amountToProduce;
        }

        public void Dispose()
        {
            _amountStored?.Dispose();
            _isProducing?.Dispose();
            _disposable?.Dispose();
            _producer?.Dispose();
        }
    }
}