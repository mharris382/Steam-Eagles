using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UniRx;


public class CategoryRecipeList : MonoBehaviour
{
    private UICrafting _uiCrafting;
    public UICrafting crafting => _uiCrafting ? _uiCrafting : _uiCrafting = GetComponentInParent<UICrafting>();
    
    
    
    private void Awake()
    {
        _uiCrafting = GetComponentInParent<UICrafting>();
    }

    public void Start()
    {
        crafting.recipes.OnCategoryChanged
            .Select(t => t == transform.GetSiblingIndex())
            .StartWith(0 == transform.GetSiblingIndex())
            .Subscribe(gameObject.SetActive).AddTo(this);
    }

    [Button]
    void SetCategoryName()
    {
        try
        {
            this.Category = crafting.recipes.categories[transform.GetSiblingIndex()];
            this.name = $"Category Recipe List: {Category}";
        }
        catch (IndexOutOfRangeException e)
        {
            gameObject.SetActive(false);
        }
    }

    public string Category { get; set; }

    public void Init()
    {
        
        SetCategoryName();
    }
[Button]
    public void InitAll()
    {
        foreach (var uiCategoryListItem in GetComponentsInChildren<UICategoryListItem>()    )
        {
            uiCategoryListItem.GetRecipe();
        }
    }
}
