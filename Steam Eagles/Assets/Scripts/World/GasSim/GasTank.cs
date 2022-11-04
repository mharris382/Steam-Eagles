using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public interface IGasSupplier
{
    bool enabled { get; }
    int GetUnclaimedSupply();
    void ClaimSupply(int amount);
}
public interface IGasConsumer
{
    bool enabled { get; }
    int GetRequestedSupply();
    void ReceiveSupply(int amount);
}

public class GasTank : MonoBehaviour
{
    public int capacity = 100;
    [Range(0, 1)]
    public float initialAmount = 1;
    
    public UnityEvent onEmpty;
    public UnityEvent<int> onAmountChanged;
    public int generatedAmount = 0;
    
    
    
    private int _storedAmount;
    public int StoredAmount
    {
        get => _storedAmount;
        private set => _storedAmount = Mathf.Clamp(value, 0, capacity);
    }
    
    private void Start()
    {
        StoredAmount = (int)(capacity * initialAmount);
        onAmountChanged.Invoke(StoredAmount);
        if(generatedAmount > 0)
            StartCoroutine(GenerateGas());
    }

    private IEnumerator GenerateGas()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if(generatedAmount > 0) AddGas(generatedAmount);
            else if(generatedAmount < 0) RemoveGas(-generatedAmount);
        }
    }

    public void AddGas(int amount)
    {
        StoredAmount += amount;
        onAmountChanged.Invoke(StoredAmount);
    }
    
    public void RemoveGas(int amount)
    {
        StoredAmount -= amount;
        onAmountChanged.Invoke(StoredAmount);
    }
}

