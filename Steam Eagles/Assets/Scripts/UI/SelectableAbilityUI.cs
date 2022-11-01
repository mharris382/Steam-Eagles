using UnityEngine;

namespace UI
{
    public class SelectableAbilityUI : MonoBehaviour
    {
        public Sprite[] abilityIcons;
        
        

        public void OnAbilityIndexChanged(int index)
        {
            UpdateUI(abilityIcons[index]);
        }

        private void UpdateUI(Sprite abilityIcon)
        {
            
        }
    }
}