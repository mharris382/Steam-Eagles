using System;
using System.Collections;
using CoreLib;
using Cysharp.Threading.Tasks;
using Interactions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;


public class OptionButton : MonoBehaviour
{
    [Required] public Image iconImage;
    [Required] public Button button;
    [Required] public TextMeshProUGUI labelText;

    [SerializeField] private IconStates iconStates;
    [Serializable]
    private class IconStates
    {
        public Sprite defaultIcon;
        public Sprite selectedIcon;
        public Sprite confirmedIcon;
        public Sprite unavailableIcon;

        public Sprite GetSpriteForState(OptionState state)
        {
            switch (state)
            {
                case OptionState.SELECTED:
                    return selectedIcon;
                    break;
                case OptionState.CONFIRMED:
                    return confirmedIcon;
                    break;
                case OptionState.DEFAULT:
                    return defaultIcon;
                    break;
                case OptionState.UNAVAILABLE:
                    return unavailableIcon;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
    
    public void SetLabel(string optionOptionName) => labelText.text = optionOptionName;

    public void SetOption(IActionOption actionOption)
    {
        SetLabel(actionOption.OptionName);
        var selectableOption = actionOption as ISelectableOption;
        if (selectableOption != null)
        {
            SetupSelectableOption(selectableOption);
        }
    }

    private void SetupSelectableOption(ISelectableOption selectableOption)
    {
        selectableOption.OnOptionStateChanged
            .StartWith(() => selectableOption.OptionState)
            .Select(t => iconStates.GetSpriteForState(t))
            .Subscribe(t => iconImage.sprite = t).AddTo(this);
    }
}