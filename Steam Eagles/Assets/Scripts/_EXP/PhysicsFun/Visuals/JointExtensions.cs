using UnityEngine;

public static class JointExtensions
{
    public static Vector3 GetWorldSpaceAnchor(this AnchoredJoint2D joint)
    {
        if(joint.connectedBody == null)
            return joint.transform.TransformPoint(joint.anchor);
        return joint.connectedBody.transform.TransformPoint(joint.connectedAnchor);
    }
    public static Vector3 GetWorldSpaceConnectedAnchor(this AnchoredJoint2D joint)
    {
        return joint.transform.TransformPoint(joint.anchor);
    }
}