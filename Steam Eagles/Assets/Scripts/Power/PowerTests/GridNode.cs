using System;
using UnityEngine;

public class GridNode : IEquatable<GridNode>, IEquatable<Vector3Int>, IComparable<GridNode>
{
    public Vector3Int Position { get; }

    public GridNode(Vector3Int position)
    {
        Position = position;
    }

    public bool Equals(GridNode other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Position.Equals(other.Position);
    }
    public bool Equals(Vector3Int other) => Position.Equals(other);
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((GridNode)obj);
    }
    public override int GetHashCode() => Position.GetHashCode();
    
    public static implicit  operator Vector3Int (GridNode node) => node.Position;
    public static implicit  operator GridNode (Vector3Int position) => new(position);

    public int CompareTo(GridNode other)
    {
        var x = other.Position.x.CompareTo(Position.x);
        if (x != 0) return x;
        var y = other.Position.y.CompareTo(Position.y);
        if (y != 0) return y;
        return 0;
    }
}