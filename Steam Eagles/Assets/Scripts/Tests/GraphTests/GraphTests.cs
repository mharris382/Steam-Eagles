using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GasSim.SimCore.DataStructures;
using NUnit.Framework;
using QuikGraph;
using UnityEngine;
using UnityEngine.TestTools;



public class GraphTests
{
    private Graph<Node> _graph;
    private Node _rootNode;
    private const int START_AMOUNT = 1;
    public const int CAP = 10;
    public const int START_EDGE_WEIGHT = 1;
    
    private LinkedList<Node> _chainOfNodes;
    private Dictionary<Node,LinkedList<Node>> _branchedChainOfNodes;
    //private BidirectionalGraph<Node, WeightedEdge> graph;

    [SetUp]
    public void SetUp()
    {
        BuildGraphAsChain();
    }
    public struct WeightedEdge : IEdge<Node>
    {
        public WeightedEdge(Node source, Node target, float weight)
        {
            this.weight = weight;
            Source = source;
            Target = target;
        }

        public Node Source { get; }
        public Node Target { get; }

        public float weight;
    }
    
    
    private void BuildGraphAsChain()
    {
       // this.graph = new BidirectionalGraph<Node, WeightedEdge>();
        
        _graph = new Graph<Node>(GraphType.DIRECTED_WEIGHTED);
        
        _chainOfNodes = new LinkedList<Node>();
        _rootNode = new Node(START_AMOUNT);
        _chainOfNodes.AddLast(_rootNode);
        _graph.AddVertex(_rootNode);
        _branchedChainOfNodes = new Dictionary<Node, LinkedList<Node>>();
        
        for (int i = 0; i < 10; i++)
        {
            var node = new Node(START_AMOUNT);
            _chainOfNodes.AddLast(node);
            _graph.AddVertex(node);
          //  graph.AddVertex(node);
        }
        
        var linkedListNode = _chainOfNodes.First;
        while(linkedListNode.Next!= null)
        {
            _graph.AddEdge(linkedListNode.Value, linkedListNode.Next.Value, 1);
           // graph.AddEdge(new WeightedEdge(linkedListNode.Value, linkedListNode.Next.Value, 1));
            linkedListNode = linkedListNode.Next;
        }
    }

    bool BuildBranchAt(Node node, int lengthOfBranchedChain, out LinkedList<Node> chain)
    {
        if (_branchedChainOfNodes.ContainsKey(node))
        {
            throw new Exception("Only one branch per node allowed");
        }
        var branchedChain = new LinkedList<Node>();
        branchedChain.AddFirst(node);
        
        _branchedChainOfNodes.Add(node, branchedChain);
        var prevNode = node;
        for (int i = 0; i < lengthOfBranchedChain; i++)
        {
            var nextNode = new Node(START_AMOUNT);
            _graph.AddEdge(prevNode, nextNode, START_EDGE_WEIGHT);
            Assert.AreEqual(START_EDGE_WEIGHT, _graph.GetEdgeWeight(prevNode, nextNode));
            Assert.True(_graph.HasEdge(from:prevNode, to:nextNode));
            _graph.AddVertex(nextNode);
            
            branchedChain.AddLast(nextNode);
            prevNode = nextNode;
        }

        chain = branchedChain;
        return true;
    }
    
    Node GetRandomNode()
    {
        var random = new System.Random();
        var randomIndex = random.Next(0, _chainOfNodes.Count);
        
        
        var node = _chainOfNodes.ElementAt(randomIndex);
        return node;
    }
    void MakeChainLoop()
    {
        _graph.AddEdge(_chainOfNodes.Last.Value, _chainOfNodes.First.Value, 1);
    }
    
    [TearDown]
    public void TearDown()
    {
        _graph = null;
        _chainOfNodes = null;
    }

    [Test]
    public void Edges_Have_Direction()
    {
        var last = _chainOfNodes.Last;
        var prev = last.Previous;
        Assert.NotNull(prev.Value);
        Assert.NotNull(last.Value);
        Assert.True(_graph.ContainsVertex(prev.Value));
        Assert.True(_graph.ContainsVertex(last.Value));

        var edgesLast = _graph.GetEdges(last.Value).ToArray(); //graph.OutEdges(last.Value).ToArray();//
        var edgesPrev =_graph.GetEdges(prev.Value).ToArray(); //graph.InEdges(prev.Value).ToArray();//
        
        Assert.True(edgesLast.Length == 0);
        Assert.True(edgesPrev.Length == 1);
        Assert.AreEqual(edgesPrev[0], last.Value);
    }
   
