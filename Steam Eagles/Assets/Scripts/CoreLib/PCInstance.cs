using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace CoreLib
{
    public class PCInstance : IDisposable
    {
        public CompositeDisposable Disposable { get; private set; }
        public PCInstance(GameObject character, GameObject camera, GameObject input)
        {
            Disposable = new CompositeDisposable();
            this.character = character;
            this.camera = camera;
            this.input = input;
        }

        public GameObject character {get; private set;}
        public GameObject camera {get; private set;}
        public GameObject input {get; private set;}

        public void Dispose()
        {
            Disposable?.Dispose();
            Disposable = null;
        }

        public bool IsValid() => character != null && camera != null && input != null;
    }

    public struct PCInstanceChangedInfo
    {
        public int playerNumber;
        public PCInstance pcInstance;

        public PCInstanceChangedInfo(int player, PCInstance pc)
        {
            playerNumber = player;
            pcInstance = pc;
        }
    }

    

    public abstract class PCSubsystem<T> : IDisposable where T : IDisposable
    {

        private readonly DataContainer[] _containers;
        private CompositeDisposable _disposable;
        
        public PCSubsystem()
        {
            _containers = new DataContainer[2] { null, null };
            var cd = new CompositeDisposable();
            MessageBroker.Default.Receive<PCInstanceChangedInfo>()
                .Subscribe(info => OnPlayerSet(info.pcInstance, info.playerNumber)).AddTo(cd);
            _disposable = cd;
        }
        
        protected abstract T CreateDataForPC(PCInstance pcInstance, int playerNumber);

        private void OnPlayerSet(PCInstance instance, int pNumber)
        {
            if(_containers[pNumber] != null)
                _containers[pNumber].Data.Dispose();
            _containers[pNumber] = new DataContainer(CreateDataForPC(instance, pNumber), instance, pNumber);
        }

        public bool HasPlayer(int player) => _containers[player] != null;
        public T CreateDataForPlayer(int player) => _containers[player].Data;

        public IEnumerable<T> GetData() => from container in _containers where container != null select container.Data;
        public IEnumerable<(PCInstance instance, T data)> GetPCData() => from container in _containers where container != null select (container.PC, container.Data);

        private class DataContainer
        {
            private readonly T _data;
            private readonly PCInstance _pcInstance;
            private readonly int _playerNumber;

            public T Data => _data;
            public PCInstance PC => _pcInstance;
            public int PlayerNumber => _playerNumber;
            
            public DataContainer(T data, PCInstance pcInstance, int playerNumber)
            {
                _data = data;
                _pcInstance = pcInstance;
                _playerNumber = playerNumber;
            }
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }
}