using System.Collections.Generic;
using NUnit.Framework;
using QuikGraph;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tests.GraphTests
{
    [TestFixture]
    public class GasNetworkTests
    {
        
    }
}

public class PipeNetwork
{
    private readonly Tilemap _tilemap;

    public PipeNetwork(Tilemap tilemap)
    {
        _tilemap = tilemap;
        Graph = new AdjacencyGraph<GasNode, Edge<GasNode>>();
    }

    public AdjacencyGraph<GasNode, Edge<GasNode>> Graph { get; set; }
    
    
    public class GasNode
    {
        public GasNode(Vector2Int coordinate)
        {
            Coordinate = coordinate;
        }

        public Vector2Int Coordinate { get; set; }
    }
}

public class GasNetwork
{
    private BidirectionalGraph<Vertex, Edge<Vertex>> graph { get; }
    private readonly Dictionary<Vector2Int, Vertex> _dictionary;
    private readonly Dictionary<Vector2Int, Reactor> _reactors;
    public GasNetwork()
    {
        _dictionary = new Dictionary<Vector2Int, Vertex>();
        graph = new BidirectionalGraph<Vertex, Edge<Vertex>>();
        graph.VertexRemoved += OnRemoved;
        void OnRemoved(Vertex v)
        {
            _dictionary.Remove(v.position);
        }
    }

    public class Vertex
    {
        public Vector2Int position;

        public Vertex(Vector2Int position)
        {
            this.position = position;
        }
    }
    
    public class Reactor
    {
        public Reactor(Vertex inVertex, Vertex outVertex1, Vertex outVertex2)
        {
            InVertex = inVertex;
            OutVertex1 = outVertex1;
            OutVertex2 = outVertex2;
        }

        public Vertex InVertex { get;}
        public Vertex OutVertex1 { get; }
        public Vertex OutVertex2 { get; }
    }
    
    
    private Vertex GetVertex(Vector2Int position)
    {
        if (!_dictionary.ContainsKey(position))
        {
            _dictionary.Add(position, new Vertex(position));
        }
        
        return _dictionary[position];
    }
    
    
    public void AddGasReactor(Vector2Int gasIn, Vector2Int gasOut1, Vector2Int gasOut2)
    {
        Vertex vIn = GetVertex(gasIn);
        Vertex vOut1 = GetVertex(gasOut1);
        Vertex vOut2 = GetVertex(gasOut2);
        _reactors.Add(vIn.position, new Reactor(vIn, vOut1, vOut2));
        
    }
    
    
}



public class GasGenerator
{
    
}

public class GasConsumer
{
    
}
public class GasProducer
{
    
}

public class GasPipe
{
    
}

