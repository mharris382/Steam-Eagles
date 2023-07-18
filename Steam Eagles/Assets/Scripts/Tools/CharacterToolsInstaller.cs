using System;
using Buildings;
using Buildings.Rooms;
using Buildings.Rooms.Tracking;
using Characters;
using CoreLib.MyEntities;
using Items;
using Sirenix.OdinInspector;
using SteamEagles.Characters;
using UniRx;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using Zenject;

namespace Tools.BuildTool
{
    public class TileDisconnector
    {
        
    }
    public class MachineDisconnector
    {
           
    }

    public interface IDisconnectHandler
    {
        bool CanDestruct(Vector2Int cell);
        void Destruct(Vector2Int cell);
    }

    public abstract class ToolControllerV2
    {
        protected readonly ToolContext context;

        public ToolControllerV2(ToolContext context)
        {
            this.context = context;
        }
    }

    public class BuildToolControllerV2
    {
        public BuildToolControllerV2(ToolContext context, ToolAimHandler aimHandler,
            IDisconnectHandler disconnectHandler)
        {
            
        }
    }

    public enum ToolMode
    {
        DISCONNECT,
        BUILD_TILE,
        BUILD_RECIPE
    }
    public class ToolContext : IInitializable, IDisposable
    {
        private readonly EntityRoomState _roomState;
        private ReactiveProperty<Building> _building = new();
        private ReactiveProperty<Room> _room = new();
        private IDisposable _d;
        private ReactiveProperty<CraftingStatus> _mode = new();
        private BoolReactiveProperty _isDisconnecting = new();
        private IReadOnlyReactiveProperty<CraftingStatus> _status;
        public IReadOnlyReactiveProperty<CraftingStatus> Mode => _status ??= _isDisconnecting.CombineLatest(_mode, (a, b) => a ? CraftingStatus.DISCONNECT : b).ToReadOnlyReactiveProperty();

        public bool IsDisconnecting
        {
            get => _isDisconnecting.Value;
            set => _isDisconnecting.Value = value;
        }
        public BuildingLayers TargetLayers
        {
            get;
            set;
        }

        public ToolMode ToolMode
        {
            get;
            set;
        }
        
        public Building building
        {
            get => _building.Value;
          private  set => _building.Value = value;
        }

        public Room room
        {
            get => _room.Value;
           private set => _room.Value = value;
        }

        public ToolContext(EntityRoomState roomState)
        {
            _roomState = roomState;
            TargetLayers = BuildingLayers.SOLID;
        }

        public void Initialize()
        {
            this._d = _roomState.CurrentRoom.StartWith(_roomState.CurrentRoom.Value)
                .Subscribe(t => Update(t));
        }

        void Update(Room room)
        {
            building = room == null ? null : room.Building;
            this.room = room;
        }

        public void Dispose()
        {
            _building?.Dispose();
            _room?.Dispose();
            _d?.Dispose();
        }
    }


    public static class InjectIDs
    {
        public const string BUILD_TOOL = "BUILD_TOOL";
        public const string CRAFT_TOOL = "CRAFT_TOOL";
        
        
    }

    [Serializable]
    public class ToolResources
    {
        [SerializeField, OnValueChanged(nameof(OnToolChanged))] private Tool tool;
        [SerializeField, OnValueChanged(nameof(OnControllerChanged))] private ToolControllerBase toolController;
        void OnToolChanged(Tool tool)
        {
            if(toolController.tool != null)
                toolController.tool = tool;
            else if(this.tool != null)
                toolController.tool = this.tool;
        }
        void OnControllerChanged(ToolControllerBase controllerBase)
        {
            if(controllerBase.tool != null)
                this.tool = controllerBase.tool;
            else if(tool != null)
                controllerBase.tool = tool;
        }
        public ToolResources(Tool tool, ToolControllerBase toolController)
        {
            this.tool = tool;
            this.toolController = toolController;
            toolController.tool = tool;
        }
    }
    public class CharacterToolsInstaller : MonoInstaller
    {
        public ToolBeltInventory toolBeltInventory;
        public EntityInitializer entityInitializer;
        public CharacterTools characterTools;
        public ToolState toolInput;
        public EntityRoomState entityRoomState;
        public CharacterState characterState;

        public ToolResources buildTool;
        public ToolResources craftTool;
        
        
        
        public override void InstallBindings()
        {
            
            
            Container.Bind<EntityInitializer>().FromInstance(entityInitializer).AsSingle().NonLazy();
            Container.Bind<CharacterState>().FromInstance(characterState).AsSingle().NonLazy();
            
            Container.Bind<EntityRoomState>().FromInstance(entityRoomState).AsSingle().NonLazy();
            Container.Bind<CharacterTools>().FromInstance(characterTools).AsSingle().NonLazy();
            Container.Bind<Transform>().FromMethod(GetToolParent).AsSingle().NonLazy();
            Container.Bind(typeof(MonoBehaviour), typeof(ToolState)).FromInstance(toolInput).AsSingle().NonLazy();
            Container.Bind(typeof(ToolAimHandler), typeof(ITickable)).To<ToolAimHandlerEx>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ToolContext>().AsSingle().NonLazy();

            Container.Bind<ToolResources>().WithId(InjectIDs.CRAFT_TOOL).FromInstance(craftTool).AsSingle().NonLazy();
            Container.Bind<ToolResources>().WithId(InjectIDs.BUILD_TOOL).FromInstance(buildTool).AsSingle().NonLazy();

            // foreach (var toolSlot in toolBeltInventory.GetToolSlots()) Container.QueueForInject(toolSlot);
        }
        
        
        public class ToolAimHandlerEx : ToolAimHandler, ITickable
        {
            private readonly ToolContext _context;

            public ToolAimHandlerEx(MonoBehaviour owner, ToolState toolState, ToolContext context) : base(owner, toolState)
            {
                _context = context;
            }

            public void Tick()
            {
                UpdateAimDirection();
                UpdateAimPosition(_context.TargetLayers);
            }
        }

        Transform GetToolParent(InjectContext context)
        {
            var tools = context.Container.Resolve<CharacterTools>();
            return tools.toolParent;
        }
    }
}