using System;
using CoreLib;
using UniRx;
using UnityEngine;

namespace Weather.Storms
{
    [Serializable]
    public class TestStorm
    {
        public bool enableStormTest = true;
        public string testStormTag = "TestStorm";
        public KeyCode testStormKey = KeyCode.None;
        public Bounds testStormBounds = new Bounds();
        public Vector2 testStormVelocity = Vector2.zero;
        public Vector2 testStormFalloff = Vector2.one;


        private Subject<Storm> _stormSubject;
        public StormCreationRequest GetStormCreationRequest(Action<Storm> onStormCreated = null, Action onStormCreatedCompleted = null)
        {
         
            var request = new StormCreationRequest(testStormBounds, testStormVelocity, testStormFalloff, testStormTag);
            if (onStormCreated != null)
                request.StormCreatedSubject.Take(1).Subscribe(storm => onStormCreated?.Invoke(storm), onStormCreatedCompleted);
            return request;
        }

        public override string ToString()
        {
            return enableStormTest ?  $"Test Storm: {testStormTag}\n: Bounds: {testStormBounds}" : "Test Storm Disabled";
        }
    }

    [Serializable]
    public class GlobalStormConfig : ConfigBase
    {
        public float minStormWidth = 200;
        public float minStormHeight = 100;
        public float minStormDepth = 10;
        public float stormHeightFloor = 0;
        
        

        public void ConstrainStormBounds(ref Bounds bounds)
        {
            var size = bounds.size;
            ConstrainStormSize(ref size);
            ConstrainStormPosition(ref bounds);
            bounds.size = size;
        }
        private void ConstrainStormSize(ref Vector3 size)
        {
            size.x = Mathf.Max(minStormWidth, size.x);
            size.y = Mathf.Max(minStormHeight, size.y);
            size.z = Mathf.Max(minStormDepth, size.z);
        }
        private void ConstrainStormPosition(ref Bounds bounds)
        {
            var center = bounds.center;
            var minY = bounds.min.y;
            if (minY < stormHeightFloor)
            {
                center.y += stormHeightFloor - minY;
                bounds.center = center;
            }
        }

        
        public bool IsValid(Bounds bounds)
        {
            var copy = bounds;
            ConstrainStormBounds(ref bounds);
            ConstrainStormPosition(ref bounds);
            return copy == bounds;
        }
        
    }
}