using System;
using UniRx;
using UnityEngine;

namespace Weather.Storms
{
    public class StormCreationRequest : IDisposable
    {
        private readonly Vector2 _stormFalloff;
        private CompositeDisposable _stormDestructor = new CompositeDisposable();
        public Bounds StormBounds { get; }
        
        public Vector2 StormVelocity { get; }
        
        public string StormTag { get; }
        
        public Subject<Storm> StormCreatedSubject { get; }
    
        public Storm CreatedStorm { get; private set; }

        public StormCreationRequest(Bounds stormBounds, Vector2 stormVelocity, Vector2 stormFalloff, string stormTag) : this(stormBounds, stormVelocity, stormFalloff, stormTag, new Subject<Storm>()){}
        public StormCreationRequest(Bounds stormBounds, Vector2 stormVelocity, Vector2 stormFalloff, string stormTag, Subject<Storm> stormCreatedSubject)
        {
            _stormFalloff = stormFalloff;
            StormBounds = stormBounds;
            StormVelocity = stormVelocity;
            StormTag = stormTag;
            StormCreatedSubject = stormCreatedSubject;
            _stormDestructor = new CompositeDisposable();
            StormCreatedSubject.Subscribe(storm => CreatedStorm = storm).AddTo(_stormDestructor);
        }

        public void Dispose()
        {
            _stormDestructor?.Dispose();
            StormCreatedSubject?.Dispose();
            CreatedStorm?.Dispose();
        }
    }
}