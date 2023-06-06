using System;
using System.Linq;
using Power;
using Power.Steam.Network;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Buildables
{
    [RequireComponent(typeof(BuildableMachine))]
    public class HypergasGenerator : MonoBehaviour
    {
        private BuildableMachine _buildableMachine;
        private HypergasEngineController _controller;

        public MachineCell inputCell;
        public MachineCell[] outputCells;

        [SerializeField] private Events events;
        
        [Serializable]
        private class Events
        {
            public UnityEvent<float> AmountStoredChanged;
            public UnityEvent<bool> ProductionStateChanged;
            
        }
        ReactiveProperty<float>_amountStored = new ReactiveProperty<float>();
        ReactiveProperty<bool> _isProducing = new ReactiveProperty<bool>();
        private HypergasEngineConfig _config;

        public float AmountStored
        {
            get => _amountStored.Value;
            set => _amountStored.Value = value;
        }

        public bool IsProducing
        {
            get => _isProducing.Value;
            set => _isProducing.Value = value;
        }

        public BuildableMachine BuildableMachine => _buildableMachine ? _buildableMachine : _buildableMachine = GetComponent<BuildableMachine>();
        
        [Inject]
        public void InjectSteamNetwork(HypergasEngineController.Factory hypergasEngineControllerFactory, HypergasEngineConfig config)
        {
           _controller = hypergasEngineControllerFactory.Create(this);
           _config = config;
        }

        private void Start()
        {
            _amountStored.Select(t => t / _config.amountStoredBeforeProduction).Subscribe(events.AmountStoredChanged.Invoke);
            _isProducing.Subscribe(events.ProductionStateChanged.Invoke);
        }

        private void Update()
        {
            if (_controller == null)
            {
                Debug.LogError($"Steam network was not injected into {name}",this);
                return;
            }
            _controller.Initialize();
            _controller.UpdateEngine(Time.deltaTime);
        }

        public Vector2Int GetInputCellPosition()
        {
            return inputCell.BuildingSpacePosition;
        }

        public Vector2Int[] GetOutputCellPositions()
        {
            return outputCells.Select(t => t.BuildingSpacePosition).ToArray();
        }
    }

    public class HypergasInstaller : Installer<HypergasInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindFactory<HypergasGenerator, HypergasEngineController, HypergasEngineController.Factory>().AsSingle().NonLazy();
            Container.BindFactory<HyperPump, HyperPumpController, HyperPumpController.Factory>().AsSingle().NonLazy();
            Container.BindFactory<SteamTurbine, SteamTurbineController, SteamTurbineController.Factory>().AsSingle().NonLazy();
            Container.BindFactory<ExhaustVent, ExhaustVentController, ExhaustVentController.Factory>().AsSingle().NonLazy();
        }
    }

    [Serializable]
    public class HypergasEngineConfig
    {
        public float amountStoredBeforeProduction = 10;
        public float consumptionRate = 1;
        
        public float pumpProductionRate = 1;
        public float pumpStorageCapacity = 10;

        public float generatorMaxConsumerRate = 5;
        public float generatorMaxProducerRate = 1;
        public float generatorStorageCapacity = 10;
        
        public float exhaustVentMaxConsumerRate = 5;
    }

    public class HypergasEngineController
    {
        public class Factory : PlaceholderFactory<HypergasGenerator, HypergasEngineController> { }
        private readonly HypergasGenerator _hypergasGenerator;
        private SteamIO.Producer.Factory _producerFactory;
        private SteamIO.Consumer.Factory _consumerFactory;
        private readonly HypergasEngineConfig _config;
        private INetwork _steamNetwork;

        private bool _isInitialized;
        
        private ISteamConsumer _consumer;
        private ISteamProducer[] _producers;
        private float __storedAmount;
        private float _usableAmount;
        private bool _isProducing;

        private float _storedAmount
        {
            get => __storedAmount;
            set
            {
                __storedAmount = Mathf.Clamp(value, 0, _config.amountStoredBeforeProduction);
                _hypergasGenerator.AmountStored = __storedAmount;
            }
        }
        private bool IsProducing
        {
            set
            {
                _isProducing = value;
                _hypergasGenerator.IsProducing = value;
                foreach (var producer in _producers)
                {
                    producer.IsActive = value;
                }

                _consumer.IsActive = true;
            }
        }
        float GetProductionRate()
        {
            if (!_isProducing)
                return 0;
            return Mathf.Min(_usableAmount, _config.consumptionRate);
        }
        
        void OnOutputProduced(float amount)
        {
            Debug.Assert(_isProducing);
            float amountToConsume = amount/_producers.Length;
            float amountToTakeFromUsable = Mathf.Min(_usableAmount, amountToConsume);
            if (amountToTakeFromUsable < _usableAmount)
            {
                _usableAmount -= amountToTakeFromUsable;
                return;
            }
            else
            {
                float amountNeededFromStored = amountToConsume - amountToConsume;
                if(amountNeededFromStored > _storedAmount)
                {
                    IsProducing = false;
                }
            }
            
            
        }
    
  
        public bool IsActive
        {
            get;
            set;
        }
        public HypergasEngineController(
            HypergasGenerator hypergasGenerator, 
            SteamIO.Producer.Factory producerFactory, 
            SteamIO.Consumer.Factory consumerFactory,
            HypergasEngineConfig config,
            INetwork steamNetwork)
        {
            _hypergasGenerator = hypergasGenerator;
            _producerFactory = producerFactory;
            _consumerFactory = consumerFactory;
            _config = config;
            _steamNetwork = steamNetwork;
        }

        public void Initialize()
        {
            if(_isInitialized)
                return;
            _isInitialized = true;
            
            _hypergasGenerator.AmountStored = 0;
            Vector2Int inputCellPosition = _hypergasGenerator.GetInputCellPosition();
            Vector2Int[] outputCellPositions = _hypergasGenerator.GetOutputCellPositions();
            _consumer = _consumerFactory.Create(inputCellPosition, ConsumptionRateGetter, OnInputConsumed);
            _producers = new ISteamProducer[outputCellPositions.Length];
            for (int i = 0; i < outputCellPositions.Length; i++)
                _producers[i] = _producerFactory.Create(outputCellPositions[i], GetProductionRate, OnOutputProduced);
            IsProducing = false;
            _consumer.IsActive = true;
        }

        public void UpdateEngine(float deltaTime)
        {
            Debug.Log("Updating engine");
        }

        float ProductionRateGetter()
        {
            return _config.consumptionRate;
        }
        float ConsumptionRateGetter()
        {
            return _config.consumptionRate;
        }
        void OnInputConsumed(float amount)
        {
            bool fullyStored = _storedAmount >= _config.amountStoredBeforeProduction;
            if (!fullyStored)
            {
                var amountToGiveToStored = Mathf.Min(_config.amountStoredBeforeProduction - _storedAmount, amount);
                _storedAmount += amountToGiveToStored;
                amount -= amountToGiveToStored;
            }
            else
            {
                IsProducing = true;    
            }
            if (amount > 0)
            {
                _usableAmount += amount;
            }
        }
    }
}