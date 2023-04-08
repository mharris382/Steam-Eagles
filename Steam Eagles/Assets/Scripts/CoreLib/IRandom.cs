using Rand = UnityEngine.Random;
namespace CoreLib
{
    public interface IRandom
    {
        public int NextInt(int minInclusive, int maxExclusive);
        public float NextFloat(float minInclusive, float maxExclusive);
        public float Value { get; }
    }

    public class UnityRandom : IRandom
    {
        public UnityRandom() { }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            return Rand.Range(minInclusive, maxExclusive);
        }

        public float NextFloat(float minInclusive, float maxExclusive)
        {
            return Rand.Range(minInclusive, maxExclusive);
        }

        public float Value => Rand.value;
    }
}