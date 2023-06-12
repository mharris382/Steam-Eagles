using System;
using System.Collections;
using Power;
using Power.Steam.Network;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Buildables
{
    public class HypergasEngineController : IDisposable
    {
        public class Factory : PlaceholderFactory<HypergasGenerator, HypergasEngineController> { }
        private readonly HypergasGenerator _hypergasGenerator;
        private SteamIO.Producer.Factory _producerFactory;
        private SteamIO.Consumer.Factory _consumerFactory;
        private readonly HypergasEngineConfig _config;
        private readonly CompositeDisposable _disposable;
        private INetwork _steamNetwork;

        private bool _isInitialized;
        
        private ISteamConsumer _consumer;
        private ISteamProducer[] _producers;
        private FloatReactiveProperty _storedAmount;
        private ReadOnlyReactiveProperty<bool> _hasStartedUp;
        private ReadOnlyReactiveProperty<float> _productionRate;

        private Subject<Unit> _timeOutputLastConsumed;
        private Subject<Unit> _timeInputLastConsumed;
        private float _usableAmount;

        private Coroutine _lastDeterioateCoroutine;
        private bool _disposed;

        public HypergasEngineController(
            HypergasGenerator hypergasGenerator, 
            SteamIO.Producer.Factory producerFactory, 
            SteamIO.Consumer.Factory consumerFactory,
            HypergasEngineConfig config,
            INetwork steamNetwork)
        {
            var cd = new CompositeDisposable();
            _disposed = false;
            Disposable.Create(() => _disposed = true).AddTo(cd);
            _hypergasGenerator = hypergasGenerator;
            _producerFactory = producerFactory;
            _consumerFactory = consumerFactory;
            _config = config;
            _steamNetwork = steamNetwork;
            _storedAmount = new();
            var isRunning = _storedAmount.Where(t => t > _config.amountStoredBeforeProduction).Select(_ => true)
                .Merge(_storedAmount.Where(t => t < 0.01f).Select(_ => false));
            _hasStartedUp = isRunning.ToReadOnlyReactiveProperty();
            _productionRate = isRunning.Select(t => t ? _config.generatorMaxProducerRate : 0).ToReadOnlyReactiveProperty();

            _storedAmount.Subscribe(t => hypergasGenerator.AmountStored = t).AddTo(cd);
            _hasStartedUp.Subscribe(t => _hypergasGenerator.HasStartedUp = t).AddTo(cd);
            _productionRate.Subscribe(t => _hypergasGenerator.ProductionRate = t).AddTo(cd);
            _timeOutputLastConsumed = new();
            _timeInputLastConsumed = new();
            
            Vector2Int inputCellPosition = _hypergasGenerator.GetInputCellPosition();
            Vector2Int[] outputCellPositions = _hypergasGenerator.GetOutputCellPositions();
            _consumer = _consumerFactory.Create(inputCellPosition, ConsumptionRateGetter, OnInputConsumed);
            _producers = new ISteamProducer[outputCellPositions.Length];
            for (int i = 0; i < outputCellPositions.Length; i++)
            {
                _producers[i] = _producerFactory.Create(outputCellPositions[i], GetProductionRate, OnOutputProduced);
                var i1 = i;
                _hasStartedUp.Subscribe(t => _producers[i1].IsActive = t).AddTo(cd);
            }

            _lastDeterioateCoroutine= hypergasGenerator.StartCoroutine(DeteriorateGas());
            _consumer.IsActive = true;
            _disposable = cd;
        }

        IEnumerator DeteriorateGas()
        {
            float timeLastConsumed = -1;
            float timeLastProduced = -1;
            _timeInputLastConsumed.Subscribe(_ => timeLastConsumed = Time.time);//.AddTo(cd);
            _timeOutputLastConsumed.Subscribe(_ => timeLastProduced = Time.time);//.AddTo(cd);
            while (Math.Abs(timeLastConsumed - (-1)) < Mathf.Epsilon)
            {
                yield return new WaitForSeconds(_config.deteriorationInterval);
            }
            while (true)
            {
                float timeSinceConsumed = Time.time - timeLastConsumed;
                float timeSinceProduced = Time.time - timeLastProduced;
                if (timeSinceProduced > _config.timeWithoutOutputBeforeGasDeterioration)
                {
                    Debug.Log("Deteriorating gas");
                    StoredAmount -= (_config.gasDeterateRate * _config.deteriorationInterval);
                }
                yield return new WaitForSeconds(_config.deteriorationInterval);
            }
        }

        private float StoredAmount
        {
            get => _storedAmount.Value;
            set => _storedAmount.Value = Mathf.Clamp(value, 0, _config.hypergasStorageCapacity);
        }

        float GetProductionRate() =>  _productionRate.Value;

        void OnOutputProduced(float amount)
        {
            if (_disposed)
            {
                Debug.LogWarning("Trying to produce gas after disposal", _hypergasGenerator);
                return;
            }
            if (amount <= 0.01f) return;
            amount /= (float)_producers.Length;            
            StoredAmount-= amount;
            _timeOutputLastConsumed.OnNext(Unit.Default);
        }

        public void Initialize()
        {
            return;
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
            
            _consumer.IsActive = true;
        }

        public void UpdateEngine(float deltaTime)
        {
            Debug.Log("Updating engine");
            _hypergasGenerator.ConsumptionRate = ConsumptionRateGetter();
            _hypergasGenerator.ProductionRate = ProductionRateGetter();
        }

        float ProductionRateGetter()
        {
            return _productionRate.Value;
        }
        float ConsumptionRateGetter()
        {
            return _config.hypergasConsumptionRate;
        }
        void OnInputConsumed(float amount)
        {
            if (_disposed)
            {
                Debug.LogWarning("Trying to consume gas after disposal", _hypergasGenerator);
                return;
            }
            if (amount <= 0.01f) return;
            StoredAmount += amount;
            if (amount >= 0.5f) _timeInputLastConsumed.OnNext(Unit.Default);
        }

        public void Dispose()
        {
            _disposed = true;
            if(_hypergasGenerator != null)
                _hypergasGenerator.StopCoroutine(_lastDeterioateCoroutine);
            _disposable?.Dispose();
            _storedAmount?.Dispose();
            _hasStartedUp?.Dispose();
            _productionRate?.Dispose();
            _timeOutputLastConsumed?.Dispose();
            _timeInputLastConsumed?.Dispose();
        }


        public void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var saveData = JsonUtility.FromJson<SaveData>(json);
                if (saveData.isJumpstarted)
                {
                    this.StoredAmount = Mathf.Max(saveData.amountStored, _config.amountStoredBeforeProduction+0.1f);
                }
                else
                {
                    this.StoredAmount = saveData.amountStored;
                }
                Debug.Log("Loaded hypergas engine from json");
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load from json: " + e);
            }
        }

        public string SaveToJson()
        {
            var saveData = new SaveData();
            saveData.isJumpstarted = this._hasStartedUp.Value;
            saveData.amountStored = this.StoredAmount;
            return JsonUtility.ToJson(saveData);
        }

        [Serializable]
        class SaveData
        {
            public bool isJumpstarted;
            public float amountStored;
        }
    }
}