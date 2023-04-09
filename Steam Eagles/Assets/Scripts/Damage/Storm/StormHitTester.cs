#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using QuikGraph;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Damage
{
    public class StormHitTester
    {
        private const int UNDAMAGED = -1;
        private const int BLOCKED = -2;
        private const int DAMAGE_SEED = 0;
        [ShowInInspector] [OnValueChanged(nameof(UpdateGridSize))]
        public int testGridSize = 10;
        
        [HorizontalGroup("h1")] [ShowInInspector] [OnValueChanged(nameof(UpdateBlockedGrid))] [TableMatrix(DrawElementMethod = nameof(DrawColoredEnumElement), ResizableColumns = false, RowHeight = 16, SquareCells = true)]
        public bool[,] blockGrid = new bool[10, 10];
        
        [HorizontalGroup("h1")] [ShowInInspector] [TableMatrix(DrawElementMethod = nameof(DrawColoredOutputResult), ResizableColumns = false, RowHeight = 16, SquareCells = true)]
        public int[,] testGrid = new int[10, 10];

        [ShowInInspector] private AdjacencyGraph<Vector2Int, Edge<Vector2Int>> graph;

        private readonly StormHit _hit;
        
        [HorizontalGroup("h2")][ShowInInspector] [TableMatrix(DrawElementMethod = nameof(DrawGraphVisual))]
        public Vector2Int[,] graphVisual = new Vector2Int[10, 10];
        
        [HorizontalGroup("h2")][ShowInInspector, TableMatrix(DrawElementMethod = nameof(DrawResultGrid))]
        public int[,] resultGrid = new int[10, 10];


        private static Color[] colorLookup = new Color[]
        {
            new Color(0, 0, 0, .5f),
            new Color(0.1f, 0.8f, 0.5f),
            new Color(0.8f, 0.1f, 0.1f),
            new Color(0.8f, 0.8f, 0.1f).Darken(.8f),
        };

        [ShowInInspector]
        public Vector2Int[] Verts => graph == null ? null : graph.Vertices.ToArray();

        static Vector2Int[] directions = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left, };
        
        private const string leftArrow = "←";

        private const string rightArrow = "→";

        private const string upArrow = "↑";

        private const string downArrow = "↓";


        void UpdateBlockedGrid()
        {
            for (int i = 0; i < testGridSize; i++)
            {
                for (int j = 0; j < testGridSize; j++)
                {
                    if (blockGrid[i, j])
                    {
                        testGrid[i, j] = -2;
                    }
                }
            }
        }

        private void ResetGraphVisual()
        {
            graphVisual = new Vector2Int[testGridSize, testGridSize];
            for (int i = 0; i < testGridSize; i++)
            {
                for (int j = 0; j < testGridSize; j++)
                {
                    graphVisual[i, j] = new Vector2Int(i, j);
                }
            }
        }

        #region [Grid Drawing]

        static bool DrawColoredEnumElement(Rect rect, bool value)
        {
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                value = !value;
                GUI.changed = true;
                Event.current.Use();
            }
            EditorGUI.DrawRect(rect.Padding(1), value ? new Color(0.1f, 0.8f, 0.2f): new Color(0,  0, 0, .5f));
            return value;
        }

        Vector2Int DrawGraphVisual(Rect rect, Vector2Int value)
        {
            
            if (graph == null) {
                EditorGUI.DrawRect(rect.Padding(1), new Color(0,  0, 0, .5f));
                CreateGraph();
                return value;
            }

            if (!graph.ContainsVertex(value))
            {
                EditorGUI.DrawRect(rect.Padding(1), new Color(.7f,  .7f, .7f, .7f));
                return value;
            }
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                CreateGraph();
                DoHit(value);
                //value = !value;
                GUI.changed = true;
                Event.current.Use();
            }

            var leftRect = rect.AlignLeft(rect.width / 2);
            var rightRect = rect.AlignRight(rect.width / 2);
            var downRect = rect.AlignBottom(rect.height / 2);
            var upRect = rect.AlignTop(rect.height / 2);
            var left = value + Vector2Int.left;
            var right = value + Vector2Int.right;
            var up = value + Vector2Int.up;
            var down = value + Vector2Int.down;
            if (graph.ContainsVertex(down) && graph.ContainsEdge(value, down))
            {
                EditorGUI.LabelField(downRect, downArrow, new GUIStyle()
                {
                    alignment = TextAnchor.LowerCenter,
                });
            }
            if(graph.ContainsVertex(up) && graph.ContainsEdge(value, up))
            {
                EditorGUI.LabelField(upRect, upArrow, new GUIStyle()
                {
                    alignment = TextAnchor.UpperCenter
                });
            }
            if(graph.ContainsVertex(left) && graph.ContainsEdge(value, left))
            {
                EditorGUI.LabelField(leftRect, leftArrow, new GUIStyle()
                {
                    alignment = TextAnchor.MiddleLeft
                });
            }
            if(graph.ContainsVertex(right) && graph.ContainsEdge(value, right))
            {
                EditorGUI.LabelField(rightRect, rightArrow, new GUIStyle()
                {
                    alignment = TextAnchor.MiddleRight,
                    clipping = TextClipping.Overflow
                });
            }
            return value;
        }

        static Color BlockedColor = new Color(0,  0, 0, .5f);
        static Color  SeedColor = new Color(0.8f, 0.1f, 0.1f);
        private static Color DamageEndColor = new Color(0.6f, 0.1f, 0.1f, .5f);
        static Color UndamagedColor = new Color(.6f,  .6f, 0.2f, .7f);
        int DrawResultGrid(Rect rect, int value)
        {
            int maxDamage = _hit.maxDamage;
            string label = "";
            Color color = Color.black;
            switch (value)
            {
                case BLOCKED:
                    color = BlockedColor;
                    label = "▢";
                    break;
                case UNDAMAGED:
                    color = UndamagedColor;
                    break;
                case DAMAGE_SEED:
                    label = "╳";
                    color = Color.red;
                    break;
                default:
                    float t = value / (float)maxDamage;
                    t = Mathf.Clamp01(t);
                    color = Color.Lerp(SeedColor, DamageEndColor, t);
                    label = value.ToString();
                    break;
            }
            EditorGUI.DrawRect(rect.Padding(1), color);
            EditorGUI.LabelField(rect.Padding(3),label);
            return value;
        }

        int DrawColoredOutputResult(Rect rect, int value)
        {
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                if (value == 0)
                {
                    value = -1;
                }
                else if (value != -2)
                {
                    value = 0;
                    RefreshGrid();
                }
                //value = !value;
                GUI.changed = true;
                Event.current.Use();
            }

            Color boxColor;
            string label = "";
            switch (value)
            {
                case -2://blocked
                    boxColor = colorLookup[1];
                    label = "[]";
                    break;
                case -1://undamaged
                    boxColor = colorLookup[0];
                    label = " ";
                    break;
                case 0://damage seed
                    boxColor = colorLookup[2];
                    label = "X";
                    break;
                default://damaged
                    boxColor = colorLookup[3];
                    label = value.ToString();
                    break;
            }

        
            
            EditorGUI.DrawRect(rect.Padding(1), boxColor);
            EditorGUI.LabelField(rect.Padding(3),label);
            return value;
        }

        #endregion

 

        #region [Helpers]

        int GetValue(Vector2Int vector2Int)
        {
            try
            {
                return testGrid[vector2Int.x, vector2Int.y];
            }
            catch (IndexOutOfRangeException e)
            {
                Debug.LogError($"GetValue() -> Index out of range: {vector2Int.ToString()}");
                throw;
            }
        }

        bool IsSeed(Vector2Int vector2Int)
        {
            try
            {
                return testGrid[vector2Int.x, vector2Int.y] == 0;
            }
            catch (IndexOutOfRangeException e)
            {
                Debug.LogError($"IsSeed() -> Index out of range: {vector2Int.ToString()}");
                return false;
            }
        }

        bool IsBlocked(Vector2Int vector2Int)
        {
            if(vector2Int.x >= testGridSize || vector2Int.y >= testGridSize || vector2Int.x < 0 || vector2Int.y < 0)
                return true;
            try
            {
                return blockGrid[vector2Int.x, vector2Int.y];
            }
            catch (IndexOutOfRangeException e)
            {
                Debug.LogError($"IsBlocked() -> Index out of range: {vector2Int.ToString()}");
                return true;
            }
        }

        void UpdateValue(Vector2Int vector2Int, int value)
        {
            try
            {
                testGrid[vector2Int.x, vector2Int.y] = value;
            }
            catch (IndexOutOfRangeException e)
            {
                Debug.LogError($"UpdateValue() -> Index out of range: {vector2Int.ToString()}");
                return;
            }
        }


        IEnumerable<Vector2Int> GetNeighbors(Vector2Int cell, AdjacencyGraph<Vector2Int, Edge<Vector2Int>> graph)
        {
            foreach (var direction in directions)
            {
                var neighbor = cell + direction;
                if(neighbor.x >= testGridSize || neighbor.y >= testGridSize || neighbor.x < 0 || neighbor.y < 0)
                    continue;
                if (graph.ContainsVertex(neighbor))
                {
                    yield return neighbor;
                }
            }
        }

        IEnumerable<(Vector2Int, int)> GetNeighborValues(Vector2Int cell, AdjacencyGraph<Vector2Int, Edge<Vector2Int>> graph)
        {
            foreach (var neighbor in GetNeighbors(cell, graph))
            {
                yield return (neighbor, testGrid[neighbor.x, neighbor.y]);
            }
        }

        #endregion

        #region [Buttons]

        [PropertyOrder(-10), ButtonGroup()] [Button]
        void ResetTestGrid()
        {
            graph  = new AdjacencyGraph<Vector2Int, Edge<Vector2Int>>();
            ResetGraphVisual();
            for (int i = 0; i < testGridSize; i++)
            {
                for (int j = 0; j < testGridSize; j++)
                {
                    if (blockGrid[i, j])
                    {
                        testGrid[i, j] = -2;
                    }
                    else
                    {
                        testGrid[i, j] = -1;
                    }
                }
            }
        }

        [PropertyOrder(-10), ButtonGroup()] [Button]
        private void RefreshGrid()
        {
            ResetGraphVisual();
            List<Vector2Int> seedPoints = new List<Vector2Int>();
            AdjacencyGraph<Vector2Int, Edge<Vector2Int>> graph = new AdjacencyGraph<Vector2Int, Edge<Vector2Int>>();
            var directions = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left, };
            for (int i = 0; i < testGridSize; i++)
            {
                for (int j = 0; j < testGridSize; j++)
                {
                    var v = new Vector2Int(i, j);
                    if (IsBlocked(v)) continue;
                    graph.AddVertex(v);
                    if (IsSeed(v))//if seed create edges from seed and set seed neighbors to 1
                    {
                        seedPoints.Add(v);
                        foreach (var direction in directions)
                        {
                            var neighbor = v + direction;
                            if (neighbor.x < 0 || neighbor.y < 0 || neighbor.x >= testGridSize ||
                                neighbor.y >= testGridSize) continue;
                            if (IsBlocked(neighbor)) continue;
                            if (graph.ContainsVertex(neighbor))
                            {
                                graph.AddEdge(new Edge<Vector2Int>(v, neighbor));
                                UpdateValue(neighbor, 1);
                            }
                        }
                    }
                    else //otherwise check neighbors for seed and set v value to 1 if seed neighbor is found
                    {
                        bool foundSeed = false;
                        foreach (var direction in directions)
                        {
                            var neighbor = v + direction;
                            if(IsBlocked(neighbor))continue;
                            if (IsSeed(neighbor))
                            {
                                UpdateValue(v, 1);
                                foundSeed = true;
                            }
                        }

                        if (!foundSeed)
                        {
                            UpdateValue(v, UNDAMAGED);
                        }
                    }
                }
            }

            //foreach (var seed in seedPoints)
            //{
            //    TestDamage(seed, graph);
            //}
            this.graph = graph;
            return;
            for (int i = 0; i < testGridSize; i++)
            {
                for (int j = 0; j < testGridSize; j++)
                {
                    var v = new Vector2Int(i, j);
                    if (graph.ContainsVertex(v) == false) continue;
                    if (IsSeed(v)) continue;
                    foreach (var direction in directions)
                    {
                        var neighbor = v + direction;
                        if (IsBlocked(neighbor)) continue;
                        if (graph.ContainsVertex(neighbor))
                        {
                            if (IsSeed(neighbor))
                            {
                                UpdateValue(v, 1);
                            }
                        }
                    }
                }
            }

            int removed = graph.RemoveVertexIf(t => blockGrid[t.x, t.y] == false);
            Debug.Log($"Removed {removed} vertices");
            
            
            
            foreach (var graphVertex in graph.Vertices)
            {
                var vertexValue = testGrid[graphVertex.x, graphVertex.y];
                foreach (var direction in directions)
                {
                    var neighbor = graphVertex + direction;
                    if (graph.ContainsVertex(neighbor))
                    {
                        
                        var neighborValue = testGrid[neighbor.x, neighbor.y];
                        if(neighborValue == -2 )continue;
                        if (neighborValue == -1)
                        {
                            graph.AddEdge(new Edge<Vector2Int>(graphVertex, neighbor));
                            //testGrid[neighbor.x, neighbor.y] = vertexValue + 1;
                        }
                    }
                }  
            }

            
            foreach (var seedPoint in seedPoints)
            {
                //TestHit(graph, seedPoint);
            }
        }


        public void CreateGraphFromSeed(Vector2Int seed, int maxDistance)
        {
            
            if (IsBlocked(seed))
            {
                return;
            }
            
            AdjacencyGraph<Vector2Int, Edge<Vector2Int>> graph = new AdjacencyGraph<Vector2Int, Edge<Vector2Int>>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            Stack< (Vector2Int pos, int distance)> unvisited = new Stack<(Vector2Int, int)>();
            
            int currentDistance = 0;
            
            unvisited.Push((seed, currentDistance));
            graph.AddVertex(seed);
            
            while (unvisited.Count > 0)
            {
                var current = unvisited.Pop();
                
                if (visited.Contains(current.pos)) continue;
                visited.Add(current.pos);
                
                currentDistance = current.distance;
                if (currentDistance >= maxDistance) continue;
                
                foreach (var neighbor in GetNeighbors(current.pos, graph))
                {
                    if (visited.Contains(neighbor)) continue;
                    if (IsBlocked(neighbor)) continue;
                    
                    graph.AddVerticesAndEdge(new Edge<Vector2Int>(current.pos, neighbor));
                    unvisited.Push((neighbor, currentDistance + 1));
                }
            }
        }
        
        [PropertyOrder(-10), ButtonGroup()] [Button]
        private void CreateGraph()
        {
            List<Vector2Int> seedPoints = new List<Vector2Int>();
            AdjacencyGraph<Vector2Int, Edge<Vector2Int>> graph = new AdjacencyGraph<Vector2Int, Edge<Vector2Int>>();
            var directions = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left, };
            for (int i = 0; i < testGridSize; i++)
            {
                for (int j = 0; j < testGridSize; j++)
                {
                    var v = new Vector2Int(i, j);
                    if (IsBlocked(v)) continue;
                    graph.AddVertex(v);
                    if (IsSeed(v))//if seed create edges from seed and set seed neighbors to 1
                    {
                        seedPoints.Add(v);
                        foreach (var direction in directions)
                        {
                            var neighbor = v + direction;
                            if (neighbor.x < 0 || neighbor.y < 0 || neighbor.x >= testGridSize ||
                                neighbor.y >= testGridSize) continue;
                            if (IsBlocked(neighbor)) continue;
                            if (graph.ContainsVertex(neighbor))
                            {
                                graph.AddEdge(new Edge<Vector2Int>(v, neighbor));
                                UpdateValue(neighbor, 1);
                            }
                        }
                    }
                }
            }
            
            foreach (var seedPoint in seedPoints)
            {
                FloodFill(graph, seedPoint);
            }
            this.graph = graph;
        }

        [EnableIf(nameof(HasGraph)), GUIColor(0.8f, .2f, .2f)]
        [PropertyOrder(-10), ButtonGroup(),Button]
        private void DoHitRandom()
        {
            int x = UnityEngine.Random.Range(0, testGridSize);
            int y = UnityEngine.Random.Range(0, testGridSize);
            var seed = new Vector2Int(x, y);
            TestDamage(_hit, seed, graph);
        }
        private void DoHit(Vector2Int seed)
        {
            
            TestDamage(_hit, seed, graph);
        }
        bool HasGraph => graph != null && graph.VertexCount > 0;
        #endregion

        private void TestDamage(StormHit hit, Vector2Int seed, AdjacencyGraph<Vector2Int, Edge<Vector2Int>> graph)
        {
            FloodFill(graph, seed, hit.maxDamage);
            for (int i = 0; i < testGridSize; i++)
            {
                for (int j = 0; j < testGridSize; j++)
                {
                    graphVisual[i, j] = new Vector2Int(i, j);
                    resultGrid[i,j] = blockGrid[i,j] ? BLOCKED : UNDAMAGED;
                }
            }

            var currentDamage = 0;
            var current = seed;
            int GetCurrentValue() => resultGrid[current.x, current.y];
            void SetCurrentValue(int value)
            {
                if (GetCurrentValue() == UNDAMAGED)
                {
                    currentDamage+=1;
                    resultGrid[current.x, current.y] = value;
                }
            }
            
            var grabBag = hit.CreateGrabBag();
            using var grabBagIterator = grabBag.GetItems(t => {
                var v = current + t;
                if (!graph.ContainsVertex(v))
                    return false;
                if (resultGrid[v.x, v.y] != UNDAMAGED)
                    return false;
                if (!graph.ContainsEdge(current, v))
                    return false;
                return true;
            }).GetEnumerator();
            int reverseCount = 0;
            Stack<Vector2Int> previous = new Stack<Vector2Int>();
            HashSet<Vector2Int> visitedTwice = new HashSet<Vector2Int>();
            while (grabBagIterator.MoveNext() && currentDamage < hit.maxDamage)
            {
                current += grabBagIterator.Current;
                SetCurrentValue(previous.Count + 1 + reverseCount);
                if (grabBag.RemainingItems == 0 && previous.Count > 0)
                {
                    current = previous.Pop();
                    //check if we have visited this vertex twice, if not add it to visited twice and check all neighbors again
                    if (visitedTwice.Contains(current))
                    {
                        continue;
                    }

                    reverseCount++;
                    visitedTwice.Add(current);
                    grabBag.ReturnUnacceptedItems();//return all items to grab bag so that iterator will check them with the new current
                    continue;
                }
                previous.Push(current);
            }
        }

        private void FloodFill(AdjacencyGraph<Vector2Int, Edge<Vector2Int>> graph, Vector2Int seedPoint)
        {
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(seedPoint);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var currentValue = testGrid[current.x, current.y];
              
                var neighborsV = GetNeighborValues(current, graph);
                foreach (var valueTuple in neighborsV)
                {
                    if (valueTuple.Item2 == UNDAMAGED || valueTuple.Item2 > currentValue)
                    {
                        //check if edge exists, if it does, continue
                        if (graph.TryGetEdge(valueTuple.Item1, current, out var e))
                        {
                            continue;
                        }
                        //otherwise add edge and update value 
                        UpdateValue(valueTuple.Item1, currentValue+1);
                        graph.AddEdge(new Edge<Vector2Int>(current, valueTuple.Item1));
                        queue.Enqueue(valueTuple.Item1);
                    }
                }
               
            }
        }

        private void FloodFill(AdjacencyGraph<Vector2Int, Edge<Vector2Int>> graph, Vector2Int seedPoint,
            int maxDistance)
        {
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(seedPoint);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var currentValue = testGrid[current.x, current.y];
                if(currentValue >= maxDistance) continue;
                var neighborsV = GetNeighborValues(current, graph);
                foreach (var valueTuple in neighborsV)
                {
                    var neighborPos = valueTuple.Item1;
                    var neighborValue = valueTuple.Item2;
                    if (neighborValue == UNDAMAGED || neighborValue > currentValue)
                    {
                        //check if edge exists, if it does, continue
                        if (graph.TryGetEdge(valueTuple.Item1, current, out var e))
                        {
                            continue;
                        }
                        //otherwise add edge and update value 
                        UpdateValue(neighborPos, currentValue+1);
                        graph.AddVerticesAndEdge(new Edge<Vector2Int>(current, neighborPos));
                        queue.Enqueue(neighborPos);
                    }
                }

            }
        }
        private void TestHit(AdjacencyGraph<Vector2Int, Edge<Vector2Int>> graph, Vector2Int seedPoint)
        {
            var stormHitGraph = new StormHitGraph(_hit, seedPoint, graph);
            int steps = 0;
            foreach (var hits in stormHitGraph.GetHits())
            {
                testGrid[hits.x, hits.y] = steps++;
            }
        }
        public StormHitTester(StormHit stormHit)
        {
            _hit = stormHit;
        }
        
        
        void UpdateGridSize(int size)
        {
            blockGrid = new bool[size, size];
            testGrid = new int[size, size];
            graphVisual = new Vector2Int[size, size];
            resultGrid = new int[size, size];
            ResetGraphVisual();
            ResetGrid();
        }

        private void ResetGrid()
        {
            for (int i = 0; i < testGridSize; i++)
            {
                for (int j = 0; j < testGridSize; j++)
                {
                    graphVisual[i, j] = new Vector2Int(i, j);
                    testGrid[i, j] = resultGrid[i,j] = blockGrid[i,j] ? BLOCKED : UNDAMAGED;
                }
            }
        }
    }
}
#endif