using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using Buildings.Rooms;
using QuikGraph;
using QuikGraph.Algorithms;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.VFX;
using Zenject;
using QuikGraph.Algorithms.Search;

namespace Buildings.Graph
{
    public class WireEffect : MonoBehaviour
    {
        [Required]
        public VisualEffect effect;

        
        Room _room ;
        BoundsLookup _boundsLookup ;
        WireTilemapGraph _wireTilemapGraph;

        private BuildingCell[] _spawnPoints = new BuildingCell[0];
        List<BuildingCell> _cells = new List<BuildingCell>(100);
        
        private List<List<BuildingCell>> _connectedSpawnPoints = new List<List<BuildingCell>>();
        
        private List<BuildingCell> cells => _cells ??= new List<BuildingCell>(100);
        private Building Building => _room.Building;
        [Inject] void Install(
            Room room, BoundsLookup boundsLookup, 
            WireTilemapGraph wireTilemapGraph)
        {
            _room = room;
            _boundsLookup = boundsLookup;
            _wireTilemapGraph = wireTilemapGraph;
        }


        private void Start()
        {
            
        }


        

        [Button, HideInEditorMode]
        void CreateEffect()
        {
           cells.Clear();
           cells.AddRange(_wireTilemapGraph.GetSourceCells().Where(t => Building.Map.GetRoom(t) == _room));
           if (cells.Count == 0) return;
           Dictionary<BuildingCell, SUndirectedEdge<BuildingCell>> predecessors = new();

           foreach (var buildingCell in cells)
           {
               DepthFirstSearchAlgorithm<BuildingCell, SUndirectedEdge<BuildingCell>> dfs
                   = new DepthFirstSearchAlgorithm<BuildingCell, SUndirectedEdge<BuildingCell>>(_wireTilemapGraph.UndirectedGraph.ToBidirectionalGraph().ToArrayBidirectionalGraph());
               StringBuilder sb = new StringBuilder();
               sb.Append($"Starting From {buildingCell}");
               dfs.TreeEdge += (edge) =>
               {
                   sb.Append(',');
                   sb.Append(edge.Source);
               };
           }
        }


        void FindPathFromPoint(BuildingCell cell, AdjacencyGraph<BuildingCell, SEdge<BuildingCell>> graph)
        {
            graph ??= new();
            if (graph.ContainsVertex(cell)) return;
            graph.AddVertex(cell);
            // BuildingCell[] neighbors = cell.GetNeighbors().Where(t => _wireTilemapGraph. !graph.ContainsVertex(cell)).ToArray();
            // if(neighbors.Length == 0)return;
            // foreach (var neighbor in cell.GetNeighbors().Where(t =>!graph.ContainsVertex(cell)))
            // {
            //     if(graph.ContainsVertex(neighbor))continue;
            // }
        }

    }
}