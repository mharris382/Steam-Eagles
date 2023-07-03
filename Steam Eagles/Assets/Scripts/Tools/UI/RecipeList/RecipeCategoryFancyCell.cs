using System;
using FancyScrollView;
using Items.UI.HUDScrollView;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


namespace Tools.UI
{
    [InfoBox("Animator Parameter: scroll")]
    [RequireComponent(typeof(Animator))]
    public class RecipeCategoryFancyCell : FancyCell<string, RecipeCategoriesContext>
    {
        public TMPro.TextMeshProUGUI categoryText;
        public Image categoryIcon;
        private float currentPosition = 0;
        
        public HUDFancyScrollView scrollView;

        private bool _initedSubView;
        
        private ICategoryIcons _icons;

        private Animator _animator;
        public Animator animator => _animator == null ? _animator = GetComponent<Animator>() : _animator;
        
        public override void UpdateContent(string itemData)
        {
            categoryText.text = itemData;
            if (_icons != null)
            {
                categoryIcon.sprite = _icons.GetIconForCategory(itemData);
            }
            else
            {
                Debug.LogError("missing icons",this);
            }

            if (!_initedSubView)
            {
                _initedSubView = true;
                var category = itemData;
                var recipes = Context.Controller.GetRecipes(category);
                Debug.Assert(recipes != null && recipes.Count > 0, this);
                scrollView.UpdateData(recipes);
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

        public override void Initialize()
        {
            Context.WaitForReady(this,OnReady);
        }

        private void OnReady()
        {
            Debug.Assert(Context.ReadyToUse);
            var controller = Context.Controller;
            var categories = Context.Controller.GetCategories();
            if(categories.Count > Index)
            {
                UpdateContent(categories[Index]);
            }
            //controller.OnCategoryChange.StartWith(controller.CurrentCategory).Select(t => t == Context.)
            
        }


        [Inject] void Install(ICategoryIcons icons)
        {
            _icons = icons;
        }
}
}