using System;
using Power;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Buildables
{
    public class ExhaustVentController
    {
        private readonly ExhaustVent _exhaustVent;
        private readonly HypergasEngineConfig _config;

        public class Factory : PlaceholderFactory<ExhaustVent, ExhaustVentController> { }
        
        
        public ExhaustVentController(ExhaustVent exhaustVent, HypergasEngineConfig config, SteamIO.Consumer.Factory consumerFactory)
        {
            _exhaustVent = exhaustVent;
            _config = config;
            var cellPosition = exhaustVent.cell.BuildingSpacePosition;
            var consumer = consumerFactory.Create(cellPosition, GetConsumptionRate, ConsumeSteam);
            consumer.IsActive = true;
        }
        float GetConsumptionRate() => _config.exhaustVentMaxConsumerRate;

        void ConsumeSteam(float amount)
        {
            _exhaustVent.onSteamConsumed.Invoke(amount);
        }
    }
    public class SteamTurbine : MonoBehaviour
    {
        [Required,ChildGameObjectsOnly] public MachineCell inputCell;
        [Required,ChildGameObjectsOnly] public MachineCell outputCell;
        private SteamTurbineController _controller;

        [SerializeField] private Events events;
        [Serializable]
        private class Events
        {
            public UnityEvent<bool> producerActive;
            public UnityEvent<float> amountFilled;
        }

        [Inject]
        void InjectMe(SteamTurbineController.Factory controllerFactory)
        {
            _controller = controllerFactory.Create(this);
        }

        public void Feedback(float filled, bool producerIsActive)
        {
            events.amountFilled.Invoke(filled);
            events.producerActive.Invoke(producerIsActive);
        }
    }


    public class SteamTurbineController
    {
        public class Factory : PlaceholderFactory<SteamTurbine, SteamTurbineController> { }
        private readonly SteamTurbine _turbine;
        private readonly HypergasEngineConfig _config;

        private float _amountConsumed;
        private readonly SteamIO.Consumer _consumer;
        private readonly SteamIO.Producer _producer;

        
        public SteamTurbineController(SteamTurbine turbine,
            SteamIO.Producer.Factory producerFactory,
            SteamIO.Consumer.Factory consumerFactory, HypergasEngineConfig config)
        {
            _turbine = turbine;
            _config = config;
            var inputCellPosition = turbine.inputCell.BuildingSpacePosition;
            var outputCellPosition = turbine.outputCell.BuildingSpacePosition;
            _producer = producerFactory.Create(inputCellPosition, GetProductionRate, ProduceSteam);
            _consumer = consumerFactory.Create(outputCellPosition, GetConsumptionRate, ConsumeSteam);
        }
        
        float GetProductionRate()
        {
            return Mathf.Min(_config.generatorMaxProducerRate, _amountConsumed);
        }
        float GetConsumptionRate()
        {
            return Mathf.Min(_config.generatorMaxConsumerRate, _config.generatorStorageCapacity - _amountConsumed);
        }
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
    }
}