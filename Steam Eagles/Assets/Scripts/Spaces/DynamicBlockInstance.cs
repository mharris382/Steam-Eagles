using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class DynamicBlockInstance : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;
    public Rigidbody2D Rigidbody2D => _rigidbody2D == null ? (_rigidbody2D = GetComponent<Rigidbody2D>()) : _rigidbody2D;

    [SerializeField]
    private DynamicBlock _block;
    public DynamicBlock Block
    {
        get => _block;
        set
        {
            _block = value;
            Sr.sprite = value.linkedStaticBlock.sprite;
            sr.color = value.linkedStaticBlock.color * Color.gray;
            Sprite sprite= sr.sprite;
            AddCollision(sprite);
        }
    }

    private static void AddCollision(Sprite sprite)
    {
        Debug.Assert(sprite.GetPhysicsShapeCount() > 0, "Sprite Missing physics shapes", sprite);
        var pointCount = sprite.GetPhysicsShapePointCount(0);
        var points = new List<Vector2>(pointCount);
        sprite.GetPhysicsShape(0, points);
    }

    private SpriteRenderer sr;

    public SpriteRenderer Sr => sr != null ? sr : (sr = new GameObject("Block Renderer").AddComponent<SpriteRenderer>());
}