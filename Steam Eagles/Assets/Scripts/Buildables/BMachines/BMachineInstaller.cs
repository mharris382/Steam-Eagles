using System;
using Buildables;
using Buildings;
using CoreLib;
using UnityEngine;
using Zenject;

namespace Buildables
{
    public class BMachineInstaller : MonoInstaller<BMachineInstaller>
    {
        [SerializeField] private BMachineConfig config;
        public override void InstallBindings()
        {
            Container.Bind<BMachineConfig>().FromInstance(config).AsSingle();
            Container.Bind<BMachineMap>().AsSingle().NonLazy();
            Container.Bind<BMachineHelper>().AsSingle().NonLazy();
            Container.BindFactory<BuildableMachineBase, Vector2Int, BMachine, BMachine.Factory>().AsSingle().NonLazy();
            Container.Bind<CellDebugger>().FromInstance(GetComponentInChildren<CellDebugger>()).AsSingle().NonLazy();
        }
        
        
        
    }

    [Serializable]
    public class BMachineConfig : ConfigBase
    {
        public bool debugFloor => debugMode == DebugMode.FLOOR;
        public bool debugCells => debugMode == DebugMode.VALID_CELLS;

        public DebugMode debugMode = DebugMode.VALID_CELLS;
        public enum DebugMode
        {
            FLOOR,
            VALID_CELLS,
            NONE
        }
    }
}