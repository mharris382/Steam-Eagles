using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using World.GasSim;

[CreateAssetMenu(menuName = "Gas Sim/Simulation State")]
public class SimulationState : ScriptableObject
{
    [SerializeField] private SimulationConfig config;
    
    [Min(0)]
    public float simulationTimeScale = 1;

    public float Rate => simulationTimeScale * config.updateRate;
    public UnityEvent updatePressure;
    public UnityEvent calculateVelocity;
    public UnityEvent<Vector3Int, int> gasAddedToTilemap;

    public bool debugSimulation;
    
    public SimulationStage Stage
    {
        get; 
        set;
    }
    
    public bool IsRunning
    {
        get; 
        set;
    }

    private int _totalGas;
    public int GasOnRecord
    {
        get => _totalGas;
        private set
        {
            if (value == 0)
            {
                Stage = SimulationStage.IDLE; //pause the simulation until gas is returned
            } else if (_totalGas == 0)
            {
                Stage = SimulationStage.UPDATE_PRESSURE; //begin the simulation with update pressure
            }
            _totalGas = value;
        }
    }
    
    public void StartSimulation(MonoBehaviour owner)
    {
        IsRunning = true;
        if (debugSimulation)
        {
            updatePressure.AddListener(() => Debug.Log("Simulation State: - Update Pressure"));
            calculateVelocity.AddListener(() => Debug.Log("Simulation State: - Calculate Velocity"));
        }
    }
    
    public void Reset()
    {
        IsRunning = false;
        _totalGas = 0;
        Stage = SimulationStage.IDLE;
    }

    public void RegisterGasToSim(Vector3Int cellPosition, int gasDensity)
    {
        if (gasDensity <= 0) return;
        if (!IsRunning) IsRunning = true;
        GasOnRecord += gasDensity;
        gasDensity = Mathf.Min(config.maxGasDensity, gasDensity);

        if (debugSimulation)
        {
            Debug.Log($"Just Registered {gasDensity}");
        }
        gasAddedToTilemap?.Invoke(cellPosition, gasDensity);
    }
}