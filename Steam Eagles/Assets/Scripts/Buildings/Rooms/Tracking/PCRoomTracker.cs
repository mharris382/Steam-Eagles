using System;
using System.Collections.Generic;
using System.Linq;
using Buildings.Messages;
using CoreLib;
using UniRx;
using UnityEngine;

namespace Buildings.Rooms.Tracking
{
    public class PCTracker : IDisposable
    {
        private readonly PC[] _instances;
        private IDisposable _disposable;
        private Subject<(int playerNumber, PC pc)> onPCChanged = new();
        
        public PCTracker()
        {
            var cd = new CompositeDisposable();

            _instances = new PC[2] { null, null };
            MessageBroker.Default.Receive<PCInstanceChangedInfo>().Subscribe(info => SetPCInstance(info.pcInstance, info.playerNumber)).AddTo(cd);

            _disposable = cd;
        }

        private void SetPCInstance(PCInstance infoPCInstance, int infoPlayerNumber)
        {
            if(_instances[infoPlayerNumber] != null)
                _instances[infoPlayerNumber].Dispose();
            _instances[infoPlayerNumber] = new PC(infoPCInstance, infoPlayerNumber);
            onPCChanged.OnNext((infoPlayerNumber, _instances[infoPlayerNumber]));
        }

        public IObservable<PC> GetPC(int playerNumber)
        {
            var stream = onPCChanged.Where(t => t.Item1 == playerNumber).Select(t => t.Item2);
            if(_instances[playerNumber] != null)
                return stream.StartWith(_instances[playerNumber]);
            return stream;
        }
        public IObservable<PC> GetPCs()
        {
            return onPCChanged.Select(t => t.Item2).StartWith(from instance in _instances where instance != null select instance);
        }
        
        public class PC : IDisposable
        {
            public readonly PCInstance Instance;
            public readonly int PlayerNumber;
            
            private IDisposable _disposable;
            
            public Room LastRoom { get; set; }
            
            private Subject<(Room prevRoom, Room newRoom)> onRoomChanged = new();
            private ReactiveProperty<Room> pcRoom = new ReactiveProperty<Room>();
            
            public IReadOnlyReactiveProperty<Room> PCRoom => pcRoom;
            public IObservable<(Room prevRoom, Room newRoom)> OnRoomChanged => onRoomChanged;
            
            
            public PC(PCInstance instance, int num)
            {
                var cd  = new CompositeDisposable();
                Instance = instance;
                PlayerNumber = num;
                _disposable = cd;
                
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

        private IEnumerable<PC> AllPCs()
        {
            foreach (var pcWrapper in _instances)
            {
                if(pcWrapper != null)
                    yield return pcWrapper;
            }
        }

        public void Dispose()
        {
            _disposable?.Dispose();
            onPCChanged?.Dispose();
        }
    }
    public class PCRoomTracker
    {
        private readonly PC[] _instances;
        private IDisposable _disposable;
        
        private PC GetWrapper(int player) => _instances[player];
        public readonly Subject<PC> onPCInstanceChanged = new();
        public PCRoomTracker()
        {
            _instances = new PC[2] { null, null };
            var cd = new CompositeDisposable();
            Debug.Log("Created PC Room Tracker");
            MessageBroker.Default.Receive<PCInstanceChangedInfo>().Subscribe(info => SetPCInstance(info.pcInstance, info.playerNumber)).AddTo(cd);
            _disposable = cd;
        }
        private void SetPCInstance(PCInstance infoPCInstance, int infoPlayerNumber)
        {
            if(_instances[infoPlayerNumber] != null)
                _instances[infoPlayerNumber].Dispose();
            _instances[infoPlayerNumber] = new PC(infoPCInstance, infoPlayerNumber);
            onPCInstanceChanged.OnNext(_instances[infoPlayerNumber]);
        }

        public void Dispose() => _disposable?.Dispose();

        public class PC : IDisposable
        {
            public readonly PCInstance Instance;
            public readonly int PlayerNumber;
            
            private IDisposable _disposable;
            
            public Room LastRoom { get; set; }
            
            private Subject<(Room prevRoom, Room newRoom)> onRoomChanged = new();
            private ReactiveProperty<Room> pcRoom = new ReactiveProperty<Room>();
            
            public IReadOnlyReactiveProperty<Room> PCRoom => pcRoom;
            public IObservable<(Room prevRoom, Room newRoom)> OnRoomChanged => onRoomChanged;
            
            
            public PC(PCInstance instance, int num)
            {
                var cd  = new CompositeDisposable();
                Instance = instance;
                PlayerNumber = num;
                _disposable = cd;
                
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

        private IEnumerable<PC> AllPCs()
        {
            foreach (var pcWrapper in _instances)
            {
                if(pcWrapper != null)
                    yield return pcWrapper;
            }
        }
    }
}