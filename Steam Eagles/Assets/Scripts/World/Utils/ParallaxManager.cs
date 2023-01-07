using System;
using System.Linq;
using CoreLib;
using UniRx;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace World
{
    public class ParallaxManager : MonoBehaviour
    {
        [SerializeField] private Camera camera;
        [SerializeField] private Transform subject;


        private ReactiveProperty<Transform> _dynamicSubject = new ReactiveProperty<Transform>();
        private Camera _dynamicCamera;
        
        public Transform Subject
        {
            get => _dynamicSubject.Value ? _dynamicSubject.Value : subject;
            set => _dynamicSubject.Value = value;
        }
        
        
        public Camera Cam
        {
            get => _dynamicCamera ? _dynamicCamera : camera;
            set => _dynamicCamera = value;
        }
        
        Transform[] parallaxTransforms;
        private ParallaxObject[] parallaxObjects;
        private bool ready = false;
        private JobResources _jobResources = new JobResources();
        
        private void Awake()
        {
            Subject = subject;
            Cam = camera;
            GameObject[] parallaxGameObjects = GameObject.FindGameObjectsWithTag("Parallax");
            GameObject[] parallaxParentGameObjects = GameObject.FindGameObjectsWithTag("ParallaxChildren");
            var pTransforms = parallaxGameObjects.Select(t => t.transform).ToList();
            foreach (var parallaxParentGameObject in parallaxParentGameObjects)
            {
                var pt = parallaxParentGameObject.transform;
                for (int i = 0; i < pt.childCount; i++)
                {
                    if (pt.GetChild(i).CompareTag("Parallax")) continue;
                    pTransforms.Add(pt.GetChild(i));
                }
            }

            
            parallaxTransforms = pTransforms.ToArray();
            int totalParallaxObjects = parallaxTransforms.Length;
            Debug.Log($"Parallax Manager {name} found <b>{totalParallaxObjects}</b> Parallax Objects",this);
            
            _dynamicSubject = new ReactiveProperty<Transform>();
            _dynamicSubject.Select(t => t ? t : subject)
                .Subscribe(OnSubjectSet).AddTo(this);
            
            parallaxObjects = new ParallaxObject[parallaxTransforms.Length];
            for (int i = 0; i < parallaxTransforms.Length; i++)
            {
                var t = parallaxTransforms[i];
                parallaxObjects[i] = new ParallaxObject(subject, t);
            }
        }
        
        void OnSubjectSet(Transform newSubject)
        {
            
            newSubject = newSubject ? newSubject : this.subject;
            parallaxObjects = parallaxTransforms.Select(t => new ParallaxObject(t, newSubject)).ToArray();
        }

        [Serializable]
        struct ParallaxObject
        {
            internal readonly Transform _transform;
            internal readonly Transform _parallaxObject;
            internal Vector2 _startPosition;
            internal float _startZ;


            public ParallaxObject(Transform transform, Transform parallaxObject)
            {
                _transform = transform;
                _parallaxObject = parallaxObject;
                _startPosition = parallaxObject.position;
                _startZ = parallaxObject.position.z;
            }

            public Vector2 StartPosition => _startPosition;
            public float StartZ => _startZ;
        }



        [Serializable]
        struct ParallaxObjectJobSafe
        {
            internal readonly int _index;
            internal Vector2 _startPosition;
            internal float _startZ;


            public ParallaxObjectJobSafe(int i, ParallaxObject parallaxObject)
            {
                _index = i;
                _startPosition = parallaxObject._startPosition;
                _startZ = parallaxObject._startZ;
            }

            public Vector2 StartPosition => _startPosition;
            public float StartZ => _startZ;
        }

        [Serializable]
        class JobResources
        {
            public ParallaxObjectJobSafe[] ParallaxObjectJobSafes;
            internal TransformAccessArray transformAccessArray;
            public NativeArray<ParallaxObjectJobSafe> parallaxObjects;
            internal ParallaxJob _parallaxJob;
            public JobHandle _parallaxJobHandle;
            public bool running;
            public bool hasResources;
            private void CreateResources(ParallaxManager manager)
            {
                if (hasResources)
                {
                    PrintErrorReason(nameof(CreateResources),  "Resources already created!");
                    return;
                }
                ParallaxObjectJobSafes = new ParallaxObjectJobSafe[manager.parallaxObjects.Length];
                for (int i = 0; i < manager.parallaxObjects.Length; i++)
                {
                    var pObj = manager.parallaxObjects[i];
                    ParallaxObjectJobSafes[i] = new ParallaxObjectJobSafe(i, pObj);
                }

                parallaxObjects = new NativeArray<ParallaxObjectJobSafe>(ParallaxObjectJobSafes, Allocator.TempJob);
                transformAccessArray = new TransformAccessArray(manager.parallaxTransforms);
                
                hasResources = true;

            }

            public bool TryCreateResources(ParallaxManager manager)
            {
                if (hasResources)
                {
                    DisposeOfResources();
                }
                if (hasResources)
                {
                    PrintErrorReason(nameof(CreateResources),  "Resources already created!");
                    return false;
                }
                CreateResources(manager);
                return hasResources;
            }

            public bool TrySchedule(ParallaxManager manager)
            {
                
                if (!TryCreateResources(manager))
                {
                    PrintErrorReason($"Schedule Parallax",  "Missing Resources");
                    return false;
                }
                if (running)
                {
                    PrintErrorReason($"Schedule Parallax",  "Job already running");
                    return false;
                }
                var cam = manager.Cam;
                if (cam == null)
                {
                    PrintErrorReason($"Schedule Parallax", "Missing Camera");
                    return false;
                }
            
                var sub =manager. Subject;
                if (sub == null)
                {
                    PrintErrorReason($"Schedule Parallax", "Missing Subject");
                    return false;
                }
                Schedule(cam, sub);
                return true;
            }

            private void Schedule(Camera cam, Transform sub)
            {
                _parallaxJob =CreateJob(cam, sub);
                _parallaxJobHandle = _parallaxJob.Schedule(transformAccessArray);
                running = true;
            }

            private ParallaxJob CreateJob(Camera cam, Transform sub)
            {
                var camPosition = cam.transform.position;
                float nearClippingPlane = camPosition.z + cam.nearClipPlane;
                float farClippingPlane = camPosition.z + cam.farClipPlane;

                Vector3 subjectPosition = sub.transform.position;
                float subjectZPosition = subjectPosition.z;
                var pJob = new ParallaxJob()
                {
                    farClippingPlane = farClippingPlane,
                    nearClippingPlane = nearClippingPlane,

                    cameraZPosition = camPosition.z,
                    cameraPosition = camPosition,

                    subjectZPosition = subjectZPosition,
                    parallaxObjects = parallaxObjects
                };
                return pJob;
            }

            
            public bool TryDispose()
            {
                if (!running)
                {
                    PrintErrorReason("Dispose of Parallax Job", "Job is not running!");
                    return false;
                }

                if (!_parallaxJobHandle.IsCompleted)
                {
                    PrintErrorReason("Dispose of Parallax Job", "Job is not completed!");
                    return false;
                }
                Dispose();
                return true;
            }
            public void Dispose()
            {
                _parallaxJobHandle.Complete();
                DisposeOfResources();
                running = false;
            }

            private void DisposeOfResources()
            {
                if(parallaxObjects.IsCreated)
                    parallaxObjects.Dispose();
                
                if(transformAccessArray.isCreated)
                    transformAccessArray.Dispose();
                
                hasResources = false;
            }

            private void PrintErrorReason(string caller, string msg)
            {
                Debug.LogError($"Cannot {caller},{msg.Bolded().InItalics()}");
            }
        }

        
        private void Update()
        {
            if (parallaxObjects == null || parallaxObjects.Length == 0)
            {
                enabled = false;
                return; 
            }
            
            if (_jobResources.TrySchedule(this))
            {
                Debug.Log("Scheduled Job Successfully!");
                return;
            }
        }

        private void LateUpdate()
        {
            if (parallaxObjects == null || parallaxObjects.Length == 0)
            {
                enabled = false;
                return; 
            }
        
            _jobResources.TryDispose();
        }

        struct ParallaxJob : IJobParallelForTransform
        {
            public float nearClippingPlane;
            public float farClippingPlane;
            public float subjectZPosition;
            public float cameraZPosition;
            public Vector2 cameraPosition;
            [ReadOnly]
            public NativeArray<ParallaxObjectJobSafe> parallaxObjects;

            public void Execute(int index, TransformAccess transform)
            {
                ParallaxObjectJobSafe pObject = parallaxObjects[index];
                Vector2 startPosition = pObject._startPosition;
                float startZ = pObject._startZ;
                float distanceFromSubject = subjectZPosition - startZ;
                bool useFarClippingPlane = distanceFromSubject > 0;
                float clippingPlane = cameraZPosition + (useFarClippingPlane ? farClippingPlane : nearClippingPlane);
                float parallaxFactor = Abs(distanceFromSubject) / clippingPlane;

                Vector2 travel = cameraPosition - startPosition;
                Vector3 newPos = startPosition + travel * parallaxFactor;
                transform.position = newPos;
            }

            static float Abs(float f)
            {
                if (f < 0) return -f;
                return f;
            }
        }
    }
}