using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GasSim
{
    public class GasTank : MonoBehaviour, IGasTank
    {
    
        public int capacity = 100;
        [Range(0, 1)]
        public float initialAmount = 1;
    
        public UnityEvent onEmpty;
        public UnityEvent<int> onAmountChanged;
        public UnityEvent<float> onAmountNormalizedChanged;
        public int generatedAmount = 0;

        public UnityEvent<float> OnAmountNormalizedChanged => onAmountNormalizedChanged;
        public UnityEvent<int> OnAmountChanged => onAmountChanged;
        public UnityEvent OnEmpty => onEmpty;
        public int Capacity => capacity;
        

        [SerializeField]
        private int _storedAmount;
        public int StoredAmount
        {
            get => _storedAmount;
            set
            {
                _storedAmount = Mathf.Clamp(value, 0, capacity);
                onAmountChanged?.Invoke(_storedAmount);
            }
        }
    
        public float StoredAmountNormalized => (float)StoredAmount / capacity;
    
        private void Start()
        {
          
            //add listerenr to amount changed and invoke amount changed normalized
            onAmountChanged.AddListener(_ => onAmountNormalizedChanged?.Invoke(StoredAmountNormalized));
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
}