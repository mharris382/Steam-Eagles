using System;
using System.Collections.Generic;
using Buildings.Graph;
using Buildings.Tiles;
using CoreLib;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace Buildings
{
    public class RemotePowerConnections
    {
        
    }
    public class ElectricityConsumers : Registry<IElectricityConsumer>, IInitializable
    {
        private readonly WireTilemapGraph _graph;
        private readonly BuildingPowerGrid _powerGrid;
        private readonly ElectricityConfig _config;
        private readonly ElectricityConsumerWrapper.Factory _factory;
        private readonly CompositeDisposable _cd = new();
        Subject<Unit> onGraphChanged = new();
        private Dictionary<IElectricityConsumer, ElectricityConsumerWrapper> _wrappers = new();

        public ElectricityConsumers(WireTilemapGraph graph, BuildingPowerGrid powerGrid, ElectricityConfig config, ElectricityConsumerWrapper.Factory factory)
        {
            _graph = graph;
            _powerGrid = powerGrid;
            _config = config;
            _factory = factory;
        }


        public class ElectricityConsumerWrapper : IPowerConsumer, IDisposable
        {
            public class Factory : PlaceholderFactory<IElectricityConsumer, ElectricityConsumerWrapper> { }

            
            private readonly IElectricityConsumer _consumer;
            private readonly Building _building;
            private readonly ElectricityConfig _config;
            private readonly WireTilemapGraph _wireTilemapGraph;
            private readonly ElectricityLineHandler _lineHandler;
            private IDisposable _currentConnection;
            private readonly Dictionary<BuildingCell, BuildingCell> _wireToDeviceLookup = new();

            private BuildingPowerGrid PowerGrid => _building.Map.PowerGrid;
            private BuildingMap Map => _building.Map;

            public ElectricityConsumerWrapper(
                IElectricityConsumer consumer, 
                Building building,
                ElectricityConfig config,
                WireTilemapGraph wireTilemapGraph,
                ElectricityLineHandler lineHandler)
            {
                _consumer = consumer;
                _building = building;
                _config = config;
                _wireTilemapGraph = wireTilemapGraph;
                _lineHandler = lineHandler;
            }

            void EstablishConsumer(BuildingCell consumerCell)
            {
                DisconnectConsumer();
                var result = PowerGrid.AddConsumer(consumerCell, this);
                if (!result)
                {
                    Debug.LogError("Failed to add consumer at cell: " + consumerCell);
                    return;
                }

                var wireCell = consumerCell;
                var deviceCell = _wireToDeviceLookup[wireCell];
                _lineHandler.AddLine(deviceCell, wireCell);
                
                _currentConnection = UniRx.Disposable.Create(() =>
                {
                    _lineHandler.RemoveLineFrom(deviceCell);
                    PowerGrid.RemoveConsumer(consumerCell);
                });
            }

            private void DisconnectConsumer()
            {
                _currentConnection?.Dispose();
            }

            public bool UpdateConnection()
            {
                DisconnectConsumer();
                
                var demandWillAdd = GetConsumptionRate();
                
                float bestSupplySurplus = _config.alwaysConnectConsumers ? float.MinValue : 0;
                BuildingCell? bestCell = null;
                
                var nearbyOpenConnections = GetConnectionsInArea();
                foreach (var nearbyOpenConnection in nearbyOpenConnections)
                {
                    var (cells, supply, demand) = nearbyOpenConnection.Value;
                    var supplySurplus = supply - demand - demandWillAdd;
                    if (supplySurplus > bestSupplySurplus)
                    {
                        bestSupplySurplus = supplySurplus;
                        bestCell = cells[0];
                    }
                }

                if (bestCell != null)
                {
                    EstablishConsumer(bestCell.Value);
                    return true;
                }

                return false;
            }

            Dictionary<int, (List<BuildingCell> cells, float supply, float demand)> GetConnectionsInArea()
            {
                Dictionary<int, (List<BuildingCell> cells, float demand, float supply)> res = new();
                foreach (var cell in CellsInArea())
                {
                    //cell is not connected to any wire
                    if (!_wireTilemapGraph.TryGetConnection(cell, out var connection)) continue;
                    
                    //only one consumer allowed per cell
                    if(PowerGrid.HasConsumer(cell)) continue;

                    //only need to calculate supply/demand total once per connection
                    if (!res.TryGetValue(connection, out var tuple))
                    {
                        //possibly redundant conditional, but finds the total supply/demand (WE ASSUME THIS IS WITHOUT THIS CONSUMER INCLUDED) of the connection
                        if (!_wireTilemapGraph.TryGetTotalSupplyDemand(connection, out var supply, out var demand)) continue;
                        res.Add(connection, tuple = ( new List<BuildingCell>(), supply, demand));
                    }
                    
                    tuple.cells.Add(cell);
                }
                return res;
            }

            IEnumerable<BuildingCell> CellsInArea()
            {
                _wireToDeviceLookup.Clear();
                var area = _consumer.GridRect;
                for (var x = area.xMin; x < area.xMax; x++)
                {
                    for (var y = area.yMin; y < area.yMax; y++)
                    {
                        var c = new BuildingCell(new Vector2Int(x, y), BuildingLayers.WIRES);
                        foreach (var cell in c.GetCellsInRadius(_config.connectRadius))
                        {
                            if (_wireToDeviceLookup.ContainsKey(cell)) continue;
                            _wireToDeviceLookup.Add(cell, c);
                            yield return cell;
                        }
                    }
                }
            }

            public float GetConsumptionRate()
            {
                return _consumer.ConsumptionRateProperty.Value;
            }

            public void Consume(float amount)
            {
                var demand = GetConsumptionRate();
                var powered = demand <= amount;
                _consumer.Powered = powered;
            }

            public void Dispose()
            {
                _currentConnection?.Dispose();
            }
        }


        protected override void AddValue(IElectricityConsumer value)
        {
            base.AddValue(value);
            if (_wrappers.ContainsKey(value)) return;
            var wrapper = _factory.Create(value);
            _wrappers.Add(value, wrapper);
            onGraphChanged.OnNext(Unit.Default);
        }

        protected override void RemoveValue(IElectricityConsumer value)
        {
            base.RemoveValue(value);
            if (_wrappers.ContainsKey(value))
            {
                _wrappers[value].Dispose();
                _wrappers.Remove(value);
                onGraphChanged.OnNext(Unit.Default);
            }
        }

        public void Initialize()
        {

            
            _graph.OnGraphChanged.AsUnitObservable().Subscribe(onGraphChanged).AddTo(_cd);
            _powerGrid.OnSupplierAdded.AsUnitObservable().Subscribe(onGraphChanged).AddTo(_cd);
            _powerGrid.OnSupplierRemoved.AsUnitObservable().Subscribe(onGraphChanged).AddTo(_cd);
            // _powerGrid.OnConsumerAdded.AsUnitObservable().Subscribe(onGraphChanged).AddTo(_cd);
            // _powerGrid.OnConsumerRemoved.AsUnitObservable().Subscribe(onGraphChanged).AddTo(_cd);
            onGraphChanged.Subscribe(_ => UpdatePowerStates()).AddTo(_cd);
        }

        void UpdatePowerStates()
        {
            foreach (var kvp in _wrappers)
            {
                if (!kvp.Value.UpdateConnection())
                {
                    kvp.Key.Powered = false;
                }
            }
        }
        
        protected override void OnDispose()
        {
            _cd.Dispose();
            
        }
    }
    
    
    public interface IElectricityConsumer
    {
        RectInt GridRect { get; }
        
        bool Powered { set; }

        IReadOnlyReactiveProperty<float> ConsumptionRateProperty { get; }
    }



 
}