using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using Buildings.BuildingTilemaps;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

public class BuildingTilemapsTest
{
    private const string Wall = "Wall";

    private Dictionary<Type, TilemapBuilderParameters> _builderParameters;
    private List<TilemapBuilderParameters> tilemapBuilders = new List<TilemapBuilderParameters>();
    public void SetUp()
    {
        if (_builderParameters == null || _builderParameters.Count == 0)
        {
            _builderParameters = new Dictionary<Type, TilemapBuilderParameters>();
            var solid = new TilemapBuilderParameters(typeof(SolidTilemap), withCollider:true,
                layer:LayerMask.NameToLayer("Solids"), tag:"Solid Tilemap");
            
            var f = new TilemapBuilderParameters(typeof(FoundationTilemap), withCollider: true,
                layer: LayerMask.NameToLayer("Ground"), tag: "Blocking Tilemap");
            
            var p = new TilemapBuilderParameters(typeof(PipeTilemap), withCollider:true, 
                withPlatformEffector:true, layer:LayerMask.NameToLayer("Pipes"), tag:"Pipe Tilemap");
            
            var w = new TilemapBuilderParameters(typeof(WallTilemap), layer:LayerMask.NameToLayer("Default"), tag:Wall);
            
            var l = new TilemapBuilderParameters(typeof(LadderTilemap), asTrigger:true,
                layer:LayerMask.NameToLayer("Platforms"), tag:"Ladder");
            
            var r = new TilemapBuilderParameters(typeof(WireTilemap), layer:LayerMask.NameToLayer("Wires"));
            
            var pl = new TilemapBuilderParameters(typeof(PlatformTilemap),
                withCollider:true, withPlatformEffector:true, 
                layer: LayerMask.NameToLayer("Platforms"), tag:"Platform Tilemap");
            
            tilemapBuilders = new List<TilemapBuilderParameters>();
            tilemapBuilders.AddRange(new []{ solid, f, p, w, l, r, pl });
            foreach (var tilemapBuilder in tilemapBuilders)
            {
                _builderParameters.Add(tilemapBuilder.TilemapType, tilemapBuilder);
            }
        }
    }

//[Test]
    public void BuildingTilemapsTestSimplePasses()
    {
        // Use the Assert class to test conditions
    }

    //[UnityTest]
    public IEnumerator BuildingTilemapsTestWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }


    private void CreateBuilding()
    {
        var building = new GameObject("Test Building", typeof(Building));
        var solidTM = AddTilemap(typeof(SolidTilemap), building);
        
        
    }

    GameObject AddTilemap(Type buildingTilemapType, GameObject building, bool withRenderer = true, bool withCollider = true) 
    {
        var tilemap = new GameObject("Test Tilemap", buildingTilemapType, typeof(Tilemap));
        if(withRenderer) tilemap.AddComponent<TilemapRenderer>();
        if(withCollider) tilemap.AddComponent<TilemapCollider2D>();
        return tilemap;
    }
}

public class TilemapBuilder
{
    public TilemapBuilder(string buildingName, params TilemapBuilderParameters[] tilemapBuilders)
    {
        BuildingName = buildingName;
        TilemapBuilders = tilemapBuilders;
    }

    public string BuildingName { get; }

    public TilemapBuilderParameters[] TilemapBuilders { get; }

    private Building _building;
    public void Build()
    {
        var go = new GameObject(BuildingName, typeof(BoxCollider2D), typeof(Building), typeof(StructureState));
        _building = go.GetComponent<Building>();
    }

    public Building GetBuilding()
    {
        return _building;
    }

    public void BuildLayers(BuildingLayers layers)
    {
        
    }
}