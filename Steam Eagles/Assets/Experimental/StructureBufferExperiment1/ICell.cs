using UnityEngine;

namespace Experimental.StructureBufferExperiment1
{
    public interface ICell
    {
        Color Color { set; get; }
        Vector3 Position { set; get; }
    }
}