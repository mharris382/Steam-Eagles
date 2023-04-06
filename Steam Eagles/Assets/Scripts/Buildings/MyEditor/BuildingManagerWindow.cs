using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Buildings.MyEditor
{
    public class BuildingManagerWindow : OdinMenuEditorWindow
    {
        [MenuItem("Tools/Building Manager")]
        private static void ShowWindow()
        {
            var window = GetWindow<BuildingManagerWindow>();
            window.titleContent = new GUIContent("Building Manager");
            window.Show();
        }


        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(true);
            tree.DefaultMenuStyle.IconSize = 28.00f;
            tree.Config.DrawSearchToolbar = true;
            var buildings = FindObjectsOfType<Building>();
            foreach (var b in buildings)
            {
                tree.Add(b.buildingName, b);
                tree.Add($"{b.buildingName}/Tilemaps", new BuildingTilemapsTable(b));
                tree.Add($"{b.buildingName}/Rooms", new BuildingRoomsTable(b));
                tree.Add($"{b.buildingName}/Mechanisms", new MechanismTable(b));
                tree.Add($"{b.buildingName}/Appliances", new ApplianceTable(b));
                tree.Add($"{b.buildingName}/Size Tool", new RoomSizerTool(b, b.Rooms.AllRooms));
                tree.Add($"{b.buildingName}/Room Cameras", new RoomCamerasTable(b));
            }
            return tree;
        }
    }
}