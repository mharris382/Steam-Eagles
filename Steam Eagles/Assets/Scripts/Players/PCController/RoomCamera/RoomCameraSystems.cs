using System;
using Buildings.Rooms;
using Buildings.Rooms.Tracking;
using CoreLib.Interfaces;
using CoreLib.Signals;
using UniRx;
using UnityEngine;
using Zenject;

namespace Players.PCController.RoomCamera
{
    public class PCRoomCameraInstaller : Installer<PCRoomCameraInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindFactoryCustomInterface<PC, RoomCameraSystem, RoomCameraSystem.Factory, ISystemFactory<RoomCameraSystem>>();
            Container.BindInterfacesTo<RoomCameraSystems>().AsSingle().NonLazy();
        }
    }
    public class RoomCameraSystems : PCSystems<RoomCameraSystem>, IInitializable, IDisposable
    {
        private readonly PCTracker _pcTracker;
        private readonly CompositeDisposable _cd;
        public RoomCameraSystems(PCTracker pcTracker, PC.Factory pcFactory, 
            ISystemFactory<RoomCameraSystem> factory) : base(pcTracker, pcFactory, factory)
        {
            _pcTracker = pcTracker;
            _cd = new CompositeDisposable();
        }

        public override RoomCameraSystem CreateSystemFor(PC pc)
        {
            var system = Factory.Create(pc);
            Debug.Log($"Assigning tracker for {pc.PlayerNumber}");
            system.AssignTracker(_pcTracker.GetTrackerFor(pc.PlayerNumber));
            return system;
        }

        public void Initialize()
        {
            Debug.Log("initialized Room Camera System");
        }

        public override void Dispose()
        {
            base.Dispose();
            _cd?.Dispose();
        }
    }

    public class RoomCameraSystem : PCSystem
    {
        private readonly IPCViewFactory _viewFactory;
        private IPCTracker _tracker;
        private GameObject _prevRoomCamera;
        private Room _prevRoom;
        private IDisposable _disposable;
        public class Factory : PlaceholderFactory<PC, RoomCameraSystem>, ISystemFactory<RoomCameraSystem> { }

        public void AssignTracker(IPCTracker tracker)
        {
            _tracker = tracker;
            Debug.Assert(_tracker != null);
            Debug.Log("Tracker Assigned");
            _disposable = _tracker.OnPcRoomChanged().Do(t =>
            {
                Debug.Log($"Player {Pc.PlayerNumber} room changed to : {(t == null ? "NO ROOM" : t.name)}", t);
            }).Select(t => t != null ? t.GetComponent<Room>() : null).Subscribe(OnPlayerRoomChanged);
        }
        public RoomCameraSystem(PC pc, IPCViewFactory viewFactory) : base(pc)
        {
            _viewFactory = viewFactory;
        }

        void OnPlayerRoomChanged(Room newRoom)
        {
            if (_prevRoomCamera  != null)
            {
                _prevRoomCamera.gameObject.SetActive(false);
            }

            _prevRoomCamera = null;
            _prevRoom = newRoom;
            if (newRoom != null && newRoom.roomCamera != null)
            {
                _prevRoomCamera = _viewFactory.Create(this.Pc.PlayerNumber, newRoom.roomCamera);
                _prevRoomCamera.gameObject.SetActive(true);
            }
        }

        public override void Dispose()
        {
            if (_prevRoomCamera != null) GameObject.Destroy(_prevRoomCamera);
            _disposable?.Dispose();
        }
    }
}