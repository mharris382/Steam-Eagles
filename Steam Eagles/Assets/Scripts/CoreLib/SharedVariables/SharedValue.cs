using UnityEngine;
using UnityEngine.Events;

namespace CoreLib
{
    //[CreateAssetMenu(menuName = "Shared Variables/Shared Value/", fileName = " Shared Value", order = -1)]
    public abstract class SharedValue<T> : ScriptableObject
    {
        [SerializeField] private T value;

        public UnityEvent<T> onValueChanged;
        public T Value
        {
            get => value;
            set
            {
                if (!value.Equals(this.value))
                {
                    this.value = value;
                    onValueChanged?.Invoke(this.value);
                }
            }
        }
    }

    //[CreateAssetMenu(menuName = "Shared Variables/Shared Value Array/", fileName = " Shared Value", order = -1)]
    public abstract class SharedValueArray<T> : ScriptableObject
    {
        [SerializeField]
        private T[] items;

        public virtual T this[int i]
        {
            get => items[i];
            set => items[i] = value;
        }

        public T[] Items => items;
    }
}