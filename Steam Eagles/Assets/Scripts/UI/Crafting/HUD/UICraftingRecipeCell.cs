
using System;
using FancyScrollView;
using Items;
using Items.UI.HUDScrollView;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Crafting.HUD
{
    [InfoBox("Should have a Animator component with a 'scroll' animation")]
    [RequireComponent(typeof(Animator))]
    public class UICraftingRecipeCell : FancyCell<Recipe, CategoryScrollContext>
    {
        public Image iconImage;
        
        float currentPosition = 0;
        private Animator _animator;
        private UICraftingCellListener[] _listeners;
        
        private Animator animator => _animator ? _animator : _animator = GetComponent<Animator>();


        private void Awake()
        {
            _listeners = GetComponentsInChildren<UICraftingCellListener>();
        }

        public override void UpdateContent(Recipe itemData)
        {
            foreach (var listener in _listeners)
            {
                listener.UpdateCellContents(itemData, Context);
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
    }

    // public class UICraftingRecipeComponents : UICraftingCellListener
    // {
    //     [Required] public RectTransform container;
    //     [Required] public HUDCraftingRecipeComponent componentPrefab;
    //     public override void UpdateCellContents(Recipe recipe,  CategoryScrollContext context)
    //     {
    //         ClearExistingComponents();
    //
    //         for (int i = 0; i < recipe.components.Count; i++)
    //         {
    //             var c = recipe.components[i];
    //             var component = Instantiate(componentPrefab, container);
    //             component.Show(context, c);
    //             component.transform.SetAsLastSibling();
    //         }
    //     }
    //
    //     private void ClearExistingComponents()
    //     {
    //         var hudElements = GetComponentsInChildren<HUDElementRecipeComponent>();
    //         for (int i = 0; i < hudElements.Length; i++)
    //         {
    //             Destroy(hudElements[i].gameObject);
    //         }
    //     }
    // }
}