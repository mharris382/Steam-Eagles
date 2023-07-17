#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Buildings;
using Buildings.Rooms;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Unity.Plastic.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace _EXP.PhysicsFun.ComputeFluid.MyEditor
{
    public class BuildingUtility : OdinEditorWindow
    {
        
        
        [MenuItem("Steam Eagles/Buildings/Building Utility")]
        private static void ShowWindow()
        {
            var window = GetWindow<BuildingUtility>();
            window.titleContent = new GUIContent("Building Utility");
            window.Show();
        }

        public Building targetBuilding;
        public bool selectingTarget => targetBuilding != null;
        private static Building[] AllBuildings => Object.FindObjectsByType<Building>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);

        void SelectNewTarget() => targetBuilding = null;
        void SelectTarget(Building building) => targetBuilding = building;
        protected override IEnumerable<object> GetTargets()
        {
            foreach (var building in AllBuildings)
            {
                yield return new ButtonTarget(building, SelectTarget);
            }
        }
        struct ButtonTarget
        {
            private readonly Building _building;
            private readonly Action<Building> _onSelect;

            public ButtonTarget(Building building, Action<Building> onSelect)
            {
                _building = building;
                _onSelect = onSelect;
            }
            
            string ButtonLabel() => _building.name;
            [Button(ButtonSizes.Medium, ButtonStyle.Box), LabelText("@ButtonLabel")]
            void OnClick() => _onSelect(_building);
        }
      
        
    }

    public class RoomInfo
    {
        public readonly Room room;

        [ShowInInspector(), FoldoutGroup("Info")] public string roomName
        {
            get => room.name;
            set => room.name = value;
        } 
        [ShowInInspector(), FoldoutGroup("Info")]  public GameObject roomVirtualCamera => room.roomCamera;
        [ShowInInspector(), FoldoutGroup("Components")]  public GameObjectContext roomContext => room.GetComponent<GameObjectContext>();
        [ShowInInspector(), FoldoutGroup("Components")]  public RoomGasSimInstaller roomGasSimInstaller => room.GetComponent<RoomGasSimInstaller>();
        [ShowInInspector(), FoldoutGroup("Components")]  public RoomTextures RoomTextures => room.GetComponent<RoomTextures>();
        [ShowInInspector(), FoldoutGroup("Components")]  public RoomEffect RoomEffect => room.GetComponent<RoomEffect>();
        [ShowInInspector(), FoldoutGroup("Components")]  public RoomSimTextures RoomSimTextures => room.GetComponent<RoomSimTextures>();
        [ShowInInspector(), FoldoutGroup("Components")]  public RoomCamera RoomCamera => room.GetComponent<RoomCamera>();
        [ShowInInspector(), FoldoutGroup("Components")]  public CapturedRoomTexture CapturedRoomTexture => room.GetComponent<CapturedRoomTexture>();
        
        
    }
    public class BuildingUtilityWrapper
    {
        private readonly Building _building;
        
        public string Name => _building.name;

        public string Info
        {
            get
            {
                var sb = new StringBuilder();
                var rooms = _building.Rooms.AllRooms.ToArray();
                sb.Append($"Room CNT= {rooms.Length}");
                
                return sb.ToString();
            }
        }

        public BuildingUtilityWrapper(Building building)
        {
            _building = building;
        }
    }
}
#endif