    [Test]
    public void Can_Build_Chain()
    {
        Assert.AreEqual(_chainOfNodes.Count, _graph.VerticesCount);
        Assert.AreEqual(_chainOfNodes.Count - 1, _graph.EdgesCount);
        var newNode = new Node(START_AMOUNT);
        _graph.AddVertex(newNode);
        Assert.AreEqual(_chainOfNodes.Count+1, _graph.VerticesCount);
        foreach (var chainOfNode in _chainOfNodes)
        {
            Assert.True(_graph.ContainsVertex(chainOfNode));
            _graph.AddEdge(chainOfNode, newNode, 1);
            Assert.IsTrue(_graph.HasEdge(chainOfNode, newNode));
            Assert.IsFalse(_graph.HasEdge(newNode, chainOfNode));
            
        }
    }

    
    [Test]
    public void Cannot_Add_Same_Edge_Twice()
    {
        var node = GetRandomNode();
        var newNode = new Node(START_AMOUNT);
        _graph.AddVertex(newNode);
        _graph.AddEdge(node, newNode, 1);
    }
    
    
    [Test]
    public void Can_Store_Edge_Values()
    {
        foreach (var edge in _graph.GetAllEdges())
        {
            Assert.AreEqual(START_EDGE_WEIGHT, edge.weight);
            var from = _chainOfNodes.Find(edge.from);
            Assert.NotNull(from);
            Assert.NotNull(from.Next);
            Assert.AreEqual(from.Next.Value, edge.to);
        }
        // Use the Assert class to test conditions
        foreach (var from in _graph.GetVertices())      
        {
            foreach (var to in _graph.GetEdges(from))
            {
                Assert.AreEqual(START_EDGE_WEIGHT, _graph.GetEdgeWeight(from, to));
            }
        }
    }

     [Test]
    public void Iterate_Across_Neighbors_Of_Node()
    {
        var node = GetRandomNode();
        var neighbors = _graph.GetEdges(node);
        foreach (var neighbor in neighbors)
        {
            Assert.True(_graph.HasEdge(node, neighbor));
        }
    }
    
    
    [Test]
    public void Remove_Vertices_From_Graph()
    {
        var node = GetRandomNode();
        var outgoingEdges = _graph.GetEdges(node).ToArray();
        
        var incomingEdges = _graph.GetIncomingEdges(node) .ToArray();
        
        foreach (var outgoingEdge in outgoingEdges) Assert.IsTrue(_graph.HasEdge(node, outgoingEdge));
        foreach (var incomingEdge in incomingEdges) Assert.IsTrue(_graph.HasEdge(incomingEdge, node));

        int edgeCount = outgoingEdges.Length + incomingEdges.Length;
        
        _graph.RemoveVertex(node);
        Assert.False(_graph.ContainsVertex(node));
        
        int expectedEdgeCount = _graph.EdgesCount - edgeCount;
        Assert.AreEqual(expectedEdgeCount, _graph.EdgesCount);
        
        foreach (var outgoingEdge in outgoingEdges) Assert.IsFalse(_graph.HasEdge(node, outgoingEdge));
        foreach (var incomingEdge in incomingEdges) Assert.IsFalse(_graph.HasEdge(incomingEdge, node));

    }
    
    
    

    [Test]
    public void Change_Edge_Values_Of_Node()
    {
        var node = GetRandomNode();
        var newNode = new Node(START_AMOUNT);
        _graph.AddVertex(newNode);
        _graph.AddEdge(node, newNode, 1);
        Assert.AreEqual(1, _graph.GetEdgeWeight(node, newNode));
        _graph.SetEdgeWeight(node, newNode, 2);
        Assert.AreEqual(2, _graph.GetEdgeWeight(node, newNode));
    }

    
    
    
    public class Node
    {
        public int value;
        public int capacity;

        public Node(int value)
        {
            this.value = value;
            this.capacity = CAP;
        }
    }
}



