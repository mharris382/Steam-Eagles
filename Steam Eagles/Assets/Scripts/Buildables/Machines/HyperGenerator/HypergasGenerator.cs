using System;
using System.Linq;
using Buildings;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Zenject;

namespace Buildables
{
    
   
    [RequireComponent(typeof(BuildableMachine))]
    public class HypergasGenerator : Machine<HypergasGenerator>, IMachineCustomSaveData
    {
        public HyperGenerator hyperGenerator;
        
        
        private BuildableMachine _buildableMachine;
        private HypergasEngineController _controller;

        // public MachineCell inputCell;
        // public MachineCell[] outputCells;

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

        public float Capacity
        {
            get => hyperGenerator.Capacity;
        }

        [ProgressBar(0, nameof(Capacity))]
        [ShowInInspector, BoxGroup("Debugging"), HideInEditorMode]
        public float AmountStored
        {
            get => hyperGenerator.StoredAmount;
            set => hyperGenerator.StoredAmount = value;
        }
        [ShowInInspector, BoxGroup("Debugging"), HideInEditorMode]
        public bool HasStartedUp
        {
            get { return hyperGenerator.StoredAmount > 0; }
        }
        [ShowInInspector, HorizontalGroup("Debugging/h1", LabelWidth = 75), HideInEditorMode]
        public float ProductionRate
        {
            get => hyperGenerator.GetProductionRate();// => _amountStored.Value;
        }
        [ShowInInspector, HorizontalGroup("Debugging/h1", LabelWidth = 75), HideInEditorMode]
        public float ConsumptionRate
        {
            get => hyperGenerator.GetConsumptionRate();
        }


     

        public BuildableMachine BuildableMachine => _buildableMachine ? _buildableMachine : _buildableMachine = GetComponent<BuildableMachine>();
        
        
       [Inject] public void InjectSteamNetwork(HypergasEngineConfig config)
        {
           _config = config;
        }

        private void Start()
        {
            // _amountStored.Select(t => t / _config.amountStoredBeforeProduction).Subscribe(events.AmountStoredChanged.Invoke);
            hyperGenerator.Initialize();
            //_isProducing.Subscribe(events.ProductionStateChanged.Invoke);
        }
        

        private void Update()
        {
            hyperGenerator.Tick(Time.deltaTime);
        }

        public Vector2Int GetInputCellPosition()
        {
            return default;
        }

        public Vector2Int[] GetOutputCellPositions()
        {
            return null;
        }

        private void OnDestroy()
        {
            _controller?.Dispose();
            
        }

        #region [Save/Loading]

        public void LoadDataFromJson(string json)
        {
            _json = json;
            LoadFromJson(json);
            events.AmountStoredChanged.Invoke(_amountStored.Value);
            events.ProductionStateChanged.Invoke(_isProducing.Value);
        }
        
        [Serializable]
        class SaveData
        {
            public bool isJumpstarted;
            public float amountStored;
        }
        public void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var saveData = JsonUtility.FromJson<SaveData>(json);
                if (saveData.isJumpstarted)
                {
                    this.AmountStored = Mathf.Max(saveData.amountStored, _config.amountStoredBeforeProduction+0.1f);
                }
                else
                {
                    this.AmountStored = saveData.amountStored;
                }
                Debug.Log("Loaded hypergas engine from json");
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load from json: " + e);
            }
        }
        public string SaveDataToJson()
        {
            var saveData = new SaveData();
            saveData.isJumpstarted = this.HasStartedUp;
            saveData.amountStored = this.AmountStored;
            return JsonUtility.ToJson(saveData);
        }

        #endregion

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