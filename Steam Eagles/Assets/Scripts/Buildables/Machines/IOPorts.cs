using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using Buildings.Tiles;
using CoreLib;
using QuikGraph;
using UniRx;

namespace Buildables
{
    public class IOPorts : Registry<IMachinePipeIOPort>
    {
        private readonly Building _building;

        
        private Dictionary<BuildingCell, IMachinePipeIOPort> _ipIOPorts;
        private UndirectedGraph<BuildingCell, SUndirectedEdge<BuildingCell>> _undirected = new();
        Subject<BuildingCell> _pipeAdded = new();
        Subject<BuildingCell> _pipeRemoved = new();


        public IEnumerable<BuildingCell> AllCells()
        {
            return _undirected.Vertices;   
        }

        public IEnumerable<BuildingCell> InputCells()
        {
            foreach (var machinePipeIOPort in _ipIOPorts)
            {
                if (machinePipeIOPort.Value.Mode() == IOPortMode.SINK) yield return machinePipeIOPort.Key;
            }
        }
        public IEnumerable<BuildingCell> OutputCells()
        {
            foreach (var machinePipeIOPort in _ipIOPorts)
            {
                if (machinePipeIOPort.Value.Mode() == IOPortMode.SOURCE) yield return machinePipeIOPort.Key;
            }
        }

        public IEnumerable<BuildingCell> Cells(IOPortMode mode)
        {
            switch (mode)
            {
                case IOPortMode.INACTIVE:
                    return _ipIOPorts.Keys.Where(t => _ipIOPorts[t].Mode() == IOPortMode.INACTIVE);
                    break;
                case IOPortMode.SOURCE:
                    return InputCells();
                    break;
                case IOPortMode.SINK:
                    return OutputCells();
                    break;
                case IOPortMode.CONNECTOR:
                    return AllCells().Where(t => !_ipIOPorts.ContainsKey(t));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        public void AddIMachinePipeIOPort(IMachinePipeIOPort port)
        {
            _ipIOPorts.Add(port.Cell, port);
            
        }
        void AddCellAndEdges(BuildingCell cell)
        {
            _undirected.AddVertex(cell);
            foreach (var neighbor in cell.GetNeighbors())
            {
                if (_undirected.ContainsVertex(neighbor))
                    _undirected.AddEdge(new SUndirectedEdge<BuildingCell>(cell, neighbor));
            }
        }
        public IOPorts(Building building)
        {
            _building = building;
            _pipeAdded.AddTo(cd);
            _pipeRemoved.AddTo(cd);
            _pipeAdded.Subscribe(AddCellAndEdges).AddTo(cd);
            _pipeRemoved.Subscribe(RemoveCellAndEdges).AddTo(cd);
            var map = building.Map;
            foreach (var room in building.Rooms.AllRooms)
            {
                var cells = room.GetBuildingCells(BuildingLayers.PIPE);
                foreach (var buildingCell in cells)
                {
                    var pipeTile = map.GetTile<PipeTile>(buildingCell);
                    if (pipeTile == null) continue;
                    _pipeAdded.OnNext(buildingCell);
                }
                _building.Map.OnTileChanged(BuildingLayers.PIPE, room)
                    .Select(t => (new BuildingCell(t.cell, BuildingLayers.PIPE), t.tile))
                    .Subscribe(t =>
                    {
                        if (t.tile == null) _pipeRemoved.OnNext(t.Item1);
                        else _pipeAdded.OnNext(t.Item1);
                    }).AddTo(cd);
            }
        }

        private void RemoveCellAndEdges(BuildingCell buildingCell)
        {
            
        }

        public void ClearAndReinitialize()
        {
            _undirected.Clear();
        }
    }
}