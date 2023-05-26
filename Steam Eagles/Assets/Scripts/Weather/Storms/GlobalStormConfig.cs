using System;
using System.Collections.Generic;
using CoreLib;
using Sirenix.OdinInspector;
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

        public StormCreationRequest GetStormCreationRequest(Action<Storm> onStormCreated = null,
            Action onStormCreatedCompleted = null)
        {

            var request = new StormCreationRequest(testStormBounds, testStormVelocity, testStormFalloff, testStormTag);
            if (onStormCreated != null)
                request.StormCreatedSubject.Take(1)
                    .Subscribe(storm => onStormCreated?.Invoke(storm), onStormCreatedCompleted);
            return request;
        }

        public override string ToString()
        {
            return enableStormTest ? $"Test Storm: {testStormTag}\n: Bounds: {testStormBounds}" : "Test Storm Disabled";
        }
    }

    [Serializable]
    public class GlobalStormConfig : ConfigBase
    {
        public float minStormWidth = 200;
        public float minStormHeight = 100;
        public float minStormDepth = 10;
        public float stormHeightFloor = 0;

        
        [ListDrawerSettings(IsReadOnly = true, DraggableItems = false, Expanded = true)]
        [SerializeField] List<StormConfig> stormConfigs = new List<StormConfig>()
        {
            new StormConfig(1),
            new StormConfig(2),
            new StormConfig(3),
            new StormConfig(4),
            new StormConfig(5)
        };


        public StormConfig GetStormConfig(int intensity)
        {
            intensity = Mathf.Clamp(intensity, 1, 5);
            return stormConfigs[intensity - 1];
        }

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


    [Serializable]
    public class StormConfig
    {
        [SerializeField, ReadOnly] private int intensity;
        public float windSpeed = 10;
        
        public int damageMax = 10;
        
        public ParticleSystem.MinMaxCurve newDamageCurve;
        public StormConfig(int intensity)
        {
            this.intensity = intensity;
        }
        
        
        public float GetNewDamageChance(int damage)
        {
            return newDamageCurve.Evaluate(damage / (float)damageMax);
        }

        /// <summary>
        /// gets new damage chance based on damage and chance factor
        /// <seealso cref="ParticleSystem.MinMaxCurve"/>
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="chanceFactor"></param>
        /// <returns></returns>
        public float GetNewDamageChance(int damage, float chanceFactor)
        {
            return newDamageCurve.Evaluate(damage / (float)damageMax,  chanceFactor);
        }
    }
    
    
    
    
}