using UnityEngine;
using System.Collections;

namespace Power
{
    public interface IPowerNetworkNode
    {
        float Power { get; }
        Vector3Int Position { get; }
        
        PowerNodeType NodeType { get; }
    }


    public enum PowerNodeType
    {
        TRANSPORT,
        SOURCE,
        SINK
    }
}

