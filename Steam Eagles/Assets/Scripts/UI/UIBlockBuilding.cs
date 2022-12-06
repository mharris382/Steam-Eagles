using System;
using CoreLib;
//using Spaces;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;

namespace UI
{
    public class UIBlockBuilding : MonoBehaviour
    {
        public string resourceTag;
        public TargetingResources targetingResources;
        private TilemapResources _tilemapResources;
        
        
        public BlockSlot[] blockSlots;
        public TilemapResources Tilemaps => _tilemapResources ?? new TilemapResources();
        
        
        [Serializable]
        public class BlockSlot
        {
            public string blockName;
           public Tile tile;
           public SharedInt blockCount;
            [Range(-1, 100)]
            public int startingBlockAmount = 5;

            public bool IsValid() => tile != null && blockCount != null;


            public void SetStartingBlockAmount()
            {
                if (startingBlockAmount == -1) return;
                if(!IsValid()) Debug.LogError("Block slot");
            }
        }
        
        
        public class TilemapResources
        {
            /// <summary>
            /// returns all tilemaps which 
            /// </summary>
            /// <returns></returns>
            public Tilemap[] GetAllBuildableTilemaps()
            {
                throw new NotImplementedException();
            }
        }
        
        
        [Serializable]
        public class TargetingResources
        {
            public string resourceTag = "Builder";
            [SerializeField] Camera camera;
            [SerializeField] SharedCamera sharedCamera;
           // [SerializeField] private bool findTaggedCamera = false;
            [SerializeField] private bool respondToEvents = false;
            [SerializeField] private bool log;
            private Camera _camera;

            void Log(string msg, UnityEngine.Object obj) { if (log) Debug.Log(msg, obj); }
            
            public Camera Camera
            {
                get
                {
                    if (_camera == null)
                    {
                        Camera FindTaggedCamera()
                        {
                            var anyCameras = FindObjectsOfType<Camera>();
                            Camera taggedCamera = null;
                            foreach (var anyCamera in anyCameras)
                            {
                                if (anyCamera.CompareTag(resourceTag))
                                {
                                    taggedCamera = anyCamera;
                                    Debug.Log($"{resourceTag} - UI Block Targeting Found Tagged Camera {anyCamera.name}");
                                    break;
                                }
                            }
                            if (taggedCamera == null) Debug.LogWarning($"{resourceTag} was unable to locate a tagged camera!");
                            return taggedCamera;
                        }

                        var taggedCamera = FindTaggedCamera();
                       
                        if (sharedCamera != null)
                        {
                            void AssignSharedCamera(Camera toAssign)
                            {
                                Debug.Assert(toAssign != null);
                                _camera = camera = sharedCamera.Value = toAssign;
                                Log($"{resourceTag} - Targeting assigned {sharedCamera.name}.Value to {camera}", camera);
                            }
                            if (taggedCamera != null)
                                AssignSharedCamera(taggedCamera);
                            if (camera != null)
                                AssignSharedCamera(camera);
                            else if (sharedCamera.HasValue)
                                AssignSharedCamera(sharedCamera.Value);
                            else
                            {
                                
                                
                            }
                            if (Camera.main != null)
                            {
                                
                            }
                            else
                            {
                                Debug.LogError($"{resourceTag} - Targeting found no Main Camera in scene!");
                               
                                
                                
                            }
                        }
                        else if (camera == null)
                        {
                            _camera = camera = Camera.main;  
                            Log($"{resourceTag} - Targeting is now hard coded to use main camera: {camera}", camera);
                        }
                        else
                        {
                            _camera = camera;
                            Log($"{resourceTag} - Targeting is hard coded to use camera: {camera}", camera);
                        }
                    }
                    Debug.Assert(_camera!=null, "Missing targeting camera!");
                    return _camera;
                }
            }
        }
    }
}