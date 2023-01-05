using UnityEngine;

namespace Characters.Actions.UI
{
    public class AbilityUI : MonoBehaviour
    {
        public AbilityPreview primaryAbilityPreview;
        public AbilityPreview secondaryAbilityPreview;


        public void UpdateAbilityUI(bool canUseAbilities, bool isPrimaryValid, bool isSecondaryValid, Vector3 position)
        {
            if (canUseAbilities)
            {
                
            }
        }

        public void HideAllAbilityPreviews()
        {
            primaryAbilityPreview.HideAbilityPreview();
            secondaryAbilityPreview.HideAbilityPreview();
        }
        
        public void ShowPrimaryAbilityPreview(Vector3 wsPos)
        {
            secondaryAbilityPreview.HideAbilityPreview();
            primaryAbilityPreview.ShowAbilityPreview(wsPos);
        }
        
        public void ShowSecondaryAbilityPreview(Vector3 wsPos)
        {
            primaryAbilityPreview.HideAbilityPreview();
            secondaryAbilityPreview.ShowAbilityPreview(wsPos);
        }

        public void ShowAbilityPreview(Vector3 wsPos, int abilityIndex)
        {
            switch (abilityIndex)
            {
                case 0:
                    primaryAbilityPreview.ShowAbilityPreview(wsPos);
                    secondaryAbilityPreview.HideAbilityPreview();
                    break;
                case 1:
                    secondaryAbilityPreview.ShowAbilityPreview(wsPos);
                    primaryAbilityPreview.HideAbilityPreview();
                    break;
                default:
                    HideAllAbilityPreviews();
                    break;
            }
            
        }
    }
}