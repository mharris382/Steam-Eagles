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


