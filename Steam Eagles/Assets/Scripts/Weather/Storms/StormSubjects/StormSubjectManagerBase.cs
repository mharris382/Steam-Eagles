using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Zenject;

namespace Weather.Storms
{
    public abstract class StormSubjectManagerBase : IInitializable, IDisposable
    {
        private  StormSubjectsRegistry _stormSubjectsRegistry;
        private CompositeDisposable _cd;
        
        [Inject]
        public void InjectStormSubjectRegistry(StormSubjectsRegistry stormSubjectsRegistry)
        {
            _stormSubjectsRegistry = stormSubjectsRegistry;
        }
        
        public StormSubjectManagerBase()
        {
            _cd = new CompositeDisposable();
        }
        
        public void Initialize()
        {
            ListenForDynamicStormSubjects(_stormSubjectsRegistry.AddStormSubject, _stormSubjectsRegistry.RemoveStormSubject, _cd);
            foreach (var stormSubject in GetInitialStormSubjects())
            {
                AddStormSubject(stormSubject);
            }
        }

        private void AddStormSubject(IStormSubject stormSubject)
        {
            _stormSubjectsRegistry.AddStormSubject(stormSubject);
        }


        protected abstract IEnumerable<IStormSubject> GetInitialStormSubjects();

        protected abstract void ListenForDynamicStormSubjects(Action<IStormSubject> onStormSubjectAdded, Action<IStormSubject> onStormSubjectRemoved, CompositeDisposable cd);
        public void Dispose()
        {
            _cd?.Dispose();
        }
    }


    public abstract class StormSubjectManagerBase<T> : StormSubjectManagerBase
    {
        private Dictionary<T, IStormSubject> _stormSubjectCache = new Dictionary<T, IStormSubject>();
        protected abstract IStormSubject CreateStormSubject(T subject);

        protected IStormSubject GetOrCreateStormSubject(T subject)
        {
            if (HasSubjectFor(subject))
            {
                return _stormSubjectCache[subject];
            }
            if (_stormSubjectCache.ContainsKey(subject)) _stormSubjectCache.Remove(subject);
            var s = CreateStormSubject(subject);
            _stormSubjectCache.Add(subject, s);
            return s;
        }

        public IStormSubject GetSubjectFor(T subject)
        {
            if (!HasSubjectFor(subject)) return null;
            return _stormSubjectCache[subject];
        }
        public bool HasSubjectFor(T subject)
        {
            if (_stormSubjectCache.ContainsKey(subject))
            {
                return _stormSubjectCache[subject] != null;
            }

            return false;
        }
        
        protected abstract IEnumerable<T> GetInitialSubjects();

        protected sealed override IEnumerable<IStormSubject> GetInitialStormSubjects() => GetInitialSubjects().Select(GetOrCreateStormSubject);
    }
}