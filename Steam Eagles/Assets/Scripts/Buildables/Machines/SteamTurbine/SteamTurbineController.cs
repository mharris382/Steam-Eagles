using System;
using Power;
using UniRx;
using UnityEngine;
using Zenject;

namespace Buildables
{
    [Obsolete("Modify so that the vent logic for all vents is controlled in one place")]
    public class SteamTurbineController : IDisposable
    {
        public class Factory : PlaceholderFactory<SteamTurbine, SteamTurbineController> { }
        private readonly SteamTurbine _turbine;
        private readonly HypergasEngineConfig _config;

        
        private float _amountConsumed;
        private readonly SteamIO.Consumer _consumer;
        private readonly SteamIO.Producer _producer;


        private CompositeDisposable _disposable;
        private FloatReactiveProperty _amountStored;
        private ReadOnlyReactiveProperty<bool> _isProducing;
        private ReadOnlyReactiveProperty<bool> _isConsuming;
        private readonly ReadOnlyReactiveProperty<float> _productionRate;
        private readonly ReadOnlyReactiveProperty<float> _consumptionRate;

        public float AmountStored
        {
            get => _amountStored.Value;
            set => _amountStored.Value = Mathf.Clamp(value, 0, _config.generatorStorageCapacity);
        }
        public SteamTurbineController(SteamTurbine turbine,
            SteamIO.Producer.Factory producerFactory,
            SteamIO.Consumer.Factory consumerFactory, HypergasEngineConfig config)
        {
            _turbine = turbine;
            _config = config;
            _amountStored = new();
            _disposable = new CompositeDisposable();
            
            var inputCellPosition = turbine.inputCell.BuildingSpacePosition;
            var outputCellPosition = turbine.outputCell.BuildingSpacePosition;

            Subject<float> onConsumed = new();
            Subject<float> onProduced = new();

            _producer = producerFactory.Create(inputCellPosition, GetProductionRate, onProduced.OnNext);
            _consumer = consumerFactory.Create(outputCellPosition, GetConsumptionRate, onConsumed.OnNext);
            
            _isProducing = _amountStored.Select(t => t > 0.01f).ToReadOnlyReactiveProperty();
            _isConsuming = _amountStored.Select(t => t < _config.generatorStorageCapacity).ToReadOnlyReactiveProperty();

            _isConsuming.Subscribe(SetConsumerActive).AddTo(_disposable);
            _isProducing.Subscribe(SetProducerActive).AddTo(_disposable);
            
            _productionRate = _isProducing.Select( t=> t ? Mathf.Min(_config.generatorMaxProducerRate, AmountStored) : 0).ToReadOnlyReactiveProperty();
            _consumptionRate = _isConsuming.Select(t => t ? Mathf.Min(_config.generatorMaxConsumerRate, _config.generatorStorageCapacity-AmountStored) : 0).ToReadOnlyReactiveProperty();

            _productionRate.Subscribe(t => turbine.ProduceRate = t).AddTo(_disposable);
            _consumptionRate.Subscribe(t => turbine.ConsumeRate = t).AddTo(_disposable);

            onProduced.Subscribe(t => AmountStored -= t);
            onConsumed.Subscribe(t => AmountStored += t);

            void SetProducerActive(bool isActive)
            {
                _producer.IsActive = isActive;
                _turbine.IsProducing = isActive;
            }
            void SetConsumerActive(bool isActive)
            {
                _consumer.IsActive = isActive;
                _turbine.IsConsuming = isActive;
            }
        }
        
        float GetProductionRate() => _productionRate.Value;
        float GetConsumptionRate() => _consumptionRate.Value;

        void ProduceSteam(float amount)
        {
            _amountConsumed -= amount;
            _amountConsumed = Mathf.Clamp(_amountConsumed, 0, _config.pumpStorageCapacity);
            OnSteamChanged();
        }
        void ConsumeSteam(float amount)
        {
            _amountConsumed += amount;
            _amountConsumed = Mathf.Clamp(_amountConsumed, 0, _config.pumpStorageCapacity);
            OnSteamChanged();
        }

        private void OnSteamChanged()
        {
            _producer.IsActive = _amountConsumed > 0;
            _consumer.IsActive = true;
            _turbine.Feedback(_amountConsumed / _config.pumpStorageCapacity, _producer.IsActive);
        }

        public void Dispose()
        {
            _consumer?.Dispose();
            _producer?.Dispose();
            _disposable?.Dispose();
            _amountStored?.Dispose();
            _isProducing?.Dispose();
            _isConsuming?.Dispose();
            _productionRate?.Dispose();
            _consumptionRate?.Dispose();
        }

        public void LoadFromJson(string json)
        {
            try
            {
                SteamTurbineSaveData saveData = JsonUtility.FromJson<SteamTurbineSaveData>(json);
                AmountStored = saveData.amountStored;
            }
            catch (Exception e)
            {
                Debug.LogError("failed to load steam turbine from json");
            }
        }
        public string SaveToJson()
        {
            var saveData = new SteamTurbineSaveData();
            saveData.amountStored = AmountStored;
            return JsonUtility.ToJson(saveData);
        }
        [System.Serializable]
        class SteamTurbineSaveData
        {
            public float amountStored;
        }
    }
}