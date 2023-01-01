using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(DistanceJoint2D))]
public class PhysicsChainLink : MonoBehaviour
{
    private DistanceJoint2D _distanceJoint2D;
    private Rigidbody2D _rigidbody2D;
    private SpriteRenderer _spriteRenderer;
    
    
    public SpriteRenderer SpriteRenderer => _spriteRenderer ? _spriteRenderer : _spriteRenderer = GetComponent<SpriteRenderer>();
    public DistanceJoint2D DistanceJoint2D => _distanceJoint2D ? _distanceJoint2D : _distanceJoint2D = GetComponent<DistanceJoint2D>();
    public Rigidbody2D Rigidbody2D => _rigidbody2D ? _rigidbody2D : _rigidbody2D = GetComponent<Rigidbody2D>();
    
    
    
    
    public int Index { get; set; }

    public IChainLink Next
    {
        get; 
        set;
    }
    public IChainLink Previous
    {
        get; 
        set;
    }
}