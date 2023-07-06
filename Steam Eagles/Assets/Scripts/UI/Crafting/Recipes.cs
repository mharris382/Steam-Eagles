using System.Collections.Generic;
using Items;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;


[System.Serializable]
public class Recipes
{
    [AssetList(AutoPopulate = true)]
    public List<Recipe> recipes;
    
    [InlineButton(nameof(FindCategories))]
    public List<string> categories;

    private bool _inited;
    private Dictionary<string, List<Recipe>> _recipes = new();
    private Dictionary<string, int> _recipeIndexes = new();
    

    private IntReactiveProperty _currentCategoryIndex = new();
    private Subject<Unit> _onRecipeIndexChanged = new();

    private ReadOnlyReactiveProperty<string> _categoryNameProp;
    private ReadOnlyReactiveProperty<Recipe> _recipeProp;
    private ReadOnlyReactiveProperty<List<Recipe>> _categoryRecipesProp;

    private IReadOnlyReactiveProperty<Recipe> _currentRecipeProp;
    public IReadOnlyReactiveProperty<List<Recipe>> CurrentRecipes => _categoryRecipesProp;
    public IReadOnlyReactiveProperty<Recipe> CurrentRecipe => _currentRecipeProp;
    public IReadOnlyReactiveProperty<string> CurrentCategoryName => _categoryNameProp;
    
    

    private List<Recipe> CurrentCategoryRecipes
    {
        get { return !_inited ? null : _recipes[CurrentCategory]; }
    }

    private string CurrentCategory
    {
        get => !_inited ? "" : categories[CurrentCategoryIndex];
    }
    private int CurrentCategoryIndex
    {
        get => !_inited ? -1 : _currentCategoryIndex.Value;
        set
        {
            if(!_inited)return;
            _currentCategoryIndex.Value = value;
        }
    }
    private int CurrentRecipeIndex
    {
        get => !_inited ? -1 : _recipeIndexes[CurrentCategory];
        set
        {
            if(!_inited)return;
            _recipeIndexes[CurrentCategory] = value;
        }
    }
    
    public void Init()
    {
        
        FindCategories();
        InitLookups();
        InitStreams();
        _inited = true;
    }
    void FindCategories()
    {
        foreach (var recipe in recipes)
        {
            if(categories.Contains(recipe.category)) continue;
            categories.Add(recipe.category);
        }
    }
    private void InitLookups()
    {
        _recipes.Clear();
        _recipeIndexes.Clear();
        foreach (var category in categories)
        {
            _recipes.Add(category, new List<Recipe>());
            _recipeIndexes.Add(category, 0);
        }

        foreach (var recipe in recipes) _recipes[recipe.category].Add(recipe);
        foreach (var recipe in _recipes)
        {
            Debug.Assert(recipe.Value.Count > 0, $"Empty Category Found: {recipe.Key}");
        }
    }
    private void InitStreams()
    {
       var categoryNameStream = _currentCategoryIndex.Select(t => categories[t]);
       _categoryNameProp = categoryNameStream.ToReadOnlyReactiveProperty();
       var recipeIndexStream = categoryNameStream.Select(t => _recipeIndexes[t]).Merge(_onRecipeIndexChanged.Select(_ => _recipeIndexes[CurrentCategory]));
       var recipeListStream = categoryNameStream.Select(t => _recipes[t]);
       _categoryRecipesProp = recipeListStream.ToReadOnlyReactiveProperty();
       _categoryRecipesProp.Subscribe(t => Debug.Assert(t.Count > 0));
       _currentRecipeProp = recipeIndexStream.Select(t => _categoryRecipesProp.Value[Mathf.Clamp(t, 0, _categoryRecipesProp.Value.Count)]).ToReadOnlyReactiveProperty();
       _currentCategoryIndex.Value = 0;
    }

    


  
    
    
    public void NextRecipe()
    {
        int cnt = CurrentCategoryRecipes.Count;
        var index = CurrentRecipeIndex;
        index++;
        index %= cnt;
        CurrentRecipeIndex = index;
        _onRecipeIndexChanged.OnNext(Unit.Default);
    }

    public void PrevRecipe()
    {
        var index = CurrentRecipeIndex;
        index--;
        if(index < 0) index = CurrentCategoryRecipes.Count - 1;
        CurrentRecipeIndex = index;
        _onRecipeIndexChanged.OnNext(Unit.Default);
    }

    public void NextCategory()
    {
        int index = CurrentCategoryIndex;
        index++;
        index %= categories.Count;
        CurrentCategoryIndex = index;
    }

    public void PrevCategory()
    {
        int index = CurrentCategoryIndex;
        index--;
        if(index < 0) index = categories.Count - 1;
        CurrentCategoryIndex = index;
    }
    public void SelectCategory(int index)
    {
        if (!_inited) return;
        index = Mathf.Clamp(index, 0, categories.Count - 1);
        if (index == -1) return;
        CurrentCategoryIndex = index;
    }
    
    public void SelectCategory(string category)
    {
        if (!_inited) return;
      var index = categories.IndexOf(category);
      if (index == -1) return;
      CurrentCategoryIndex = index;
    }
    public void SelectRecipe(string recipe)
    {
        if (!_inited) return;
        var index = CurrentCategoryRecipes.FindIndex(t => t.name == recipe);
        if (index == -1) return;
        CurrentRecipeIndex = index;
    }

}

