using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace _EXP.PhysicsFun.ComputeFluid
{
    public class LimitedDynamicIO : DynamicIOObject
    {
        [FoldoutGroup("Events")]
        [Tooltip("Normalized value of stored amount. 0 = empty, 1 = full")]
        public UnityEvent<float> onStoredAmountChanged;
        
        [SerializeField]  float storedAmountMax = 50;
        [FormerlySerializedAs("_currentAmount")] [SerializeField]  private FloatReactiveProperty currentAmount = new();

        [ShowInInspector, ProgressBar(0, nameof(storedAmountMax)), ReadOnly]
        private float CurrentAmount
        {
            get
            {
                return currentAmount.Value;
            }
            set
            {
                 currentAmount.Value = value;
            }
        }

        public float Percentage => CurrentAmount / storedAmountMax;
        bool IsConsumer => base.GetTargetGasIOValue() < 0;
        
        

        private void Awake()
        {
            currentAmount.Select(t => t / storedAmountMax).Subscribe(t => onStoredAmountChanged?.Invoke(t)).AddTo(this);
            if (IsConsumer)
            {
                CurrentAmount = 0;
            }
            else
            {
                CurrentAmount = storedAmountMax;
            }
        }

        public override float GetTargetGasIOValue()
        {
            if (IsConsumer) //stop when full
            {
                var available = storedAmountMax - CurrentAmount;
                return Mathf.Min(available, base.GetTargetGasIOValue());
            }
            else //stop when empty
            {
                var available = CurrentAmount;
                return Mathf.Max(-available, base.GetTargetGasIOValue());
            }
        }

        public override void OnGasIO(float gasDelta)
        {
            CurrentAmount += gasDelta;
            CurrentAmount = Mathf.Clamp(CurrentAmount, 0, storedAmountMax);
        }
    }
}