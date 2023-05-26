using System;
using System.Collections.Generic;
using Buildings;
using CoreLib;
using CoreLib.Entities;
using CoreLib.Signals;
using UniRx;
using UnityEngine;
using Zenject;

namespace Weather.Storms
{
    public class PcStormSubjectManager : StormSubjectManagerBase<PCInstance>
    {
        private readonly GlobalPCInfo _pcRegistry;
        private readonly PCStormSubject.Factory _pcStormSubjectFactory;

        public PcStormSubjectManager(GlobalPCInfo pcRegistry, PCStormSubject.Factory pcStormSubjectFactory)
        {
            _pcRegistry = pcRegistry;
            _pcStormSubjectFactory = pcStormSubjectFactory;
        }
        protected override void ListenForDynamicStormSubjects(Action<IStormSubject> onStormSubjectAdded, Action<IStormSubject> onStormSubjectRemoved, CompositeDisposable cd)
        {
            _pcRegistry.OnPCAdded.Select(_pcRegistry.GetInstance).Select(CreateStormSubject).Subscribe(onStormSubjectAdded).AddTo(cd);
            _pcRegistry.OnPCRemoved.Select(_pcRegistry.GetInstance).Select(GetSubjectFor).Subscribe(onStormSubjectRemoved).AddTo(cd);
        }

        protected override IStormSubject CreateStormSubject(PCInstance subject)
        {
            var tracker = _pcRegistry.GetTracker(subject.PlayerNumber);
            var camera = subject.camera.GetComponent<Camera>();
            return _pcStormSubjectFactory.Create(subject, tracker, subject.character, camera);
        }

        protected override IEnumerable<PCInstance> GetInitialSubjects()
        {
            for (int i = 0; i < 2; i++)
            {
                if (_pcRegistry.HasPc(i))
                {
                    yield return _pcRegistry.GetInstance(i);
                }
            }
        }
    }


    public class StormViewRegistry
    {
        
    }

    public class PCStormSubject : EntityStormSubject
    {
        private readonly PCInstance _pc;
        private readonly IPCTracker _tracker;
        private readonly Camera _camera;
        private readonly EntityInitializer _pcEntityHook;

        public Camera Camera => _camera;
        
        public PCStormSubject(PCInstance pc, IPCTracker tracker, GameObject character, Camera camera) : base(character)
        {
            _pc = pc;
            _tracker = tracker;
            _camera = camera;
            Debug.Assert(_camera != null, "Camera missing from PC Storm Subject", character);
        }
        public class Factory : PlaceholderFactory<PCInstance, IPCTracker, GameObject, Camera, PCStormSubject> { }
        
        public override Bounds SubjectBounds
        {
            get
            {
                var size = Mathf.Pow(_camera.orthographicSize*2, 2) +25;
                var screenSpaceRect = _camera.pixelRect;
                var screenSpaceSize = new Vector2(size * screenSpaceRect.width, size * screenSpaceRect.height);
                var center = _camera.transform.position;
                center.z = 0;
                return new Bounds(center, new Vector3(screenSpaceSize.x, screenSpaceSize.y, 1));
            }
        }

        public override void OnStormAdded(Storm storm)
        {
            var stormView = storm.GetOrCreateView();
            int playerNumber = _pc.PlayerNumber;
            stormView.NotifyPlayerEntered(playerNumber, this);
        }

        public override void OnStormRemoved(Storm storm)
        {
            if (storm.HasView == false) return;
            var stormView = storm.GetView();
            int playerNumber = _pc.PlayerNumber;
            stormView.NotifyPlayerExited(playerNumber, this);
        }
    }
}