using System;
using System.Collections;
using System.Collections.Generic;
using CoreLib;
using Sirenix.OdinInspector;
using TMPro;
using UI;
using UniRx;
using UnityEngine;

public class Counter : HUDElement
{
    [Required]
    public TextMeshProUGUI counterText;

    public int maxDisplayCount = 99;
    public string maxDisplayText = "99+";
    private ReactiveProperty<string> _countText = new ReactiveProperty<string>();
    private Coroutine _updateCoroutine;
    public ICountable Countable { get; set; }

    private void Start() => _countText.Subscribe(OnTextChanged).AddTo(this);

    protected override void OnBecameVisible()
    {
       
        if(_updateCoroutine != null)
            StopCoroutine(_updateCoroutine);
        _updateCoroutine = StartCoroutine(UpdateCounter());
    }

    protected override void OnBecameHidden()
    {
        _countText.Value = "";
        
        if(_updateCoroutine != null)
            StopCoroutine(_updateCoroutine);
        _updateCoroutine = null;
    }

    private void OnTextChanged(string newText) => counterText.text = newText;

    private IEnumerator UpdateCounter()
    {
        while (true)
        {
            if (Countable == null)
            {
                _countText.Value = "";
            }
            else
            {
                _countText.Value = GetCountText(Countable.Count);    
            }
            yield return null;
        }
    }
    
    private string GetCountText(int cnt) => cnt > maxDisplayCount ? maxDisplayText : cnt.ToString();
}