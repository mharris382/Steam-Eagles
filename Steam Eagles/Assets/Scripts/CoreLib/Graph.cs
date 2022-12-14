#if GODOT
using Godot;
#elif UNITY_5_3_OR_NEWER
using UnityEngine;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace GasSim.SimCore.DataStructures
{
    public enum GraphType
    {
        DIRECTED_WEIGHTED = 0b11,
        DIRECTED_UNWEIGHTED = 0b10,
        UNDIRECTED_UNWEIGHTED = 0b00,
        UNDIRECTED_WEIGHTED = 0b01,
        DIRECTED = 0b10,
        WEIGHTED = 0b01
    }

    public class Graph<T>
    {

        #region [Inner Classes]

        /// <summary>
        /// data access wrapper for directed graphs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class DirectedNode
        {
            private List<Edge> _incomingNodes;
            private List<Edge> _outgoingNodes;
        
            private T _value;
            private Graph<T> _graph;
        
        
            public int IncomingEdgeCount => _incomingNodes.Count;
            public int OutgoingEdgeCount => _outgoingNodes.Count;
            public T Value => _value;
        
            public Graph<T> Graph => _graph;
        
            (T, float) GetEdge(T to) => (to, _graph.GetEdgeWeight(Value, to));
        
            public DirectedNode(T value, Graph<T> graph)
            {
                _value = value;
                _graph = graph;
            }
        }
        
        public class Node
        {
            public Graph<T> graph { get; }
            public T node { get; }

            internal Node(Graph<T> graph, T node)
            {
                this.node = node;
                graph = graph;
            }
        }

        #endregion
        
        
        private readonly GraphType _type;
        private Dictionary<T, Dictionary<T, float>> _edgeLookup;
        private Dictionary<T, HashSet<T>> _incomingEdges;
        public GraphType GraphType => _type;
        private int _vertexCount;


        public int VerticesCount => _edgeLookup.Count;
        public int EdgesCount => _edgeLookup.Sum(x => x.Value.Count);

        public bool IsDirected => (((int)_type) & 2) != 0;
        public bool IsWeighted => ((int)_type & 1) != 0;

        public Graph(GraphType type)
        {
            _type = type;
            _vertexCount = 0;
            _edgeLookup = new Dictionary<T, Dictionary<T, float>>();
            _incomingEdges = new Dictionary<T, HashSet<T>>();
        }

        /// <summary>
        /// adds a vertex to the graph if it didn't already exist
        /// TODO: write unit tests for all cases (1 per graph type) 
        /// </summary>
        /// <param name="vertex"></param>
        public void AddVertex(T vertex)
        {
            if (ContainsVertex(vertex)) return;
            _edgeLookup.Add(vertex, new Dictionary<T, float>());
            _incomingEdges.Add(vertex, new HashSet<T>());
            _vertexCount++;
        }

        /// <summary>
        /// removes a vertex from the graph, as well as all connected edges
        /// TODO: write unit tests for all cases (1 per graph type) 
        /// </summary>
        /// <param name="vertex"></param>
        public void RemoveVertex(T vertex)
        {
            var incomingEdges = GetIncomingEdges(vertex).ToArray();
            foreach (var incomingEdge in incomingEdges)
            {
                _edgeLookup[incomingEdge].Remove(vertex);
            }

            _edgeLookup.Remove(vertex);
        }


        /// <summary>
        /// iterator of outgoing edges starting from the given vertex
        /// TODO: write unit tests for all cases (1 per graph type) 
        /// </summary>
        /// <param name="vertex">get the outgoing edges originating from this vertex</param>
        /// <returns></returns>
        public IEnumerable<T> GetEdges(T vertex)
        {
            if (!ContainsVertex(vertex)) yield break;
            var edges = _edgeLookup[vertex];
            foreach (var edge in edges)
                yield return edge.Key;
        }

        public IEnumerable<(T from, T to, float weight)> GetAllEdges()
        {
            foreach (var vertex in _edgeLookup.Keys)
            {
                foreach (var edge in GetEdges(vertex))
                {
                    yield return (vertex, edge, GetEdgeWeight(vertex, edge));
                }
            }
        }

        /// <summary>
        ///  iterator of outgoing weighted edges starting from the given vertex
        /// TODO: write unit tests for all cases (1 per graph type) 
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public IEnumerable<(T dest, float weight)> GetWeightedEdges(T vertex)
        {
            if (!ContainsVertex(vertex)) yield break;
            var edges = _edgeLookup[vertex];

            foreach (var edge in edges)
                yield return (edge.Key, edge.Value);
        }


        /// <summary>
        /// iterator returns all vertices O(V) operation if all vertices are traversed 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> GetVertices()
        {
            foreach (var kvp in _edgeLookup)
            {
                yield return kvp.Key;
            }
        }

        //TODO change signature so it matches GetWeightedEdges
        public IEnumerable<T> GetIncomingEdges(T vertex)
        {
            if (!ContainsVertex(vertex)) yield break;
            var edges = _incomingEdges[vertex];
            foreach (var edge in edges)
            {
                yield return edge;
            }
        }

        public IEnumerable<(T, float)> GetIncomingWeightedEdge(T toVertex)
        {
            if (!ContainsVertex(toVertex)) yield break;
            var edges = _incomingEdges[toVertex];
            foreach (var edge in edges.Select(fromVertex => (fromVertex, GetEdgeWeight(fromVertex, toVertex))))
            {
                yield return edge;
            }
        }

        public bool ContainsVertex(T vertex) => _edgeLookup.ContainsKey(vertex);

        public bool HasEdge(T from, T to)
        {
            return _edgeLookup[from].ContainsKey(to);
        }

        public bool TryGetEdgeWeight(T from, T to, out float weight)
        {
            if (!HasEdge(from, to))
            {
                weight = 0;
                return false;
            }

            weight = _edgeLookup[from][to];
            return true;
        }

        public float GetEdgeWeight(T from, T to)
        {
            if (!HasEdge(from, to))
                throw new KeyNotFoundException($"Edge from {from} to {to} does not exist");
            return _edgeLookup[from][to];
        }

        public void AddEdge(T from, T to, float weight = 0)
        {
            //if not weighted, don't accept weighted input values
            if (!IsWeighted) weight = 0;

            //if edge doesn't exist add one
            if (!HasEdge(from, to))
            {
                _edgeLookup[from].Add(to, weight);

            }
            else //just update the weight
            {
                _edgeLookup[from][to] = weight;
            }

            //if not directed, also add the inverse edge
            if (!IsDirected)
                AddUndirectedEdge(from, to, weight);

        }

        private void AddUndirectedEdge(T from, T to, float weight = 0)
        {
            if (!IsWeighted) weight = 0;
            if (!HasEdge(from, to))
            {
                _edgeLookup[from].Add(to, weight);
            }
            else //edge exists so update weight
            {
                _edgeLookup[from][to] = weight;
            }
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            var verts = new List<T>(GetVertices());
            verts.Sort((t1, t2) => String.Compare(t1.ToString(), t2.ToString(), StringComparison.Ordinal));

            AddHeader();
            foreach (var from in verts)
            {
                if (IsWeighted) AddLinesWeighted(from);
                else AddLinesUnweighted(from);
            }

            return sb.ToString();

            void AddHeader()
            {
                sb.Append(IsDirected ? "directed" : "undirected");
                sb.Append(' ');
                sb.Append(IsWeighted ? "weighted" : "unweighted");
                sb.Append('\n');
            }

            void AddLinesWeighted(T from)
            {
                foreach (var edge in GetWeightedEdges(from))
                {
                    sb.AppendLine($"{from.ToString()}={edge.dest.ToString()}={edge.weight:F2}");
                }
            }

            void AddLinesUnweighted(T from)
            {
                foreach (var edge in GetEdges(from))
                {
                    sb.AppendLine($"{from.ToString()}={edge.ToString()}");
                }
            }
        }


        public DirectedNode GetDirectedNode(T vertex)
        {
            if (!ContainsVertex(vertex)) return null;
            return new DirectedNode(vertex, this);
        }
        
        public struct Edge
        {
            private readonly T _node;
            private readonly float _weight;

            public Edge(T node, float weight)
            {
                _node = node;
                _weight = weight;
            }

            public Edge((T node, float weight) tuple) : this(tuple.node, tuple.weight)
            {
            }
        }

        public void SetEdgeWeight(T node, T newNode, int i)
        {
            _edgeLookup[node][newNode] = i;
        }
    }

   

    



    public struct DirectedWeightedEdge<T>
    {
        public T from;
        public T to;
        public float weight;

        public DirectedWeightedEdge(T from, T to, float weight)
        {
            this.from = from;
            this.to = to;
            this.weight = weight;
        }
    }

    

}