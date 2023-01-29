using System;
using System.Collections.Generic;
using UnityEngine;

public class CellAbilityPreview : MonoBehaviour
{
    private List<SpriteRenderer> _sprites;

    private void Awake()
    {
        _sprites = new List<SpriteRenderer>();
        var srs = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in srs)
        {
            _sprites.Add(sr);
        }
    }

    private void OnEnable()
    {
        foreach (var spriteRenderer in _sprites)
        {
            spriteRenderer.enabled = true;
        }
    }

    private void OnDisable()
    {
        foreach (var spriteRenderer in _sprites)
        {
            spriteRenderer.enabled = false;
        }
    }

    public void ShowAbilityPreview(Vector3 wsPos)
    {
        transform.position = wsPos;
        enabled = true;
    }
}