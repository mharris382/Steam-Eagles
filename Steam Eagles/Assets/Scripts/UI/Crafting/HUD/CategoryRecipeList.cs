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

    public UICategoryListItem recipePrefab;


    private string _category;
    [Button(ButtonSizes.Medium)]
    void CreateRecipeList()
    {
        ClearChildren();
        try
        {
            var category = crafting.recipes.categories[transform.GetSiblingIndex()];
            var recipes = crafting.recipes.GetRecipes(category);
            foreach (var recipe in recipes)
            {
                var recipeItem = Instantiate(recipePrefab, transform);
                recipeItem.SetupForRecipe();
                
            }
        }
        catch (Exception e)
        {
          gameObject.SetActive(false);
        }
    }

    void ClearChildren()
    {
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            children.Add(transform.GetChild(i));
        
        }

        foreach (var child in children)
        {
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
    private void Awake()
    {
      
    }

    void SetupListeners()
    {
        crafting.recipes.CurrentCategoryName.Subscribe(OnCategoryChanged).AddTo(this);
    }
    void SetCategoryActive(bool active)
    {
        gameObject.SetActive(active);
    }
    void OnCategoryChanged(string category)
    {
        gameObject.SetActive(category == _category);
    }
    public void Start()
    {
        try
        {
            var category = crafting.recipes.categories[transform.GetSiblingIndex()];
            _category = category;
            CreateRecipeList();
            var items = GetComponentsInChildren<UICategoryListItem>();
            foreach (var uiCategoryListItem in items)
            {
                uiCategoryListItem.SetupForRecipe();
                uiCategoryListItem.SubscribeTo(crafting.recipes).AddTo(this);
            }
        }
        catch (IndexOutOfRangeException e)
        {
            gameObject.SetActive(false);
            return;
        }
        _uiCrafting = GetComponentInParent<UICrafting>();
        _uiCrafting.recipes.OnCategoryChanged.Select(t => t == transform.GetSiblingIndex()).Subscribe(SetCategoryActive).AddTo(this);

        if (!gameObject.activeSelf) return;
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
            uiCategoryListItem.SetupForRecipe();
        }
    }
}
