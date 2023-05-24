using UniRx;
using UnityEngine;
using Zenject;

namespace Weather.Storms
{
    public class StormTester : ITickable
    {
        private readonly GlobalStormConfig _globalStormConfig;
        private Storm _testStorm;
        private bool _requestSent;
        private readonly TestStorm testConfig;

        public StormTester(GlobalStormConfig globalStormConfig, TestStorm testStorm)
        {
            _globalStormConfig = globalStormConfig;
            this.testConfig = testStorm;
        }

        public void Tick()
        {
            
            if (!_requestSent)
            {
                _globalStormConfig.Log($"Storm Tester Tick: press {testConfig.testStormKey} to request storm", true);       
            }
            if (testConfig.enableStormTest == false) return;
            var tag = testConfig.testStormTag;
            var bounds = testConfig.testStormBounds;
            
            _globalStormConfig.ConstrainStormBounds(ref bounds);
            if (Input.GetKeyDown(testConfig.testStormKey) && _testStorm == null && !_requestSent)
            {
                StartTestStorm(testConfig);
            }
            else if (_testStorm == null && _requestSent)
            {
                _globalStormConfig.Log("Waiting for Storm Creation Request to Complete", true);
            }
            else if (_testStorm != null)
            {
                _globalStormConfig.Log($"Running Test Storm: {_testStorm}", true);
            }
        }

        private void StartTestStorm(TestStorm testConfig)
        {
            _globalStormConfig.Log(testConfig.ToString, true);
            _requestSent = true;
            MessageBroker.Default.Publish(testConfig.GetStormCreationRequest(
                res =>
                {
                    if (res == null) Debug.LogError("Failed to create storm");
                    else
                    {
                        _globalStormConfig.Log($"Storm Created: {res}", true);
                        _testStorm = res;
                    }
                },
                () => _globalStormConfig.Log("Storm Creation Request Completed", true)));
        }
    }
}