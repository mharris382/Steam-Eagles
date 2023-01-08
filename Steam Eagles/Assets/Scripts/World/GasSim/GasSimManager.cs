using System;
using System.Collections.Generic;
using CoreLib;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace GasSim
{
    
    
    public class GasSimulation : MonoBehaviour, IGasSim
    {
        public float updateRate = 0.125f;
        public BoxCollider2D simulationBounds;
        public GasGridController gridController;
        public GasParticlesController particlesController;
        
        
        private IPressureGrid _pressureGrid;
        private GasSimParticleSystem.PressureGrid _pressureGridWrapper;
        private GasSimParticleSystem.GridHelper _gridHelper;
        private void Awake()
        {
            var size = simulationBounds.size;
            var cellSize = gridController.CellSize;
            var gridWidth = (int) (size.x / cellSize.x);
            var gridHeight = (int) (size.y / cellSize.y);
            var gridOffset = simulationBounds.offset;
            
            simulationBounds.OnTriggerEnter2DAsObservable()
                .Select(t => t.GetComponent<GasAgent>())
                .Where(t => t != null)
                .Subscribe(RegisterAgentToSimulation).AddTo(this);

            simulationBounds.OnTriggerExit2DAsObservable()
                .Select(t => t.GetComponent<GasAgent>())
                .Where(t => t != null)
                .Subscribe(UnregisterAgentToSimulation).AddTo(this);
        }

        void RegisterAgentToSimulation(GasAgent agent)
        {
            
        }
        
        void UnregisterAgentToSimulation(GasAgent agent)
        {
               
        }

        public Grid Grid => gridController.Grid;
        public RectInt SimulationRect { get; }
        
        
        public void SetStateOfMatter(Vector2Int coord, StateOfMatter stateOfMatter)
        {
            _pressureGridWrapper.SetState(coord, stateOfMatter);
        }

        public bool TryAddGasToCell(Vector2Int coord, int amount)
        {
            throw new NotImplementedException();
        }

        public bool TryRemoveGasFromCell(Vector2Int coord, int amount)
        {
            throw new NotImplementedException();
        }

        public bool CanAddGasToCell(Vector2Int coord, ref int amount)
        {
            throw new NotImplementedException();
        }

        public bool CanRemoveGasFromCell(Vector2Int coord, ref int amount)
        {
            throw new NotImplementedException();
        }
    }
}