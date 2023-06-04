using Power;
using UnityEngine;
using Zenject;

namespace Buildables
{
    public class HyperPumpController
    {
        private readonly HypergasEngineConfig _config;
        private float _amountStored;
        private readonly SteamIO.Producer _producer;
        private float StorageCapacity => _config.pumpStorageCapacity;

        private float AmountStored
        {
            get => _amountStored;
            set
            {
                _amountStored = Mathf.Clamp(value, 0, StorageCapacity);
                _producer.IsActive = _amountStored > 0;
            }
        }

        public class Factory : PlaceholderFactory<HyperPump, HyperPumpController> { }

        public HyperPumpController(
            HyperPump pump,
            HypergasEngineConfig config,
            SteamIO.Producer.Factory producerFactory)
        {
            _config = config;
            Vector2Int cell = pump.GetOutputCell();
            _producer = producerFactory.Create(cell, GetProductionRate, ProduceSteam);
        }

        float GetProductionRate() => Mathf.Min(_config.pumpProductionRate, AmountStored);
        void ProduceSteam(float amount)
        {
            AmountStored -= amount;
            if(AmountStored <= 0)
                _producer.IsActive = false;
        }

        public void OnInteraction()
        {
            var amountToProduce = _config.pumpProductionRate;
            AmountStored += amountToProduce;
            _producer.IsActive = true;
        }
    }
}