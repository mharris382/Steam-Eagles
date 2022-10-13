using System;
using UnityEngine;
using UnityEngine.Events;

namespace CoreLib
{
    
    public abstract class SharedVariable<T> : ScriptableObject where T : class
    {
        
        [SerializeField] private T value;
        [SerializeField] private bool debugVariable;
        
        public UnityEvent<T> onValueChanged;
        private bool _hasValue;
        
        public T Value
        {
            get => value;
            set
            {
                if (value != this.value)
                {
                    
                    this.value = value;
                    onValueChanged?.Invoke(this.value);
                    if (debugVariable) Debug.Log($"SHARED VARIABLE:{name} changed (new value={value}", this);
                }
                _hasValue = this.value != null;
            }
        }

        public bool HasValue
        {
            get
            {
                return _hasValue;
            }
        }

        private void Awake()
        {
            Value = value;//this ensures that _hasValue is accurate
        }

        private void OnEnable()
        {
            Value = value;//this ensures that _hasValue is accurate
        }
    }

    public abstract class SharedArray<T> : SharedVariable<T[]> where T : class
    {
        
    }

    public abstract class SharedVariableEvents<T, TShared> : MonoBehaviour
        where T : UnityEngine.Object
        where TShared : SharedVariable<T>
    {
        [SerializeField] private TShared sharedVariable;
        
        
        public UnityEvent<T> onValueChanged;

        private void OnEnable()
        {
            sharedVariable.onValueChanged.AddListener(OnVariableChanged);
            if (sharedVariable.Value != null) OnVariableChanged(sharedVariable.Value);
        }

        private void OnDisable() => sharedVariable.onValueChanged.RemoveListener(OnVariableChanged);

        void OnVariableChanged(T variable) => onValueChanged?.Invoke(variable);
    }

    public class SharedComponentAssigner<T, TShared> : SharedVariableAssigner<T, TShared>
        where T : UnityEngine.Component
        where TShared : SharedVariable<T>
    {
        public bool checkParents;
        public bool checkChildren;
        protected override T GetVariableAssignment()
        {
            if (checkChildren)
            {
                return GetComponentInChildren<T>();
            }
            return GetComponent<T>();
        }
    }

    public class SharedComponentsAssigner<T, TShared> : SharedArrayAssigner<T, TShared>
        where T :  Component
        where TShared : SharedArray<T>
    {
        protected override T[] GetVariableAssignment() => GetComponentsInChildren<T>();
    }

    public abstract class SharedArrayAssigner<T, TShared> : MonoBehaviour
        where T : class
        where TShared : SharedArray<T>
    {
        [SerializeField] private TShared sharedArray;
        protected abstract T[] GetVariableAssignment();

        private void OnEnable() => sharedArray.Value = GetVariableAssignment();
    }
   
    public abstract class SharedVariableAssigner<T, TShared> : MonoBehaviour
        where T : UnityEngine.Object
        where TShared : SharedVariable<T>
    {
        [SerializeField] private TShared sharedVariable;
        
        protected abstract T GetVariableAssignment();

        private void OnEnable() => sharedVariable.Value = GetVariableAssignment();
    }
}