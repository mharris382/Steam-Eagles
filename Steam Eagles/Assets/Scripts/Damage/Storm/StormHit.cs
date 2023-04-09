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
}