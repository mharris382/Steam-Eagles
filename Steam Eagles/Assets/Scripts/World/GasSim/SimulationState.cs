using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gas Sim/Simulation State")]
public class SimulationState : ScriptableObject
{
    private Dictionary<Vector3Int, int> _pressureDictionary = new Dictionary<Vector3Int, int>();



    private void OnDisable()
    {
        _pressureDictionary.Clear();
    }

    public bool IsPressuredChanged(Vector3Int position, int tilePressure)
    {
        if (!Application.isPlaying)
        {
            
            return true;
        }
        if (_pressureDictionary.ContainsKey(position))
        {
            int lastPressure = _pressureDictionary[position];
            _pressureDictionary[position] = tilePressure;
            return lastPressure != tilePressure;
        }
        else
        {
            _pressureDictionary.Add(position, tilePressure);
            return true;
        }
    }
}