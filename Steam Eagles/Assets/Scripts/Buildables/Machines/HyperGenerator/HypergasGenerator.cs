using System;
using System.Linq;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Zenject;

namespace Buildables
{
    
   
    [RequireComponent(typeof(BuildableMachine))]
    public class HypergasGenerator : MonoBehaviour, IMachineCustomSaveData
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
        private string _json;

        [ShowInInspector, BoxGroup("Debugging"), HideInEditorMode]
        public float AmountStored
        {
            get => _amountStored.Value;
            set => _amountStored.Value = value;
        }
        [ShowInInspector, BoxGroup("Debugging"), HideInEditorMode]
        public float ProductionRate
        {
            get;// => _amountStored.Value;
            set;// => _amountStored.Value = value;
        }
        [ShowInInspector, BoxGroup("Debugging"), HideInEditorMode]
        public float ConsumptionRate
        {
            get;// => _amountStored.Value;
            set;// => _amountStored.Value = value;
        }


        [ShowInInspector, BoxGroup("Debugging"), HideInEditorMode]
        public bool HasStartedUp
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
           if (!string.IsNullOrEmpty(_json))
           {
               _controller.LoadFromJson(_json);
               _json = null;
           }
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

        private void OnDestroy()
        {
            _controller?.Dispose();
        }

        public void LoadDataFromJson(string json)
        {
            if (_controller == null)
            {
                _json = json;
            }
            else
            {
                _controller.LoadFromJson(json);
                events.AmountStoredChanged.Invoke(_amountStored.Value);
                events.ProductionStateChanged.Invoke(_isProducing.Value);
            }
        }

        public string SaveDataToJson() => _controller.SaveToJson();
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
       [FoldoutGroup("Hypergas Generator")] public float amountStoredBeforeProduction = 10;
       [FoldoutGroup("Hypergas Generator")] public float hypergasStorageCapacity = 15;
       [FormerlySerializedAs("consumptionRate")] 
       [FoldoutGroup("Hypergas Generator")] public float hypergasConsumptionRate = 1;
       [BoxGroup("Hypergas Generator/Deterioration")] public float timeWithoutOutputBeforeGasDeterioration = 5;
       [BoxGroup("Hypergas Generator/Deterioration")] public float gasDeterateRate = 1;
       [BoxGroup("Hypergas Generator/Deterioration")] public float deteriorationInterval = 1;

       [FoldoutGroup("Hand Pump")]    public float pumpProductionRate = 1;
       [FoldoutGroup("Hand Pump")]    public float pumpStorageCapacity = 10;

        [FoldoutGroup("Steam Turbine")]  public float generatorMaxConsumerRate = 5;
        [FoldoutGroup("Steam Turbine")]  public float generatorMaxProducerRate = 1;
        [FoldoutGroup("Steam Turbine")]  public float generatorStorageCapacity = 10;
        
        [FoldoutGroup("Exhaust Vent")] public float exhaustVentMaxConsumerRate = 5;
    }
}