using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class JointHelper : MonoBehaviour
{
    private Rigidbody2D _rb;
    private LineRenderer _lr;

    [Header("Linear Offset")]
    public bool setDistance;
    public Vector2 offset = new Vector2(0, 1);

    [Header("Midpoints")]
    public bool updateKinematicMidpoints = true;
    public bool rotateKinematicMidpoints = false;
    public float moveSpeed = 1;

    [Header("Angular Offset")]
    public bool useConstantAngle = false;
    public float constantAngle;
    public Vector2 angle;
    public float angleStart => angle.x;
    public float angleEnd => angle.y;
    
    public AnimationCurve angleCurve = AnimationCurve.Constant(0,1, 1);
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


    public float GetAngle(int index, int count)
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
        _lr.positionCount = transform.childCount+1;
        _lr.SetPosition(0, transform.position);
        for (int i = 0; i < transform.childCount; i++)
        {
            _lr.SetPosition(i+1,transform.GetChild(i).position);
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

        for (int i = 1; i <cnt; i++)
        {
            var t0 = t.GetChild(i - 1);
            var t1 = t.GetChild(i);
            Attach(t0.GetComponent<Rigidbody2D>(), t1.GetComponents<Joint2D>(), i);
            if (updateKinematicMidpoints && t0.childCount > 0)
            {
                for (int j = 0; j < t0.childCount; j++)
                {
                    Vector2 p0 = t0.position;
                    Vector2 p1 = t1.position;
                    var child = t0.GetChild(j).GetComponent<Rigidbody2D>();
                    var target = p1 - p0 / 2f;  
                    child.transform.position = (moveSpeed <= 0 || !Application.isPlaying)
                        ? target
                        : Vector2.MoveTowards(child.position, target, Time.deltaTime * moveSpeed);
                    
                    if (rotateKinematicMidpoints)
                    {
                        var diff = p1 - p0;
                        child.rotation =  Vector2.SignedAngle(Vector2.right, diff.normalized);
                    }

                }
            }
        }
        
        void Attach(Rigidbody2D rb, Joint2D[] joints, int i)
        {
            foreach (var joint2D in joints)
            {
                joint2D.connectedBody = rb;
                if (joint2D is RelativeJoint2D anchoredJoint)
                {
                    anchoredJoint.linearOffset = this.offset;
                    if(i > -1)
                        UpdateAngle(anchoredJoint, i, cnt);
                }
            }
        }
    }
}
