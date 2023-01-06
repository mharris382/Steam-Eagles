using System;
using Buildings.BuildingTilemaps;
using PhysicsFun;
using PhysicsFun.Buildings;
using UniRx;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;

namespace Buildings
{
    [RequireComponent(typeof(WallTilemapFader))]
    [RequireComponent(typeof(TilemapRenderer))]
    public class CoverTilemap : RenderedTilemap
    {
        private StructureState _state;


        private WallFaderBase _fader;
        
        private void Awake()
        {
            _state = GetComponentInParent<StructureState>();
            _fader = GetComponent<WallFaderBase>();
            Debug.Assert(_state != null, "Cover tilemap missing structure state!", this);
            
        }

        private void Start()
        {
            _state.PlayerCountChanged.Select(t => t > 0).Subscribe(UpdateCoverMap);
        }

        void UpdateCoverMap(bool hasPlayers)
        {
            _fader.SetWallAlpha(hasPlayers ? 0 : 1);
        }

        public override BuildingLayers Layer { get; }

        public override int GetSortingOrder(Building building)
        {
            return building.orderInLayer;
        }

        public override string GetSortingLayerName(Building building)
        {
            return "Near FG";
        }
    }
}