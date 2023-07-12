using System;
using System.Collections.Generic;
using System.Linq;
using Buildings.Rooms;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.Utillity
{
    [Serializable]
    public class CopierConfig
    {
        [VerticalGroup("layers/from"),LabelText("From"), HorizontalGroup("layers",width:0.5f)]
        public BuildingLayers copyFromLayer= BuildingLayers.FOUNDATION;
        
        [VerticalGroup("layers/to"),LabelText("To"), HorizontalGroup("layers",width:0.5f)]
        public BuildingLayers copyToLayer= BuildingLayers.SOLID;
        
        [VerticalGroup("layers/from"), LabelText("Tile (optional)")] public TileBase copyFrom;
        [VerticalGroup("layers/to"), LabelText("Tile"),Required] public TileBase copyTo;
    }
    public class Copier : IDisposable
    {
        private CopierConfig _config;
        private CompositeDisposable _cd = new();

        public enum CopySizeMode
        {
            SAME,
            LARGER_TO_SMALLER,
            SMALLER_TO_LARGER
        }
        private readonly Building _building;
        private readonly BuildingLayers _fromLayer;
        private readonly BuildingLayers _toLayer;
        
        private readonly CopySizeMode _copySizeMode;
        private readonly BoundsInt[] _fromRoomAreas;
        private readonly BoundsInt[] _toRoomAreas;
        private readonly Vector2 _fromSize;
        private readonly Vector2 _toSize;

        public Copier(Building building, CopierConfig config) : this(building, config.copyFromLayer, config.copyToLayer)
        {
            _config = config;
        }
        public Copier(Building building, BuildingLayers fromLayer, BuildingLayers toLayer) : this(building, fromLayer, toLayer, building.Rooms.AllRooms.ToArray())
        {

        }
        public Copier(Building building, BuildingLayers fromLayer, BuildingLayers toLayer, params Room[] rooms)
        {
            _building = building;
            _fromLayer = fromLayer;
            _toLayer = toLayer;
            _fromSize = building.Map.GetCellSize(_fromLayer);
            _toSize = building.Map.GetCellSize(_toLayer);
            Debug.Assert(_fromSize.x == _fromSize.y && _toSize.x == _toSize.y, "Only square tiles are supported");
            if (Math.Abs(_fromSize.x - _toSize.x) < Mathf.Epsilon)
                _copySizeMode = CopySizeMode.SAME;
            else if (_fromSize.x > _toSize.x)
                _copySizeMode = CopySizeMode.LARGER_TO_SMALLER;
            else
                _copySizeMode = CopySizeMode.SMALLER_TO_LARGER;

            var ares = from room in rooms select  room.Building.Map.GetCellsForRoom(room, fromLayer);
            _fromRoomAreas = ares.ToArray();
            Init();
            Subject<(Vector3Int cell, TileBase)> onRelevantTileChanged = new();
            foreach (var room in rooms)
                building.Map.OnTileChanged(_fromLayer, room).Subscribe(onRelevantTileChanged).AddTo(_cd);
            
            var tileRemovedStream = onRelevantTileChanged.Where(x => x.Item2 == null).Select(x => new BuildingCell(x.cell, _fromLayer));
            var tilePlacedStream = onRelevantTileChanged.Where(x => x.Item2 != null).Select(t => new BuildingTile(t.cell, _fromLayer, t.Item2));
            
            onRelevantTileChanged.AddTo(_cd);
        }
        void Init()
        {
            
        }

        TileBase GetCopiedTile(TileBase copyFrom)
        {
            if (_config == null) return copyFrom;
            throw new NotImplementedException();
        }
        void OnTargetTilePlaced(BuildingTile buildingTile)
        {
            var cell = buildingTile.cell;
            switch (_copySizeMode)
            {
                case CopySizeMode.SAME:
                    var toCell = new BuildingCell(cell.cell, _toLayer);
                    var toTile = GetCopiedTile(buildingTile.tile);
                    _building.Map.SetTile(toCell, toTile);
                    break;
                case CopySizeMode.LARGER_TO_SMALLER:
                    throw new NotImplementedException();
                    break;
                case CopySizeMode.SMALLER_TO_LARGER:
                    throw new InvalidOperationException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void OnTargetTileCleared(BuildingCell buildingCell)
        {
            switch (_copySizeMode)
            {
                case CopySizeMode.SAME:
                    var toCell = new BuildingCell(buildingCell.cell, _toLayer);
                    _building.Map.SetTile(toCell, null);
                    break;
                case CopySizeMode.LARGER_TO_SMALLER:
                    throw new NotImplementedException();
                    break;
                case CopySizeMode.SMALLER_TO_LARGER:
                    throw new InvalidOperationException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void Init_ModeSame(BoundsInt fromArea)
        {
            HashSet<Vector3Int> _targetCells = new();
            for (int x = fromArea.xMin; x < fromArea.xMax; x++)
            {
                for (int y = fromArea.yMin; y < fromArea.yMax; y++)
                {
                    var fromCell = new Vector3Int(x, y, 0);
                    var toCell = new Vector3Int(x, y, 0);
                    var fromTile = _building.Map.GetTile(fromCell, _fromLayer);
                    if (fromTile != null && _config != null && _config.copyTo != null && _config.copyTo != fromTile)
                    {
                        continue;
                    }
                    if (fromTile != null && _config != null)
                    {
                        fromTile = _config.copyTo;
                    }
                    switch (_copySizeMode)
                    {
                        case CopySizeMode.SAME:
                            _building.Map.SetTile(fromCell, _toLayer, fromTile);
                            break;
                        case CopySizeMode.LARGER_TO_SMALLER:
                            foreach (var cell in _building.Map.ConvertBetweenLayers(new BuildingCell(fromCell, _fromLayer), _toLayer))
                            {
                                _building.Map.SetTile(cell, fromTile);
                            }
                            break;
                        case CopySizeMode.SMALLER_TO_LARGER:
                            throw new InvalidOperationException();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            
        }

        public void Dispose()
        {
            _cd?.Dispose();
        }
    }
    public interface ICopySource
    {
         
    }

    public interface ICopyDestination
    {
        void CopyTo(Building building, IEnumerable<BuildingCell> cells);
    }
    
    [CreateAssetMenu(fileName = "BuildingTileSyncSettings",
        menuName = "Steam Eagles/Utilities/BuildingTileSyncSettings")]

    public class BuildingTileSyncSettings : SerializedScriptableObject
    {
        [Serializable]
        public class SyncTarget
        {
            [Serializable]
            public class CopyFrom : ICopySource
            {
                public enum CopyMode
                {
                    TILE,
                    TILEMAP,
                    CUSTOM
                }

                public CopyMode mode = CopyMode.TILE;

                [ShowIf("@this.mode == CopyMode.TILE")] public TileBase tile;
                [ShowIf("@this.mode == CopyMode.TILEMAP||this.mode == CopyMode.TILE")]public BuildingLayers buildingLayers = BuildingLayers.WALL;

                
                
            }
            [Serializable]
            public class CopyTo
            {
            
            }
        }

      
    }
}