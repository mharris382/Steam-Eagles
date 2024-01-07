using System;
using Buildables;
using Buildings;
using Buildings.Tiles;
using Items;
using UI.Crafting.Events;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace UI.Crafting.Destruction
{
    public class TileDestructionHandler : DestructionHandler
    {
        private readonly CraftingEventPublisher _tileEventPublisher;

        public class Factory : PlaceholderFactory<Recipe, TileDestructionHandler> { }
        BuildingCell lastCell;
        float timeLastDestruction = 0;
        private TileBase _destructTarget;

        public TileDestructionHandler(Recipe recipe, DestructionPreview destructionPreview, CraftingEventPublisher tileEventPublisher) : base(recipe, destructionPreview)
        {
            _tileEventPublisher = tileEventPublisher;
        }

        public override bool HasDestructionTarget(Building building, BuildingCell cell)
        {
            if (cell == lastCell) return (Time.time - timeLastDestruction > 1);
            
            var hasTile = building.Map.GetTile(cell) != null;
            if (!hasTile)
                return false;
            _destructTarget = building.Map.GetTile(cell); 
            var bmachines = building.GetComponent<BMachines>();
            var bmachine = bmachines.Map.GetMachine(cell.cell2D + Vector2Int.up);
            if (bmachine != null && bmachine.snapsToGround)
            {
                return false;
            }

            return true;
        }

        public override Vector2 GetDestructionTargetSize(Building building, BuildingCell cell)
        {
            return building.Map.GetCellSize(cell.layers);
        }

        public override void Destruct(Building building, BuildingCell cell)
        {
            timeLastDestruction = Time.time;
            lastCell = cell;
            var tile = building.Map.GetTile<DamageableTile>(cell);
            if (tile != null)
            {
                var damagedTile = tile.GetDamagedTileVersion();
                building.Map.SetTile(cell,  damagedTile);
                _tileEventPublisher.OnTileSwapped(cell, tile, damagedTile);
            }
            else
            {
                building.Map.SetTile(cell, null);
                _tileEventPublisher.OnTileRemoved(cell,  _destructTarget);
            }
        }
    }
}