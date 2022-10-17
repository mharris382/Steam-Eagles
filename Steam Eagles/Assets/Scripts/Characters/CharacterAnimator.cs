using System.Collections;
using System.Collections.Generic;
using System.Text;
using UniRx;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{

    public CharacterState state;
    private Animator _anim;
    public GroundCheck groundCheck;
    private float timeLastGrounded = 0;
    private bool isGrounded = false;
    private SpriteRenderer _sr;
    public float timeToConsiderGrounded = 0.25f;
    public Collider2D collider2D;

    public string info;
    List<ContactPoint2D> contactPoint2Ds;
    void Awake()
    {
        contactPoint2Ds = new List<ContactPoint2D>();
        _anim = GetComponent<Animator>();
        if (state == null)
            state = GetComponentInParent<CharacterState>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        state.IsGroundedEventStream.Subscribe(grounded =>
        {
            
        });
    }
    
    void Update()
    {
        StringBuilder sb = new StringBuilder();
        int contactCount = collider2D.GetContacts(contactPoint2Ds);
        if (contactCount > 0)
        {
            foreach (var contact in contactPoint2Ds)
            {
                if (Vector2.Dot(contact.normal, Vector2.up) > 0)
                {
                    _anim.SetBool("is grounded", true);
                }
                
            }
        }
        else
        {
            _anim.SetBool("is grounded", groundCheck.IsGrounded);
        }
        
        _anim.SetFloat("velocity x", state.VelocityX);
        _anim.SetFloat("speed", Mathf.Abs(state.VelocityX));
        if (Mathf.Abs(state.VelocityX) > 0.1)
        {
            _sr.flipX = state.VelocityX < 0;
        }
        _anim.SetFloat("velocity y", state.VelocityY);
        _anim.SetBool("jumping", state.JumpHeld);
    }
}
