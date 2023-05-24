using UnityEngine;

namespace Damage.Core
{
    
    
    public class StormGenerator
    {
        public const int MAX_STORM_INTENSITY = 5;
        public const int MIN_STORM_INTENSITY = 1;
        private IStormFactory _stormFactory;
        private Transform _stormParent;
        private int _nextStormId;
        
        
        
        
        public StormGenerator(IStormFactory stormFactory)
        {
            _stormFactory = stormFactory;
            _nextStormId = 0;
            _stormParent = new GameObject("[STORMS]").transform;
        }
        
        
        public StormHandle GenerateStorm(Vector2Int intensityRange, Vector3 stormCenterPosition, string stormTag="", bool autoStart =false)
        {
            var id = _nextStormId++;
            var storm = new GameObject($"Storm[{id}] ({stormTag})");
            var stormTransform = storm.transform;
            stormTransform.position = stormCenterPosition;
            var handle = _stormFactory.CreateStormInstance(intensityRange.y, intensityRange.x);
            if (autoStart)
            {
                handle.Start();
            }
            return handle;
        }

        public StormHandle GenerateStorm(int intensity, Vector3 stormCenterPosition, string stormTag="") => GenerateStorm(new Vector2Int(intensity, intensity), stormCenterPosition, stormTag);


        void ValidateIntensityRange(ref Vector2Int range)
        {
            range.x = Mathf.Clamp(range.x, MIN_STORM_INTENSITY, MAX_STORM_INTENSITY);
            range.y = Mathf.Clamp(range.y, MIN_STORM_INTENSITY, MAX_STORM_INTENSITY);
            if (range.x > range.y) (range.x, range.y) = (range.y, range.x);
        }
    }
}