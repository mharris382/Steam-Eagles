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
        private bool _removeRequestSent;
        
        private readonly TestStorm testConfig;

        public StormTester(GlobalStormConfig globalStormConfig, TestStorm testStorm)
        {
            _globalStormConfig = globalStormConfig;
            this.testConfig = testStorm;
        }

        public void Tick()
        {
            if (_removeRequestSent )
            {
                if(!_testStorm.IsCompleted)
                    _globalStormConfig.Log($"Removal Request sent, Waiting for removal request to terminate storm...", true);
                else
                {
                    _testStorm = null;
                    _removeRequestSent = false;
                }
                return;
            }
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
                _globalStormConfig.Log($"Running Test Storm: {_testStorm}\n Press {testConfig.testStormKey} to End Storm", true);
                if (Input.GetKeyDown(testConfig.testStormKey) && !_removeRequestSent)
                {
                    MessageBroker.Default.Publish(new StormRemovalRequest(_testStorm));
                    _removeRequestSent = true;
                }
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
                        _removeRequestSent = false;
                    }
                },
                () => _globalStormConfig.Log("Storm Creation Request Completed", true)));
        }
    }
}