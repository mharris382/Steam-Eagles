using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Buildables
{
    public class BuildableVariant : MonoBehaviour, IMachineCustomSaveData
    {
        [Required]
        public SpriteRenderer spriteRenderer;

        private int _variantIndex;

        [PreviewField(Alignment = ObjectFieldAlignment.Center)]
        [SerializeField] Sprite[] variants;
        
        public bool allowsColorVariation;

        
        
        [ButtonGroup(), Button]
        void PreviewNext()
        {
            VariantIndex++;
            
        }
        [ButtonGroup(), Button]
        void PreviewPrevious()
        {
            if(VariantIndex > 0)
                VariantIndex--;
            else
                VariantIndex = variants.Length - 1;
        }
        
        public int VariantIndex
        {
            get => _variantIndex;
            set
            {
                _variantIndex = value;
                spriteRenderer.sprite = variants[_variantIndex % variants.Length];
            }
        }

        public Color VariantColor
        {
            get => spriteRenderer.color;
            set => spriteRenderer.color = allowsColorVariation ? value : Color.white;
        }

        public void LoadDataFromJson(string json)
        {
            var saveData = JsonUtility.FromJson<VariantSaveData>(json);
            VariantIndex = saveData.variantIndex;
            VariantColor = saveData.color;
        }

        public string SaveDataToJson()
        {
            var saveData = new VariantSaveData
            {
                variantIndex = VariantIndex,
                color = VariantColor
            };
            return JsonUtility.ToJson(saveData);
        }
        
        
        
    }

    [Serializable]
    public class VariantSaveData
    {
        public int variantIndex;
        public Color color;
    }
}