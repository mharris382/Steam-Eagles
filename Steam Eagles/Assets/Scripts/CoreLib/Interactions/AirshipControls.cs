using UnityEngine;

namespace CoreLib.Interactions
{
    public class AirshipControls : MonoBehaviour
    {
        private const int MAX_POWER_STEPS = 4;
        
        [Range(-1,1)]public float xInput;
        [Range(-1,1)]public float yInput;

        [SerializeField, Range(0, MAX_POWER_STEPS)] public int thrusterPower = 1;
        [SerializeField,Range(0, MAX_POWER_STEPS)] public int heatPower = 1;
        
        
        [SerializeField] private float timeBetweenSteps = 0.5f;
        
        private float _lastThrusterStepTime;
        private float _lastHeatStepTime;
        private IPilot _currentPilot;
        
        private float timeSinceHeatStep => Time.time - _lastHeatStepTime;
        private float timeSinceThrusterStep => Time.time - _lastThrusterStepTime;

        public IPilot CurrentPilot => _currentPilot;
        
        public void DecreasePowerToThrusters()
        {
            if(timeSinceThrusterStep < timeBetweenSteps)
                return;

            if (thrusterPower > 0)
            {
                _lastThrusterStepTime = Time.time;
                thrusterPower--;
            }
        }
        public void IncreasePowerToThrusters()
        {
            if(timeSinceThrusterStep < timeBetweenSteps)
                return;
            if (thrusterPower < MAX_POWER_STEPS)
            {
                _lastThrusterStepTime = Time.time;
                thrusterPower++;
            }
        }
        public void DecreasePowerToHeat()
        {
            if(timeSinceHeatStep < timeBetweenSteps)
                return;
            if (heatPower > 0)
            {
                _lastHeatStepTime = Time.time;
                heatPower--;
            }
        }
        public void IncreasePowerToHeat()
        {
            if(timeSinceHeatStep < timeBetweenSteps)
                return;
            if (heatPower < MAX_POWER_STEPS)
            {
                _lastHeatStepTime = Time.time;
                heatPower++;
            }
        }


        public void AssignPilot(IPilot pilot)
        {
            if(_currentPilot==pilot)
                return;
            if (_currentPilot != null)
            {
                _currentPilot.OnPowerToHeatChanged -= OnPilotHeatPowerChanged;
                _currentPilot.OnPowerToThrustersChanged -= OnPilotThrusterPowerChanged;
            }
            _currentPilot = pilot;
            if (_currentPilot != null)
            {
                _currentPilot.OnPowerToHeatChanged += OnPilotHeatPowerChanged;
                _currentPilot.OnPowerToThrustersChanged += OnPilotThrusterPowerChanged;
            }
        }

        void OnPilotHeatPowerChanged(int i)
        {
            if (i > 0)
            {
                IncreasePowerToHeat();
            }
            else
            {
                DecreasePowerToHeat();
            }
        }
        void OnPilotThrusterPowerChanged(int i)
        {
            if (i > 0)
            {
                IncreasePowerToThrusters();
            }
            else
            {
                DecreasePowerToThrusters();
            }
        }
    }
}