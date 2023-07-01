using UnityEngine;
[ExecuteAlways]
[RequireComponent(typeof(AnchoredJoint2D))]
public class JointLine : AutoLineRenderer
{
    private AnchoredJoint2D _joint;
    private AnchoredJoint2D joint => _joint ? _joint : _joint = GetComponent<AnchoredJoint2D>();

    public override Vector3 GetEndPoint()
    {
        return joint.GetWorldSpaceConnectedAnchor();
    }

    public override Vector3 GetStartPoint()
    {
        return joint.GetWorldSpaceAnchor();
    }
}