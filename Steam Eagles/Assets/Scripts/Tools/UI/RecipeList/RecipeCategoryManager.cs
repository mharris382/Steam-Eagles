using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Items;
using Sirenix.OdinInspector;
using UI.PlayerGUIs;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Tools.UI
{
    public interface ICategoryIcons
    {
        Sprite GetIconForCategory(string category);
        Color GetColorForCategory(string category);
    }

/// <summary>
    /// manages the creation of recipe lists for each category, passes event streams to the recipe list controller
    /// </summary>
    public class RecipeCategoryManager : MonoBehaviour
    {
        public RecipeCategoryUI categoryUI;
        public Controllers controllers;
        

      
        private Dictionary<string, RecipeListController> _recipeLists = new();

        private PlayerCharacterGUIController _guiController;
        private ToolState _toolState;
        private string _lastCategory;
        
        
        [Serializable]
        public class Controllers 
        {
            public RecipeListController recipeListPrefab;
            public RectTransform recipeListParent;
            private Dictionary<string, RecipeListController> _controllers = new();
            private DiContainer _container;

            public RecipeListController Get(string category, IRecipeCategoryUIController categoryUIController)
            {
                if(_controllers.ContainsKey(category) && _controllers[category] != null)
                    return _controllers[category];
                
                
                var controller = GameObject.Instantiate(recipeListPrefab, recipeListParent);
                controller.name = $"{category} Recipe List";
                
                var r = categoryUIController.GetCurrentRecipeForCategory(category);
                Debug.Assert(r != null);
                
                controller.AssignAll(categoryUIController.GetRecipes(category), category, r, categoryUIController.OnRecipeChange);
                
                if(_controllers.ContainsKey(category)) _controllers[category] = controller;
                else _controllers.Add(category, controller);
                
                
                return controller;
            }


            [Inject] void Install(DiContainer container)
            {
                _container = container;
                foreach (var recipeListController in _controllers)
                {
                    container.Inject(recipeListController.Value);
                }
            }
        }
        

        [Serializable]
        public class RecipeCategoryUI : ICategoryIcons
        {
            public TMPro.TextMeshProUGUI categoryText;
            [TableList]
            public RecipeCategoryIcon[] categoryIcons;
            
            [Serializable]
            public class RecipeCategoryIcon
            {
                public string category;
                [Required] public Sprite categoryIcon;
                public Color textColor;
                public Color categoryColor;
            }
            public void ShowRecipeCategory(string recipeCategory)
            {
                var categoryIcon = categoryIcons.FirstOrDefault(t => t.category == recipeCategory);
                if (categoryIcon != null)
                {
                    categoryText.text = recipeCategory;
                }
                // if(index < 0)
                //     return;
                // if (categoryIcon != null && categoryIcon.categoryIcon != null)
                // {
                //     categoryIcon.sprite = categoryIcon.categoryIcon;
                //     categoryIcon.color = categoryIcon.categoryColor;
                // }
                // categoryText.text = recipeCategory;
                // categoryText.color = categoryIcons[index].textColor;
            }

            public Sprite GetIconForCategory(string category)
            {
                var categoryIcon = categoryIcons.FirstOrDefault(t => t.category == category);
                return categoryIcon.categoryIcon;
            }
            
            public Color GetColorForCategory(string category)
            {
                var index = Array.IndexOf(categoryIcons, category);
                if(index < 0)
                    return Color.white;
                return categoryIcons[index].categoryColor;
            }
        }

  
       
        private PlayerCharacterGUIController GUIController => _guiController == null
            ? _guiController = GetComponentInParent<PlayerCharacterGUIController>()
            : _guiController;

        
        [Inject]
         IRecipeCategoryUIController controller;
        

        private IEnumerator Start()
        {
            while (controller == null)
            {
                yield return null;
            }
            while (!controller.IsInitialized)
            {
                yield return null;
            }

            foreach (var category in controller.GetCategories())
            {
                var controller = controllers.Get(category, this.controller);
                Debug.Log($"Created Controller: {controller.name}", controller);
            }
        }
    }
}