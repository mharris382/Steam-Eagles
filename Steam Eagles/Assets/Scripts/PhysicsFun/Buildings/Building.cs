﻿using System;
using System.Collections.Generic;
using Buildings;
using Buildings.BuildingTilemaps;
using CoreLib;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhysicsFun.Buildings
{
    [ExecuteAlways]
    [RequireComponent(typeof(Grid))]
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    [RequireComponent(typeof(StructureState), typeof(BuildingPlayerTracker))]
    public class Building : MonoBehaviour, IStructure
    {
        #region [Inspector Fields]

        [OnValueChanged(nameof(UpdateName))]
        public string buildingName;
        
        
        public int orderInLayer;
        
        public Rect sizeWorldSpace;

        #endregion

        #region [Private Fields]

        private StructureState _structureState;
        private Grid _grid;
        private Rigidbody2D _rb;
        private BoxCollider2D _box;

        private WallTilemap _wallTilemap;
        private FoundationTilemap _foundationTilemap;
        private SolidTilemap _solidTilemap;
        private PipeTilemap _pipeTilemap;
        private CoverTilemap _coverTilemap;
        private PlatformTilemap _platformTilemap;

        #endregion
        
        #region [Properties]

        public string ID => string.IsNullOrEmpty(buildingName) ? name : buildingName;
        public StructureState State => _structureState ? _structureState : _structureState = GetComponent<StructureState>();
        public Grid Grid => _grid ? _grid : _grid = GetComponent<Grid>();
        public Rigidbody2D Rb => _rb ? _rb : _rb = GetComponent<Rigidbody2D>();

        
        
        public FoundationTilemap FoundationTilemap => _foundationTilemap ? _foundationTilemap : _foundationTilemap = GetComponentInChildren<FoundationTilemap>();
        public SolidTilemap SolidTilemap => _solidTilemap ? _solidTilemap : _solidTilemap = GetComponentInChildren<SolidTilemap>();
        public PipeTilemap PipeTilemap => _pipeTilemap ? _pipeTilemap : _pipeTilemap = GetComponentInChildren<PipeTilemap>();
        public CoverTilemap CoverTilemap => _coverTilemap ? _coverTilemap : _coverTilemap = GetComponentInChildren<CoverTilemap>();
        public WallTilemap WallTilemap => (_wallTilemap)  ? _wallTilemap : _wallTilemap = GetComponentInChildren<WallTilemap>();

        public PlatformTilemap PlatformTilemap => (_platformTilemap) ? _platformTilemap : _platformTilemap = GetComponentInChildren<PlatformTilemap>();

        public bool HasResources =>
            WallTilemap != null
            && FoundationTilemap != null
            && SolidTilemap != null
            && PipeTilemap != null
            && CoverTilemap != null;

        #endregion

        #region [MonoBehaviour Events]

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _structureState = GetComponent<StructureState>();
            _box = GetComponent<BoxCollider2D>();
            
            _wallTilemap = GetComponent<WallTilemap>();
            _foundationTilemap = GetComponent<FoundationTilemap>();
            _solidTilemap = GetComponent<SolidTilemap>();
            _pipeTilemap = GetComponent<PipeTilemap>();
            _coverTilemap = GetComponent<CoverTilemap>();
            
            SetupPhysics();
            
            void SetupPhysics()
            {
                gameObject.layer = LayerMask.NameToLayer("Triggers");
                _box.isTrigger = true;
            }
        }

        

        #endregion

        #region [Public Methods]

        

        
        public void SaveBuilding()
        {
            
        }

        
        public void LoadBuilding()
        {
            
        }
        
        #endregion

        #region [Helper Methods]

        public IEnumerable<BuildingTilemap> GetAllBuildingLayers() => GetComponentsInChildren<BuildingTilemap>();

        #endregion
        
        #region [Editor Stuff]

        private void UpdateName(string n)
        {
            var st = string.IsNullOrEmpty(n) ? name : n;
            Dictionary<Type, List<BuildingTilemap>> buildingTilemaps = new Dictionary<Type, List<BuildingTilemap>>();
            foreach (var buildingTilemap in GetAllBuildingLayers())
            {
                var type = buildingTilemap.GetType();
                if (!buildingTilemaps.ContainsKey(type))
                    buildingTilemaps.Add(type, new List<BuildingTilemap>());
                buildingTilemaps[type].Add(buildingTilemap);                
            }

            foreach (var kvp in buildingTilemaps)
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    var buildingTilemap = kvp.Value[i];
                    if (kvp.Value.Count > 1)
                    {
                        buildingTilemap.name = $"{st} {kvp.Key.Name} {i}";
                    }
                    else
                    {
                        buildingTilemap.name = $"{st} {kvp.Key.Name}";
                    }
                }
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            this.sizeWorldSpace.DrawGizmos();
        }
#endif

        #endregion
    }
}