using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using QuikGraph;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif
using UnityEngine;

namespace Damage
{
    [Serializable]
    public class StormHit
    {
        [SerializeField]
        public int maxDamage = 6;
        
        [System.Serializable]
        private class Direction
        {
            public Vector2Int direction = Vector2Int.right;
            public int count = 1;
        }
        
        
        [SerializeField, TableList(AlwaysExpanded = true)]
        private List<Direction> directions = new List<Direction>(new [] {
            new Direction()
            {
                direction = Vector2Int.right,
                count = 1
            },
            new Direction()
            {
                direction = Vector2Int.up,
                count = 1
            },
            new Direction()
            {
                direction = Vector2Int.left,
                count = 1
            },
            new Direction()
            {
                direction = Vector2Int.down,
                count = 1
            },
        });

        public (Vector2Int, int)[] GetDirections()
        {
            return directions.Select(t => (t.direction, t.count)).ToArray();
        }
        
        public GrabBag<Vector2Int> CreateGrabBag()
        {
            GrabBag<Vector2Int> grabBag = new GrabBag<Vector2Int>();
            grabBag.Init(GetDirections());
            return grabBag;
        }

        [Button]
        public void OpenTester()
        {
#if UNITY_EDITOR
            OdinEditorWindow.InspectObject(new StormHitTester(this));
#endif
        }
    }


    public class StormHitGraph
    {
        private readonly StormHit _hit;
        private readonly AdjacencyGraph<Vector2Int, Edge<Vector2Int>> _graph;
        
        private int _hitCount;
        private Vector2Int _activePosition;

        private GrabBag<Vector2Int> _directionGrabBag;
        public StormHitGraph(StormHit hit, 
            Vector2Int seed,
            AdjacencyGraph<Vector2Int, Edge<Vector2Int>> graph)
        {
            _hit = hit;
            _activePosition = seed;
            _graph = graph;
            _directionGrabBag = new GrabBag<Vector2Int>();
            _directionGrabBag.Init(_hit.GetDirections());
        }

        public IEnumerable<Vector2Int> GetHits()
        {
            Stack<Vector2Int> previousPositions = new Stack<Vector2Int>();
            Predicate<Vector2Int> isDirectionUnBlocked = t =>
            {
                if (previousPositions.Contains(_activePosition + t))
                    return false;
                return _graph.ContainsEdge(_activePosition, _activePosition + t);
            };
            using ( var grabBagIterator = _directionGrabBag.GetItems(isDirectionUnBlocked).GetEnumerator())
            {
                while (previousPositions.Count < _hit.maxDamage && grabBagIterator.MoveNext())
                {
                    Debug.Assert(!previousPositions.Contains(_activePosition));
                    yield return _activePosition;
                    previousPositions.Push(_activePosition);
                    _activePosition += grabBagIterator.Current;
                }
            }
        }
    }





    public class StormHitGrid
    {
        private readonly Vector2Int size;
        private readonly Cell[,] cells;
        private readonly HashSet<Vector2Int> blockedCells;
        

        public int Width => size.x;
        public int Height => size.y;


        public Cell this[Vector2Int v]
        {
            get
            {
                if (IsBlocked(v))
                {
                    return new Cell(v.x, v.y, true);
                }
                return cells[v.x, v.y];
            }
            set
            {
                if(IsBlocked(v))
                    return;
                cells[v.x, v.y] = value;
            }
        }

        public StormHitGrid(int width, int height, IEnumerable<Vector2Int> blockedCells)
        {
            size = new Vector2Int(width, height);
            this.blockedCells = new HashSet<Vector2Int>(blockedCells);
            cells = new Cell[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    cells[i, j] = new Cell(i, j, IsBlocked(i, j));
                    if (!IsBlocked(i + 1, j))
                    {
                        cells[i,j].cardinalDirections |= CardinalDirections.RIGHT;
                    }

                    if (!IsBlocked(i - 1, j))
                    {
                        cells[i, j].cardinalDirections |= CardinalDirections.LEFT;
                    }
                    
                    if(!IsBlocked(i, j + 1))
                    {
                        cells[i, j].cardinalDirections |= CardinalDirections.UP;
                    }

                    if (!IsBlocked(i, j - 1))
                    {
                        cells[i, j].cardinalDirections |= CardinalDirections.DOWN;
                    }
                }
            }
        }


        public struct Cell
        {
            public Vector2Int position;
            public bool isBlocked;
            public int value;
            public CardinalDirections cardinalDirections;

