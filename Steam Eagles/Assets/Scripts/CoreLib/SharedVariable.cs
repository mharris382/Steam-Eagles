using UnityEngine;
using UnityEngine.Events;

namespace CoreLib
{
    
    public abstract class SharedVariable<T> : ScriptableObject where T : class
    {
        
        [SerializeField] private T value;
        [SerializeField] private bool debugVariable;
        
        public UnityEvent<T> onValueChanged;
        
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
            }
        }
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

    
    public abstract class SharedVariableAssigner<T, TShared> : MonoBehaviour
        where T : UnityEngine.Object
        where TShared : SharedVariable<T>
    {
        [SerializeField] private TShared sharedVariable;
        
        protected abstract T GetVariableAssignment();

        private void OnEnable() => sharedVariable.Value = GetVariableAssignment();
    }
}