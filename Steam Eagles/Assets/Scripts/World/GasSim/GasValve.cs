using System;
using System.Collections.Generic;
using System.Linq;
using StateMachine;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace GasSim
{
    public class GasValve : CellHelper
    {
        public SharedTransform gasSimObject;
        public GasSimParticleSystem targetGasSim;

        public Vector2Int size = new Vector2Int(4, 4);

        [Tooltip("Slower numbers are faster")] [Range(16, 1)] [SerializeField]
        private int slowdown = 1;


        [SerializeField] private bool useConstantAmount;
        [Range(0, 16)] [SerializeField] private int constantAmount = 1;

        [Range(0, 16)] public int amountMin = 1;
        [Range(1, 16)] public int amountMax = 1;

        public UnityEvent<int> onGasAddedToTank;
        public UnityEvent<int> onGasRemovedFromTank;

        public Vector2Int Size
        {
            get => size;
            set => size = value;
        }

        [SerializeField, Range(-16, 16)] private int currentFlow;

        public int CurrentFlow
        {
            get => currentFlow;
            set
            {
                currentFlow = Mathf.Clamp(value, -15, 15);
                onFlowChanged?.Invoke(currentFlow);
            }
        }

        public FlowControl inFlow;
        public FlowControl outFlow;


        [Serializable]
        public class FlowControl
        {
            public float valueInMin;
            public float valueInMax = 1;

            public int valueOutMax = 16;

            public int GetFlowRate(float value)
            {
                float t = Mathf.InverseLerp(valueInMin, valueInMax, value);
                var i = Mathf.RoundToInt(t * valueOutMax);

                return i;
            }
        }

        public float PercentOutflow
        {
            set => CurrentFlow = outFlow.GetFlowRate(value);
        }

        public float PercentInflow
        {
            set => CurrentFlow = inFlow.GetFlowRate(value);
        }

        private int _count;

        private IGasSink _sink;
        private IGasSource _source;
        [SerializeField] private UnityEvent<int> onFlowChanged;

        private void Awake()
        {
            if (targetGasSim == null)
            {
                targetGasSim = GameObject.FindWithTag("Gas Sim").GetComponent<GasSimParticleSystem>();
            }

            Debug.Assert(targetGasSim != null, this);
            grid = targetGasSim.GetComponent<Grid>();
            _sink = new Sink(this);
            _source = new Source(this);

        }

        private void Start()
        {
            var gasTank = GetComponent<GasTank>();

            IObservable<int> desiredFlowRateChanged;
            IObservable<bool> valveIsSink;
            IObservable<bool> valveIsSource;
            if (gasTank != null)
            {
                var amountInTankChanged = gasTank.onAmountChanged.AsObservable();
                desiredFlowRateChanged =
                    onFlowChanged.AsObservable().Merge(amountInTankChanged.Select(t => CurrentFlow));
                valveIsSink = desiredFlowRateChanged.Select(t => t < 0 && gasTank.StoredAmount < gasTank.capacity);
                valveIsSource = desiredFlowRateChanged.Select(t => t > 0 && gasTank.StoredAmount > 0);
            }
            else
            {
                desiredFlowRateChanged = onFlowChanged.AsObservable();
                valveIsSink = desiredFlowRateChanged.Select(t => t < 0 && gasTank.StoredAmount < gasTank.capacity);
                valveIsSource = desiredFlowRateChanged.Select(t => t > 0 && gasTank.StoredAmount > 0);
            }

            valveIsSink.Subscribe(isSinkActive =>
            {
                if (isSinkActive)
                {
                    Debug.Log("Sink active");
                    targetGasSim.AddGasSinkToSimulation(_sink);
                    targetGasSim.RemoveGasSourceFromSimulation(_source);
                }
                else
                {
                    targetGasSim.RemoveGasSinkFromSimulation(_sink);
                }
            });
            valveIsSource.Subscribe(isSourceActive =>
            {
                if (isSourceActive)
                {
                    Debug.Log("Source active");
                    targetGasSim.AddGasSourceToSimulation(_source);
                    targetGasSim.RemoveGasSinkFromSimulation(_sink);
                }
                else
                {
                    targetGasSim.RemoveGasSourceFromSimulation(_source);
                }
            });
            if (gasTank != null)
            {
                onGasAddedToTank.AddListener(gasTank.AddGas);
                onGasRemovedFromTank.AddListener(gasTank.RemoveGas);
            }
            
            InvokeRepeating(nameof(GasSimDirectIO), 0.125f, 0.125f);
        }

        void GasSimDirectIO()
        {
            if (targetGasSim == null) return;
            
        }

        public bool diamond = false;
        public int sourceMax = 100;
        
        public virtual IEnumerable<(Vector2Int coord, int amount)> GetSourceCells()
        {
            _count++;
            if ((_count % slowdown) != 0) yield break;

            Vector2Int c0 = (Vector2Int)targetGasSim.Grid.WorldToCell(transform.position);
            int max = Mathf.Min(size.x, size.y);
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    if (diamond && x + y > max) continue;
                    //if(x+y > max) continue;
                    int amt = GetSupplyAmount();
                    
                    if (amt > 0)
                    {
                        Vector2Int offset = new Vector2Int(x, y);
                        yield return (c0 + offset, amt);
                    }
                }
            }
        }
        public virtual int GetSupplyAmount()
        {
            return useConstantAmount ? constantAmount : Random.Range(amountMin, amountMax);
        }

        class Sink : IGasSink
        {
            private GasValve owner;

            public Sink(GasValve owner)
            {
                this.owner = owner;
                this.tank = owner.GetComponent<GasTank>();
            }

            private GasTank tank;

            public IEnumerable<(Vector2Int coord, int amount)> GetSourceCells()
            {
                if (owner.CurrentFlow < 0)
                {
                    int total = 0;
                    var ownerCells = owner.GetSourceCells();
                    foreach (var valueTuple in ownerCells)
                    {
                        var amt = valueTuple.amount;
                        amt = Mathf.Max(amt, tank.StoredAmount);
                        if (owner.targetGasSim.CanAddGasToCell(valueTuple.coord, ref amt))
                        {
                            tank.StoredAmount -= amt;
                            total += amt;
                        }
                        if(total > owner.sourceMax) yield break;
                    }
                }
            }
            public void GasTakenFromSource(int amountTaken) => throw new Exception("WHY IS THIS ACTING LIKE A SOURCE!");

            public void GasAddedToSink(int amountAdded) => owner.GasAddedToSink(amountAdded);
        }
        class Source : IGasSource
        {
            private GasValve owner;
            private GasTank tank;
            public Source(GasValve owner)
            {
                this.owner = owner;
                this.tank = owner.GetComponent<GasTank>();
            }

            public IEnumerable<(Vector2Int coord, int amount)> GetSourceCells()
            {
                if (owner.currentFlow > 0)
                {
                    int total = 0;
                    var ownerCells = owner.GetSourceCells();
                    foreach (var valueTuple in ownerCells)
                    {
                        var amt = valueTuple.amount;
                        amt = Mathf.Min(amt, tank.capacity - tank.StoredAmount);
                        if (owner.targetGasSim.CanRemoveGasFromCell(valueTuple.coord, ref amt))
                        {
                            tank.StoredAmount += amt;
                            total += amt;
                        }
                        if(total >= owner.sourceMax) yield break;
                    }
                }
            }

            public void GasTakenFromSource(int amountTaken) => owner.GasTakenFromSource(amountTaken);
        }

        
        

        public virtual void GasTakenFromSource(int amountTaken)
        {
            Debug.Log("Gas Taken From Source");
            onGasRemovedFromTank?.Invoke(amountTaken);
        }

        public void GasAddedToSink(int amountAdded)
        {
            Debug.Log("Gas Added to Sink");
            onGasAddedToTank?.Invoke(amountAdded);
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(GasValve))]
    public class GasValveEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (Application.isPlaying && GUILayout.Button("Update Flow"))
            {
                ((GasValve)target).CurrentFlow = ((GasValve)target).CurrentFlow;
            }
            base.OnInspectorGUI();
        }
    }
#endif
    
}