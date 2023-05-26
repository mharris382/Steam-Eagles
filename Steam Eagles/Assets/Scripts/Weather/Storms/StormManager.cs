using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using UniRx;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Zenject;


namespace Weather.Storms
{
    public class StormManager : IExtraSlowTickable, IDisposable, IInitializable
    {
        private readonly GlobalStormConfig _config;
        private readonly StormRegistry _stormRegistry;
        private readonly StormSubjectsRegistry _subjectsRegistry;

        private CompositeDisposable _cd;
        private readonly Storm.Factory _stormFactory;
        private Queue<StormCreationRequest> _creationRequests = new Queue<StormCreationRequest>();
        private Queue<StormRemovalRequest> _removalRequests = new Queue<StormRemovalRequest>();

        private NativeArray<Bounds> _stormOuterBounds;
        private NativeArray<Bounds> _subjectBounds;
        private NativeArray<int> _subjectStormIndices;
        private Dictionary<StormSubject, int> _subjectIndices = new Dictionary<StormSubject, int>();
        
        private int _subjectCount;
        private int _stormCount;
        private JobHandle _boundsCheckJobHandle;

        private int StormCount
        {
            get => _stormCount;
            set
            {
                if (_stormCount != value)
                {
                    _stormOuterBounds.Dispose();
                    _stormOuterBounds = new NativeArray<Bounds>(value, Allocator.Persistent);
                    _stormCount = value;
                }
            }
        }
        private int SubjectCount
        {
            get => _subjectCount;
            set
            {
                if (_subjectCount != value)
                {
                    _subjectBounds.Dispose();
                    _subjectStormIndices.Dispose();
                    _subjectBounds = new NativeArray<Bounds>(value, Allocator.Persistent);
                    _subjectStormIndices = new NativeArray<int>(value, Allocator.Persistent);
                    foreach (var subject in _subjectsRegistry.AllSubjects()) 
                    {
                        if (!_subjectIndices.ContainsKey(subject))
                        {
                            _subjectIndices.Add(subject, _subjectIndices.Count);
                        }
                    }
                    _subjectCount = value;
                }
            }
        }
        
        public StormManager(GlobalStormConfig config,
            StormRegistry stormRegistry,
            StormSubjectsRegistry subjectsRegistry,
            Storm.Factory stormFactory)
        {
            _cd = new CompositeDisposable();
            _config = config;
            _stormRegistry = stormRegistry;
            _subjectsRegistry = subjectsRegistry;
            
            _stormFactory = stormFactory;
            MessageBroker.Default.Receive<StormCreationRequest>()
                .Subscribe(request => _creationRequests.Enqueue(request)).AddTo(_cd);
            MessageBroker.Default.Receive<StormRemovalRequest>().Subscribe(_removalRequests.Enqueue).AddTo(_cd);

        }


        public void ExtraSlowTick(float deltaTime)
        {
            CreateStorms();
            RemoveStorms();
            StormCount = _stormRegistry.Count;
            SubjectCount = _subjectsRegistry.Count;


            if (StormCount == 0) return;
            if (SubjectCount == 0) return;
            
            CompleteStormJobs();
            
            _config.Log($"StormManager ExtraSlowTick:\n Active Storm Count:{_stormRegistry.Count}");
            
            RunStormJobs();
        }

        private void CreateStorms()
        {
            while (_creationRequests.Count > 0)
            {
                var request = _creationRequests.Dequeue();
                var storm = _stormFactory.Create(request.StormBounds, request.StormVelocity, request.StormFalloff);
                
                if (!string.IsNullOrEmpty(request.StormTag))
                    _stormRegistry.AddStorm(storm, request.StormTag);
                else
                    _stormRegistry.AddStorm(storm);
                request.CreatedStorm = storm;
                request.StormCreatedSubject.OnNext(storm);
            }
        }

        private void RemoveStorms()
        {
            while (_removalRequests.Count > 0)
            {
                var request = _removalRequests.Dequeue();
                if(request.Storm == null || request.Storm.IsCompleted)
                    continue;
                _stormRegistry.RemoveStorm(request.Storm);
                foreach (var stormSubject in _subjectsRegistry.AllSubjects())
                {
                    if (stormSubject.SubjectStorm == request.Storm)
                    {
                        stormSubject.SubjectStorm = null;
                    }
                }
            }
        }
        
        
        private void CompleteStormJobs()
        {
            // _boundsCheckJobHandle.Complete();
            //
            // for (int i = 0; i < SubjectCount; i++)
            // {
            //     var subject = _subjectsRegistry.GetSubject(i);
            //     var stormIndex = _subjectStormIndices[i];
            //     subject.SubjectStorm = stormIndex == -1 ? null : _stormRegistry.GetStorm(stormIndex);
            // }
        }
        
        private void RunStormJobs()
        {
            for (int i = 0; i < SubjectCount; i++)
            {
                var subject = _subjectsRegistry.GetSubject(i);
                bool foundStorm = false;
                for (int j = 0; j < StormCount; j++)
                {
                    var storm = _stormRegistry.GetStorm(j);
                    if (subject.SubjectBounds.Intersects(storm.InnerBoundsWs))
                    {
                        subject.SubjectStorm = storm;
                        foundStorm = true;
                        break;
                    }
                }
                if(!foundStorm)
                    subject.SubjectStorm = null;
            }
            // if (StormCount > 0 && SubjectCount > 0)
            // {
            //     for (int i = 0; i < _stormCount; i++)
            //     {
            //         var storm = _stormRegistry.GetStorm(i);
            //         _stormOuterBounds[i] = storm.OuterBoundsWs;
            //     }
            //
            //     for (int i = 0; i < SubjectCount; i++)
            //     {
            //         var subject = _subjectsRegistry.GetSubject(i);
            //         _subjectBounds[i] = subject.SubjectBounds;
            //     }
            //     
            //     var boundsCheckJob = new BoundsCheckJob()
            //     {
            //         results = _subjectStormIndices,
            //         stormBounds = _stormOuterBounds,
            //         stormCount = StormCount,
            //         subjectBounds = _subjectBounds
            //     };
            //    _boundsCheckJobHandle = boundsCheckJob.Schedule(SubjectCount, 1);
            // }
        }


        /// <summary>
        /// NOTE: this implementation does not allow subjects to be in more than one storm
        /// </summary>
        struct BoundsCheckJob : IJobParallelFor
        {
            public NativeArray<Bounds> subjectBounds;
            
            //index of the storm that the subject is in, -1 if not in a storm
            public NativeArray<int> results; 

            public NativeArray<Bounds> stormBounds;
            public int stormCount;
            
            public void Execute(int index)
            {
                var subjectBound = subjectBounds[index];
                for (int i = 0; i < stormCount; i++)
                {
                    var stormBound = stormBounds[i];
                    if (subjectBound.Intersects(stormBound))
                    {
                        results[index] = i;
                        break;
                    }
                }
                results[index] = -1;
            }
        }

        public void Dispose()
        {
            _stormOuterBounds.Dispose();
            _cd.Dispose();
        }

        public void Initialize()
        {
            _subjectsRegistry.OnSubjectAdded.Subscribe(subject =>
            {
                if(!_subjectIndices.ContainsKey(subject))
                    _subjectIndices.Add(subject, -1);
            }).AddTo(_cd);
            _subjectsRegistry.OnSubjectRemoved.Subscribe(subject => _subjectIndices.Remove(subject)).AddTo(_cd);
        }
    }
}