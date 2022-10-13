using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{

    public CharacterState state;
    private Animator _anim;
    
    private float timeLastGrounded = 0;
    private bool isGrounded = false;
    private SpriteRenderer _sr;


    void Awake()
    {
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
        _anim.SetBool("is grounded", state.IsGrounded);
        _anim.SetFloat("velocity x", state.VelocityX);
        if (Mathf.Abs(state.VelocityX) > 0.1)
        {
            _sr.flipX = state.VelocityX < 0;
        }
        _anim.SetFloat("velocity y", state.VelocityY);
        _anim.SetBool("jumping", state.JumpHeld);
    }
}
