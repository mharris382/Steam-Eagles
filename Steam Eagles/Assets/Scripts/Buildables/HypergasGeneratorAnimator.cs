using UnityEngine;

namespace Buildables
{
    public class HypergasGeneratorAnimator : MonoBehaviour
    {
        private Animator _anim;
        private static readonly int AmountStoredProperty = Animator.StringToHash("Amount Stored");
        private static readonly int Property = Animator.StringToHash("Is Producing");

        private void Awake()
        {
            _anim = GetComponent<Animator>();
        }

        public void OnAmountStoredChanged(float amountStored)
        {
            _anim.SetFloat(AmountStoredProperty, amountStored);
        }

        public void OnProducingChanged(bool isProducing)
        {
            _anim.SetBool(Property , isProducing);
        }
    }
}