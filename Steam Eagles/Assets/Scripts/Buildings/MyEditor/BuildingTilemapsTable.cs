using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Buildings.MyEditor
{
    public class BuildingTilemapsTable : IBuildingTable
    {
        private readonly Building _building;
        
        public bool IsValid => _building != null;
        [ShowInInspector, InlineButton(nameof(Select))]
        public string BuildingName
        {
            get => _building.buildingName;
            set => _building.buildingName = value;
        }

        void Select() => Selection.activeGameObject = _building.gameObject;

        [TableList(IsReadOnly = true, AlwaysExpanded = true), ShowInInspector]
        public readonly List<BuildingTilemapWrapper> tilemaps;

        private static BuildingLayers[] ignoreLayers = new[]
        {
            BuildingLayers.NONE,
            BuildingLayers.REQUIRED
        };
        
        public BuildingTilemapsTable(Building building)
        {
            this._building = building;
            HashSet<BuildingLayers> ignores = new HashSet<BuildingLayers>(ignoreLayers);
            var values = Enum.GetValues(typeof(BuildingLayers)).Cast<BuildingLayers>();
            
            tilemaps = new List<BuildingTilemapWrapper>(
                values
                    .Where(t => !ignores.Contains(t))
                    .Select(t => new BuildingTilemapWrapper(t, this))
            );
            tilemaps.Sort();
        }
        
        bool HasTilemap(BuildingLayers layer) => _building.GetTilemap(layer) != null;


        void SelectLayer(BuildingLayers layers)
        {
            if (HasTilemap(layers))
            {
                Selection.activeGameObject = _building.GetTilemap(layers).gameObject;
            }
        }

        void SelectSolid() => SelectLayer(BuildingLayers.SOLID);
        void SelectGround() => SelectLayer(BuildingLayers.FOUNDATION);
        void SelectWall() => SelectLayer(BuildingLayers.WALL);
        
        public class BuildingTilemapWrapper : IComparable<BuildingTilemapWrapper>
        {
            [GUIColor(nameof(SelectButtonColor))]
            [ButtonGroup("Tools")]
            [Button, ShowIf(nameof(TilemapExists))]
            void Select() => _table.SelectLayer(_layer);
            
            [ButtonGroup("Tools")]
            [GUIColor(nameof(CreateButtonColor))]
            [Button, HideIf(nameof(TilemapExists))]
            void Create() => _table.Create(_layer);
            
            Color CreateButtonColor => Color.Lerp(Color.white, Color.red, 0.6f);
            Color SelectButtonColor => Color.Lerp(Color.white, Color.blue, 0.6f);
            
            [ShowInInspector, ReadOnly]
            private BuildingLayers _layer;
            private readonly BuildingTilemapsTable _table;  
            private bool TilemapExists => _table.HasTilemap(_layer);
            public BuildingTilemapWrapper(BuildingLayers layer, BuildingTilemapsTable table)
            {
                _layer = layer;
                _table = table;
            }

            
            public int CompareTo(BuildingTilemapWrapper other)
            {
                if (ReferenceEquals(this, other)) return 0;
                if (ReferenceEquals(null, other)) return 1;
                bool hasThis = TilemapExists;
                bool hasOther = other.TilemapExists;
                if (hasThis && !hasOther) return -1;
                if (!hasThis && hasOther) return 1;
                return _layer.CompareTo(other._layer);
            }
        }

        
        private void Create(BuildingLayers layer)
        {
            var tmType = layer.GetBuildingTilemapType();
            
            var builder = layer.GetBuilderParameters();
            
            var parent = _building.tilemapParent;
            if (_building.tilemapParent == null)
            {
                var parentGo = new GameObject("[Tilemaps]");
                parentGo.transform.SetParent(_building.transform);
                parentGo.transform.localPosition = Vector3.zero;
                parent = _building.tilemapParent;
            }
            
            var res = builder.CreateTilemap(parent);
            res.name = $"{_building.buildingName} {tmType.ToString()}";


        }
    }
}