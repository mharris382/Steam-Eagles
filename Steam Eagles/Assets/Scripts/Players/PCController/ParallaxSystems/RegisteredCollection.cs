using System;
using System.Collections.Generic;
using UniRx;

namespace Players.PCController.ParallaxSystems
{
    public abstract class RegisteredCollection<T>
    {
        protected readonly List<T> _items = new List<T>();
        private readonly Subject<int> _onSpriteAdded = new Subject<int>();
        private readonly Subject<int> _onSpriteRemoved = new Subject<int>();
        
        public IObservable<int> OnSpriteAdded => _onSpriteAdded;
        public IObservable<int> OnSpriteRemoved => _onSpriteRemoved;
        public IObservable<int> OnSpriteChanged => _onSpriteAdded.Merge(_onSpriteRemoved);

        public void AddObject(T obj)
        {
            if(_items.Contains(obj))
                return;
            _items.Add(obj);
            OnObjectAdded(obj);
            _onSpriteAdded.OnNext(_items.Count - 1);
        }

        public virtual int CountObjects() => _items.Count;

        public abstract void OnObjectAdded(T sprite);
        public abstract void OnObjectRemoved(T sprite);
    }
}