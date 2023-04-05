using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class JointModule
{
    
}
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
    [SerializeField]private JointTypeMask jointTypeMask = JointTypeMask.ALL;
    JointTypeMask GetJointTypeMask() => jointTypeMask;
    [Header("Joint Settings")] public float jointRadius = 1;
    [Header("External Joints")]
    
    public Joint2D[] jointsToAttachToStart;
    public Rigidbody2D bodyToAttachToStart;
    public Joint2D[] jointsToAttachToEnd;
    public Rigidbody2D bodyToAttachToEnd;
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
    [Header("Rendering")]
    public bool attachLineToStart;
    public bool attachLineToEnd;
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

    [Flags]
    enum JointTypeMask
    {
        DISTANCE = 1 << 0,
        RELATIVE= 1 << 1,
        SPRING =  1 << 2,
        FRICTION = 1 << 3,
        HINGE = 1 << 4,
        FIXED = 1 << 5,
        ALL = DISTANCE | RELATIVE | SPRING | FRICTION | HINGE | FIXED
    }

    bool MaskIncludesDistance(JointTypeMask mask) => (mask & JointTypeMask.DISTANCE) > 0;
    bool MaskIncludesRelative(JointTypeMask mask) => (mask & JointTypeMask.DISTANCE) > 0;
    bool MaskIncludesFriction(JointTypeMask mask) => (mask & JointTypeMask.DISTANCE) > 0;
    private Dictionary<Type, JointTypeMask> maskLookup;
    void SetupTypeDictionary()
    {
         maskLookup = new Dictionary<Type, JointTypeMask>();
        maskLookup.Add(typeof(DistanceJoint2D), JointTypeMask.DISTANCE);
        maskLookup.Add(typeof(RelativeJoint2D), JointTypeMask.RELATIVE);
        maskLookup.Add(typeof(SpringJoint2D), JointTypeMask.SPRING);
        maskLookup.Add(typeof(HingeJoint2D), JointTypeMask.HINGE);
        maskLookup.Add(typeof(FixedJoint2D), JointTypeMask.FIXED);
        maskLookup.Add(typeof(FrictionJoint2D), JointTypeMask.FRICTION);
    }

    bool MaskIncludesType(Type jointType)
    {
        if (maskLookup == null)
        {
            SetupTypeDictionary();
        }

        if (maskLookup.ContainsKey(jointType))
            return (GetJointTypeMask() & maskLookup[jointType]) > 0;

        return false;
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
        List<Vector3> points = new List<Vector3>();
        _lr = GetComponent<LineRenderer>();
        if (_lr == null) return;

        

        var bodies = GetJointBodies(transform);
        for (int i = 0; i < bodies.Count; i++)
        {
            points.Add(bodies[i].position);
        }
        if(!attachLineToEnd && bodyToAttachToEnd!=null)points.RemoveAt(points.Count-1);
        if (!attachLineToStart) points.RemoveAt(0);

        _lr.positionCount = points.Count;
        _lr.SetPositions(points.ToArray());

    }

    struct JointConnection
    {
        public Rigidbody2D connectedBody;
        public Rigidbody2D attachedBody;
        public readonly int JointIndex;

        public JointConnection(Rigidbody2D connectedBody, Rigidbody2D attachedBody, int jointIndex)
        {
            this.connectedBody = connectedBody;
            this.attachedBody = attachedBody;
            this.JointIndex = jointIndex;
        }
    }
    
    private void UpdateJoints()
    {
        var t = transform;
        if (t.childCount < 1) return;
        int cnt = t.childCount;
        var bodies = GetJointBodies(transform).ToList();
        if (bodies.Count == 0) return;
        var connections = GetConnections(bodies).ToList();
        
        if (connections.Count == 0) return;
        foreach (var connection in connections)
        {
            Attach(connection, connections.Count);
        }
    }
    IEnumerable GetJointBodiesIterator(Transform ropeJointParent)
    {
        var startBody = bodyToAttachToStart ? bodyToAttachToStart : GetComponent<Rigidbody2D>();
        if (startBody != null) yield return startBody;

        for (int i = 0; i < ropeJointParent.childCount; i++)
        {
            var childBody = ropeJointParent.GetChild(i).GetComponent<Rigidbody2D>();
            if (childBody) yield return childBody;
        }

        if (bodyToAttachToEnd) yield return bodyToAttachToEnd;
        
    }
    IEnumerable<JointConnection> GetConnections(List<Rigidbody2D> jointBodies)
    {
        if (jointBodies.Count < 1)
        {
            yield break;
        }

        for (int i = 1; i < jointBodies.Count; i++)
        {
            var b0 = jointBodies[i - 1];
            var b1 = jointBodies[i];
            yield return new JointConnection(b0, b1, i-1);
        }
    }
    List<Rigidbody2D> GetJointBodies(Transform ropeJointParent)
    {
        List<Rigidbody2D> bodies = new List<Rigidbody2D>();
        
        
        var startBody = bodyToAttachToStart ? bodyToAttachToStart : GetComponent<Rigidbody2D>();
        if (startBody != null) bodies.Add(startBody);

        for (int i = 0; i < ropeJointParent.childCount; i++)
        {
            var childBody = ropeJointParent.GetChild(i).GetComponent<Rigidbody2D>();
            if (childBody) bodies.Add(childBody);
        }
        
        if (bodyToAttachToEnd) bodies.Add(bodyToAttachToEnd);
        return bodies;
    }

    private void Attach(Rigidbody2D rb, Joint2D[] joints, int i)
    {
        
    }

    private void Attach(JointConnection connection, int connectionsCount)
    {
        if (connection.JointIndex ==0)
            connection = new JointConnection(connection.attachedBody, connection.connectedBody, connection.JointIndex);
        Joint2D[] joints = connection.attachedBody.GetComponents<Joint2D>();
        bool isFirstOrLast = connection.JointIndex == 0 || connection.JointIndex == connectionsCount - 1; 
        foreach (var joint2D in joints)
        {
            joint2D.connectedBody = connection.connectedBody;
            UpdateCollider(joint2D);
            if (!MaskIncludesType(joint2D.GetType()))
                continue;
            switch (joint2D.GetType())
            {
                case var _ when joint2D is RelativeJoint2D relativeJoint2D:
                    
                    relativeJoint2D.linearOffset = GetRelativeJointLinearOffset(relativeJoint2D, connection.JointIndex, connectionsCount);
                    UpdateAngle(relativeJoint2D, connection.JointIndex, connectionsCount);
                    break;
                case var _ when joint2D is DistanceJoint2D distanceJoint:
                    distanceJoint.distance = !isFirstOrLast ? GetDistanceJointDistance(distanceJoint, connection.JointIndex, connectionsCount) : 0.1f;
                    break;
                case var _ when joint2D is SpringJoint2D springJoint:
                    springJoint.frequency = GetSpringJointFrequency(springJoint, connection.JointIndex, connectionsCount);
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


    private void OnDrawGizmos()
    {
        var bodies = GetJointBodies(transform);
        if (bodies.Count <= 1)
        {
            return;
        }

        for (int i = 1; i < bodies.Count; i++)
        {
            var body0 = bodies[i - 1];
            var body1 = bodies[i];
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(body0.position,jointRadius);
            Gizmos.DrawWireSphere(body1.position, jointRadius);
            Gizmos.DrawLine(body0.position, body1.position);
        }
    }
}
