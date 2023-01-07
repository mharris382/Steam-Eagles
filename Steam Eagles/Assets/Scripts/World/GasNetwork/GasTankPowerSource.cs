using System;
using GasSim;
using UnityEngine;

[RequireComponent(typeof(GasTank))]
public class GasTankPowerSource : MonoBehaviour, IGasPowerSource
{
    private GasTank _gasTank;
    private GasTank gasTank => _gasTank ? _gasTank : _gasTank = GetComponent<GasTank>();
    public float gasToPower = 4;
    
    private void Awake()
    {
        _gasTank = GetComponent<GasTank>();
    }

    public float PowerCapacity => gasTank.capacity * gasToPower;
    public float AvailablePower => gasTank.StoredAmount * gasToPower;
    
    public void ConsumePower(float amount)
    {
        int amt = Mathf.RoundToInt( amount * gasToPower);
        gasTank.StoredAmount -= amt;
        gasTank.StoredAmount=Mathf.Max(0, gasTank.StoredAmount);
    }
}