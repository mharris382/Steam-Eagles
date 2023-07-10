using System;
using Buildings;
using Buildings.Tiles;
using CoreLib.Structures;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities;

namespace UI.Crafting.Events
{
    public class CraftingEventPublisher : IDisposable
    {
        private readonly CraftingBuildingTarget _buildingTarget;
        private bool HasBuilding => _buildingTarget.BuildingTarget != null;
        private Building _building => _buildingTarget.BuildingTarget;

        private readonly CompositeDisposable _cd = new();

        public CraftingEventPublisher(CraftingBuildingTarget buildingTarget)
        {
            _buildingTarget = buildingTarget;
        }


        public void OnTileBuilt(BuildingCell cell, TileBase tile)
        {
            
            var info = new TileEventInfo() {   
                building = HasBuilding ? _building.gameObject : null,
                tilePosition = cell.cell2D,
                layer = (int)cell.layers,
                tile = tile,
                type = CraftingEventInfoType.BUILD
            };
            MessageBroker.Default.Publish(info);
        }

        public void OnTileRemoved(BuildingCell cell, TileBase tile)
        {
            
            var info = new TileEventInfo() {   
                building = HasBuilding ? _building.gameObject : null,
                tilePosition = cell.cell2D,
                layer = (int)cell.layers,
                tile = tile,
                oldTile = tile,
                type = CraftingEventInfoType.BUILD
            };
            MessageBroker.Default.Publish(info);
        }
        
        public void OnTileSwapped(BuildingCell cell, TileBase oldTile, TileBase newTile)
        {
            
            CraftingEventInfoType type;
            if (oldTile is RepairableTile && newTile is DamageableTile)
            {
                //this was a repair
                type = CraftingEventInfoType.REPAIR;
            }
            else if(oldTile is DamageableTile && newTile is RepairableTile)
            {
                //this was a deconstruct
                type = CraftingEventInfoType.DECONSTRUCT;
            }
            else
            {
                //this was a swap
                type = CraftingEventInfoType.SWAP;
            }
            var info = new TileEventInfo()
            {   
                building = HasBuilding ? _building.gameObject : null,
                tilePosition = cell.cell2D,
                layer = (int)cell.layers,
                tile = newTile,
                oldTile = oldTile,
                type = type
            };
            MessageBroker.Default.Publish(info);
        }



        public void OnPrefabBuilt(BuildingCell cell, GameObject prefabInstance, GameObject prefab, bool isFlipped)
        {
            
            var info = new PrefabEventInfo()
            {
                building = HasBuilding ? _building.gameObject : null,
                tilePosition = cell.cell2D,
                layer = (int)cell.layers,
                prefab = prefabInstance,
                isFlipped = isFlipped,
                type = CraftingEventInfoType.BUILD
            };
            MessageBroker.Default.Publish(info);
        }
        public void OnPrefabDeconstruct(BuildingCell cell, GameObject prefab, bool isFlipped)
        {
            
            var info = new PrefabEventInfo()
            {
                building = HasBuilding ? _building.gameObject : null,
                tilePosition = cell.cell2D,
                layer = (int)cell.layers,
                prefab = prefab,
                isFlipped = isFlipped,
                type = CraftingEventInfoType.BUILD
            };
            MessageBroker.Default.Publish(info);
        }
        public void Dispose()
        {
            _cd.Dispose();
        }
    }
}