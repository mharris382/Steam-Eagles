using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Buildables.Wireframe
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class WireframeInstance : MonoBehaviour
    {
        public const string WIREFRAME_PREFAB_ADDRESS = "Buildable Wireframe";


        public static bool IsWireframePrefabLoaded;
        
        private static GameObject _wireframePrefab;

        public static async UniTask<GameObject> GetWireframePrefabAsync()
        {
            if (IsWireframePrefabLoaded)
                return _wireframePrefab;
            
            _wireframePrefab = await Addressables.LoadAssetAsync<GameObject>(WIREFRAME_PREFAB_ADDRESS).ToUniTask();
            IsWireframePrefabLoaded = true;
            return _wireframePrefab;
        }
        

        private SpriteRenderer _sr;
        public SpriteRenderer Sr => _sr!=null ? _sr : _sr = GetComponent<SpriteRenderer>();

        
        [LabelText("BM"), OnValueChanged(nameof(SetMachine))]
        public BuildableMachine machine;


        private void Awake()
        {
            Sr.drawMode = SpriteDrawMode.Sliced;
        }

        public void SetMachine(BuildableMachine bm)
        {
            SetMachineToTarget(bm, null);
        }
        public void SetMachineToTarget(BuildableMachine bm, GameObject buildingTarget)
        {
            if (buildingTarget== null && bm.buildingTarget == null)
            {
                Debug.LogError("You must specify a building target for the wireframe",this);
                return;
            }
            machine = bm;
            Sr.size = bm.WsSize;
        }
    }
}