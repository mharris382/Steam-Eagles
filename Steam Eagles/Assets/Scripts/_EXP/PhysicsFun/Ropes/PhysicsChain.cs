using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsChain : MonoBehaviour
{
    public List<PhysicsChainLink> links;
}


public interface IChainLink
{
    public Rigidbody2D Rigidbody2D { get; }
    public DistanceJoint2D DistanceJoint2D { get; }
    public IChainLink Next { get; }
    public IChainLink Previous { get; }
    public float Distance { get; }
}