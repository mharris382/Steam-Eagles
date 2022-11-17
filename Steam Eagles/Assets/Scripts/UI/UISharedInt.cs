using System;
using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public class UISharedInt : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI text;
        
        public SharedInt sharedInt;
        public UnityEvent<int> onValueChanged;
        private void Awake()
        {
            sharedInt.onValueChanged.AsObservable().TakeUntilDestroy(this).StartWith(sharedInt.Value).Subscribe(OnValueChanged);
            OnValueChanged(sharedInt.Value);
        }

        void OnValueChanged(int newInt)
        {
            text.text = newInt.ToString();
            onValueChanged?.Invoke(newInt);
        }
    }
}