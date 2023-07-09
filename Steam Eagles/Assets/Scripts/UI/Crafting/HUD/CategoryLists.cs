using System;
using System.Collections.Generic;
using CoreLib;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

public class CategoryLists : MonoBehaviour
{
    [Required, AssetsOnly] public CategoryRecipeList recipeListPrefab;
    
    
    private UICrafting _uiCrafting;
    private DynamicReactiveProperty<CategoryRecipeList> _selectedList = new DynamicReactiveProperty<CategoryRecipeList>();
    
    Dictionary<string, CategoryRecipeList> _lists = new Dictionary<string, CategoryRecipeList>();
    
    public UICrafting crafting => _uiCrafting ? _uiCrafting : _uiCrafting = GetComponentInParent<UICrafting>();

    
    

    private void Start()
    {
        CreateRecipeLists();

        CreateListeners();
    }

    private void CreateListeners()
    {
        _selectedList.OnSwitched.Subscribe(t =>
        {
            if (t.previous != null) t.previous.gameObject.SetActive(false);
            if (t.next != null) t.next.gameObject.SetActive(true);
        }).AddTo(this);

        crafting.recipes.OnRecipeChanged.Select(t => t.category).Subscribe(category =>
        {
            _lists.TryGetValue(category, out var list);
            _selectedList.Value = list;
        }).AddTo(this);
    }

    private void CreateRecipeLists()
    {
        var categories = crafting.recipes.categories;
        for (int i = 0; i < categories.Count; i++)
        {
            var category = categories[i];
            var recipeList = Instantiate(recipeListPrefab, transform);
            recipeList.SetupForCategory(crafting, this, category);
            _lists.Add(category, recipeList);
        }
    }
}