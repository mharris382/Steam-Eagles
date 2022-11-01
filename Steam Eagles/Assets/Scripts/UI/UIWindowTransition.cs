using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UIWindowTransition : MonoBehaviour
{
    public Component windowTween;
    
 
    
    
    public void OpenWindow()
    {
        windowTween.DOKill();
        windowTween.DOPlayForward();
    }

    public void CloseWindow()
    {
        windowTween.DOKill();
        windowTween.DOPlayBackwards();
    }

}
