using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace GasSim
{
    public class SimIORegistry : MonoBehaviour
    {
        public List<SimIOPoint> dynamicSources = new List<SimIOPoint>();
        private List<IGasSink> _sinks = new List<IGasSink>();
        private List<IGasSource> _sources = new List<IGasSource>();

        [ToggleGroup(nameof(enableFixedSources))]public bool enableFixedSources = true;
        [ToggleGroup(nameof(enableFixedSources))]public int testSourceAmount = 1;
        [ToggleGroup(nameof(enableFixedSources))]public List<Vector2Int> testSources = new List<Vector2Int>();
    
        private BoundsInt _simBounds;
        private Grid _grid;
        private BoxCollider2D _collider;

        private BoxCollider2D Collider => _collider ? _collider : _collider = GetComponent<BoxCollider2D>();


        public void InitializeIOTracking(BoxCollider2D boundingArea, Grid grid, BoundsInt simBounds)
        {
            boundingArea.OnTriggerEnter2DAsObservable()
                .Select(t => t.GetComponent<SimIOPoint>())
                .Where(t => t != null)
                .Subscribe(OnIOEntered).AddTo(this);
        
            boundingArea.OnTriggerExit2DAsObservable()
                .Select(t => t.GetComponent<SimIOPoint>())
                .Where(t => t != null)
                .Subscribe(OnIOExited).AddTo(this);

            this._grid = grid;
            this._simBounds = simBounds;
        }

        public IEnumerable<(Vector2Int cellSpacePos, int amount, Action<int> onGasAdded)> GetSimIOSources()
        {
            foreach (var testSource in testSources)
            {
                if (_simBounds.Contains(new Vector3Int(testSource.x, testSource.y, 0)))
                {
                    yield return (testSource, testSourceAmount, null);
                }
            }

            foreach (var gasSource in _sources)
            {
                int rate = gasSource.SourceFlowRate;
                if (rate <= 0) continue;
                foreach (var cell in gasSource.GetCellsFromRect(_grid, _simBounds))
                {
                    yield return (cell,Mathf.Min(rate, gasSource.RatePerCell), t =>
                    {
                        gasSource.OnGasAddedToSim(t);
                        rate -= t;
                    });
                    if (rate <= 0)
                        break;
                }
            }
        }

        public IEnumerable<(Vector2Int cellSpacePos, int amount, Action<int> onGasRemoved)> GetSimIOSinks()
        {
            if (enableFixedSources)
            {
                var bounds = _simBounds;
                var maxY = bounds.max.y-1;
                for (int x = 0; x < bounds.xMax; x++)
                {
                    var cellPos = new Vector2Int(x, maxY);
                    yield return (cellPos, 15, null);
                }
            }

            foreach (var gasSink in _sinks)
            {
                int rate = gasSink.SinkFlowRate;
                if (rate == 0) continue;
                foreach (var cell in gasSink.GetCellsFromRect(_grid, _simBounds))
                {
                    yield return (cell, rate, gasSink.OnGasRemovedFromSim);
                }
            }
        }

        void OnIOEntered(SimIOPoint simIOPoint)
        {
            Debug.Log($"SimIOPoint {simIOPoint.name} Entered", this);
            if(dynamicSources.Contains(simIOPoint)) return;
            dynamicSources.Add(simIOPoint);
            var sink = simIOPoint.GetComponent<IGasSink>();
            var src = simIOPoint.GetComponent<IGasSource>();
            
            if (sink != null)
            {
                _sinks.Add(sink);
            }
            if(src != null)
            {
                _sources.Add(src);
            }
        }

        void OnIOExited(SimIOPoint simIOPoint)
        {
            Debug.Log($"SimIOPoint {simIOPoint.name} Exited", this);
            dynamicSources.Remove(simIOPoint);
            var sink = simIOPoint.GetComponent<IGasSink>();
            var src = simIOPoint.GetComponent<IGasSource>();
            
            if (sink != null)
            {
                _sinks.Remove(sink);
            }
            if(src != null)
            {
                _sources.Remove(src);
            }
        }


        private void OnDrawGizmos()
        {
            if (_grid == null)
            {
                _grid = GetComponent<Grid>();
            }
            foreach (var testSource in testSources)
            {
                Gizmos.color = Color.blue;
                var wsPos = _grid.GetCellCenterWorld(new Vector3Int(testSource.x, testSource.y, 0));
                Gizmos.DrawCube(wsPos, _grid.cellSize);
            }
        }
    }

    public interface ISimIO
    {
        IEnumerable<Vector2Int> GetCellsFromRect(Grid grid, BoundsInt simBounds);

    }
    public interface IGasSink : ISimIO
    {
        /// <summary>
        /// amount of gas that will be added to the simulation per cell that is sampled
        /// </summary>
        int SinkFlowRate { get; }
        void OnGasRemovedFromSim(int amountRemoved);
    }
    public interface IGasSource : ISimIO
    {
     
        /// <summary>
        /// amount of gas that will be added to the simulation per cell that is sampled
        /// </summary>
        int SourceFlowRate { get; }
        int RatePerCell { get; }
        void OnGasAddedToSim(int amountAdded);
    }
}