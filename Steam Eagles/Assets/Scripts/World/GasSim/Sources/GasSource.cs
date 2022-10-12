using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GasSource : MonoBehaviour
{
    [Tooltip("how frequently the gas source attempts to inject gas into the simulation")]
    public float sourceRate = 0.2f;


    public TileBase gasTile;
    
    
}
