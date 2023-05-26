using System;
using System.Collections.Generic;
using Buildings;
using CoreLib;
using UniRx;
using UnityEngine;

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
}