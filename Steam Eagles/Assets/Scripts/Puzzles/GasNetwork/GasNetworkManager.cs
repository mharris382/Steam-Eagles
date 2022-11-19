using System;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
//#define BURST

namespace Puzzles.GasNetwork
{
    public class GasNetworkManager : MonoBehaviour, IPipeGraph
    {
        public int pipeNetworkSize = 1000;
        public bool useJobs = false;
        [Header("Visualize Graph")]
        public bool visualizeNetwork;

        public float nodeSize = 0.5f;
        [Range(0.01f, 1)]
        public float arrowWidth = 0.05f;
        [Range(0.01f, 1)]
        public float arrowLength = 0.2f;
        private PipeNetwork _pipeNetwork;
        
        public IPipeGraph PipeGraph => PipeNetwork;
        
        private PipeNetwork PipeNetwork => _pipeNetwork ??= new PipeNetwork(pipeNetworkSize);

        public Grid Grid
        {
            get;
            set;
        }

        private void OnDestroy()
        {
            if(useJobs)
                PipeNetwork.Dispose();
        }

        public void RemoveNode(Vector3Int position) => PipeGraph.RemoveNode(position);
        public void AddNode(Vector3Int position) => PipeGraph.AddNode(position);
        
        public void AddGas(Vector3Int position, int amount) => PipeGraph.AddGas(position, amount);

        public void RemoveGas(Vector3Int position, int amount) => PipeGraph.RemoveGas(position, amount);

        private void Update()
        {
            if (useJobs)
            {
                PipeNetwork.ScheduleJobs();
            }
            else
            {
                
                PipeNetwork.UpdateEdges();
            }
        }

        private void LateUpdate()
        {
            if (useJobs)
            {
                PipeNetwork.CompleteJobs();
            }
        }

        private void OnDrawGizmos()
        {
            if (Grid == null) return;
            if (visualizeNetwork)
            {
                var graph = PipeNetwork.Graph;
                Color color = Color.cyan;
                color.a = 0.7f;
                Gizmos.color = color;
                
                foreach (var vert in graph.Vertices)
                {
                    var position = vert.Position;
                    var worldPosition = Grid.GetCellCenterWorld(position);
                    Gizmos.DrawWireSphere(worldPosition, nodeSize);
                }
                foreach (var graphEdge in graph.Edges)
                {
                    var source = graphEdge.Source.Position;
                    var target = graphEdge.Target.Position;
                    var sourceWorld = Grid.GetCellCenterWorld(source);
                    var targetWorld = Grid.GetCellCenterWorld(target);
                    Gizmos.DrawLine(sourceWorld, targetWorld);
                    var line = targetWorld - sourceWorld;
                    var dir = line.normalized;
                    var dist = line.magnitude;
                    dist -= arrowLength;
                    
                    var ortho = Vector3.Cross(dir, Vector3.forward);
                    var arrowStart = sourceWorld + dir * dist;
                    var arrowLeft = arrowStart + (ortho * arrowWidth);
                    var arrowRight = arrowStart - (ortho * arrowWidth);
                    Gizmos.DrawLine(targetWorld, arrowLeft);
                    Gizmos.DrawLine(targetWorld, arrowRight);
                }
                
                
            }
        }
    }

    public interface IPipeGraph
    {
        void RemoveNode(Vector3Int position);
        void AddNode(Vector3Int position);
        
        void AddGas(Vector3Int position, int amount);
        void RemoveGas(Vector3Int position, int amount);
    }

    public class PipeNetwork : IPipeGraph, IDisposable
    {
        private NativePipeNetwork _nativePipeNetwork;
        private AdjacencyGraph<PipeNode, PipeEdge> _graph = new AdjacencyGraph<PipeNode, PipeEdge>();
        public AdjacencyGraph<PipeNode, PipeEdge> Graph => _graph;

        private Dictionary<Vector3Int, PipeNode> _pipeNodeLookup;
        private Queue<Vector3Int> _addedNodes = new Queue<Vector3Int>();
        private bool useJobs;
#if BURST
        private NativeHashMap<Vector3Int, NativePipeNode> _pipeNodes;//
#endif
        public PipeNetwork(int nodeCapacity = 1000, bool useJobs=false)
        {
            #if BURST
            _pipeNodes = new NativeHashMap<Vector3Int, NativePipeNode>(nodeCapacity, Allocator.Persistent);
            #endif
            this.useJobs = useJobs;
            if(useJobs)
                _nativePipeNetwork = new NativePipeNetwork(nodeCapacity);
            _pipeNodeLookup = new Dictionary<Vector3Int, PipeNode>(nodeCapacity);
        }

