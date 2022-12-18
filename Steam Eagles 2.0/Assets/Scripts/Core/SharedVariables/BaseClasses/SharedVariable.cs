using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Core.SharedVariables
{
    public abstract class SharedVariable : ScriptableObject { }

    public abstract class SharedVariable<T> : SharedVariable
    {
        [FormerlySerializedAs("value")] [SerializeField] private T _value;
        public UnityEvent<T> onValueChanged;
        [SerializeField] private bool alwaysRaiseEvent = true;
        public virtual T Value
        {
            get => _value;
            set
            {
                if(!value.Equals(this._value) || alwaysRaiseEvent) 
                {
                    this._value = value;
                    onValueChanged?.Invoke(value);    
                }
            }
        }
    }
}