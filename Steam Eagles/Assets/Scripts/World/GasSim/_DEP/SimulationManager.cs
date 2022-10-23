using System;
using System.Collections;
using UnityEngine;

namespace World.GasSim
{
    public class SimulationManager : MonoBehaviour
    {
        public SimulationState simulationState;
        public SharedTilemap gasTilemap;

   

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                
            }
        }
    }
}