using System;
using Items;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICategoryListItem : MonoBehaviour
{
    private UICrafting _uiCrafting;
    private CategoryRecipeList _categoryRecipeList;
    public BasicGUIElements guiElements;
    public UICrafting crafting => _uiCrafting ? _uiCrafting : _uiCrafting = GetComponentInParent<UICrafting>();
    public CategoryRecipeList categoryRecipeList => _categoryRecipeList ? _categoryRecipeList : _categoryRecipeList = GetComponentInParent<CategoryRecipeList>();
    public Recipes recipes => crafting.recipes;

    private Recipe _recipe;

    [Serializable]
    public class BasicGUIElements
    {
        public Image icon;
        public TextMeshProUGUI name;
        
        public void  Setup(Recipe recipe)
        {
            if(icon) icon.sprite = recipe.icon;
            if(name) name.text = recipe.FriendlyName;
        }
    }

    [Button]
   public void GetRecipe()
    {
        try
        {
            recipes.InitForEditor();
            categoryRecipeList.Init();
            var category = categoryRecipeList.Category;
            var recipeList = recipes.GetRecipes(category);
            _recipe = recipeList[transform.GetSiblingIndex()];
            SetupUI(_recipe);
        }
        catch (IndexOutOfRangeException e)
        {
            gameObject.SetActive(false);
        }
    }

    protected virtual void SetupUI(Recipe recipe)
    {
        guiElements.Setup(recipe);
    }
}