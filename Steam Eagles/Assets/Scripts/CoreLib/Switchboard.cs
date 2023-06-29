using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

namespace CoreLib
{
    public class DynamicReactiveProperty<T> : IDisposable, IReadOnlyReactiveProperty<T>
    {
        private Subject<T> _changeValue;
        private ReplaySubject<(T previous, T next)> _onSwitched = new(); 
        private ReadOnlyReactiveProperty<T> _current;

        public T Value
        {
            get => _current.Value;
            set => Set(value);
        }

        public bool HasValue => _current.HasValue;

        public IObservable<(T previous, T next)> OnSwitched => _onSwitched;

        public DynamicReactiveProperty()
        {
            _changeValue = new();
            _current = _changeValue.ToReadOnlyReactiveProperty();
        }
        public void Set(T value)
        {
            _onSwitched.OnNext((_current.Value, value));
            _changeValue.OnNext(value);
        }

        public void Dispose()
        {
            _changeValue?.Dispose();
            _current?.Dispose();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _current.Subscribe(observer);
        }
    }

    /// <summary>
    /// the point of this class is to enable a switchboard to consider the state of it's values
    /// before allowing a value to become active.  The use case for this was toolbelt slots, if
    /// a particular slot is empty, we would like the switchboard to skip that value and choose
    /// the next (or previous) valid value, if any.  We would also like to reselect whenever
    /// the the selected value becomes invalid <see cref="ISlowTickable"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SmartSwitchboard<T> : Switchboard<T> , ISlowTickable
    {
        protected abstract override bool IsValid(int index, T value);
        public void SlowTick(float deltaTime)
        {
            if (!IsValid(_values.IndexOf(Current), Current)) 
                ReplaceCurrent();
        }

        public override bool Next() => Next(0);
        public override bool Prev() => Prev(0);

        private bool Prev(int count)
        {
            if (count >= Count)
                return false;
            var i = _values.IndexOf(_currentSelected.Value)-1;
            for (int j = 0; j < Count-1; j++)
            {
                var index = (i - j) ;
                if(index < 0)
                    index = _values.Count + index;
                var value = _values[index];
                if (IsValid(index, value))
                {
                    _currentSelected.Set(value);
                    return true;
                }
            }
            return false;
        }
        private bool Next(int count)
        {
            if (count >= Count)
                return false;

            var i = _values.IndexOf(_currentSelected.Value)+1;
            for (int j = 0; j < Count-1; j++)
            {
                var index = (i + j) % Count;
                var value = _values[index];
                if (IsValid(index, value))
                {
                    _currentSelected.Set(value);
                    return true;
                }
            }
            return false;
        }

    }
     public class Switchboard<T> : IDisposable, IEnumerable<T>
    {
        internal ReactiveCollection<T> _values = new();
        internal DynamicReactiveProperty<T> _currentSelected = new();
        internal CompositeDisposable _disposable;
        private bool _isDisposed;
        public T Current => _currentSelected.Value;

        public bool HasValue => Current != null;
        public bool Contains(T value) => _values.Contains(value);

        public IObservable<T> SelectedValueStream => _currentSelected.OnSwitched.Select(t => t.next).StartWith(_currentSelected.Value);

        public int Count => _values.Count;
        public Switchboard()
        {
            _isDisposed = false;
            var cd = new CompositeDisposable();
            
            //if a value is added and we dont have a value, set that value
            _values.ObserveAdd()
                .Where(_ => !HasValue).Where(t => IsValid(t.Index, t.Value))
                .Select(t => t.Value)
                .Subscribe(_currentSelected.Set).AddTo(cd);
            
            //if the selected value is removed, try to replace with next non-null value in the list
            _values.ObserveRemove()
                .Where(_ => HasValue && _.Value.Equals(Current))
                .Subscribe(_ => ReplaceCurrent()).AddTo(cd);
            _currentSelected.OnSwitched.Subscribe(t =>
            {
                ValueDeactivated(t.previous);
                ValueActivated(t.next);
            }).AddTo(cd);
            _disposable = cd;
        }

        protected virtual bool IsValid(int index, T value) => true;

        public virtual bool Next()
        {
            if (_values.Count <= 1)
                return false;
            var i = _values.IndexOf(_currentSelected.Value)+1;
            if (i < 0) i = _values.Count-1;
            if (i >= Count) i = 0;
            _currentSelected.Set(_values[i]);
            return true;
        }
        public virtual bool Prev()
        {
            if (_values.Count <= 1)
                return false;
            var i = _values.IndexOf(_currentSelected.Value)-1;
            if (i < 0) i = _values.Count-1;
            _currentSelected.Set(_values[i]);
            return true;
        }
        public virtual bool Add(T value)
        {
            if (_isDisposed || Contains(value)) return false;
            _values.Add(value);
            return true;
        }

        public virtual bool Remove(T value)
        {
            if (_isDisposed || !Contains(value)) return false;
            bool reselect = _currentSelected.Value.Equals(value);
            if (!_values.Remove(value))
                return false;
            for (int i = Count-1; i >= 0; i--)
            {
                if (_values[i] == null) _values.RemoveAt(i);
            }

            var ind = _values.IndexOf(_currentSelected.Value);
            if (reselect)
            {
                ReplaceCurrent();
            }
            return true;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            _values?.Dispose();
            _currentSelected?.Dispose();
            _disposable.Dispose();
        }


        /// <summary>
        /// replaces current selection with any other value, otherwise sets to null
        /// </summary>
        protected void ReplaceCurrent()
        {
            for (int i = 0; i < Count; i++)
            {
                var value = _values[i];
                if (value != null && !value.Equals(_currentSelected.Value) && IsValid(i, value))
                {
                    _currentSelected.Set(value);
                    return;
                }
            }
            _currentSelected.Set(default);
        }

        protected virtual void ValueActivated(T value)
        {
            
        }

        protected virtual void ValueDeactivated(T value)
        {
            
        }

        public IEnumerator<T> GetEnumerator()
        {
            int cnt = 0;
            return _values.Where(t => IsValid(cnt++, t)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            int cnt = 0;
            return ((IEnumerable)(_values.Where(t => IsValid(cnt++, t)))).GetEnumerator();
        }
    }


    public class ComponentSwitchboard<T>  : Switchboard<T> where T : Component
    {
        
    }
}