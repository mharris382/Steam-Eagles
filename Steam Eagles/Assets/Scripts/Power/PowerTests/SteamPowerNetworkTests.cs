using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuikGraph;
using UniRx;
using UnityEngine;
using UnityEngine.TestTools;

public class SteamPowerNetworkTests
{
   
}
public class GridGraphTests
{
    private Vector3Int[] _directions = new Vector3Int[] {
        Vector3Int.up,
        Vector3Int.right,
        Vector3Int.down,
        Vector3Int.left
    };

    private EdgeFactory<int> edgeFactory;
    private GridGraph<int> graph;
    private GridNode seedNode;
    private int expectedEdgeCount = 0;
    private int expectedNodeCount = 0;
    private CompositeDisposable disposable;

    private List<GridNode> addedNodes;
    private List<GridNode> removedNodes;
    private List<TaggedUndirectedEdge<GridNode,int>> addedEdges;
    private List<TaggedUndirectedEdge<GridNode,int>> removedEdges;
    [SetUp]
    public void SetUp()
    {
        edgeFactory = new EdgeFactory<int>();
        this.seedNode = new GridNode(new Vector3Int(32, 1, 0));
        this.graph = new GridGraph<int>();
        CompositeDisposable cd = new CompositeDisposable();
        addedEdges = new List<TaggedUndirectedEdge<GridNode,int>>();
        removedEdges = new List<TaggedUndirectedEdge<GridNode,int>>();
        addedNodes = new List<GridNode>();
        removedNodes = new List<GridNode>();
        expectedEdgeCount = 0;
        expectedNodeCount = 0;
        graph.OnEdgeAdded.Subscribe(e => addedEdges.Add(e)).AddTo(cd);
        graph.OnEdgeRemoved.Subscribe(e => removedEdges.Add(e)).AddTo(cd);
        graph.OnNodeAdded.Subscribe(n => addedNodes.Add(n)).AddTo(cd);
        graph.OnNodeRemoved.Subscribe(n => removedNodes.Add(n)).AddTo(cd);
        this.disposable = cd;
    }

    [TearDown]
    public void TearDown()
    {
        disposable?.Dispose();
    }
    [Test]
    public void CanAddNodeToGridGraph()
    {
        //add neighbor nodes
        foreach (var direction in _directions)
        {
            var node = seedNode.Position + direction;
            expectedNodeCount++;
            var result = graph.AddNode(node);
            Assert.IsTrue(result);
            Assert.AreEqual(expectedNodeCount, graph.Graph.VertexCount);
            Assert.AreEqual(expectedEdgeCount, graph.Graph.EdgeCount);
        }
        expectedNodeCount++;
        expectedEdgeCount += 4;
        bool nodeAdded = graph.AddNode(seedNode);
        Assert.IsTrue(nodeAdded);
        Assert.AreEqual(expectedNodeCount, graph.Graph.VertexCount);
        Assert.AreEqual(expectedEdgeCount, graph.Graph.EdgeCount);

        Assert.AreEqual(4, graph.Graph.EdgeCount);
        foreach (var direction in _directions)
        {
            expectedEdgeCount++;
            expectedNodeCount++;
            var node = seedNode.Position + direction + direction;
            var expectedEdgeSource = seedNode.Position + direction; 
            
            var result = graph.AddNode(node);
            Assert.IsTrue(result);
            Assert.AreEqual(expectedNodeCount, graph.Graph.VertexCount);
            Assert.AreEqual(expectedEdgeCount, graph.Graph.EdgeCount);
            Assert.IsTrue(addedNodes.Contains(node));
            Assert.IsTrue(addedEdges.First(t =>
            {
                return t.Source == expectedEdgeSource && t.Target == node ||
                       t.Source == node && t.Target == expectedEdgeSource;
            }).Tag.Equals(0));
        }
    }

