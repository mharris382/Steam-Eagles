using System.Collections;
using Items;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class DebugUICrafting : MonoBehaviour
{
    public UICrafting uiCrafting;
    public TextMeshProUGUI categoryText;
    public TextMeshProUGUI recipeText;
    public Image recipeImage;
    public CanvasGroup canvasGroup;

    private IEnumerator Start()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0;
        while (!uiCrafting.IsReady)
        {
            yield return new WaitForSeconds(1);
        }

        canvasGroup.alpha = 1;

        SetupUI();
    }

    private void SetupUI()
    {
        uiCrafting.recipes.CurrentCategoryName
            .StartWith(uiCrafting.recipes.CurrentCategoryName.Value)
            .Subscribe(n => categoryText.text = n)
            .AddTo(this);

        uiCrafting.recipes.CurrentRecipe
            .StartWith(uiCrafting.recipes.CurrentRecipe.Value)
            .Subscribe(OnRecipeChanged)
            .AddTo(this);
    }

    void OnRecipeChanged(Recipe currentRecipe)
    {
        if (currentRecipe == null)
        {
            recipeText.text = "";
            recipeImage.sprite = null;
            return;
        }
        recipeText.text = currentRecipe.name;
        recipeImage.sprite = currentRecipe.icon;
    }
}