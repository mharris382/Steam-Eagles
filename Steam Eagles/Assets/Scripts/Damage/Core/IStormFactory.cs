namespace Damage.Core
{
    public interface IStormFactory
    {
        StormHandle CreateStormInstance(int maxIntensity, int minIntensity);
    }
}