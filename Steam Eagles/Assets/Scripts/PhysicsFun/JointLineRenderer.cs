using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[ExecuteAlways]
public class JointLineRenderer : MonoBehaviour
{
    private LineRenderer _lr;
    internal LineRenderer lr => _lr ? _lr : _lr = GetComponent<LineRenderer>();

    [SerializeField]
    internal AnchoredJoint2D joint;

    internal AnchoredJoint2D Joint => joint ? joint : joint = GetComponent<AnchoredJoint2D>(); 
    public bool HasResources() => Joint != null && Joint.connectedBody != null;
    public float zOffset = 0.1f;
    private void Update()
    {
        if (!HasResources())
        {
            return;
        }

        lr.enabled = true;
        var points = new Vector3[2]
        {
            Joint.attachedRigidbody.transform.position + Vector3.forward*zOffset,
            Joint.connectedBody.transform.position + Vector3.forward*zOffset
        };
        lr.positionCount = points.Length;
        lr.SetPositions(points);
    }
}