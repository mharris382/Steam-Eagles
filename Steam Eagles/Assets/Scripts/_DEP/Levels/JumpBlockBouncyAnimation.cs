using System;
using DG.Tweening;
using UnityEngine;

public class JumpBlockBouncyAnimation : MonoBehaviour
{
    public float duration = 1;
    [Range(0, 8)]
    public int vibrato = 1;
    public float elasticity = 1;
    public Ease ease;
    private Vector3 targetScale;
    private void Awake()
    {
        targetScale = transform.localScale;
    }


    private float _animTime;
    

    public void OnBounce(GameObject character)
    {
        //transform.DOKill();
       // transform.localScale = targetScale - (Vector3.one + Vector3.up * 0.01f);
        
        
    }

    Vector3 GetCurrentScale() => transform.localScale;
    void SetCurrentScale(Vector3 scale) => transform.localScale = new Vector3(transform.localScale.x, scale.y, transform.localScale.z);
}