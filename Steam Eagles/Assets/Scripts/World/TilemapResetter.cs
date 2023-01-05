using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

/// <summary>
/// Turns the game object off and then back on again
/// </summary>
public class TilemapResetter : MonoBehaviour
{
    
    void Start()
    {
        gameObject.SetActive(false);
        Observable.Timer(TimeSpan.FromMilliseconds(2)).First().Subscribe(_ => gameObject.SetActive(true)).AddTo(this);
    }

    
}
