using System;
using System.Collections;
using System.Collections.Generic;
using CoreLib.MyEntities;
using UniRx;

namespace Characters.Narrative.Installers
{
    public abstract class EntityTypeTracker : IList<Entity>, IReadOnlyDictionary<string, Entity>, IDisposable
    {
        protected readonly CompositeDisposable _disposables = new CompositeDisposable();
        protected readonly List<Entity> _trackedEntities = new List<Entity>();
        private Dictionary<string, int> _entityNameToIndex = new Dictionary<string, int>();

        public abstract bool IsEntityTrackedByThis(Entity entity);

        

        public EntityTypeTracker()
        {
            MessageBroker.Default.Receive<EntityInitializedInfo>().Where(t => IsEntityTrackedByThis(t.entity)).Subscribe(OnEntityInitialized).AddTo(_disposables);
        }
        void OnEntityInitialized(EntityInitializedInfo info)
        {
            if(_trackedEntities.Contains(info.entity))
                return;
            _trackedEntities.Add(info.entity);
            _entityNameToIndex.Add(info.entity.name, _trackedEntities.Count - 1);
        }
        
        public void Dispose() => _disposables?.Dispose();

        IEnumerator<KeyValuePair<string, Entity>> IEnumerable<KeyValuePair<string, Entity>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Entity> GetEnumerator() => _trackedEntities.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_trackedEntities).GetEnumerator();

        public void Add(Entity item) => throw new InvalidOperationException();

        public void Clear()  => throw new InvalidOperationException();

        public bool Contains(Entity item) => _trackedEntities.Contains(item);

        public void CopyTo(Entity[] array, int arrayIndex) => _trackedEntities.CopyTo(array, arrayIndex);

        public bool Remove(Entity item) => throw new InvalidOperationException();

        public int Count => _trackedEntities.Count;

        public bool IsReadOnly => ((ICollection<Entity>)_trackedEntities).IsReadOnly;

        public int IndexOf(Entity item)
        {
            if (item == null) return -1;
            if(!_entityNameToIndex.ContainsKey(item.entityGUID)) return -1;
            return _entityNameToIndex[item.entityGUID];
        }

        public void Insert(int index, Entity item) =>throw new InvalidOperationException();

        public void RemoveAt(int index) => throw new InvalidOperationException();

        public Entity this[int index]
        {
            get => _trackedEntities[index];
            set => throw new InvalidOperationException();
        }

        public bool ContainsKey(string key) => _entityNameToIndex.ContainsKey(key);

        public bool TryGetValue(string key, out Entity value)
        {
            if (_entityNameToIndex.TryGetValue(key, out var index))
            {
                value = _trackedEntities[index];
                return value != null;
            }
            value = null;
            return false;
        }

        public Entity this[string key] => _trackedEntities[_entityNameToIndex[key]];
        public IEnumerable<string> Keys => _entityNameToIndex.Keys;
        public IEnumerable<Entity> Values => _trackedEntities;
    }
}