using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using UniRx;
using UnityEngine;

namespace Weather.Storms
{
    public class StormSubjectsRegistry
    {
        private readonly GlobalStormConfig _stormConfig;
        private readonly StormSubject.Factory _stormSubjectFactory;

        private readonly Dictionary<IStormSubject, StormSubject> _stormSubjects =
            new Dictionary<IStormSubject, StormSubject>();

        private ReactiveCollection<StormSubject> _stormSubjectsCollection = new ReactiveCollection<StormSubject>();
        
        public IObservable<StormSubject> OnSubjectAdded => _stormSubjectsCollection.ObserveAdd().Select(t => t.Value);
        public IObservable<StormSubject> OnSubjectRemoved => _stormSubjectsCollection.ObserveRemove().Select(t => t.Value);

        public StormSubject GetSubject(int index) => _stormSubjectsCollection[index];

        public StormSubjectsRegistry(GlobalStormConfig stormConfig, StormSubject.Factory stormSubjectFactory)
        {
            _stormConfig = stormConfig;
            _stormSubjectFactory = stormSubjectFactory;
        }

        public int Count => _stormSubjects.Count();
        public void AddStormSubject(IStormSubject stormSubject)
        {
            if (stormSubject == null)
                return;
            if (_stormSubjects.ContainsKey(stormSubject))
            {
                _stormConfig.Log($"Already Registered Storm Subject {stormSubject}".Bolded());
                Debug.LogWarning($"Already Registered Storm Subject {stormSubject}".Bolded());
                throw new NotImplementedException();
            }

            _stormConfig.Log($"Removed Storm Subject {stormSubject}");
            _stormSubjects.Add(stormSubject, _stormSubjectFactory.Create(stormSubject));
            _stormSubjectsCollection.Add(_stormSubjects[stormSubject]);
        }

        public void RemoveStormSubject(IStormSubject stormSubject)
        {
            if (!_stormSubjects.ContainsKey(stormSubject))
                return;
            _stormSubjectsCollection.Remove(_stormSubjects[stormSubject]);
            _stormSubjects[stormSubject].Dispose();
            _stormSubjects.Remove(stormSubject);
            _stormConfig.Log($"Removed Storm Subject {stormSubject}");
        }


        public IEnumerable<StormSubject> AllSubjects() => _stormSubjects.Values;
    }
}