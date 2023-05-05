using System;
using System.Collections;
using CoreLib;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class OptionButton : MonoBehaviour
{
    [Required] public Button button;
    [Required] public TextMeshProUGUI labelText;
    
    public void SetLabel(string optionOptionName) => labelText.text = optionOptionName;

    public void SetOption(IActionOption actionOption) => SetLabel(actionOption.OptionName);
}