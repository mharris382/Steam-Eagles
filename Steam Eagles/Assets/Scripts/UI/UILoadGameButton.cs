using System;
using System.IO;
using CoreLib;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
/// <summary>
/// displays a button that when pressed loads game into a specific save file
/// </summary>
public class UILoadGameButton : MonoBehaviour
{
    public Button _button;
    public Button loadButton => _button != null ? _button : _button =GetComponent<Button>();
    private TextMeshProUGUI _text;
    public TextMeshProUGUI Text => _text != null ? _text : _text = GetComponentInChildren<TextMeshProUGUI>();
    public string savePath;

    public string GetFullSavePath()
    {
        return !savePath.StartsWith(Application.persistentDataPath) ?  $"{Application.persistentDataPath}/{savePath}" : savePath;
    }

    protected virtual void Awake()
    {
        var fullPath = GetFullSavePath();
        if (Directory.Exists(fullPath))
        {
            loadButton.interactable = true;
            loadButton.onClick.AsObservable().Subscribe(_ => MessageBroker.Default.Publish(new LoadGameRequestedInfo(GetFullSavePath()))).AddTo(this);
        }
        else
        {
            loadButton.interactable = false;
        }
    }
}