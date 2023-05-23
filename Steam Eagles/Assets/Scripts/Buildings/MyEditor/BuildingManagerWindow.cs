using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Buildings.MyEditor
{
    public class BuildingManagerWindow : OdinMenuEditorWindow
    {
        public class BuildingGroup
        {
            private List<IBuildingTable> _buildingTables = new List<IBuildingTable>();
            internal Building _building;
            internal BuildingTilemapsTable _tilemapTable;
            internal BuildingRoomsTable _roomsTable;
            internal MechanismTable _mechanismTable;
            internal ApplianceTable _applianceTable;
            internal RoomSizerTool _roomSizeTool;
            internal RoomCamerasTable _roomCameraTable;

            public BuildingGroup(Building b)
            {
                _building = b;
                _tilemapTable = new BuildingTilemapsTable(b);
                _buildingTables.Add(_tilemapTable);
                _roomsTable = new BuildingRoomsTable(b);
                _buildingTables.Add(_roomsTable);
                _mechanismTable = new MechanismTable(b);
                _buildingTables.Add(_mechanismTable);
                _applianceTable = new ApplianceTable(b);
                _buildingTables.Add(_applianceTable);
                _roomSizeTool = new RoomSizerTool(b,b.Rooms.AllRooms);
                _buildingTables.Add(_roomSizeTool);
                _roomCameraTable = new RoomCamerasTable(b);
                _buildingTables.Add(_roomCameraTable);
            }


            public void BuildTree(OdinMenuTree tree)
            {
                tree.Add($"{_building.buildingName}/Room Cameras", _roomCameraTable);
                tree.Add(_building.buildingName, _building);
                tree.Add($"{_building.buildingName}/Tilemaps", _tilemapTable);
                tree.Add($"{_building.buildingName}/Rooms", _roomsTable);
                tree.Add($"{_building.buildingName}/Mechanisms", _mechanismTable);
                tree.Add($"{_building.buildingName}/Appliances", _applianceTable);
                tree.Add($"{_building.buildingName}/Size Tool", _roomSizeTool );
            }

            public bool ShouldBdRemoved()
            {
                return _building == null;
            }
        }


        private List<IBuildingTable> _buildingTables = new List<IBuildingTable>();



        private List<BuildingGroup> _groups = new List<BuildingGroup>();
        
        private Dictionary<Building, BuildingGroup> _registeredGroups = new Dictionary<Building, BuildingGroup>();

        

        [MenuItem("Tools/Building Manager")]
        private static void ShowWindow()
        {
            var window = GetWindow<BuildingManagerWindow>();
            window.titleContent = new GUIContent("Building Manager");
            window.Show();
            var buildings = FindObjectsOfType<Building>();
            window.UpdateGroups(buildings);
        }

        
        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(true);
            tree.DefaultMenuStyle.IconSize = 28.00f;
            tree.Config.DrawSearchToolbar = true;
            var buildings = FindObjectsOfType<Building>();
            foreach (var group in _registeredGroups.Values)
            {
                group.BuildTree(tree);
            }
            return tree;
        }

        protected override void DrawMenu()
        {
            var buildings = FindObjectsOfType<Building>();
            if (buildings.Length != _groups.Count)
            {
                UpdateGroups(buildings);    
                ForceMenuTreeRebuild();
            }
            
            
            if (_registeredGroups.Count > 0)
            {
                base.DrawMenu();
            }
        }

        private void UpdateGroups(Building[] buildings)
        {
            var newLookup = new Dictionary<Building, BuildingGroup>();
            var newList = new List<BuildingGroup>();
            foreach (var building in buildings)
            {
                if (_registeredGroups.ContainsKey(building))
                {
                    newLookup.Add(building, _registeredGroups[building]);
                }
                else
                {
                    newLookup.Add(building, new BuildingGroup(building));
                }
                newList.Add(newLookup[building]);
            }
            _registeredGroups = newLookup;
        }

        private bool CheckForRebuild(Building[] buildings)
        {
            foreach (var kvp in _registeredGroups)
            {
                if (kvp.Value.ShouldBdRemoved())
                {
                    return true;
                }
            }

            return false;
        }
    }

    public interface IBuildingTable
    {
        bool IsValid { get; }
    }
}
