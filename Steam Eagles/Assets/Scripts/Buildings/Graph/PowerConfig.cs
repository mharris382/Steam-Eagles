using System;
using System.Collections.Generic;
using QuikGraph;
using UnityEngine;
using Zenject;

namespace Buildings.Graph
{
    [Serializable]
    public class PowerConfig
    {
        [System.Obsolete("Need to switch to a reactive system, which only updates when the graph changes. " +
                         "Will need to specify a min period of time after a query before the consumer/producer state is updated. alternatively, update " +
                         "the consumer/producer state on every frame and use delta time to determine how much power to consume/produce." +
                         "better yet. Perform the calculation and store the result in a graph")]
        public float updateRate = 0.5f;
        public int maxNodesToProcessPerUpdate = 100;
        
        
        public static class PoweredLayersBuilding
        {
            public static readonly BuildingLayers[] PoweredLayers = new[]
            {
                BuildingLayers.PIPE,
                BuildingLayers.WIRES
            };
        }
        
        public class Power : IInitializable, IDisposable
        {
            private readonly BuildingMap _building;

            public Power(BuildingMap building, WireTilemapGraph wireTilemapGraph, PipeTilemapGraph pipeTilemapGraph)
            {
                _building = building;
                _powerGraphs.Add(BuildingLayers.PIPE, pipeTilemapGraph);
                _powerGraphs.Add(BuildingLayers.WIRES, wireTilemapGraph);
                foreach (var poweredLayer in PoweredLayersBuilding.PoweredLayers)
                {
                    Debug.Assert(_powerGraphs.ContainsKey(poweredLayer), $"{poweredLayer} needs to be added to the power graph");
                }
            }
            
            private AdjacencyGraph<BuildingCell, PowerEdge> _powerGraph = new();
            private HashSet<int> _graphComponentsWithNonZeroSources = new();

            private readonly Dictionary<BuildingLayers, PowerTilemapGraph> _powerGraphs = new ();
            
            private PowerTilemapGraph GetGraphForCell(BuildingCell cell) => _powerGraphs[cell.layers];
            

            #region [Graph Construction]

            public struct PowerEdge : IEdge<BuildingCell>
            {
                public readonly INodeConsumer source;
                public readonly INodeSupplier consumer;

                public PowerEdge(INodeConsumer source, INodeSupplier consumer)
                {
                    this.source = source;
                    this.consumer = consumer;
                }

                public BuildingCell Source => source.Position;
                public BuildingCell Target => consumer.Position;
            }
            
            // we have to query whenever the graph updates, but we only need to verify whether or not the removed node was previously part of
            // our graph component (loosely connected graph), if it did we need to check all our consumer cells and determine if they are still connected.
            //      - if the consumer cell is still on the same graph component, edge remains unchanged
            //      - if the consumer cell is no longer on the same graph component, edge is removed

            #endregion

            #region [Interfaces]

            public interface IPowerNode
            {
                BuildingCell Position { get; }
            }
            public interface INodeConsumer : IPowerConsumer, IPowerNode { }
            public interface INodeSupplier : IPowerSupplier, IPowerNode { }
            

            #endregion
            #region [Internal Structures]
            
            private struct SupplierWrapper : INodeSupplier
            {
                private readonly BuildingCell _buildingCell;
                private readonly IPowerSupplier _supplier;

                public BuildingCell Position => _buildingCell;

                public SupplierWrapper(BuildingCell buildingCell, IPowerSupplier supplier)
                {
                    _buildingCell = buildingCell;
                    _supplier = supplier;
                }

                public IObservable<float> GetSupplyRateChanges()
                {
                    return _supplier.GetSupplyRateChanges();
                }

                public float GetSupplyRate()
                {
                    return _supplier.GetSupplyRate();
                }

                public float Supply(float supply)
                {
                    return _supplier.Supply(supply);
                }
            }
            private struct ConsumerWrapper : INodeConsumer
            {
                private readonly BuildingCell _cell;
                private readonly IPowerConsumer _consumer;
                public BuildingCell Position => _cell;

                public ConsumerWrapper(BuildingCell cell, IPowerConsumer consumer)
                {
                    _cell = cell;
                    _consumer = consumer;
                }

                public IObservable<float> GetConsumptionRateChanges()
                {
                    return _consumer.GetConsumptionRateChanges();
                }

                public float GetConsumptionRate()
                {
                    return _consumer.GetConsumptionRate();
                }

                public void Consume(float amount)
                {
                    _consumer.Consume(amount);
                }
            }

            #endregion

            
            /// <summary>
            /// there are 2 ways the graph can be updated:
            ///     1. the consumption rate and/or the production rate changes on one of the connected components
            ///     2. the graph connections change (i.e. the tilemap will result in an update of the powered tilemap being changed)
            /// </summary>
            /// <exception cref="NotImplementedException"></exception>

            public void Initialize()
            {
                //Set up listeners
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }
       
        
    }
}