            public Cell(int x, int y, bool isBlocked ) : this()
            {
                this.position = new Vector2Int(x,y);
                this.isBlocked = isBlocked;
                value = 0;
            }

            public bool HasDirection(Vector2Int direction)
            {
                if(direction.x > 0)
                    return (cardinalDirections & CardinalDirections.RIGHT) != 0;
                else if(direction.x < 0)
                    return (cardinalDirections & CardinalDirections.LEFT) != 0;
                else if(direction.y > 0)
                    return (cardinalDirections & CardinalDirections.UP) != 0;
                else if (direction.y < 0)
                    return (cardinalDirections & CardinalDirections.DOWN) != 0;
                else
                    throw new System.Exception("Invalid direction");
                
            }
            

            public IEnumerable<Vector2Int> GetUnblockedNeighborPositions()
            {
                if((cardinalDirections & CardinalDirections.UP) != 0)
                    yield return position + Vector2Int.up;
                if((cardinalDirections & CardinalDirections.DOWN) != 0)
                    yield return position + Vector2Int.down;
                if((cardinalDirections & CardinalDirections.LEFT) != 0)
                    yield return position + Vector2Int.left;
                if((cardinalDirections & CardinalDirections.RIGHT) != 0)
                    yield return position + Vector2Int.right;
            }
        }

        public bool IsBlocked(int x, int y) => IsBlocked(new Vector2Int(x, y));
        public bool IsBlocked(Vector2Int position)
        {
            if(position.x < 0 || position.x >= size.x)
                return true;
            if(position.y < 0 || position.y >= size.y)
                return true;
            return blockedCells.Contains(position);
        }
    }

    public class StormHitGraph2
    {
        private readonly StormHitGrid _stormHitGrid;
        private readonly Vector2Int _seedPosition;
        private readonly StormHit _hit;
        private GrabBag<Vector2Int> _directionGrabBag;
        public int MaxDistance => _hit.maxDamage;

        private bool finished = false;
        private Vector2Int _activePosition;
        private AdjacencyGraph<Vector2Int, TaggedEdge<Vector2Int, int>> _graph;
        public StormHitGraph2(StormHit hit, Vector2Int seedPosition, StormHitGrid stormHitGrid)
        {
            _hit = hit;
            _seedPosition = seedPosition;
            _stormHitGrid = stormHitGrid;
            _directionGrabBag = new GrabBag<Vector2Int>();
            _directionGrabBag.Init(_hit.GetDirections());
            
            var cell = _stormHitGrid[_seedPosition];
            if (cell.isBlocked) finished = true;

            var stack = new Stack<TaggedEdge<Vector2Int, int>>();
            _graph = new AdjacencyGraph<Vector2Int, TaggedEdge<Vector2Int, int>>();
            _graph.AddVertex(_seedPosition);
            foreach (var unblockedNeighborPosition in cell.GetUnblockedNeighborPositions())
            {
                _graph.AddVertex(unblockedNeighborPosition);
                var newEdge = new TaggedEdge<Vector2Int, int>(_seedPosition, unblockedNeighborPosition, 1);
                stack.Push(newEdge);
                _graph.AddEdge(newEdge);
            }
            //
            // while (stack.Count > 0)
            // {
            //     var edge = stack.Pop();
            //     var targetCell = _stormHitGrid[edge.Source];
            //     targetCell.value = edge.Tag + 1;
            //     
            //     if (targetCell.value >= _hit.maxDamage)
            //         continue;
            //     foreach (var unblockedNeighborPosition in targetCell.GetUnblockedNeighborPositions())
            //     {
            //         //check if neighbor cell is already visited
            //         var neighborCell = _stormHitGrid[unblockedNeighborPosition];
            //         
            //         // check if neighbor cell has been visited by another path, and check if this path is longer
            //         if(neighborCell.value >= 0 || neighborCell.value < edge.Tag + 1)//already visited
            //             continue;
            //
            //         //if not visited, add to graph
            //         var newEdge = new TaggedEdge<Vector2Int, int>(edge.Source, unblockedNeighborPosition, edge.Tag + 1);
            //         _graph.AddVerticesAndEdge(newEdge);
            //         stack.Push(newEdge);
            //         
            //         //update neighbor cell value
            //         neighborCell.value = newEdge.Tag;
            //         _stormHitGrid[unblockedNeighborPosition] = neighborCell;
            //     }
            // }
            //
        }
        
    }
}