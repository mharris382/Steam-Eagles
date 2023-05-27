using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace CoreLib
{
    public abstract class RegistryBase<T> : IDisposable
    {
        private List<T> _values = new List<T>();
        private ReactiveProperty<int> _count;
        private Subject<T> _onValueAdded = new Subject<T>();
        private Subject<T> _onValueRemoved = new Subject<T>();
        
        private ReactiveProperty<int> Count => _count??=new ReactiveProperty<int>(_values.Count);
        public IObservable<T> OnValueAdded => _onValueAdded;
        public IObservable<T> OnValueRemoved => _onValueRemoved;
        public IReadOnlyReactiveProperty<int> ValueCount => Count;

        public RegistryBase() { }

        
        public T this[int index] { get => _values[index]; }
        public IEnumerable<T> Values => _values;
        
        public void Register(T value)
        {
            if(_values.Contains(value))
                return;
            _values.Add(value);
            ValueAdded(value);
            _onValueAdded.OnNext(value);
        }
        public void Unregister(T value)
        {
            if (_values.Remove(value))
            {
                ValueRemoved(value);
                _onValueRemoved.OnNext(value);
            }
        }


        protected virtual void ValueAdded(T value)
        {
            
        }
        protected virtual void ValueRemoved(T value)
        {
            
        }

        public void Dispose()
        {
            _count?.Dispose();
            _onValueAdded?.Dispose();
            _onValueRemoved?.Dispose();
        }
    }
    
    
}