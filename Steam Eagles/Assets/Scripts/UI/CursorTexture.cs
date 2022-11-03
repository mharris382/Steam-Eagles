using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CursorTexture : MonoBehaviour
{

    [SerializeField] private int cursorTexture;
    [SerializeField] private List<Texture2D> cursorTextures;

    public int InUseCursorTexture
    {
        set
        {
            cursorTexture = value % cursorTextures.Count;
            OnCursorTextureChanged();
        }
        get => cursorTexture;
    }

    private void Awake()
    {
        OnCursorTextureChanged();
    }

    internal void OnCursorTextureChanged()
    {
        if (Application.isPlaying)
        {
            Cursor.SetCursor(cursorTextures[cursorTexture], Vector2.zero, CursorMode.Auto);
        }
    }
}

