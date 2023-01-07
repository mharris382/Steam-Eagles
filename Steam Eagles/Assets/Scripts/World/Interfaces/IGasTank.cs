using UnityEngine.Events;

namespace GasSim
{
    public interface IGasTank
    {
        int Capacity { get; }
        UnityEvent OnEmpty { get; }
        UnityEvent<int> OnAmountChanged { get; }
        UnityEvent<float> OnAmountNormalizedChanged { get; }
        int StoredAmount { get; set; }
        float StoredAmountNormalized { get; }
        void AddGas(int amount);
        void RemoveGas(int amount);
    }
}