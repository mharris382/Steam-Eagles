using System;
using CoreLib;
using CoreLib.MyEntities;
using UniRx;
using UnityEngine;
using Zenject;

namespace Weather.Storms
{
    public class StormSubject : IDisposable
    {
        public class Factory : PlaceholderFactory<IStormSubject, StormSubject> { }
        
        private readonly IStormSubject _stormSubject;
        private readonly CompositeDisposable _cd;

        public EntityType SubjectEntityType => _stormSubject.SubjectEntityType;

        public Bounds SubjectBounds => _stormSubject.SubjectBounds;


        private ReactiveProperty<Storm> _subjectStorm;
        public Storm SubjectStorm
        {
            get => _subjectStorm.Value;
            set
            {
                if (value != _subjectStorm.Value)
                {
                    if (_subjectStorm.Value != null) _stormSubject.OnStormRemoved(_subjectStorm.Value);
                    _subjectStorm.Value = value;
                    if (value != null) _stormSubject.OnStormAdded(value);
                }
                
            }
        }

        public ReadOnlyReactiveProperty<Storm> SubjectStormObservable => _subjectStorm.ToReadOnlyReactiveProperty();
        
        public StormSubject(IStormSubject stormSubject)
        {
            _stormSubject = stormSubject;
            _subjectStorm = new ReactiveProperty<Storm>();
            
            _cd = new CompositeDisposable();
        }
        
        public void Dispose()
        {
            _cd?.Dispose();
        }

    }
}