using System;
using Items;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
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
    public UnityEvent<bool> Selected;
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
   public void SetupForRecipe(CategoryRecipeList list)
    {
        try
        {
            recipes.InitForEditor();
            list.Init();
            var category = list.Category;
            var recipeList = recipes.GetRecipes(category);
            try
            {
                _recipe = recipeList[transform.GetSiblingIndex()];
            }
            catch (ArgumentOutOfRangeException e)
            {
                gameObject.SetActive(false);
                return;
            }
            SetupUI(_recipe);
        }
        catch (IndexOutOfRangeException e)
        {
            gameObject.SetActive(false);
        }
    }


   public IDisposable SubscribeTo(Recipes recipes)
   {
       return recipes.CurrentRecipe.Select(t => _recipe == t).Subscribe(OnSelected);
   }

   void OnSelected(bool selected)
   {
         Selected?.Invoke(selected);   
   }
   

    protected virtual void SetupUI(Recipe recipe)
    {
        guiElements.Setup(recipe);
    }
}