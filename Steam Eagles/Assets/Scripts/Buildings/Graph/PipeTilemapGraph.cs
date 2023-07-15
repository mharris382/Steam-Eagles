using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Buildings.Rooms;
using Cysharp.Threading.Tasks;
using QuikGraph;
using UniRx;
using UnityEngine;
using Zenject;

namespace Buildings.Graph
{
    public abstract class PowerTilemapGraph : BuildingTilemapGraph
    {
        private readonly BuildingPowerGrid _powerGrid;
        private readonly CoroutineCaller _caller;
        private readonly PowerConfig _config;
        Dictionary<int, (List<IPowerSupplier> suppliers , List<IPowerConsumer> consumers)> _components = new();
        protected PowerTilemapGraph(Building building,  BuildingPowerGrid  powerGrid, CoroutineCaller caller, PowerConfig config) : base(building)
        {
            _powerGrid = powerGrid;
            _caller = caller;
            _config = config;
        }

        protected override void OnInitialize()
        {
            _powerGrid.OnConsumerAdded.Subscribe(OnConsumerAdded).AddTo(_cd);
            _powerGrid.OnConsumerRemoved.Subscribe(OnConsumerRemoved).AddTo(_cd);
            _powerGrid.OnSupplierAdded.Subscribe(OnSupplierAdded).AddTo(_cd);
            _powerGrid.OnSupplierRemoved.Subscribe(OnSupplierRemoved).AddTo(_cd);
            var routine = _caller.StartCoroutine(PowerUpdate());
            Disposable.Create(() =>
            {
                if (_caller == null || routine == null) return;
                _caller.StopCoroutine(routine);
            }).AddTo(_cd);
        }

        IEnumerator PowerUpdate()
        {
            int operationCount = 0;
            while (_building != null)
            {
                UpdateConnectedGrids();
                
                foreach (var kvp in _components)
                {
                    if (operationCount >= _config.maxNodesToProcessPerUpdate)
                    {
                        yield return null;
                        operationCount=0;
                    }
                    var (suppliers, consumers) = kvp.Value;
                    operationCount += UpdateComponent(suppliers, consumers);
                }
                
                yield return new WaitForSeconds(_config.updateRate);
            }
        }

        private int UpdateComponent(List<IPowerSupplier> suppliers, List<IPowerConsumer> consumers)
        {
            
            if (suppliers.Count == 0 || consumers.Count == 0) return 0;
            
            int operations = 0;
            
            float supplyTotal = suppliers.Sum(t => t.GetSupplyRate());
            float demandTotal = consumers.Sum(t => t.GetConsumptionRate());
            if (supplyTotal < demandTotal) HandleSupplyDeficit(suppliers, consumers, supplyTotal, demandTotal);

            int currentConsumer = 0;

            int lastSupplier = suppliers.Count - 1;
            int lastConsumer = consumers.Count - 1;

            float currentConsumerDemand = consumers[currentConsumer].GetConsumptionRate();

            bool TryGetNextConsumer()
            {
                currentConsumer++;
                if (currentConsumer > lastConsumer) return false;
                currentConsumerDemand = consumers[currentConsumer].GetConsumptionRate();
                if(currentConsumerDemand == 0) return TryGetNextConsumer();
                operations++;
                return true;
            }

            for (int s = 0; s < lastSupplier; s++)
            {
                var supplier = suppliers[s];
                float amountAvailable = supplier.GetSupplyRate();
                if(amountAvailable == 0) continue;
                if (amountAvailable >= currentConsumerDemand)
                {
                    consumers[currentConsumer].Consume(supplier.Supply(currentConsumerDemand));
                    if (!TryGetNextConsumer())
                        break;
                }
                else
                {
                    consumers[currentConsumer].Consume(supplier.Supply(amountAvailable));
                    currentConsumerDemand -= amountAvailable;
                }
                operations++;
            }
            return operations;
        }

        protected abstract void HandleSupplyDeficit(List<IPowerSupplier> suppliers, List<IPowerConsumer> consumers, float supplyTotal, float demandTotal);

        private void UpdateConnectedGrids()
        {
            if (this.IsDirty)
            {
                this.UpdateConnectedComponents();
            }
            _components.Clear();
            foreach (var valueTuple in _powerGrid.GetSuppliers())
            {
                var cell = valueTuple.cell;
                int c = GetComponent(cell);
                if (c == -1) continue;
                if (!_components.TryGetValue(c, out var t))
                {
                    t = (new List<IPowerSupplier>(), new List<IPowerConsumer>());
                }
                t.suppliers.Add(valueTuple.supplier);
            }

            foreach (var valueTuple in _powerGrid.GetConsumers())
            {
                var cell = valueTuple.cell;
                int c = GetComponent(cell);
                if (c == -1) continue;
                //Don't continue for consumers, since we know this component does not have a supplier
                if (!_components.ContainsKey(c)) continue;
                _components[c].consumers.Add(valueTuple.consumer);
            }
        }

        protected virtual void OnConsumerAdded((BuildingCell cell, IPowerConsumer consumer) tuple)
        {
            
        }
        protected virtual void OnConsumerRemoved((BuildingCell cell, IPowerConsumer consumer) tuple)
        {
            
        }
        protected virtual void OnSupplierAdded((BuildingCell cell, IPowerSupplier consumer) tuple)
        {
            
        }
        protected virtual void OnSupplierRemoved((BuildingCell cell, IPowerSupplier consumer) tuple)
        {
            
        }
        public override void OnEdgeAdded(SUndirectedEdge<BuildingCell> edge)
        {
            IPowerSupplier powerSupplier;
            IPowerConsumer powerConsumer;
        }

        public override void OnEdgeRemoved(SUndirectedEdge<BuildingCell> edge)
        {
            
        }
    }
    public class PipeTilemapGraph : PowerTilemapGraph
    {
        // AdjacencyGraph<BuildingTile, SEdge<BuildingTile>> _graph = new();
        
        
        

        public override BuildingLayers Layers => BuildingLayers.PIPE;
        public override void OnTileAdded(BuildingTile tile)
        {
            Debug.Log($"Pipe Tile Added: {tile}");
            // _graph.AddVertex(tile);
            // foreach (var buildingCell in tile.root.GetNeighbors())
            // {
            //     
            // }
            // Debug.Assert(_graph.ContainsVertex(tile));
        }

        public override void OnTileRemoved(BuildingTile tile)
        {
            Debug.Log($"Pipe Tile Removed: {tile}");
        }

        protected override void HandleSupplyDeficit(List<IPowerSupplier> suppliers, List<IPowerConsumer> consumers, float supplyTotal, float demandTotal)
        {
            Debug.Log($"Pipe Supply Deficit: {supplyTotal} < {demandTotal}");
        }

        public override void OnEdgeAdded(SUndirectedEdge<BuildingCell> edge)
        {
            Debug.Log($"Pipe Tile Added: {edge.ToString()}");
        }

        public override void OnEdgeRemoved(SUndirectedEdge<BuildingCell> edge)
        {
            Debug.Log($"Pipe Tile Removed: {edge.ToString()}");
        }

        public PipeTilemapGraph(Building building, BuildingPowerGrid powerGrid, CoroutineCaller caller, PowerConfig config) : base(building, powerGrid, caller, config)
        {
        }
    }
}