using System;
using GasSim;
using UniRx;
using UnityEngine;
using UnityEngine.Events;


namespace Characters
{
    public class Valve : MonoBehaviour
    {
        [System.Serializable]
        public class Events
        {
            
            public UnityEvent<float> onValvePercentChanged;
            
            public UnityEvent<float> onValvePushAmountChanged;
            
            public UnityEvent<float> onValvePullAmountChanged;
            
            [Tooltip("Called when valve is opened in pull mode")] public UnityEvent onValvePulling;
            [Tooltip("Called when valve is opened in push mode")] public UnityEvent onValvePushing;
            [Tooltip("Called when valve is closed")] public UnityEvent onValveClosed;
        }

        public Events events;

        public GasTank attachedGasTank;

        [Header("Settings")]
        [Range(1, 5)]
        [SerializeField] private int discreteSteps = 5;
        [SerializeField] private bool clampToSteps = true;
        
        [Header("DEBUG")]
        [SerializeField] private int _valveStep;
        [SerializeField] private float _valvePercentOpen;
        
        
        /// <summary>
        /// float value with full range of control over the valve. range is -1 to 1
        /// <para>1 is open with gas being pushed out</para>
        /// <para>-1 is open with gas being sucked in</para>
        /// <para>0 is closed</para>
        /// </summary>
        public float ValvePercentOpen
        {
            get => _valvePercentOpen;
            set {
                value = Mathf.Clamp(value, -1, 1);
                if (Mathf.Abs(_valvePercentOpen - value) > Mathf.Epsilon)
                {
                    _valvePercentOpen = value;
                    _valveStep = Mathf.RoundToInt(_valvePercentOpen * discreteSteps);
                    events.onValvePercentChanged.Invoke(_valvePercentOpen);
                }
            }
        }

        /// <summary>
        /// integer value with discrete steps. range is -discreteSteps to discreteSteps.
        /// controls the valve in the same way as ValvePercentOpen but at discrete amounts
        /// </summary>
        public int ValveStep
        {
            get => _valveStep;
            set
            {
                value = Mathf.Clamp(value, -discreteSteps, discreteSteps);
                if (_valveStep != value)
                {
                    _valveStep = value;
                    _valvePercentOpen = _valveStep / (float) discreteSteps;
                    events.onValvePercentChanged.Invoke(_valvePercentOpen);
                }
            }
        }
        public ValveDirection Direction
        {
            get
            {
                if (_valvePercentOpen == 0) return ValveDirection.CLOSED;
                if (_valvePercentOpen > 0)
                {
                    return ValveDirection.OPEN_PUSH;
                }
                return ValveDirection.OPEN_PULL;
            }
        }
        
        public int DiscreteSteps => discreteSteps;
        
        public enum ValveDirection
        {
            CLOSED = 0,
            OPEN_PUSH = 1,
            OPEN_PULL = -1
        }


        private void Awake()
        {
            var onValveChanged = events.onValvePercentChanged.AsObservable().Select(t => (Direction, ValveStep, t));
            onValveChanged.Where(t => t.Item1 == ValveDirection.OPEN_PUSH).Subscribe(t => events.onValvePushAmountChanged?.Invoke(t.t));
            onValveChanged.Where(t => t.Item1 == ValveDirection.OPEN_PULL).Subscribe(t => events.onValvePullAmountChanged?.Invoke(t.t));
            
            onValveChanged.Where(t => t.Item1 == ValveDirection.CLOSED).Subscribe(t => events.onValveClosed?.Invoke());
            onValveChanged.Where(t => t.Item1 == ValveDirection.OPEN_PUSH).DistinctUntilChanged().Subscribe(t => events.onValvePushing?.Invoke());
            onValveChanged.Where(t => t.Item1 == ValveDirection.OPEN_PULL).DistinctUntilChanged().Subscribe(t => events.onValvePulling?.Invoke());
            if (attachedGasTank != null)
            {
                attachedGasTank.OnAmountNormalizedChanged.AsObservable().Subscribe(OnGasTankAmountChanged).AddTo(this);
            }
        }

        void OnGasTankAmountChanged(float amountFull)
        {
            if(ValveStep < 0 && amountFull >= 1)
            {
                ValveStep = 0;
            }
            else if(ValveStep > 0 && amountFull <= 0)
            {
                ValveStep = 0;
            }
        }

        /// <summary>
        /// nudges the value in the desired direction by 1 discrete step
        /// </summary>
        /// <param name="direction"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void NudgeValve(ValveDirection direction)
        {
            switch (direction)
            {
                case ValveDirection.CLOSED:
                    break;
                case ValveDirection.OPEN_PUSH:
                    if(attachedGasTank != null && attachedGasTank.StoredAmountNormalized <= 0)
                    {
                        ValveStep = 0;
                        return;
                    }
                    NudgeValveOpenPushing();
                    break;
                case ValveDirection.OPEN_PULL:
                    if (attachedGasTank != null && attachedGasTank.StoredAmountNormalized >= 1)
                    {
                        ValveStep = 0;
                        return;
                    }
                    NudgeValveOpenPulling();
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
        
        
        
        private void NudgeValveOpenPushing()
        {
            switch (Direction)
            {
                case ValveDirection.CLOSED:
                case ValveDirection.OPEN_PUSH:
                    ValveStep++;
                    break;
                case ValveDirection.OPEN_PULL:
                    ValveStep = 0;
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// if the valve is open in push mode, close it.  Otherwise increase the amount of pulling the valve is doing
        /// </summary>
        private void NudgeValveOpenPulling()
        {
            switch (Direction)
            {
                    
                case ValveDirection.OPEN_PUSH:
                    ValveStep = 0;
                    break;
                case ValveDirection.CLOSED:
                case ValveDirection.OPEN_PULL:
                    ValveStep--;
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }


        

      
    }


    
}