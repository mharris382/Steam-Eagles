using System;
using System.Collections.Generic;
using Buildings;
using CoreLib.Structures;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.VFX;

namespace _EXP.PhysicsFun.ComputeFluid.Extras
{
    public abstract class DynamicEffectProperties<T>
    {
        public abstract IEnumerable<(T, string)> GetEffectProperties();
    }
    public abstract class VFXPropertyHelper<T>
    {
        public string name;
        public abstract void Set(VisualEffect visualEffect, T value);
    }
    [Serializable]
    public class Vector2VFXProperty : VFXPropertyHelper<Vector2>
    {
        public Vector2VFXProperty(string name) => this.name = name;

        public override void Set(VisualEffect visualEffect, Vector2 value)
        {
            visualEffect.SetVector2(name, value);
        }
    }

    public class TileVFX : VFXEventListener<TileEventInfo>
    {
        public TilePositionModule positionModule;
        public RoomInfoModule roomInfoModule;
        public EventModule[] eventModules;
        
        public abstract class TileModule
        {

            public bool enabled = true;
            
            public void Apply(VisualEffect effect, TileEventInfo info)
            {
                if(!enabled )return;
                if (info.building == null) return;
                var building = info.building.GetComponent<Building>();
                var cell = new BuildingCell(info.tilePosition, (BuildingLayers)info.layer);
                ApplyTo(effect, building, cell, info);
            }

            protected abstract void ApplyTo(VisualEffect effect, Building building, BuildingCell buildingCell, TileEventInfo info);
        }
        
        [Serializable] public class TilePositionModule : TileModule
        {
            public bool setLocal = false;
            public bool setCenter = true;
            public Vector2VFXProperty position = new Vector2VFXProperty("position");
            public Vector2VFXProperty size = new Vector2VFXProperty("size");
            public void SetTile(VisualEffect visualEffect, Building building, BuildingCell cell)
            {
                var s = building.Map.GetCellSize(cell.layers);
                var p = setLocal ? 
                    setCenter ? building.Map.CellToWorldCentered(cell) : building.Map.CellToWorld(cell) :
                    setCenter ? building.Map.CellToLocalCentered(cell) : building.Map.CellToLocal(cell);
                
                position.Set(visualEffect, p);
                size.Set(visualEffect, s);
            }

            protected override void ApplyTo(VisualEffect effect, Building building, BuildingCell buildingCell, TileEventInfo info) => SetTile(effect, building, buildingCell);
        }
        [Serializable] public class RoomInfoModule : TileModule
        {
            public bool useLocal = false;
            public bool useCenter = false;
            public Vector2VFXProperty roomPosition = new Vector2VFXProperty("roomPosition");
            public Vector2VFXProperty roomSize = new Vector2VFXProperty("roomSize");

            protected override void ApplyTo(VisualEffect effect, Building building, BuildingCell buildingCell, TileEventInfo info)
            {
                var room = building.Map.GetRoom(buildingCell);
                if(room == null)return;
                var bounds = useLocal ? room.LocalSpaceBounds : room.WorldSpaceBounds;
                var p = useCenter ? bounds.center : bounds.min;
                var s = bounds.size;
                roomPosition.Set(effect, p);
                roomSize.Set(effect, s);
            }
        }
        [Serializable] public class EventModule : TileModule
        {
            public string eventName = "eventName";
            public CraftingEventInfoType type = CraftingEventInfoType.BUILD;

            public EventModule(string eventName) => this.eventName = eventName;

            protected override void ApplyTo(VisualEffect effect, Building building, BuildingCell buildingCell, TileEventInfo info)
            {
                if(info.type == type)
                    effect.SendEvent(eventName);
            }
        }
        
        private IEnumerable<TileModule> GetTileModules()
        {
            yield return positionModule;
            yield return roomInfoModule;
            foreach (var eventModule in eventModules)
                yield return eventModule;
        }

        public override bool Filter(TileEventInfo value)
        {
            return !value.isPreview;
        }

        public override void OnEvent(TileEventInfo value)
        {
            foreach (var module in GetTileModules())
            {
                module.Apply(Effect, value);
            }
        }

        public override void Cleanup()
        {
            
        }
    }
}