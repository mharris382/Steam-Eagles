using System;
using System.Diagnostics;
using GasSim;
using UnityEngine;

public interface IGasSupplier
{
    bool enabled { get; }
    int GetUnclaimedSupply();
    void ClaimSupply(int amount);
}

public interface IGasSimConsumer : IGasSupplier 
{
    IGasSim GasSim { get; }
    IGasSink SimulationSink { get; }
}

/// <summary>
/// interface for entities that put gas into the pipe network by taking it out of the environment
/// </summary>
public interface IGasSimProducer : IGasConsumer
{
    IGasSim GasSim { get; }
    IGasSource SimulationSource { get; }
}


public class GasStorageUnit : MonoBehaviour
{



    public bool isConnected;
    public float currentStoredGas;
    public Transform connectionPoint;
    private GasTank _tank;


    private void Awake()
    {
        _tank = GetComponent<GasTank>();
        
    }

    void OnAmountChanged(float containedGas)
    {
        if (currentStoredGas == containedGas) return;
        var previousStoredGas = currentStoredGas;
        currentStoredGas = containedGas;
        if (isConnected) return;

        if (previousStoredGas > containedGas)
        {
            //lost gas while not connected so we need to release it to the simulation
        }
        else
        {
            //gained gas while not connected so we need to claim it from the simulation
        }
}
}