using UnityEngine;
using Zenject;

namespace Buildables
{
    public class HypergasGeneratorAnimator : MonoBehaviour
    {
        private Animator _anim;
        private static readonly int AmountStoredProperty = Animator.StringToHash("Amount Stored");
        private static readonly int Property = Animator.StringToHash("Is Producing");
        private HypergasEngineConfig _config;

        private void Awake()
        {
            _anim = GetComponent<Animator>();
        }

        [Inject]
        void Inject(HypergasEngineConfig config)
        {
            _config = config;
        }
        
        public void OnAmountStoredChanged(float amountStored)
        {
            _anim.SetFloat(AmountStoredProperty, amountStored/_config.amountStoredBeforeProduction);
        }
        
        

        public void OnProducingChanged(bool isProducing)
        {
            _anim.SetBool(Property , isProducing);
        }
    }
}