        #region [API]

        public void AddNode(PipeNode node)
        {
            if (_pipeNodeLookup.ContainsKey(node.Position))
            {
                var pipeNode = _pipeNodeLookup[node.Position];
                _graph.RemoveVertex(pipeNode);
                _pipeNodeLookup.Remove(node.Position);
            }

           // _nativePipeNetwork.AddNode(node);
            _pipeNodeLookup.Add(node.Position, node);
            _graph.AddVertex(node);
            _addedNodes.Enqueue(node.Position);
        }
        public void RemoveNode(PipeNode node)
        {
            if (_pipeNodeLookup.ContainsKey(node.Position))
            {
                _pipeNodeLookup.Remove(node.Position);
            }
            _graph.RemoveVertex(node);
        }
       
        public void RemoveNode(Vector3Int position)
        {
            if (_pipeNodeLookup.ContainsKey(position))
            {
                var node = _pipeNodeLookup[position];
                RemoveNode(node);
            }
        }
        public void AddNode(Vector3Int position)
        {
            var node = new PipeNode(position);
            AddNode(node);
        }

        public void AddGas(Vector3Int position, int amount)
        {
            if (_pipeNodeLookup.ContainsKey(position))
            {
                var node = _pipeNodeLookup[position];
                node.AddGas(amount);
            }
        }

        public void RemoveGas(Vector3Int position, int amount)
        {
            throw new NotImplementedException();
        }


        public void AddEdge(PipeEdge edge)
        {
            _graph.AddEdge(edge);
        }
        #endregion

        public void UpdateEdges()
        {
            while (_addedNodes.Count > 0)
            {
                var next = _addedNodes.Dequeue();
                var node = _pipeNodeLookup[next];
                var neighbors = GetNeighbors(node.Position);
                foreach (var pipeEdge in neighbors
                             .Where(t => _pipeNodeLookup.ContainsKey(t))
                             .Select(t => _pipeNodeLookup[t])
                             .SelectMany(t => GetEdges(node, t)))
                {
                    _graph.AddEdge(pipeEdge);
                }
            }
        }

        public void ScheduleJobs()
        {
            if (!useJobs) return;
            while (_addedNodes.Count > 0)
            {
                var nodeAdded = _addedNodes.Dequeue();
                _nativePipeNetwork._addedNodes.Enqueue(nodeAdded);
                _nativePipeNetwork.AddNode(_pipeNodeLookup[nodeAdded]);
            }
            _nativePipeNetwork.ScheduleJobs(this);
        }

        public void CompleteJobs()
        {
            if (!useJobs) return; 
            _nativePipeNetwork.CompleteJobs(nativeEdge => AddEdge(GetEdge(nativeEdge)));
            PipeEdge GetEdge(NativePipeEdge nativePipeEdge) => new(_pipeNodeLookup[nativePipeEdge.Source.Position], _pipeNodeLookup[nativePipeEdge.Target.Position]);
        }
        
        private IEnumerable<PipeEdge> GetEdges(PipeNode src, PipeNode dst)
        {
            if (src.GasStored < dst.GasStored) (dst, src) = (src, dst);
            yield return new PipeEdge(src, dst);
            
            if (src.GasStored == dst.GasStored)
            {
                yield return new PipeEdge(dst, src);
            }
        }

        static Vector3Int[] GetNeighbors(Vector3Int position)
        {
            Vector3Int[] neighbors = new Vector3Int[4];
            neighbors[0] = position + Vector3Int.up;
            neighbors[1] = position + Vector3Int.down;
            neighbors[2] = position + Vector3Int.left;
            neighbors[3] = position + Vector3Int.right;
            return neighbors;
        }


        public void Dispose()
        {
            _nativePipeNetwork?.Dispose();
        }
    }
    public enum FlowDirection
    {
        NONE,
        UP,
        DOWN,
        LEFT,
        RIGHT
    }


    public class NativePipeNetwork : IDisposable
    {
        internal NativeHashMap<Vector3Int, NativePipeNode> _pipeNodes;
        internal NativeQueue<Vector3Int> _addedNodes;
        internal UpdateEdgeJob _updateEdgeJob;
        internal JobHandle _updateEdgeJobHandle;
        private NativeArray<NativePipeEdge> _nativePipeEdges;
        
