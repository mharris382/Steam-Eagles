using System;
using System.Collections;
using System.Collections.Generic;
using CoreLib;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class GasSimParticleSystem : MonoBehaviour
{
    private ParticleSystem _ps;

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
    }

    
    private void OnParticleUpdateJobScheduled()
    {
        Debug.Log("OnParticleUpdateJobScheduled".Bolded());
    }
}


public class GasSimulation
{
    private Dictionary<Vector3Int, CellHandle> _cellCache = new Dictionary<Vector3Int, CellHandle>();
    
    private class CellHandle
    {
        private CellHandle _referencedHandle;
        public CellHandle(GasSimulation gasSim, Vector3Int cell)
        {
            
        }
    }
}

/// <summary>
///  
/// </summary>
public interface ICellHandle
{
    int CurrentGas { get; }
}

/// <summary>
/// allows external systems to safely interface with the the gas simulation
/// 
/// gas system handle defines an interface for systems that want to transfer
/// gas in and out of the actual simulation. In this sense each unblocked cell
/// on the tilemap can be considered a gas system handle.  
/// </summary>
public interface IGasSystemHandle
{
    
    int AvailableGas { get; }
    /// <summary>
    /// how much 
    /// </summary>
    int AvailableCapacity { get; }
}