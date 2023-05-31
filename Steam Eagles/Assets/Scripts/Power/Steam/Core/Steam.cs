namespace Power.Steam.Core
{
    public struct Steam
    {
        public float pressure;
        public float temperature;

        public Steam(float pressure, float temperature)
        {
            this.pressure = pressure;
            this.temperature = temperature;
        }
    }
}