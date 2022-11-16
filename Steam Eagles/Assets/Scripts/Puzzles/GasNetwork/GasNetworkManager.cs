using QuikGraph;
using UnityEngine;

namespace Puzzles.GasNetwork
{
    public class GasNetworkManager : MonoBehaviour
    {
        
        public struct GasVert
        {
            public GasVert(Vector3Int coord)
            {
                this.Coordinate = coord;
            }
            public GasVert(Vector2Int coord)
            {
                this.Coordinate = (Vector3Int)coord;
            }
            public Vector3Int Coordinate { get; }
            
            public static implicit operator GasVert(Vector3Int coord) => new GasVert(coord);
            public static implicit operator GasVert(Vector2Int coord) => new GasVert(coord);
            public static implicit operator Vector2Int(GasVert coord) => (Vector2Int)coord.Coordinate;
            public static implicit operator Vector3Int(GasVert coord) => coord.Coordinate;
        }
        
        public class GasEdge : IEdge<GasVert>
        {
            public int gasInEdge = 0;

            public GasEdge(Vector2Int src, Vector2Int des, int gas)
            {
                Source = src;
                Target = des;
                gas = gasInEdge;
            }

            public GasVert Source{ get; }
            public GasVert Target { get; }
        }
        
        
        private void Awake()
        {
            AdjacencyGraph<GasVert, GasEdge> graph = new AdjacencyGraph<GasVert, GasEdge>();
        }
    }
}