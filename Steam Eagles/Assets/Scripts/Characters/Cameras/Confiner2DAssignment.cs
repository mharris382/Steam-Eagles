using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Confiner2DAssignment : MonoBehaviour
{
[SerializeField]    private CinemachineConfiner2D confiner2D;

public void OnColliderAssigned(PolygonCollider2D collider2D)
{
    if (confiner2D == null)
    {
        confiner2D = GetComponent<CinemachineConfiner2D>();
        if (confiner2D == null) return;
    }
    confiner2D.m_BoundingShape2D = collider2D;
    
}
}
