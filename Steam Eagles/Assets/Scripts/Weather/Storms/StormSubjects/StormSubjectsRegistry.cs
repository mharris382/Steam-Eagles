using System;
using System.Collections.Generic;
using CoreLib;
using UnityEngine;

namespace Weather.Storms
{
    public class StormSubjectsRegistry
    {
        private readonly GlobalStormConfig _stormConfig;
        private readonly StormSubject.Factory _stormSubjectFactory;
        private readonly Dictionary<IStormSubject, StormSubject> _stormSubjects = new Dictionary<IStormSubject, StormSubject>();

        public StormSubjectsRegistry(GlobalStormConfig stormConfig, StormSubject.Factory stormSubjectFactory)
        {
            _stormConfig = stormConfig;
            _stormSubjectFactory = stormSubjectFactory;
        }

        public void AddStormSubject(IStormSubject stormSubject)
        {
            if(stormSubject == null )
                return;
            if (_stormSubjects.ContainsKey(stormSubject))
            {
                _stormConfig.Log($"Already Registered Storm Subject {stormSubject}".Bolded());
                Debug.LogWarning($"Already Registered Storm Subject {stormSubject}".Bolded());
                throw new NotImplementedException();
            }
            _stormConfig.Log($"Removed Storm Subject {stormSubject}");
            _stormSubjects.Add(stormSubject, _stormSubjectFactory.Create(stormSubject));
        }
        public void RemoveStormSubject(IStormSubject stormSubject)
        {
            if (!_stormSubjects.ContainsKey(stormSubject))
                return;
            _stormSubjects[stormSubject].Dispose();
            _stormSubjects.Remove(stormSubject);
            _stormConfig.Log($"Removed Storm Subject {stormSubject}");
        }
    }
}