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
        private Subject<TileEventInfo> _previewEventSubject = new Subject<TileEventInfo>();
        private Subject<TileEventInfo> _actionEventSubject = new Subject<TileEventInfo>();
        public CraftingEventPublisher(CraftingBuildingTarget buildingTarget)
        {
            _buildingTarget = buildingTarget;
            _actionEventSubject.Do(t => t.isPreview = false).Subscribe(t => MessageBroker.Default.Publish(t)).AddTo(_cd);
            _previewEventSubject.Do(t => t.isPreview = true).Subscribe(t => MessageBroker.Default.Publish(t)).AddTo(_cd);
        }
        CraftingEventInfoType GetEventType(BuildingTile oldTile, BuildingTile newTile)
        {
            var eventType = CraftingEventInfoType.BUILD;
            if (oldTile.IsEmpty && newTile.IsEmpty)
            {
                eventType = CraftingEventInfoType.NO_ACTION;
            }
            else if (!oldTile.IsEmpty && !newTile.IsEmpty)
            {
                if (oldTile.tile == newTile.tile)
                {
                    eventType = CraftingEventInfoType.NO_ACTION;
                }
                else if(oldTile.tile is RepairableTile && newTile.tile is DamageableTile)
                {
                    eventType = CraftingEventInfoType.REPAIR;
                }
                else if(oldTile.tile is DamageableTile && newTile.tile is RepairableTile)
                {
                    eventType = CraftingEventInfoType.DAMAGED;
                }
                else 
                {
                    eventType = CraftingEventInfoType.SWAP;
                }
            }
            else if (oldTile.IsEmpty && !newTile.IsEmpty)
            {
                eventType = CraftingEventInfoType.BUILD;
            }
            else if(!oldTile.IsEmpty && newTile.IsEmpty)
            {
                eventType = CraftingEventInfoType.DECONSTRUCT;
            }

            return eventType;
        }
        
        CraftingEventInfoType GetEventType(BuildingCell pos, TileBase oldTile, TileBase newTile)
        {
            return GetEventType(new BuildingTile(pos, oldTile), new BuildingTile(pos, newTile));
        }
        
        public void OnTilePreview(Building building, BuildingTile oldTile, BuildingTile newTile)
        {
            var eventType = GetEventType(oldTile, newTile);
            var eventInfo = new TileEventInfo() {
                building = building.gameObject,
                wsPosition = HasBuilding ? _building.Map.CellToWorldCentered(newTile.cell) : default,
                isPreview = true,
                layer = (int)oldTile.cell.layers,
                tilePosition = oldTile.cell.cell2D,
                tile = newTile.tile,
                oldTile = oldTile.tile,
                type = eventType
            };
            _previewEventSubject.OnNext(eventInfo);
        }
        public void OnTilePreview(Building building, BuildingCell cell, TileBase oldTile, TileBase newTile)
        {
            return;
            OnTilePreview(building, new BuildingTile(cell, oldTile), new BuildingTile(cell, newTile));
        }

        
        public void OnTileChanged(Building building, BuildingCell cell, TileBase oldTile, TileBase newTile)
        {
            var t0 = new BuildingTile(cell, oldTile);
            var t1 = new BuildingTile(cell, newTile);
            var eventType = GetEventType(t0, t1);
            var eventInfo = new TileEventInfo() {
                building = building.gameObject,
                wsPosition = HasBuilding ? _building.Map.CellToWorldCentered(cell) : default,
                isPreview = false,
                layer = (int)cell.layers,
                tilePosition = cell.cell2D,
                tile = t1.tile,
                oldTile = t0.tile,
                type = eventType
            };
            _actionEventSubject.OnNext(eventInfo);
        }
        
        public void OnTileBuilt(BuildingCell cell, TileBase tile)
        {
            
            var info = new TileEventInfo() {   
                building = HasBuilding ? _building.gameObject : null,
                wsPosition = HasBuilding ? _building.Map.CellToWorldCentered(cell) : default,
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
                wsPosition = HasBuilding ? _building.Map.CellToWorldCentered(cell) : default,
                tilePosition = cell.cell2D,
                layer = (int)cell.layers,
                tile = tile,
                oldTile = tile,
                type = CraftingEventInfoType.DECONSTRUCT
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
                wsPosition = HasBuilding ? _building.Map.CellToWorldCentered(cell) : default,
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