namespace Power.Steam
{
    public struct SteamState
    {
        public float Pressure;
        public float InFlow;
        public float OutFlow;

        public SteamState(float pressure, float inFlow, float outFlow)
        {
            Pressure = pressure;
            InFlow = inFlow;
            OutFlow = outFlow;
        }
    }
}