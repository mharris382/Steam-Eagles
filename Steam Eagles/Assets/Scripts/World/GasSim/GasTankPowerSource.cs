using System;
using GasSim;
using UnityEngine;

[RequireComponent(typeof(GasTank))]
public class GasTankPowerSource : MonoBehaviour, IGasPowerSource
{
    private GasTank _gasTank;
    public float gasToPower = 4;
    
    private void Awake()
    {
        _gasTank = GetComponent<GasTank>();
    }

    public float PowerCapacity => _gasTank.capacity * gasToPower;
    public float AvailablePower => _gasTank.StoredAmount * gasToPower;
    
    public void ConsumePower(float amount)
    {
        int amt = Mathf.RoundToInt( amount * gasToPower);
        _gasTank.StoredAmount -= amt;
        _gasTank.StoredAmount=Mathf.Max(0, _gasTank.StoredAmount);
    }
}