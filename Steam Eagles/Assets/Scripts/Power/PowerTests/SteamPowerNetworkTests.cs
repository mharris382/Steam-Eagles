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