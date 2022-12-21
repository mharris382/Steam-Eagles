namespace CoreLib.Interfaces
{
    public interface IPowerSource
    {
        public float PowerCapacity { get; }
        public float AvailablePower { get; }
        public void ConsumePower(float amount);
    }
}