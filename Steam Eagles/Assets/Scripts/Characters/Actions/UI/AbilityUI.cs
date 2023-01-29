using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.Actions.UI
{
    public class AbilityUI : MonoBehaviour
    {
       [Serializable]
       public class AbilityPreviewPair
       {
           [Required]public CellAbility ability;
           [Required]public CellAbilityPreview preview;
       }
       
       [TableList]
       [SerializeField] private List<AbilityPreviewPair> preview;

        public AbilityPreview primaryAbilityPreview;
        public AbilityPreview secondaryAbilityPreview;

       //[ColorUsage(true, true)]public Color primaryColor = Color.green;
       //[ColorUsage(true, true)] public Color secondaryColor = Color.red;
       //[ColorUsage(true, true)] public Color disabledColor = Color.gray;
       //public Material previewMaterial;
       //public Transform previewParent;
       //private static readonly int Color1 = Shader.PropertyToID("_Color");


       private Dictionary<CellAbility, CellAbilityPreview> _cellAbilityPreviews;


       private void Awake()
       {
           _cellAbilityPreviews = new Dictionary<CellAbility, CellAbilityPreview>();
           List<CellAbilityPreview> previews = new List<CellAbilityPreview>();
           foreach (var abilityPreviewPair in preview)
           {
               Debug.Assert(abilityPreviewPair.ability != null, "null ability",this);
               Debug.Assert(abilityPreviewPair.preview != null, "null preview",this);
               Debug.Assert(!_cellAbilityPreviews.ContainsKey(abilityPreviewPair.ability), $"Ability {abilityPreviewPair.ability.name} in list twice",this);
                _cellAbilityPreviews.Add(abilityPreviewPair.ability, abilityPreviewPair.preview);
                previews.Add(abilityPreviewPair.preview);
           }

       }

       private void OnDisable()
        {
            HideAllAbilityPreviews(true);
        }

        public void HideAllAbilityPreviews(bool isDisabled = false)
        {
            if (isDisabled)
            {
                primaryAbilityPreview.HideAbilityPreview();
                secondaryAbilityPreview.HideAbilityPreview();
               // previewMaterial.SetColor(Color1, disabledColor);
            }
            else
            {
               // previewMaterial.SetColor(Color1, Color.clear);
            }
        }
        
        public void ShowAbilityPreview(Vector3 wsPos, int abilityIndex)
        {
            switch (abilityIndex)
            {
                case 0:
                    primaryAbilityPreview.ShowAbilityPreview(wsPos);
                    secondaryAbilityPreview.HideAbilityPreview();
                    //previewMaterial.SetColor(Color1, primaryColor);
                    //previewParent.position = wsPos;
                    break;
                case 1:
                    secondaryAbilityPreview.ShowAbilityPreview(wsPos);
                    primaryAbilityPreview.HideAbilityPreview();
                    //previewMaterial.SetColor(Color1, secondaryColor);
                    //previewParent.position = wsPos;
                    break;
                default:
                    HideAllAbilityPreviews();
                    break;
            }
            
        }


        public CellAbilityPreview GetAbilityPreview(CellAbility cellAbility)
        {
            return _cellAbilityPreviews[cellAbility];
        }

        public void ShowAbilityPreviewForAbility(CellAbility ability, Vector3 wsPos)
        {
            var preview = GetAbilityPreview(ability);
            foreach (var cellAbilityPreview in _cellAbilityPreviews)
            {
                cellAbilityPreview.Value.enabled = cellAbilityPreview.Value == preview;
            }
            preview.ShowAbilityPreview(wsPos);
        }
    }
}