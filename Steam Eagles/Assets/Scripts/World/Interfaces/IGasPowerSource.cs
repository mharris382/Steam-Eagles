namespace GasSim
{
    public interface IGasPowerSource
    {
        public float PowerCapacity { get; }
        public float AvailablePower { get; }
        public void ConsumePower(float amount);
    }
}