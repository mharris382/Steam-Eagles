using System;
using System.Linq;
using Buildings;
using Cysharp.Threading.Tasks;
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

        // public MachineCell inputCell;
        // public MachineCell[] outputCells;

        [SerializeField] private Events events;
        
        [Serializable]
        private class Events
        {
            public UnityEvent<float> AmountStoredChanged;
            public UnityEvent<float> AmountStoredChangedNormalized;
            public UnityEvent<bool> ProductionStateChanged;

            public void SubscribeTo(MonoBehaviour owner, HyperGenerator generator)
            {
                generator.StartUpShutdownStream().Subscribe(t => ProductionStateChanged.Invoke(t)).AddTo(owner);
                generator.PowerStorageUnit.StoredAmountChangedNormalized.Subscribe(t => AmountStoredChangedNormalized.Invoke(t)).AddTo(owner);
                generator.PowerStorageUnit.StoredAmountChanged.Subscribe(t => AmountStoredChanged.Invoke(t)).AddTo(owner);

            }
        }
        
        
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
            get => hyperGenerator.IsStartedUp;
            set => hyperGenerator.IsStartedUp = value;
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

        private void Start()
        {
            events.SubscribeTo(this, hyperGenerator);
            hyperGenerator.Initialize();
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
            hyperGenerator.Dispose();
        }

        #region [Save/Loading]

        public void LoadDataFromJson(string json)
        {
            _json = json;
            LoadFromJson(json);
            events.AmountStoredChanged.Invoke(AmountStored);
            events.ProductionStateChanged.Invoke(HasStartedUp);
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
                this.AmountStored = saveData.amountStored;
                this.HasStartedUp = saveData.isJumpstarted;
                if (saveData.isJumpstarted)
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
            //Container.BindFactory<HyperPump, HyperPumpController, HyperPumpController.Factory>().AsSingle().NonLazy();
            // Container.BindFactory<SteamTurbine, SteamTurbineController, SteamTurbineController.Factory>().AsSingle().NonLazy();
            //Container.BindFactory<ExhaustVent, ExhaustVentController, ExhaustVentController.Factory>().AsSingle().NonLazy();
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

    [Serializable]
    public class SteamTurbineV2
    {
        private const string ELECTRICAL = "Electrical";
        private const string STEAM = "Steam";
        private const string TAB_IO = "IO";
        private const string TAB_STORAGE = "Storage";
        private const string TAB_INFO = "Info";
        private const string TABS = "/Tabs";
        private const string DEBUG = "Debugging/";
        [SerializeField] private float steamToElectricityRatio = 2f;
        
        [TabGroup(ELECTRICAL+TABS, TAB_STORAGE)]
        [BoxGroup(ELECTRICAL)] [SerializeField] private PowerStorageUnit internalElectricalStorage;
        [TabGroup(ELECTRICAL+TABS, TAB_IO)]
        [BoxGroup(ELECTRICAL)] [Required] public OverridablePowerSupplier electricalOutput;
        
        [TabGroup(STEAM+TABS, TAB_STORAGE)]
        [BoxGroup(STEAM)] [SerializeField] private PowerStorageUnit internalSteamStorage;
        [TabGroup(STEAM+TABS, TAB_IO)]
        [BoxGroup(STEAM)] public OverridablePowerSupplier outputPipe;
        [TabGroup(STEAM+TABS, TAB_IO)]
        [BoxGroup(STEAM)] public OverridablePowerConsumer inputPipe;
        
        [TabGroup(ELECTRICAL + TABS, TAB_STORAGE)]
        [ShowInInspector, BoxGroup(ELECTRICAL), ProgressBar(0, nameof(CurrentElectricalCapacity))] public float CurrentElectricityStored
        {
            get => internalElectricalStorage.currentStored;
            set => internalElectricalStorage.currentStored = Mathf.Clamp(value, 0, CurrentElectricalCapacity);
        }
        
        [TabGroup(STEAM + TABS, TAB_STORAGE)]
        [ShowInInspector, BoxGroup(  STEAM), ProgressBar(0, nameof(CurrentSteamCapacity))] public float CurrentSteamStored
        {
            get => internalSteamStorage.currentStored;
            set => internalSteamStorage.currentStored = Mathf.Clamp(value, 0, CurrentSteamCapacity);
        }

        [TabGroup(ELECTRICAL + TABS, TAB_STORAGE)]
        [ShowInInspector, BoxGroup( ELECTRICAL)] public float CurrentElectricalCapacity => internalElectricalStorage.capacity;
        
        [TabGroup(STEAM + TABS, TAB_STORAGE)]
        [ShowInInspector, BoxGroup( STEAM)] public float CurrentSteamCapacity => internalSteamStorage.capacity;

        public float CurrentElectricityStoredNormalized => CurrentElectricityStored/ CurrentElectricalCapacity;
        public float CurrentSteamStoredNormalized => CurrentSteamStored/ CurrentSteamCapacity;
        public void Initialize()
        {
            inputPipe.SetOverride(GetConsumptionRate, ConsumeSteam);
            outputPipe.SetOverride(GetProductionRate, ReleaseSteam);
            electricalOutput.SetOverride(GetElectricalProductionRate, SupplyElectricity);

            
        }
        void ConsumeSteam(float amountOfSteamEnteringTurbine)
        {
            float amountOfElectricalProduced = amountOfSteamEnteringTurbine * steamToElectricityRatio;
            internalElectricalStorage.currentStored += amountOfElectricalProduced;
            internalSteamStorage.currentStored += amountOfSteamEnteringTurbine;
        }
            
        float ReleaseSteam(float amountOfSteamToRelease)
        {
            internalSteamStorage.currentStored -= amountOfSteamToRelease;
            return amountOfSteamToRelease;
        }
            
            
        float SupplyElectricity(float amountOfElectricityRequested)
        {
            internalElectricalStorage.currentStored -= amountOfElectricityRequested;
            return amountOfElectricityRequested;
        }

        public void Dispose()
        {
            internalSteamStorage.Dispose();
        }

        public float GetElectricalProductionRate() => internalElectricalStorage.MaxCanRemoveRaw;
        public float GetConsumptionRate() => internalSteamStorage.MaxCanAddRaw;
        public float GetProductionRate() => internalSteamStorage.MaxCanRemoveRaw;
    }
}