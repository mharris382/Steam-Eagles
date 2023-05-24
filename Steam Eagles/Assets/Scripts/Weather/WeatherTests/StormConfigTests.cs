using NUnit.Framework;
using UnityEngine;
using Weather.Storms;

namespace Weather.WeatherTests
{
    public class StormConfigTests
    {
        private const float STORM_FLOOR = 0.0f;
        private const float STORM_MIN_X = 100;
        private const float STORM_MIN_Y = 100;
        private const float STORM_MIN_Z = 10;
        private GlobalStormConfig _stormConfig;

        [SetUp]
        public void SetUp()
        {
            _stormConfig = new GlobalStormConfig()
            {
                minStormHeight = STORM_MIN_Y,
                minStormWidth = STORM_MIN_X,
                stormHeightFloor = STORM_FLOOR
            };
        }

        [Test]
        public void CheckDefaultTestingBoundsIsValid()
        {
            var bounds = GetDefaultTestingBounds();
            Assert.IsTrue(_stormConfig.IsValid(bounds));
        }

        [Test]
        public void CheckZSizeCannotBeZero()
        {
            Bounds stormBounds = GetDefaultTestingBounds();
            stormBounds.size = new Vector3(stormBounds.size.x, stormBounds.size.y, 0);
            Assert.IsFalse(_stormConfig.IsValid(stormBounds));
            _stormConfig.ConstrainStormBounds(ref stormBounds);
            Assert.IsTrue(_stormConfig.IsValid(stormBounds));
            Assert.Greater(stormBounds.size.z, 0);
        }

        
        [Test]
        public void CheckStormCannotBePlacedBelowHeightLimit()
        {
            Bounds stormBounds = GetDefaultTestingBounds();
            var center = stormBounds.center;
            center.y -= 5f;
            stormBounds.center = center;
            Assert.IsFalse(_stormConfig.IsValid(stormBounds));
            _stormConfig.ConstrainStormBounds(ref stormBounds);
            Assert.IsTrue(_stormConfig.IsValid(stormBounds));
            Assert.AreEqual(STORM_FLOOR, stormBounds.min.y);
        }

           
        [Test]
        public void CheckStormCannotBeResizedBelowHeightLimit()
        {
            Bounds stormBounds = GetDefaultTestingBounds();
            var prevCenter = stormBounds.center;
            var size = stormBounds.size;
            size.y += 5;
            stormBounds.size = size;
            Assert.IsFalse(_stormConfig.IsValid(stormBounds));
            _stormConfig.ConstrainStormBounds(ref stormBounds);
            Assert.IsTrue(_stormConfig.IsValid(stormBounds));
            Assert.AreNotEqual(prevCenter, stormBounds.center);
            var prevY = prevCenter.y;
            var newY = stormBounds.center.y;
            // Storm should have been moved up 
            Assert.Less(prevY, newY);
        }

        [Test]
        public void CheckStormSizeMustBeLargerThanMinimums()
        {
            Bounds stormBounds = GetDefaultTestingBounds();
            var size= stormBounds.size;
            var prevSize = size;
            size.x = STORM_MIN_X - 1;
            Assert.AreEqual(_stormConfig.minStormWidth, STORM_MIN_X);
            stormBounds.size = size;
            Assert.IsFalse(_stormConfig.IsValid(stormBounds));
            _stormConfig.ConstrainStormBounds(ref stormBounds);
            Assert.IsTrue(_stormConfig.IsValid(stormBounds));

            size = prevSize;
            size.y = STORM_MIN_Y - 1;
            stormBounds.size = size;
            Assert.IsFalse(_stormConfig.IsValid(stormBounds));
            _stormConfig.ConstrainStormBounds(ref stormBounds);
            Assert.IsTrue(_stormConfig.IsValid(stormBounds));
        }
        
        private Bounds GetDefaultTestingBounds() =>  new Bounds(new Vector3(0, STORM_FLOOR + (STORM_MIN_Y / 2f), 0), new Vector3(STORM_MIN_X, STORM_MIN_Y, STORM_MIN_Z));
    }
}