using System;
using System.Collections.Generic;
using System.Linq;
using Buildings.Rooms;
using CoreLib.Extensions;
using QuikGraph;
using QuikGraph.Algorithms;
using UniRx;
using UnityEngine;
using Zenject;

namespace Buildings.Graph
{
    public abstract class BuildingTilemapGraph : IInitializable, IDisposable
    {
        
        
        public abstract BuildingLayers Layers { get; }
        
        private UndirectedGraph<BuildingCell, SUndirectedEdge<BuildingCell>> _undirectedGraph = new();
        
        private Dictionary<BuildingCell, int> _connectedComponents = new();
        private Dictionary<int, List<BuildingCell>> _components = new();
        
        private UnityEngine.Pool.ListPool<BuildingCell> _listPool = new();

        public UndirectedGraph<BuildingCell, SUndirectedEdge<BuildingCell>> UndirectedGraph => _undirectedGraph;
        private int _connectedComponentsCount;

        public int NodeCount => _undirectedGraph.VertexCount;
        public int EdgeCount => _undirectedGraph.EdgeCount;

        private bool _dirty;
        private HashSet<BuildingCell> _dirtyCells = new();
        private BuildingMap _map;
        protected readonly Building _building;
        private readonly Room[] _rooms;
        protected readonly CompositeDisposable _cd = new();

        private Subject<BuildingTile> _tileChanged = new();
        
        public IObservable<BuildingTile> OnGraphChanged => _tileChanged;

        public bool IsComponentDirty(int component)
        {
            if(component > _connectedComponentsCount)
                throw new ArgumentOutOfRangeException(nameof(component));
            if (!_dirty) return false;
            if(_components.TryGetValue(component, out var list))
                return list.Any(t => _dirtyCells.Contains(t));
            return false;
        }
        public bool IsDirty => _dirty;
        public bool IsDisposed => _cd.IsDisposed;

        public BuildingTilemapGraph(Building building)
        {
            _map = building.Map;
            _building = building;
            _rooms = building.Rooms.AllRooms.ToArray();
        }

        private void AddNode(BuildingTile tile)
        {
            if(_undirectedGraph.ContainsVertex(tile.cell))
            {
                OnTileChanged(tile);
                return;
            }
            
            SetDirty(tile.cell);
            
            //add node
            _undirectedGraph.AddVertex(tile.cell);
            OnTileAdded(tile);
            
            //create edges
            foreach (var buildingCell in GetNeighborsOnGraph(tile.cell))
            {
                var edge = new SUndirectedEdge<BuildingCell>(tile.cell, buildingCell);
                _undirectedGraph.AddEdge(edge);
                OnEdgeAdded(edge);
            }
        }
        private void RemoveNode(BuildingTile tile)
        {
            if (!_undirectedGraph.ContainsVertex(tile.cell))
            {
                OnTileChanged(tile);
                return;
            }
            
            SetDirty(tile.cell);
            
            //remove edges
            foreach (var buildingCell in GetNeighborsOnGraph(tile.cell))
            {
                if (_undirectedGraph.TryGetEdge(buildingCell, tile.cell, out var edge))
                {
                    _undirectedGraph.RemoveEdge(edge);
                    OnEdgeRemoved(edge);
                }
            }
            
            //remove node
            _undirectedGraph.RemoveVertex(tile.cell);
            OnTileRemoved(tile);
        }

        private void SetDirty(BuildingCell buildingCell)
        {
            _dirty = true;
            foreach (var cell in GetDirtyNodesOnGraph(buildingCell)) _dirtyCells.SafeAdd(cell);
        }
       protected IEnumerable<BuildingCell> GetDirtyNodesOnGraph(BuildingCell cell) => GetNeighborsOnGraph(cell).Append(cell);
       protected IEnumerable<BuildingCell> GetNeighborsOnGraph(BuildingCell cell) => cell.GetNeighbors().Where(t => _undirectedGraph.ContainsVertex(t));

       public bool HasTile(BuildingCell cell) => _undirectedGraph.ContainsVertex(cell);
       /// <summary>
       /// called when a tile changes but the graph is not affected because there was already vertex for this cell
       /// </summary>
       /// <param name="tile"></param>
       protected virtual void OnTileChanged(BuildingTile tile) { }

       public abstract void OnTileAdded(BuildingTile tile);
        public abstract void OnTileRemoved(BuildingTile tile);
        
        public virtual void OnEdgeAdded(SUndirectedEdge<BuildingCell> edge) { }
        public virtual void OnEdgeRemoved(SUndirectedEdge<BuildingCell> edge) { }

        public void Dispose()
        {
            _cd.Dispose();
            _tileChanged.Dispose();
            OnDispose();
        }

        public void Initialize()
        {
            Subject<BuildingTile> onTileChanged = new();
            
            var onTileRemoved = onTileChanged.Where(t => t.Layer == this.Layers && t.IsEmpty);
            var onTileAdded = onTileChanged.Where(t => t.Layer == this.Layers && !t.IsEmpty);
            
            onTileAdded.Subscribe(t =>
            {
                AddNode(t);
                _tileChanged.OnNext(t);
            }).AddTo(_cd);
            onTileRemoved.Subscribe(t =>
            {
                RemoveNode(t);
                _tileChanged.OnNext(t);
            }).AddTo(_cd);
            
            _map.OnTileSetStream.Where(t => t.cell.layers == Layers).Subscribe(onTileChanged).AddTo(_cd);
            foreach (var room in _rooms)
            {
                var bounds = _map.GetCellsForRoom(room, Layers);
                for (int x = bounds.xMin; x < bounds.xMax; x++)
                {
                    for (int y = bounds.yMin; y < bounds.yMax; y++)
                    {
                        var cell = new BuildingCell(new Vector2Int(x,y), Layers);
                        var tile = _map.GetTile(cell);
                        if (tile!= null)
                        {
                            onTileChanged.OnNext(new BuildingTile(cell, tile));
                        }
                    }
                }
            }
            OnInitialize();
        }

        protected virtual void OnInitialize()
        {
        }

        protected virtual void OnDispose()
        {
            
        }

        public int GetComponent(BuildingCell cell)
        {
            if (!_undirectedGraph.ContainsVertex(cell))
                return -1;
            if(_dirtyCells.Contains(cell))UpdateConnectedComponents();
            return _connectedComponents[cell];
        }
        public void UpdateConnectedComponents()
        {
            if(!_dirty) return;
            _connectedComponentsCount = _undirectedGraph.ConnectedComponents(_connectedComponents);
            _dirty = false;
            _dirtyCells.Clear();
        }
    }



    public abstract class PowerGraph : BuildingTilemapGraph
    {
        protected PowerGraph(Building building) : base(building)
        {
        }
    }
}