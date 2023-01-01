using System;
using PhysicsFun.Buildings;
using UnityEngine;
using World;

namespace Buildings.BuildingTilemaps
{
    public abstract class EditableTilemap : RenderedTilemap
    {
        [SerializeField]
        private BuildingLayers blockingLayers = BuildingLayers.FOUNDATION;
        public virtual BuildingLayers GetBlockingLayers() => blockingLayers;
        
        public bool CanBuild(Building building, Vector3Int position)
        {
            return !IsBlocked(building, position);
        }
        
        public bool CanDestroy(Building building, Vector3Int position)
        {
            return IsBlocked(building, position);
        }

        protected virtual bool IsBlocked(Building building, Vector3Int position)
        {
            throw new NotImplementedException();
        }
    }
}