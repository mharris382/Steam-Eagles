using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using Buildings.Rooms;
using CoreLib;
using CoreLib.Structures;
using ModestTree;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
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
        [FormerlySerializedAs("eventName")] public string buildEventName = "OnBuild";
        public string deconstructEventName = "OnDestroy";
        public string repairEventName = "OnRepair";
        public string damageEventName = "OnDamage";
        
        public string previewBuildEventName = "OnPreviewBuild";
        public string previewDeconstructEventName = "OnPreviewDestroy";
        public string previewRepairEventName = "OnPreviewRepair";
        public string previewDamageEventName = "OnPreviewDamage";
        public string noActionEventName = "OnPreviewNoAction";
        public ModeEffectParameterNames parameterNames;
        [ValidateInput(nameof(Validate)), TableList]
        public List<ModeEffectParameters> effectParametersArray;


        bool Validate(List<ModeEffectParameters> parametersList)
        {
            
            var cnt = Enum.GetNames(typeof(CraftingEventInfoType)).Length;
            if (parametersList.Count != cnt)
            {
                Debug.LogError($"Effect parameters count {parametersList.Count} does not match CraftingEventInfoType count {cnt}");
                return false;
            }
            return true;
        }
        
        [Serializable]
        public class ModeEffectParameterNames
        {
            public string colorGradientName = "_colorGradient";
            public string sizeName = "_size";
            public string countName = "_count";
            public string lifetimeName = "_lifetime";
            public string speedName = "_speed";
        }
        [Serializable]
        public class ModeEffectParameters
        {
            [EnumPaging]
            public CraftingEventInfoType type;
            public Gradient color = new Gradient();
            public Vector2 count = new Vector2(32, 50);
            public Vector2 size = new Vector2(0.1f, 0.5f);
            public Vector2 lifetime = new Vector2(1, 3);
            public Vector2 speed = new Vector2(1, 5);
            public bool SetParams(VisualEffect effect, ModeEffectParameterNames parameterNames, TileEventInfo info)
            {
                if(info.type != type)
                    return false;
                effect.SetVector2(parameterNames.countName, count);
                effect.SetGradient(parameterNames.colorGradientName, color);
                effect.SetVector2(parameterNames.sizeName, size);
                effect.SetVector2(parameterNames.lifetimeName, lifetime);
                effect.SetVector2(parameterNames.speedName, speed);
                return true;
            }
        }

        private bool TryGetModeEffects(TileEventInfo eventInfo, out ModeEffectParameters effectParameters)
        {
            foreach (var modeEffectParameter in this.effectParametersArray)
            {
                if (modeEffectParameter.SetParams(effect, parameterNames, eventInfo))
                {
                    effectParameters = modeEffectParameter;
                    return true;
                }
            }
            effectParameters = null;
            return false;
        }
        
        
        private Room _room;

        private int _sizeId;
        private int _positionId;
        private int _roomPositionId;
        private int _roomSizeId;
        private int _previewBuildEventId;
        private int _previewDeconstructEventId;
        private int _previewRepairEventId;
        private int _previewDamageEventId;
        private int _buildEventId;
        private int _deconstructEventId;
        private int _repairEventId;
        private int _damageEventId;

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
            _previewBuildEventId = Shader.PropertyToID(previewBuildEventName);
            _previewDeconstructEventId = Shader.PropertyToID(previewDeconstructEventName);
            _previewRepairEventId = Shader.PropertyToID(previewRepairEventName);
            _previewDamageEventId = Shader.PropertyToID(previewDamageEventName);
            _buildEventId = Shader.PropertyToID(buildEventName);
            _deconstructEventId = Shader.PropertyToID(deconstructEventName);
            _repairEventId = Shader.PropertyToID(repairEventName);
            _damageEventId = Shader.PropertyToID(damageEventName);
        }

        private void OnEnable()
        {
            var eventInRoom = MessageBroker.Default.Receive<TileEventInfo>()
                .Where(t => _room.Building.Map.GetRoom(t.GetBuildingCell()) == _room);
            var previewEventInRoom = eventInRoom.Where(t => t.isPreview);
            var actionEventInRoom = eventInRoom.Where(t => !t.isPreview && t.type != CraftingEventInfoType.NO_ACTION);
            
            actionEventInRoom
                .TakeUntilDisable(this)
                .Subscribe(PlayEffectOnTile)
                .AddTo(this);
            
            previewEventInRoom
                .TakeUntilDisable(this)
                .Subscribe(DrawPreviewOnTile)
                .AddTo(this);
        }

        bool HasResources() => _room != null && _room.Building != null;

        public void DrawPreviewOnTile(TileEventInfo eventInfo)
        {
            if(!HasResources()) return;
            UpdateEffectParameters(eventInfo);
            string eventName = GetPreviewEventName(eventInfo.type);
            effect.SendEvent(eventName);
        }
        public void PlayEffectOnTile(TileEventInfo eventInfo)
        {
            if(!HasResources()) return;
            UpdateEffectParameters(eventInfo);
            string eventName = GetActionEventName(eventInfo.type);
            effect.SendEvent(eventName);
        }

        private string GetPreviewEventName(CraftingEventInfoType eventInfoType)
        {
            switch (eventInfoType)
            {
                case CraftingEventInfoType.DECONSTRUCT:
                    return previewDeconstructEventName;
                case CraftingEventInfoType.BUILD:
                    return previewBuildEventName;
                case CraftingEventInfoType.DAMAGED:
                    return previewDamageEventName;
                case CraftingEventInfoType.SWAP:
                    return previewBuildEventName;
                case CraftingEventInfoType.REPAIR:
                    return previewRepairEventName;
                default:
                case CraftingEventInfoType.NO_ACTION:
                    return noActionEventName;
            }
        }
        private string GetActionEventName(CraftingEventInfoType eventInfoType)
        {
            switch (eventInfoType)
            {
                case CraftingEventInfoType.DECONSTRUCT:
                    return deconstructEventName;
                case CraftingEventInfoType.BUILD:
                    return buildEventName;
                case CraftingEventInfoType.DAMAGED:
                    return damageEventName;
                case CraftingEventInfoType.SWAP:
                    return buildEventName;
                case CraftingEventInfoType.REPAIR:
                    return repairEventName;
                default:
                case CraftingEventInfoType.NO_ACTION:
                    return noActionEventName;
            }
        }


        void UpdateEffectParameters(TileEventInfo eventInfo)
        {
            UpdateCellRuntime(eventInfo.GetBuildingCell());
            UpdateRoomRuntime();
            TryGetModeEffects(eventInfo, out _);
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

        void UpdateCellRuntime(BuildingCell cell)
        {
            var wsPos = _room.Building.Map.CellToWorld(cell);
            var cellSize = _room.Building.Map.GetCellSize(cell.layers);
            effect.SetVector2(_positionId, wsPos);
            effect.SetVector2(_sizeId, cellSize);
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