    [Test]
    public void CanRemoveNodesFromGraph()
    {
        var nodeToAdd = new GridNode(new Vector3Int(32, 1, 0));
        CanAddNodeToGridGraph();
        foreach (var direction in _directions)
        {
            var nodeToRemove = nodeToAdd.Position + direction;
            expectedNodeCount--;
            expectedEdgeCount -= 2;
            var result = graph.RemoveNode(nodeToRemove);
            Assert.IsTrue(result);
            Assert.AreEqual(expectedNodeCount, graph.Graph.VertexCount);
            Assert.AreEqual(expectedEdgeCount, graph.Graph.EdgeCount);
        }
    }

    [Test]
    public void GetCorrectNeighborCount()
    {
        CanAddNodeToGridGraph();
        int expectedNeighborCount = 4;
        int actualNeighborCount = graph.GetNeighborCount(seedNode);
        Assert.AreEqual(expectedNeighborCount, actualNeighborCount);
        foreach (var direction in _directions)
        {
            var nodePos = seedNode.Position + direction;
            var nodePos2 = nodePos + direction;
            var node = graph.GetNode(nodePos);
            var node2 = graph.GetNode(nodePos2);
            Assert.NotNull(node);
            Assert.NotNull(node2);
            Assert.AreEqual(2, graph.GetNeighborCount(node));
            Assert.AreEqual(1, graph.GetNeighborCount(node2, out var neighbors));
            Assert.AreEqual(neighbors[0], node);
        }
    }
}


public class GridNodeTests
{
    private GraphWrapper graph;

    [SetUp]
    public void SetUp()
    {
        graph = new GraphWrapper();
    }

    [TearDown]
    public void TearDown()
    {
        graph.Dispose();
    }
    [Test]
    public void TestContainsVertex()
    {
        var pos = new Vector2Int(0, 0);
        var node = new GridNode(pos);
        Assert.AreEqual(new GridNode(new Vector3Int(0,0,0)), node);
        var graph = new AdjacencyGraph<GridNode, SEdge<GridNode>>();
        graph.AddVertex(node);
        Assert.IsTrue(graph.ContainsVertex(new Vector3Int(0,0,0)));
        pos += Vector2Int.right;
        graph.AddVertex(pos);
        Assert.IsTrue(graph.ContainsVertex(new Vector3Int(1, 0,0)));
    }
    
    [Test]
    public void TestContainsEdges()
    {
        var pos = new Vector2Int(0, 0);
        var node = new GridNode(pos);
        Assert.AreEqual(new GridNode(new Vector3Int(0,0,0)), node);
        var graph = new AdjacencyGraph<GridNode, SEdge<GridNode>>();
        graph.AddVertex(node);
        Assert.IsTrue(graph.ContainsVertex(new Vector3Int(0,0,0)));
        pos += Vector2Int.right;
        graph.AddVertex(pos);
        Assert.IsTrue(graph.ContainsVertex(new Vector3Int(1, 0,0)));
        var added1 = graph.AddEdge(new SEdge<GridNode>(Vector2Int.zero, pos));
        var added2 = graph.AddEdge(new SEdge<GridNode>(pos, Vector2Int.zero));
        Assert.IsTrue(added1);
        Assert.IsTrue(added2);

        var edge = new SEdge<GridNode>(Vector2Int.zero, Vector2Int.right);
        var edge2 = new SEdge<GridNode>(Vector2Int.right, Vector2Int.zero);
        Assert.IsTrue(graph.ContainsEdge(edge));
        Assert.IsTrue(graph.ContainsEdge(Vector2Int.zero, Vector2Int.right));
        Assert.IsTrue(graph.ContainsEdge(Vector2Int.right, Vector2Int.zero));
    }


    [Test]
    public void TestWrapperAddVertex()
    {
        WrapperAddTestVerts();
    }

    private void WrapperAddTestVerts()
    {
        var pos = new Vector2Int(0, 0);
        var node = new GridNode(pos);
        Assert.AreEqual(new GridNode(new Vector3Int(0, 0, 0)), node);

        graph.AddVertex(node);
        Assert.IsTrue(graph.Graph.ContainsVertex(new Vector3Int(0, 0, 0)));
        pos += Vector2Int.right;
        graph.AddVertex(pos);
        Assert.IsTrue(graph.Graph.ContainsVertex(new Vector3Int(1, 0, 0)));
    }

