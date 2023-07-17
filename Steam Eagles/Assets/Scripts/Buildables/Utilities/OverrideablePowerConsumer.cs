using System;

namespace Buildings
{
    public class OverrideablePowerConsumer : CustomPowerConsumer
    {
        public float defaultConsumptionRate = 1;
        private bool _assigned;
        private Func<float> _consumptioRate;
        private Action<float> _consume;
        protected override float GetConsumptionRate()
        {
            return  _assigned ? _consumptioRate() : defaultConsumptionRate;
        }

        protected override void Consume(float amount)
        {
            if(_assigned) _consume(amount);
        }
    }
}