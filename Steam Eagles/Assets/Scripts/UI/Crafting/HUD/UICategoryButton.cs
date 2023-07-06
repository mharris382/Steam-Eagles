using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICategoryButton : MonoBehaviour
{
    private UICrafting _uiCrafting;
    public UICrafting crafting => _uiCrafting ? _uiCrafting : _uiCrafting = GetComponentInParent<UICrafting>();
    
    
    public TextMeshProUGUI text;

    private string _categoryName;

    public UnityEvent<bool> setIsCategorySelected;

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
            if (text != null) text.text = _categoryName;
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

    void OnCategoryChanged(string category)
    {
        bool isCategory = category == _categoryName;
        setIsCategorySelected?.Invoke(isCategory);
    }
    
    
}
