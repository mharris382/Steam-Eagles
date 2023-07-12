using System.Collections;
using System.Collections.Generic;
using Buildings;
using Buildings.BuildingTilemaps;
using UnityEngine;

public class GasTilemap : BuildingTilemap
{
    
    public override BuildingLayers Layer => BuildingLayers.GAS;
    public override string GetSaveID() => "Gas";
}


