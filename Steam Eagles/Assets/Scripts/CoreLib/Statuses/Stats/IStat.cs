namespace Statuses.Stats
{
    public interface IStat
    {
        public string StatName { get; }
        public float Value { get; set; }
        public float MaxValue { get; }
        public float GetMinValue( ) => 0;
    }
}