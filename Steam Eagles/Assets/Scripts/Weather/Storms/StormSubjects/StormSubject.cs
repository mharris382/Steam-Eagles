using System;
using CoreLib;
using CoreLib.Entities;
using UniRx;
using Zenject;

namespace Weather.Storms
{
    public class StormSubject : IDisposable
    {
        public class Factory : PlaceholderFactory<IStormSubject, StormSubject> { }
        
        private readonly IStormSubject _stormSubject;
        private readonly CompositeDisposable _cd;

        public EntityType SubjectEntityType => _stormSubject.SubjectEntityType;

        public StormSubject(IStormSubject stormSubject)
        {
            _stormSubject = stormSubject;
            _cd = new CompositeDisposable();
        }
        
        public void Dispose()
        {
            _cd?.Dispose();
        }
    }
}