using System;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Buildables
{
    [Serializable]
    public class SteamStorageUnit : IReactiveProperty<float>
    {
        [PropertyRange(0, nameof(amountStoredMax))]
        public FloatReactiveProperty amountStored;
        [Min(1)]
        public float amountStoredMax = 100;

        [ProgressBar(0, nameof(amountStoredMax)), ShowInInspector, HideInEditorMode]
        public float CurrentAmount => Value;
        public IDisposable Subscribe(IObserver<float> observer)
        {
            return amountStored.Subscribe(observer);
        }
       
        public float Value
        {
            get => amountStored.Value;
            set => amountStored.Value = value;
        }

        public bool HasValue => amountStored.HasValue;
    }
}