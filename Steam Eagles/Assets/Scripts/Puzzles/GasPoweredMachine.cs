using System;
using UniRx;
using UnityEngine;

public class GasPoweredMachine : MonoBehaviour
{
    private SpriteRenderer _sr;
    private IDisposable _disposable;

    public void OnGasTankAttached(GasTank gasTank)
    {
        this._sr = gasTank.GetComponent<SpriteRenderer>();
        gasTank.transform.SetAsLastSibling();
        _disposable = Disposable.Create(() =>
        {

        });
    }

    public void OnGasTankDetached(GasTank gasTank)
    {
        
    }
    
}