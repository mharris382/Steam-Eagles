using System;
using FancyScrollView;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Items.UI.HUDScrollView
{
    public class HUDFancyCellRecipe : FancyCell<Recipe, HUDToolRecipeContext>
    {
        [SerializeField,Required] Animator animator = default;
        [Required,ChildGameObjectsOnly] public Image iconImage;
        [SerializeField] private CountText countText;
        [SerializeField] private SelectionFeedback feedback;
        [Required, AssetsOnly] public HUDElementRecipeComponent componentPrefab;
        
        
        [Serializable]
        private class SelectionFeedback
        {
            [Required]
            [SerializeField] private Image backgroundImage;
            [SerializeField] private SelectionSprite sprite;
            [SerializeField] private SelectionColor color;
            
            [Serializable]
            private class SelectionSprite
            {
                public bool applySelectionSprite;
                public Sprite selectedSprite;
                public Sprite deselectedSprite;
                
                public void Apply(Image image, bool isSelected)
                {
                    if (!applySelectionSprite) return;
                    image.sprite = isSelected ? selectedSprite : deselectedSprite;
                }
            }
            
            [Serializable,InlineProperty]
            private class SelectionColor
            {
              [ToggleGroup(nameof(applySelectionColor))] public bool applySelectionColor;
              [ToggleGroup(nameof(applySelectionColor))] public Color selectedColor = Color.white;
              [ToggleGroup(nameof(applySelectionColor))] public Color deselectedColor = Color.white;

                public void Apply(Image image, bool isSelected)
                {
                    if (!applySelectionColor) return;
                    image.color = isSelected ? selectedColor : deselectedColor;
                }
            }

            public void Apply(bool b)
            {
                color.Apply(this.backgroundImage, b);
                sprite.Apply(this.backgroundImage, b);
            }
        }
        
        [Serializable]
        private class CountText
        {
            public TextMeshProUGUI countText;
            public Color invalidColor = Color.grey;
            public Color validColor = Color.white;
            public void SetCountText(int count)
            {
                countText.text = count.ToString();
                countText.color = count > 0 ? validColor : invalidColor;
            }
        }
        
        public override void UpdateContent(Recipe itemData)
        {
            iconImage.sprite = itemData.icon;
            UpdateCountText(itemData);
            ShowComponents(itemData);
            feedback.Apply(Context.SelectedIndex == Index);
        }

        private void ShowComponents(Recipe itemData)
        {
            ClearExistingComponents();

            for (int i = 0; i < itemData.components.Count; i++)
            {
                var c = itemData.components[i];
                var component = Instantiate(componentPrefab, transform);
                component.Show(Context, c);
                component.transform.SetAsLastSibling();
            }
        }

        private void UpdateCountText(Recipe itemData)
        {
            int lowestAmount = int.MaxValue;
            foreach (var itemDataComponent in itemData.components)
            {
                var amountRequired = itemDataComponent.Count;
                var item = itemDataComponent.item;
                if (item == null)
                {
                    Debug.LogError($"Null item in recipe {itemData.name} component", itemData);
                    continue;
                }

                var inventory = Context.Inventory;
                var amountHeld = inventory.GetItemCount(item);
                if (amountHeld > amountRequired)
                {
                    var quantityCanMake = amountHeld / amountRequired;
                    if (quantityCanMake < lowestAmount)
                        lowestAmount = quantityCanMake;
                }
                else
                {
                    lowestAmount = 0;
                }
            }
            countText.SetCountText(lowestAmount);
        }

        void ClearExistingComponents()
        {
            var hudElements = GetComponentsInChildren<HUDElementRecipeComponent>();
            for (int i = 0; i < hudElements.Length; i++)
            {
                Destroy(hudElements[i].gameObject);
            }
        }
        

        public override void UpdatePosition(float position)
        {
            currentPosition = position;
            if (animator.isActiveAndEnabled)
            {
                animator.Play("scroll", -1, position);
            }

            animator.speed = 0;
        }
        
        float currentPosition = 0;
    }
}