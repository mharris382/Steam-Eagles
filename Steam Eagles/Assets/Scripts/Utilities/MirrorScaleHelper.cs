using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorScaleHelper : MonoBehaviour
{
    [SerializeField] Transform mirror;
    public Transform Mirror { get => mirror;
        set => mirror = value;
    }

    private Vector3 _localScale;
    private void Awake()
    {
        if (mirror == null) mirror = transform;
        _localScale = mirror.localScale;
    }

    public void SetMirrorScaleX(bool mirrorX)
    {
        var localScale = _localScale;
        var scaleX = Mathf.Abs(localScale.x);
        if (mirrorX) scaleX *= -1;
        localScale = new Vector3(scaleX, localScale.y, localScale.z);
        transform.localScale = localScale;
    }
    public void SetMirrorScaleY(bool mirrorY)
    {
        var localScale = _localScale;
        var scaleY = Mathf.Abs(localScale.y);
        if (mirrorY) scaleY *= -1;
        localScale = new Vector3(localScale.x, scaleY, localScale.z);
        transform.localScale = localScale;
    }

    public void SetMirrorScaleZ(bool mirrorZ)
    {
        var localScale = _localScale;
        var scaleZ = Mathf.Abs(localScale.z);
        if (mirrorZ) scaleZ *= -1;
        localScale = new Vector3(localScale.x, localScale.y, scaleZ);
    }
}

