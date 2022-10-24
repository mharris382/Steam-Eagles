using System;
using System.Collections;
using System.Collections.Generic;
using GasSim.SimCore.DataStructures;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;

namespace Puzzles
{
    public class PipeTilemapBridge : MonoBehaviour
    {
        public SharedTilemap tilemap;
        public CellHelper[] startNodes;
        public CellHelper[] endNodes;

        private List<List<Vector3Int>> _pathsFound = new List<List<Vector3Int>>();
        private Dictionary<Vector3Int, Graph<Vector3Int>> _graphsFound = new Dictionary<Vector3Int, Graph<Vector3Int>>();
        IEnumerator Start()
        {
            while (!tilemap.HasValue)
            {
                yield return null;
            }

            Tilemap tm = tilemap.Value;
            
            foreach (var ce in startNodes)
            {
                _graphsFound.Add(ce.CellCoordinate, CellHelper.BuildFrom(ce.CellCoordinate, tilemap.Value, out var path));
                _pathsFound.Add(path);
                Debug.Log($"Found path from {ce} of Length: {path.Count}");
            }
            
            // int numTiles = tm.GetUsedTilesCount();
            // var bounds = tm.cellBounds;
            // Vector3Int[] usedTiles = new Vector3Int[bounds.x * bounds.y];
            // TileBase[] tiles = new TileBase[bounds.x * bounds.y];
            // var count = tm.GetTilesRangeNonAlloc(bounds.min, bounds.max, usedTiles, tiles);
            // Debug.Log($"Found {count} tiles on map {tilemap}");
            //
            // int cnt = 0;
            // Dictionary<Vector3Int, int> _connectedLookup = new Dictionary<Vector3Int, int>();
            //
            // Queue<Vector3Int> _toSearch = new Queue<Vector3Int>();
            // foreach (var vector3Int in usedTiles)
            // {
            //     var tile = tm.GetTile(vector3Int);
            //     _toSearch.Enqueue(vector3Int);
            //     _connectedLookup.Add(vector3Int, -1);
            //     if (tile != null) cnt++;
            // }
            //
            // HashSet<Vector3Int> traversed = new HashSet<Vector3Int>();
            //
            // Dictionary<Vector3Int, int> _startTrees = new Dictionary<Vector3Int, int>();
            // List<List<Vector3Int>> _trees = new List<List<Vector3Int>>();
            //
            // List<Vector3Int> GetTree(Vector3Int cell)
            // {
            //     if (_startTrees.ContainsKey(cell))
            //     {
            //         return _trees[_startTrees[cell]];
            //     }
            //     return null;
            // }
            //
            // foreach (var cellHelper in startNodes)
            // {
            //     _startTrees.Add(cellHelper.CellCoordinate, _trees.Count);
            //    _trees.Add(new List<Vector3Int>());
            // }
            //
            // void TryAttach(Vector3Int cell, CellHelper node)
            // {
            //     if (_connectedLookup.ContainsKey(cell))
            //     {
            //         //TODO: Traverse Pipes Recursively, until either a endNode is reached or a pipe segment has no more neighbors.  Must not traverse a cell more than once
            //     }
            // }
            // Debug.Log($"Found {cnt} tiles on map {tilemap}");
            //
            
        }

        private void OnDrawGizmos()
        {
            if (tilemap.HasValue == false) return;
            if (!Application.isPlaying) return;
            
            Gizmos.color = Color.blue;
            foreach (var ce in startNodes)
            {
                Gizmos.DrawSphere(ce.CellCenter, 0.25f);
            }
            
            if (_pathsFound == null || _pathsFound.Count == 0) return;
            Gizmos.color = Color.yellow;
            
            foreach (var path in _pathsFound)
            {
                if (path == null || path.Count <= 1) continue;
                for (int i = 1; i < path.Count; i++)
                {
                    var p0 = tilemap.Value.GetCellCenterWorld(path[i - 1]);
                    var p1 =tilemap.Value.GetCellCenterWorld( path[i]);
                    Gizmos.DrawLine(p0, p1);
                }
            }
            if (_graphsFound == null) return;
        }
    }
}