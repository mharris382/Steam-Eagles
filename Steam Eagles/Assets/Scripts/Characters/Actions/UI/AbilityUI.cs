using System;
using UnityEngine;

namespace Characters.Actions.UI
{
    public class AbilityUI : MonoBehaviour
    {
        public AbilityPreview primaryAbilityPreview;
        public AbilityPreview secondaryAbilityPreview;

        private void OnDisable()
        {
            HideAllAbilityPreviews();
        }

        public void HideAllAbilityPreviews()
        {
            primaryAbilityPreview.HideAbilityPreview();
            secondaryAbilityPreview.HideAbilityPreview();
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