    [Test]
    public void TestWrapperAddBiDirectionalEdge()
    {
        WrapperAddBiDirectionalEdges();
    }

    private void WrapperAddBiDirectionalEdges()
    {
        WrapperAddTestVerts();
        var added1 = graph.AddBiDirectionalEdge(Vector2Int.zero, Vector2Int.right);
        Assert.IsTrue(added1);

        var edge = new SEdge<GridNode>(Vector2Int.zero, Vector2Int.right);
        var edge2 = new SEdge<GridNode>(Vector2Int.right, Vector2Int.zero);
        Assert.IsTrue(graph.Graph.ContainsEdge(edge));
        Assert.IsTrue(graph.Graph.ContainsEdge(Vector2Int.zero, Vector2Int.right));
        Assert.IsTrue(graph.Graph.ContainsEdge(Vector2Int.right, Vector2Int.zero));
    }

    [Test]
    public void TestWrapperAddDirectionalEdge()
    {
        WrapperAddDirectionalEdge();
    }

    private void WrapperAddDirectionalEdge()
    {
        WrapperAddTestVerts();
        Assert.IsTrue(graph.Graph.ContainsVertex(new Vector3Int(1, 0, 0)));
        var added1 = graph.AddDirectionalEdge(Vector2Int.zero, Vector2Int.right);
        Assert.IsTrue(added1);
        Assert.IsTrue(graph.Graph.ContainsEdge(Vector2Int.zero, Vector2Int.right));
        Assert.IsFalse(graph.Graph.ContainsEdge(Vector2Int.right, Vector2Int.zero));
    }


    [Test]
    public void TestWrapperFiresAddVertex()
    {
        List<GridNode> nodes = new();
        var d = graph.OnNodeAdded.Subscribe(nodes.Add);
        TestWrapperAddVertex();
        d.Dispose();
        Assert.IsTrue(nodes.Contains(Vector2Int.zero));
        Assert.IsTrue(nodes.Contains(Vector2Int.right));
    }
    [Test]
    public void TestWrapperFiresRemoveVertex()
    {
        List<GridNode> nodes = new();
        var d = graph.OnNodeRemoved.Subscribe(nodes.Add);
        using (d)
        {
            WrapperAddTestVerts();
        
            Assert.IsFalse(nodes.Contains(Vector2Int.zero));
            Assert.IsFalse(nodes.Contains(Vector2Int.right));
        
            graph.Graph.RemoveVertex(Vector2Int.zero);
            Assert.IsTrue(nodes.Contains(Vector2Int.zero));
        
            graph.Graph.RemoveVertex(Vector2Int.right);
            Assert.IsTrue(nodes.Contains(Vector2Int.right));
        }
    }
    [Test]
    public void TestWrapperFiresAddEdge()
    {
        List<SEdge<GridNode>> sEdges = new();
        var d = graph.OnEdgeAdded.Subscribe(sEdges.Add);
        using (d)
        {
            WrapperAddBiDirectionalEdges();
            Assert.IsTrue(sEdges.Contains(new SEdge<GridNode>(Vector2Int.zero, Vector2Int.right)));
            Assert.IsTrue(sEdges.Contains(new SEdge<GridNode>(Vector2Int.right,Vector2Int.zero)));
        }
    }
    
    [Test]
    public void TestWrapperFiresRemoveEdge()
    {
        List<SEdge<GridNode>> sEdges = new();
        var d = graph.OnEdgeRemoved.Subscribe(sEdges.Add);
        using (d)
        {
            WrapperAddBiDirectionalEdges();
            Assert.IsFalse(sEdges.Contains(new SEdge<GridNode>(Vector2Int.zero, Vector2Int.right)));
            Assert.IsFalse(sEdges.Contains(new SEdge<GridNode>(Vector2Int.right,Vector2Int.zero)));
            graph.Graph.RemoveVertex(Vector2Int.zero);
            Assert.IsTrue(sEdges.Contains(new SEdge<GridNode>(Vector2Int.zero, Vector2Int.right)));
            Assert.IsTrue(sEdges.Contains(new SEdge<GridNode>(Vector2Int.right,Vector2Int.zero)));
        }
    }
}