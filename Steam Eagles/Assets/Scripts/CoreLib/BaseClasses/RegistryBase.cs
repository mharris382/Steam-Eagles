using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace CoreLib
{
    public interface IRegistry<T>
    {
        bool Register(T value);
        bool Unregister(T value);
        IEnumerable<T> Values { get; }
        IObservable<T> OnValueAdded { get; }
        IObservable<T> OnValueRemoved { get; }
        IReadOnlyReactiveProperty<int> ValueCount { get; }
    }
    
    public abstract class Registry<T> : IRegistry<T>, IDisposable
    {
        private List<T> _values = new List<T>();
        private ReactiveProperty<int> _count;
        private Subject<T> _onValueAdded;
        private Subject<T> _onValueRemoved;
        protected readonly CompositeDisposable cd;

        public int ListCount => _values.Count;

        private ReactiveProperty<int> Count => _count??=new ReactiveProperty<int>(_values.Count);
        public IObservable<T> OnValueAdded => _onValueAdded;
        public IObservable<T> OnValueRemoved => _onValueRemoved;
        public IReadOnlyReactiveProperty<int> ValueCount => Count;
        public CompositeDisposable Disposable => cd;

        

        public Registry()
        {
            cd = new CompositeDisposable();
            _onValueAdded = new Subject<T>();
            _onValueRemoved = new Subject<T>();
            _count = new ReactiveProperty<int>();
            _count.AddTo(cd);
            _onValueAdded.AddTo(cd);
            _onValueRemoved.AddTo(cd);
        }
        public T this[int index]
        {
            get => _values[index];
        }
        public IEnumerable<T> Values => _values;
        
        public bool Register(T value)
        {
            if(_values.Contains(value) || !CanRegister(value))
            {
                return false; 
            }
            _values.Add(value);
            AddValue(value);
            _onValueAdded.OnNext(value);
            _count.Value = _values.Count;
            return true;
        }
        public bool Unregister(T value)
        {
            if (_values.Remove(value))
            {
                _onValueRemoved.OnNext(value);
                _count.Value = _values.Count;
                RemoveValue(value);
                return true;
            }
            return false;
        }
        
        protected virtual bool CanRegister(T value)
        {
            return true;
        }
        
        protected virtual void ReRegister(T oldValue, T value)
        {
               
        }
        protected virtual void AddValue(T value)
        {
            
        }
        protected virtual void RemoveValue(T value)
        {
            
        }

        public void Dispose()
        {
            cd?.Dispose();
            OnDispose();
        }
        
        protected virtual void OnDispose(){}
    }
    
    
}