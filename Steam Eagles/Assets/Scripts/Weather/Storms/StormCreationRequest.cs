using System;
using UniRx;
using UnityEngine;

namespace Weather.Storms
{
    public struct StormRemovalRequest
    {
        public Storm Storm { get; }

        public StormRemovalRequest(Storm storm)
        {
            Storm = storm;
        }
    }
    public class StormCreationRequest : IDisposable
    {
        private readonly Vector2 _stormFalloff;
        private CompositeDisposable _stormDestructor = new CompositeDisposable();
        public Bounds StormBounds { get; set; }
        
        public Vector2 StormVelocity { get; set;}
        public Vector2 StormFalloff => _stormFalloff;
        public string StormTag { get; }
        
        public Subject<Storm> StormCreatedSubject { get; }
    
        public Storm CreatedStorm { get; internal set; }

        public StormCreationRequest(Bounds stormBounds, Vector2 stormVelocity, Vector2 stormFalloff, string stormTag)
        {
            _stormFalloff = stormFalloff;
            StormBounds = stormBounds;
            StormVelocity = stormVelocity;
            StormTag = stormTag;
            StormCreatedSubject = new Subject<Storm>();
            _stormDestructor = new CompositeDisposable();
            
        }

        public void Dispose()
        {
            _stormDestructor?.Dispose();
            StormCreatedSubject?.Dispose();
            CreatedStorm?.Dispose();
        }
    }
}