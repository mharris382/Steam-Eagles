using System;
using System.Linq;
using Buildings;
using UniRx;
using UnityEngine;

namespace Buildables
{
    public abstract class MachineCell : MonoBehaviour
    {
        private BuildableMachineBase _buildableMachineBase;
        private CompositeDisposable _disposable;
        [SerializeField] private Vector2Int cellPosition;
        
        public Vector2Int CellPosition => cellPosition;

        public Vector2Int BuildingSpacePosition
        {
            get;
            set;
        }
        public BuildableMachineBase BuildableMachineBase
        {
            get
            {
                if (_buildableMachineBase == null) _buildableMachineBase = GetComponentInParent<BuildableMachineBase>();
                return _buildableMachineBase;
            }
        }

        public abstract BuildingLayers TargetLayer { get; }

        public abstract Color GizmoColor { get; }

        public bool HasResources() => BuildableMachineBase != null && BuildableMachineBase.HasResources();

        //TODO: need to call this when we destroy the machine
        public void OnMachineDestroyed(Vector2Int cell, Building building)
        {
            Debug.Assert(HasResources(),this);
            throw new NotImplementedException();
        }

        public void OnMachineBuilt(Vector2Int cell, Building building)
        {
            Debug.Assert(HasResources(),this);
            //if machine is on different layer then we need to convert the cell position to the correct layer
            var machineLayer = BuildableMachineBase.GetTargetLayer();
            var targetLayer = TargetLayer;
            if (machineLayer != targetLayer)
            {
                cell = (Vector2Int)building.Map.ConvertBetweenLayers(machineLayer, targetLayer, (Vector3Int)cell).First();
            }
            var offset = BuildableMachineBase.IsFlipped ? -cellPosition : cellPosition;
            var position = cell + offset;
            _disposable = new CompositeDisposable();
            OnCellBuilt((Vector3Int)position, building);
            BuildingSpacePosition = position;
        }
        
        /// <summary>
        /// called when the machine is built.  
        /// </summary>
        /// <param name="cell">the cell where the machine cell is placed</param>
        /// <param name="building">the building the machine was built on</param>
        protected abstract void OnCellBuilt(Vector3Int cell, Building building);


        #region [Helper Methods]

        /// <summary>
        /// consolidates disposable objects into a single disposable object that will be disposed when this machine cell is destroyed (i.e. when the machine is destroyed)
        /// </summary>
        /// <param name="disposable"></param>
        /// <exception cref="Exception"></exception>
        protected void AddDisposable(params IDisposable[] disposable)
        {
            if(_disposable == null)
                throw new Exception("Cannot add disposable before OnCellBuilt");

            foreach (var disposable1 in disposable) disposable1.AddTo(_disposable);
        }
        
        /// <summary>
        /// logs message conditionally based on whether machine debug mode is enabled
        /// </summary>
        /// <param name="message"></param>
        protected void Log(string message)
        {
            if(BuildableMachineBase.debug) Debug.Log($"{BuildableMachineBase.name} {name}: {message}", this);
        }

        #endregion
    }
}