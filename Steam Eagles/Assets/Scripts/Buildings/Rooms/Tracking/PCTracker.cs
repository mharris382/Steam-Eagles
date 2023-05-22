using System;
using System.Collections.Generic;
using System.Linq;
using Buildings.Messages;
using CoreLib;
using UniRx;

namespace Buildings.Rooms.Tracking
{
    /// <summary>
    /// tracks all PCs in the game and which room they are in
    /// </summary>
    public class PCTracker : IDisposable
    {
        private readonly TrackedPC[] _instances;
        private IDisposable _disposable;
        private Subject<(int playerNumber, TrackedPC pc)> onPCChanged = new();
        public IObservable<(int, TrackedPC)> OnPCChanged => onPCChanged;//.Select(t => (t.playerNumber, t.pc.Instance));
        public PCTracker()
        {
            var cd = new CompositeDisposable();

            _instances = new TrackedPC[2] { null, null };
            MessageBroker.Default.Receive<PCInstanceChangedInfo>().Subscribe(info => SetPCInstance(info.pcInstance, info.playerNumber)).AddTo(cd);

            _disposable = cd;
        }

        private void SetPCInstance(PCInstance infoPCInstance, int infoPlayerNumber)
        {
            if(_instances[infoPlayerNumber] != null)
                _instances[infoPlayerNumber].Dispose();
            _instances[infoPlayerNumber] = new TrackedPC(infoPCInstance, infoPlayerNumber);
            onPCChanged.OnNext((infoPlayerNumber, _instances[infoPlayerNumber]));
        }

        public IObservable<TrackedPC> GetPC(int playerNumber)
        {
            var stream = onPCChanged.Where(t => t.Item1 == playerNumber).Select(t => t.Item2);
            if(_instances[playerNumber] != null)
                return stream.StartWith(_instances[playerNumber]);
            return stream;
        }
        public IObservable<TrackedPC> GetPCs()
        {
            return onPCChanged.Select(t => t.Item2).StartWith(from instance in _instances where instance != null select instance);
        }
        
        /// <summary>
        /// tracks room that a PC is in
        /// </summary>
        public class TrackedPC : IDisposable
        {
            public readonly PCInstance Instance;
            public readonly int PlayerNumber;
            
            private IDisposable _disposable;
            
            public Room LastRoom { get; set; }
            
            private Subject<(Room prevRoom, Room newRoom)> onRoomChanged = new();
            private ReactiveProperty<Room> pcRoom = new ReactiveProperty<Room>();
            private ReadOnlyReactiveProperty<Building> pcBuilding;

            public IReadOnlyReactiveProperty<Room> PCRoom => pcRoom;
            public IReadOnlyReactiveProperty<Building> PCBuilding => pcBuilding;
            public IObservable<(Room prevRoom, Room newRoom)> OnRoomChanged => onRoomChanged;

            
            
            public TrackedPC(PCInstance instance, int num)
            {
                var cd  = new CompositeDisposable();
                Instance = instance;
                PlayerNumber = num;
                _disposable = cd;
                
                pcBuilding = pcRoom.Select((t => t == null ? null : t.Building)).DistinctUntilChanged()
                    .ToReadOnlyReactiveProperty();
                
                MessageBroker.Default.Receive<EntityChangedRoomMessage>()
                    .Where(t => t.Entity.LinkedGameObject == instance.character)
                    .Subscribe(t => OnPCChangedRooms(t.Room)).AddTo(cd);
            }

            void OnPCChangedRooms(Room newRoom)
            {
                var prev = PCRoom.Value;
                pcRoom.Value = newRoom;
                onRoomChanged.OnNext((prev, newRoom));
            }

            public void Dispose() => _disposable?.Dispose();
        }

        public IEnumerable<TrackedPC> AllPCs()
        {
            foreach (var pcWrapper in _instances)
            {
                if(pcWrapper != null)
                    yield return pcWrapper;
            }
        }

        public IEnumerable<(int, TrackedPC)> AllPCsAndPlayerNumbers()
        {
            int cnt = 0;
            foreach (var pcWrapper in _instances)
            {
                if(pcWrapper != null)
                    yield return (cnt, pcWrapper);
                cnt++;
            }
        }
        public void Dispose()
        {
            _disposable?.Dispose();
            onPCChanged?.Dispose();
        }
    }
}