using System;
using Buildings;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UniRx;
using Zenject;

namespace Buildables
{
    
    public class ExhaustVent : Machine<ExhaustVent>
    {
        [ChildGameObjectsOnly]  public Transform steamInput;
        [ChildGameObjectsOnly] public Transform steamOutput;
        
        public SteamStorageUnit ventInternalStorage;
        
        public UnityEvent<float> onSteamConsumed;
        private ExhaustVentController _controller;


        
       public bool HasOutput => steamOutput != null;
        
       public BuildingCell OutputCell => !HasOutput ? default : (Building == null ? default : Building.Map.WorldToBCell(steamOutput.position, BuildingLayers.PIPE));
        
        public bool HasInput => steamInput != null;
        
        public BuildingCell InputCell => !HasInput ? default : (Building == null ? default : Building.Map.WorldToBCell(steamOutput.position, BuildingLayers.PIPE));
        
        
        
        [ShowInInspector, ReadOnly, FoldoutGroup("Debugging"), HideInEditorMode]
        public int ConsumptionCount
        {
            get;
            set;
        }

        [ShowInInspector, ReadOnly, FoldoutGroup("Debugging"), HideInEditorMode]
        public float ConsumptionAmountTotal
        {
            get;
            set;
        }

        private void Awake()
        {
            ventInternalStorage.Subscribe(t => onSteamConsumed?.Invoke(t));
        }
        //
        // [Inject]
        // public void InjectMe(ExhaustVentController.Factory controllerFactory)
        // {
        //     _controller = controllerFactory.Create(this);
        // }

        // private void OnDestroy()
        // {
        //     _controller.Dispose();
        // }
    }
}