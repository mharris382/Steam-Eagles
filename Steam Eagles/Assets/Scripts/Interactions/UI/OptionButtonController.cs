using System.Collections.Generic;
using CoreLib;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class OptionButtonController : MonoBehaviour
{
    [Required, AssetsOnly]
    public OptionButton optionButtonPrefab;


    private List<OptionButton> _options;

    [Inject]
    public void InjectMe(IActionOptionsProvider optionsProvider)
    {
        _options = new List<OptionButton>();
        foreach (var actionOption in optionsProvider.GetOptions())
        {
            var button = Instantiate(optionButtonPrefab, transform);
            button.SetOption(actionOption);
        }
    }
}