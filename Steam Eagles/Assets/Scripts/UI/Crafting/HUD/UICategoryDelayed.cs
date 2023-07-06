using System;
using System.Collections;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

public abstract class UICategoryDelayed : MonoBehaviour
{
    private UICrafting _uiCrafting;
    public UICrafting crafting => _uiCrafting ? _uiCrafting : _uiCrafting = GetComponentInParent<UICrafting>();

    [ToggleGroup(nameof(label),"label")]  public bool label = true;
    [ToggleGroup(nameof(label),"label")]  public TextMeshProUGUI text;
    private string _categoryName;
    public UnityEvent<bool> setIsCategorySelected;
    
    public string CategoryName => _categoryName;
    
    private bool _isCategorySelected;
    private IEnumerator Start()
    {
        if (!SetupCategory()) yield break;
        while (!crafting.IsReady)
        {
            yield return null;
        }

        crafting.recipes.OnCategoryChanged.Select(t => t == transform.GetSiblingIndex())
            .DistinctUntilChanged()
            .Subscribe(enable =>
            {
                setIsCategorySelected?.Invoke(enable);
            }).AddTo(this);
    }
    
    [Button]
    private bool SetupCategory()
    {
        int index = transform.GetSiblingIndex();
        try
        {
            _categoryName = crafting.recipes.categories[index];
            if (label && text != null) text.text = _categoryName;
            name = $"{_categoryName} Category Button";
        }
        catch (IndexOutOfRangeException indexOutOfRangeException)
        {
            Debug.LogWarning("Category Button is out of range.", this);
            gameObject.SetActive(false);
            return false;
        }

        return true;
    }
}