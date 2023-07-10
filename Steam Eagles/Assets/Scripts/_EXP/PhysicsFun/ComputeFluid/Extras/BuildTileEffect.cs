using System;
using Buildings;
using Buildings.Rooms;
using CoreLib;
using CoreLib.Structures;
using Sirenix.OdinInspector;
using UniRx;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.VFX;
using Zenject;

namespace _EXP.PhysicsFun.ComputeFluid.Extras
{
    public class BuildTileEffect : MonoBehaviour
    {
        [Required]
        public VisualEffect effect;
        public bool setCenter = true;
        public string positionName = "position";
        public string sizeName = "size";
        public string roomPositionName = "roomPosition";
        public string roomSizeName = "roomSize";
        public string eventName = "OnBuild";
        private Room _room;

        private int _sizeId;
        private int _positionId;
        private int _roomPositionId;
        private int _roomSizeId;

        private BoundsLookup _boundsLookup;
        [Inject] void Install(Room room, BoundsLookup boundsLookup)
        {
            this._boundsLookup = boundsLookup;
            _room = room;
        }

        private void Awake()
        {
            _positionId = Shader.PropertyToID(positionName);
            _sizeId = Shader.PropertyToID(sizeName);
            _roomPositionId = Shader.PropertyToID(roomPositionName);
            _roomSizeId = Shader.PropertyToID(roomSizeName);
        }

        private void OnEnable()
        {
            MessageBroker.Default.Receive<TileEventInfo>()
                .Where(t => t.type == CraftingEventInfoType.BUILD &&
                            _room.Building.Map.GetRoom(t.GetBuildingCell()) == _room)
                .TakeUntilDisable(this)
                .Subscribe(PlayEffectOnTile)
                .AddTo(this);
        }

        bool HasResources() => _room != null && _room.Building != null;
        public void PlayEffectOnTile(TileEventInfo eventInfo)
        {
            if(!HasResources()) return;
            var bounds = _boundsLookup.GetWsBounds();
            
            var cell = eventInfo.GetBuildingCell();
            var wsPos = _room.Building.Map.CellToWorld(cell);
            var cellSize = _room.Building.Map.GetCellSize(cell.layers);
            effect.SetVector2(_positionId, wsPos);
            effect.SetVector2(_sizeId, cellSize);
            UpdateRoomsBounds();
            effect.SendEvent(eventName);
        }
        [Button(ButtonSizes.Gigantic)]
        void UpdateRoomsBounds()
        {
            if (Application.isPlaying)
            {
                UpdateRoomRuntime();
                return;
            }

            _boundsLookup = new BoundsLookup(_room = GetComponentInParent<Room>());
            var bounds = _boundsLookup.GetWsBounds();
            var gridBounds = _boundsLookup.GetBounds(BuildingLayers.SOLID);
            var rand = new UnityRandom();
            var x = rand.NextInt(gridBounds.xMin, gridBounds.xMax);
            var y = rand.NextInt(gridBounds.yMin, gridBounds.yMax);
            var z = 1;
            var gridSize = _room.Building.Map.GetCellSize(BuildingLayers.SOLID);
            var cell = new BuildingCell(new Vector2Int(x, y), BuildingLayers.SOLID);
            var pos = _room.Building.Map.CellToWorld(cell);
            effect.SetVector2(positionName, pos);
            effect.SetVector2(sizeName, gridSize);
            Vector3 position = setCenter ? bounds.center : bounds.min;
            Vector3 size = bounds.size;
            effect.SetVector2(roomPositionName, position);
            effect.SetVector2(roomSizeName, size);
        }

        private void UpdateRoomRuntime()
        {
            var bounds = _boundsLookup.GetWsBounds();
            Vector3 position = setCenter ? bounds.center : bounds.min;
            Vector3 size = bounds.size;
            effect.SetVector2(_roomPositionId, position);
            effect.SetVector2(_roomSizeId, size);
        }
    }

  
}


public static class TileEventExtensions
{
    public static BuildingLayers GetBuildingLayer(this TileEventInfo info)
    {
        return (BuildingLayers) info.layer;
    }
    
    public static BuildingCell GetBuildingCell(this TileEventInfo info)
    {
        return new BuildingCell(info.tilePosition, info.GetBuildingLayer());
    }
}