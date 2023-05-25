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
            return _pcStormSubjectFactory.Create(tracker, subject.character, camera);
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


    public class PCStormSubject : EntityStormSubject
    {
        private readonly IPCTracker _tracker;
        private readonly Camera _camera;
        private readonly EntityInitializer _pcEntityHook;

        public PCStormSubject(IPCTracker tracker, GameObject character, Camera camera) : base(character)
        {
            _tracker = tracker;
            _camera = camera;
            Debug.Assert(_camera != null, "Camera missing from PC Storm Subject", character);
        }
            
        public class Factory : PlaceholderFactory<IPCTracker, GameObject, Camera, PCStormSubject> { }
    }
}