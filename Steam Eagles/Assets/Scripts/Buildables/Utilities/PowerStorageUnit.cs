using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Buildings
{
    [Serializable]
    public class PowerStorageUnit : IDisposable
    {
        public float capacity = 50;
        
        public float inflowRate = 5;
        public float outflowRate = 5;

        [ProgressBar(0, nameof(capacity))] [SerializeField]
        private FloatReactiveProperty currStored = new();
        [FoldoutGroup("Events")] public UnityEvent<float> onStoredAmountChanged;
        [FoldoutGroup("Events")] public UnityEvent<float> onStoredAmountChangedNormalized;
        
        
        
        public float currentStored
        {
            get => currStored.Value;
            set => currStored.Value = Mathf.Clamp(value, 0, capacity);
        }
        
        public float MaxCanAddRaw => capacity - currentStored;
        public float MaxCanRemoveRaw => currentStored;


        private CompositeDisposable _cd;

        public float MaxCanRemove(float deltaTime)
        {
            return Mathf.Min(MaxCanRemoveRaw, outflowRate * deltaTime);
        }
        
        public float MaxCanAdd(float deltaTime)
        {
            return Mathf.Min(MaxCanAddRaw, inflowRate * deltaTime);
        }

        public void Setup()
        {
            if(_cd !=null) _cd.Dispose();
            _cd = new();
            currStored.StartWith(currentStored).Subscribe(t => onStoredAmountChanged?.Invoke(t)).AddTo(_cd);
            currStored.StartWith(currentStored).Select(t => t / capacity).Subscribe(t => onStoredAmountChangedNormalized?.Invoke(t)).AddTo(_cd);
        }
        public void Dispose()
        {
            _cd?.Dispose();
            _cd = null;
            currStored?.Dispose();
        }
    }
}