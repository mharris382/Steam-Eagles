using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UICanvasGroupHelper : MonoBehaviour
{

    public UICanvasGroupHelper[] siblingGroups;
    private CanvasGroup _canavsGroup;
    public bool blockRaycast;
    private void Awake()
    {
        _canavsGroup = GetComponent<CanvasGroup>();
    }

    public void SetCanvasGroupActive()
    {
        foreach (var uiCanvasGroupHelper in siblingGroups)
        {
            SetGroupActive(uiCanvasGroupHelper._canavsGroup, false);
        }
        SetGroupActive(_canavsGroup, true);
    }

    public void SetGroupInactive()
    {
        SetGroupActive(_canavsGroup, false);
    }


    private void SetGroupActive(CanvasGroup group, bool isActive)
    {
        group.interactable = isActive;
        group.alpha = isActive ? 1 : 0;
        group.blocksRaycasts = blockRaycast && isActive;
    }
}
