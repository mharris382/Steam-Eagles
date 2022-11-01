using System;
using System.Collections;
using System.Collections.Generic;
using GasSim.SimCore.DataStructures;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;

public class CellHelper : MonoBehaviour
{
    [SerializeField]
    private SharedTilemap tilemap;

    public Grid grid;
    
    /// <summary>
    /// get nearest cell coordinate in tilemap grid space
    /// </summary>
    public Vector3Int CellCoordinate
    {
        get => grid != null ? grid.WorldToCell(transform.position) : Tilemap.WorldToCell(transform.position);
    }

    /// <summary>
    /// center of cell in world space
    /// </summary>
    public Vector3 CellCenter
    {
        get => grid != null ? grid.WorldToCell(transform.position) : Tilemap.GetCellCenterWorld(CellCoordinate);
    }

    private Tilemap _fallback;

    protected Tilemap Tilemap
    {
        get
        {
            if (tilemap == null)
            {
                FindFallback();
            }

            if (tilemap.HasValue == false)
            {
                FindFallback();
            }

            return tilemap.HasValue ? tilemap.Value : _fallback;
        }
    }

    private void FindFallback()
    {
        _fallback = GameObject.FindWithTag("Pipe Tilemap").GetComponent<Tilemap>();
    }

    public bool HasTilemap => Tilemap != null ;
    
    
    private void Awake()
    {
        if (grid == null || tilemap != null)
        {
            tilemap.onValueChanged.AddListener(tm =>
            {
                enabled = tm != null;
            });
            enabled = tilemap.HasValue;
        }
    }

    private void Update()
    {
        
        if (tilemap == null) return;
        if (!tilemap.HasValue)
        {
            enabled = false;
            return;
        }
        
    }

    public static Graph<Vector3Int> BuildFrom(Vector3Int cell, Tilemap tilemap, out List<Vector3Int> path)
    {
        var graph = new Graph<Vector3Int>(GraphType.DIRECTED_UNWEIGHTED);
        graph.AddVertex(cell);
        path = new List<Vector3Int>();
        path.Add(cell);
        foreach (var vector3Int in GetNeighbors(cell))
        {
            if (tilemap.HasTile(vector3Int) )
            {
                if (graph.ContainsVertex(vector3Int))
                {
                    //TODO: break cell
                    throw new NotImplementedException("NO LOOPS RIGHT NOW!");
                }
                else
                {
                    
                    BuildFrom(tilemap, cell, vector3Int, graph, path);
                }
            }
        }
        return graph;
    }

    private static void BuildFrom(Tilemap tilemap, Vector3Int parent, Vector3Int cell, Graph<Vector3Int> graph, List<Vector3Int> path)
    {
        graph.AddVertex(cell);
        graph.AddEdge(parent, cell);
        path.Add(cell);        
        foreach (var vector3Int in GetNeighbors(cell))
        {
            if (vector3Int == parent) continue;
            if (graph.ContainsVertex(vector3Int)) continue;
            if (tilemap.HasTile(vector3Int))
            {
                BuildFrom(tilemap, cell, vector3Int, graph, path);
            }
        }
    }
    public static IEnumerable<Vector3Int> GetNeighbors(Vector3Int current)
    {
        yield return current + Vector3Int.right;
        yield return current + Vector3Int.left;
        yield return current + Vector3Int.up;
        yield return current + Vector3Int.down;
    }
    public static IEnumerable<Vector3Int> GetNonEmptyNeighbors(Vector3Int current, Tilemap tilemap)
    {
        var r = current + Vector3Int.right;
        var l = current + Vector3Int.left;
        var u = current + Vector3Int.up;
        var d = current + Vector3Int.down;

        if (tilemap.HasTile(r))
        {
            yield return r;
        }
        if (tilemap.HasTile(l))
        {
            yield return l;
        }
        if (tilemap.HasTile(u))
        {
            yield return u;
        }
        if (tilemap.HasTile(d))
        {
            yield return d;
        }
    }

    public IEnumerable<Vector3Int> GetNeighbors()
    {
        var current = CellCoordinate;
        
        yield return current + Vector3Int.right;
        yield return current + Vector3Int.left;
        yield return current + Vector3Int.up;
        yield return current + Vector3Int.down;
    }
    public IEnumerable<Vector3Int> GetNonEmptyNeighbors()
    {
        var current = CellCoordinate;
        
        var r = current + Vector3Int.right;
        var l = current + Vector3Int.left;
        var u = current + Vector3Int.up;
        var d = current + Vector3Int.down;

        if (tilemap.Value.HasTile(r))
        {
            yield return r;
        }
        if (tilemap.Value.HasTile(l))
        {
            yield return l;
        }
        if (tilemap.Value.HasTile(u))
        {
            yield return u;
        }
        if (tilemap.Value.HasTile(d))
        {
            yield return d;
        }
    }
}





public static class PipeCells
{
    // /// <summary>
    // /// just to make the code a little bit more explicit 
    // /// </summary>
    // public enum Direction
    // {
    //     RIGHT, LEFT, UP, DOWN
    // }
    //
    // internal static Dictionary<Direction, Vector3Int> _enumToDirection;// = new Dictionary<Direction, Vector3Int>();
    //
    //
    // public static Vector3Int ToVec(this Direction direction) => _enumToDirection[direction];
    //
    // public static Vector3Int Get(this Vector3Int vector3Int, Direction dir) => vector3Int + dir.ToVec();

    // static PipeCells()
    // {
    //     _enumToDirection = new Dictionary<Direction, Vector3Int>();
    //     _enumToDirection.Add(Direction.RIGHT, Vector3Int.right);
    //     _enumToDirection.Add(Direction.UP, Vector3Int.up);
    //     _enumToDirection.Add(Direction.LEFT, Vector3Int.left);
    //     _enumToDirection.Add(Direction.DOWN, Vector3Int.down);
    // }
    // public class PipeSwitch
    // {
    //     public Tilemap tilemap;
    //     public Vector3Int position;
    //     public Vector3Int[] outputDirections;
    //     public int currentDirection;
    //
    //     public PipeSwitch(Tilemap tilemap, Vector3Int position)
    //     {
    //         
    //     }
    // }
    public class PipePathway
    {
        public readonly Vector3Int startPosition;
        
        public PipePathway(Tilemap tilemap, List<Vector3Int> path)
        {
            startPosition = path[0];
        }
    }
    public struct PipeCell
    {
        public int index;
        public Vector3Int position;

        public PipeCell(int index, Vector3Int position)
        {
            this.index = index;
            this.position = position;
        }
    
    }
}