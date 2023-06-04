using System;
using System.Linq;
using Power;
using Power.Steam.Network;
using UnityEngine;
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
        public float AmountStored
        {
            get;
            set;
        }

        public BuildableMachine BuildableMachine => _buildableMachine ? _buildableMachine : _buildableMachine = GetComponent<BuildableMachine>();
        
        [Inject]
        public void InjectSteamNetwork(HypergasEngineController.Factory hypergasEngineControllerFactory)
        {
           _controller = hypergasEngineControllerFactory.Create(this);
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
        }
    }

    [Serializable]
    public class HypergasEngineConfig
    {
        public float amountStoredBeforeProduction = 10;
        public float consumptionRate = 1;
        
        public float pumpProductionRate = 1;
        public float pumpStorageCapacity = 10;
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
        private float _storedAmount;
        private float _usableAmount;
        private bool _isProducing;

        private bool IsProducing
        {
            set
            {
                _isProducing = false;
                foreach (var producer in _producers)
                {
                    producer.IsActive = value;
                }
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
            IsActive = false;
            _hypergasGenerator.AmountStored = 0;
            Vector2Int inputCellPosition = _hypergasGenerator.GetInputCellPosition();
            Vector2Int[] outputCellPositions = _hypergasGenerator.GetOutputCellPositions();
            _consumer = _consumerFactory.Create(inputCellPosition, ConsumptionRateGetter, OnInputConsumed);
            _producers = new ISteamProducer[outputCellPositions.Length];
            for (int i = 0; i < outputCellPositions.Length; i++)
                _producers[i] = _producerFactory.Create(outputCellPositions[i], ProductionRateGetter, OnOutputProduced);
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
            if (fullyStored)
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