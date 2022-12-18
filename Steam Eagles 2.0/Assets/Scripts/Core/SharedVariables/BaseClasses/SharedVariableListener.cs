using UnityEngine;
using UnityEngine.Events;

namespace Core.SharedVariables
{
    public abstract class SharedVariableListener<T, TShared> : MonoBehaviour where TShared : SharedVariable<T>
    {
        [SerializeField] private TShared sharedVariable;
        
        public UnityEvent<T> onValueChanged;
        

        private void OnEnable()
        {
            sharedVariable.onValueChanged.AddListener(OnVariableChanged);
            if (sharedVariable.Value != null) OnVariableChanged(sharedVariable.Value);
        }

        private void OnDisable() => sharedVariable.onValueChanged.RemoveListener(OnVariableChanged);

        public virtual void OnVariableChanged(T variable)
        {
            onValueChanged?.Invoke(variable);
        }
    }
}