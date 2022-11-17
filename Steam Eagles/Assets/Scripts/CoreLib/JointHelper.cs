using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class JointHelper : MonoBehaviour
{
    private Rigidbody2D _rb;
    private LineRenderer _lr;

    enum ColliderMode
    {
        IGNORE,
        DISABLE,
        ENABLE
    }

    [SerializeField] ColliderMode handleColliders = ColliderMode.IGNORE;

    [Header("Relative Joint - Linear Offset")]
    public bool setLinearOffset = true;

    public Vector2 offset = new Vector2(0, 1);

    [Header("Relative Joint - Angular Offset")]
    public bool useConstantAngle = false;

    public float constantAngle;
    public Vector2 angle;
    public AnimationCurve angleCurve = AnimationCurve.Constant(0, 1, 1);

    [Header("Distance Joint")] public bool updateDistanceJoints = true;
    public float distanceJointOffset = 1;

    [Header("Spring Joint")] public bool updateSpringJoints = true;
    public float springJointFrequency = 1;

    public float angleStart => angle.x;
    public float angleEnd => angle.y;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _lr = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        UpdateJoints();
        UpdateLine();
    }



    private float GetAngle(int index, int count)
    {
        float t = index / (float)count;
        return Mathf.Lerp(angleStart, angleEnd, angleCurve.Evaluate(t));
    }

    void UpdateAngle(RelativeJoint2D joint, int index, int count)
    {
        joint.angularOffset = useConstantAngle ? constantAngle : GetAngle(index, count);
    }

    private void UpdateLine()
    {
        _lr = GetComponent<LineRenderer>();
        if (_lr == null) return;
        _lr.positionCount = transform.childCount + 1;
        _lr.SetPosition(0, transform.position);
        for (int i = 0; i < transform.childCount; i++)
        {
            _lr.SetPosition(i + 1, transform.GetChild(i).position);
        }
    }

    private void UpdateJoints()
    {
        var t = transform;
        if (t.childCount < 1) return;
        int cnt = t.childCount;

        if (_rb != null)
        {
            Attach(_rb, t.GetChild(0).GetComponents<Joint2D>(), -1);
        }

        for (int i = 1; i < cnt; i++)
        {
            var t0 = t.GetChild(i - 1);
            var t1 = t.GetChild(i);
            Attach(t0.GetComponent<Rigidbody2D>(), t1.GetComponents<Joint2D>(), i);
        }
    }

    private void Attach(Rigidbody2D rb, Joint2D[] joints, int i)
    {
        int cnt = transform.childCount;
        foreach (var joint2D in joints)
        {
            joint2D.connectedBody = rb;
            // if (joint2D is RelativeJoint2D anchoredJoint)
            // {
            //     anchoredJoint.linearOffset = this.offset;
            //     if(i > -1) UpdateAngle(anchoredJoint, i, cnt);
            // }
            UpdateCollider(joint2D);
            switch (joint2D.GetType())
            {
                case var _ when joint2D is RelativeJoint2D relativeJoint2D:
                    relativeJoint2D.linearOffset = GetRelativeJointLinearOffset(relativeJoint2D, i, cnt);
                    UpdateAngle(relativeJoint2D, i, cnt);
                    break;
                case var _ when joint2D is DistanceJoint2D distanceJoint:
                    distanceJoint.distance = GetDistanceJointDistance(distanceJoint, i, cnt);
                    break;
                case var _ when joint2D is SpringJoint2D springJoint:
                    springJoint.frequency = GetSpringJointFrequency(springJoint, i, cnt);
                    break;
            }
        }
    }

    private void UpdateCollider(Joint2D joint2D)
    {
        if (handleColliders != ColliderMode.IGNORE)
        {
            var col = joint2D.gameObject.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = handleColliders == ColliderMode.ENABLE;
        }
    }

    Vector2 GetRelativeJointLinearOffset(RelativeJoint2D relativeJoint2D, int i, int cnt)
    {
        return setLinearOffset ? this.offset : relativeJoint2D.linearOffset;
    }

    float GetDistanceJointDistance(DistanceJoint2D distanceJoint, int i, int cnt)
    {
        return updateDistanceJoints ? distanceJointOffset : distanceJoint.distance;
    }

    float GetSpringJointFrequency(SpringJoint2D springJoint, int i, int cnt)
    {
        return updateSpringJoints ? springJointFrequency : springJoint.frequency;
    }

    public void UpdateColliders()
    {
        foreach (var joint2D in GetComponentsInChildren<Joint2D>())
        {
            UpdateCollider(joint2D);
        }
    }

    public void EnableColliders()
    {
        handleColliders = ColliderMode.ENABLE;
        UpdateColliders();
    }

    public void DisableColliders()
    {
        handleColliders = ColliderMode.DISABLE;
        UpdateColliders();
    }
    public void EnableCollidersWithDelay(float delay)
    {
        Invoke(nameof(EnableColliders), delay);
    }
    public void DisableCollidersWithDelay(float delay)
    {
        Invoke(nameof(DisableColliders), delay);   
    }

}
