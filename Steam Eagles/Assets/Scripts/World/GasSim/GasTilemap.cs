using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;

public class GasTilemap : MonoBehaviour
{
    public SimulationState simulationState;
    private Tilemap _tilemap;
    

    private void Awake()
    {
        _tilemap = GetComponent<Tilemap>();
        
    }

    private IEnumerator Start()
    {
        while (!simulationState.IsRunning)
        {
            Debug.Log("Gas Tilemap is waiting for simulation to start!");
            yield return null;
        }
        _tilemap.RefreshAllTiles();
    }
}