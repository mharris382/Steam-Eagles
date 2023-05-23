using System;
using UnityEngine;

namespace CoreLib
{
    /// <summary>
    /// Vector3Int wrapper a machine cell position on the building grid.  Does not actually know about or store the
    /// information in that position, but is used to reference a grid position from a Vector3Int 
    ///
    /// TODO: add global callbacks for machine cell changes, allowing listeners to respond to a specific cell changing or any cell changing 
    /// </summary>
    public struct MachineVec : IEquatable<MachineVec>
    {
        public readonly Vector3Int Cell;

        public MachineVec(Vector3Int cell)
        {
            this.Cell = cell;
        }

        public bool Equals(MachineVec other) => Cell.Equals(other.Cell);

        public override bool Equals(object obj) => obj is MachineVec other && Equals(other);

        public override int GetHashCode() => Cell.GetHashCode();

        public static implicit operator Vector3Int(MachineVec cell) => cell.Cell;
        public static implicit operator MachineVec(Vector3Int cell) => new(cell);
        public static implicit operator MachineVec(Vector2Int cell) => new((Vector3Int)cell);
        public static explicit operator Vector2Int(MachineVec cell) => (Vector2Int)cell.Cell;
    }
}