        public NativePipeNetwork(int nodeCapacity)
        {
            _pipeNodes = new NativeHashMap<Vector3Int, NativePipeNode>(nodeCapacity, Allocator.Persistent);
            _addedNodes = new NativeQueue<Vector3Int>(Allocator.Persistent);
        }

        private bool runningJob = false;
        
        public void ScheduleJobs(PipeNetwork controlThreadPipeNetwork)
        {
            if (_addedNodes.Count == 0 || _updateEdgeJobHandle.IsCompleted == false)
            {
                runningJob = !_updateEdgeJobHandle.IsCompleted;
                return;
            }

            runningJob = true;

            var edges = new NativeArray<NativePipeEdge>((_addedNodes.Count * 8)+1, Allocator.TempJob);
            _updateEdgeJob = new UpdateEdgeJob()
            {
                PipeNodes = _pipeNodes,
                AddedNodes = _addedNodes,
                createdEdges = edges,
                index = 0
            };
            _updateEdgeJobHandle = _updateEdgeJob.Schedule();
        }

        public void CompleteJobs(Action<NativePipeEdge> onEdgeCreated)
        {
            if (!runningJob) return;
            _updateEdgeJobHandle.Complete();
            _nativePipeEdges = _updateEdgeJob.createdEdges;
            for (int i = 0; i < _updateEdgeJob.index; i++)
            {
                var edge = _nativePipeEdges[i];
                onEdgeCreated(edge);
            }
            _nativePipeEdges.Dispose();
        }

        public void Dispose()
        {
            _pipeNodes.Dispose();
            _nativePipeEdges.Dispose();
            _addedNodes.Dispose();
        }

        public void AddNode(PipeNode node)
        {
            if (_pipeNodes.ContainsKey(node.Position))
                _pipeNodes[node.Position] = node;
            else
                _pipeNodes.Add(node.Position, node);
        }
    }

    
    public struct UpdateEdgeJob : IJob
    {
        [ReadOnly] internal NativeHashMap<Vector3Int, NativePipeNode> PipeNodes;
        internal NativeQueue<Vector3Int> AddedNodes;
        [WriteOnly] internal NativeArray<NativePipeEdge> createdEdges;
        public int index;
        public void Execute()
        {
            while (!AddedNodes.IsEmpty())
            {
                var next = AddedNodes.Dequeue();
                var left = next + Vector3Int.left;
                var right = next + Vector3Int.right;
                var up = next + Vector3Int.up;
                var down = next + Vector3Int.down;
                var src =PipeNodes[next];
                
                CheckEdge(left, src);
                CheckEdge(right, src);
                CheckEdge(up, src);
                CheckEdge(down, src);
            }
        }

        private void CheckEdge(Vector3Int targetCell, NativePipeNode src)
        {
            if (PipeNodes.ContainsKey(targetCell))
            {
                var dest = PipeNodes[targetCell];
                if (dest.GasStored == src.GasStored)
                {
                    createdEdges[index++] = new NativePipeEdge(src, dest);
                    createdEdges[index++] = new NativePipeEdge(dest, src);
                }
                else
                {
                    //if dest has more gas than the source, edge is directed towards the source
                    if (dest.GasStored > src.GasStored)
                    {
                        //swap dest and source
                        (dest, src) = (src, dest);
                    }

                    createdEdges[index++] = new NativePipeEdge(src, dest);
                }
            }
        }
    }
    
    public struct NativePipeEdge : IEdge<NativePipeNode>
    {
        public NativePipeEdge(NativePipeNode source, NativePipeNode target)
        {
            Source = source;
            Target = target;
        }
        
        public NativePipeNode Source { get; }
        public NativePipeNode Target { get; }
    }

    public struct NativePipeNode
    {
        public readonly Vector3Int _position;
        private int _gasStored;
        public readonly int _gasCapacity;

        public Vector3Int Position => _position;
        public int GasStored => _gasStored;
        public int GasCapacity => _gasCapacity;
        
        public NativePipeNode(Vector3Int position, int gasStored, int gasCapacity)
        {
            _position = position;
            _gasStored = gasStored;
            _gasCapacity = gasCapacity;
        }

        public static implicit operator NativePipeNode(PipeNode pipeNode) => new NativePipeNode(pipeNode.Position, pipeNode.GasStored, pipeNode.GasCapacity);

        public override int GetHashCode()
        {
            return _position.GetHashCode();
        }
    }
    
   

}