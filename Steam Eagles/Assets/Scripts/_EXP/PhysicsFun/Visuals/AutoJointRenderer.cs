using System;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// used to automatically render joints using Line Renderers 
/// </summary>
public class AutoJointRenderer : MonoBehaviour
{
    
    [Sirenix.OdinInspector.Required] public GameObject target;
    [Sirenix.OdinInspector.Required,OnValueChanged(nameof(UpdateLrs))] public Material lineMaterial;
    public bool createParent = true;

    public float zOffset = 0;
    [Range(0, 10),OnValueChanged(nameof(UpdateLrs))] public float width = 1;
    [OnValueChanged(nameof(UpdateLrs))] public Color lineColor = Color.white;

    [SerializeField, OnValueChanged(nameof(UpdateLrs))] private string sortingLayer = "Default";
    [SerializeField, OnValueChanged(nameof(UpdateLrs))] private int orderInLayer;

    private AnchoredJoint2D[] _joints;
    private JointLineRenderer[] _jointsRenderers;
    private void Awake()
    {
        _joints = target.GetComponents<AnchoredJoint2D>();
        _jointsRenderers = new JointLineRenderer[_joints.Length];
        var parent = gameObject;
        if (createParent)
        {
            parent = new GameObject("JointRenderers");
            parent.transform.SetParent(transform);
        }
        for (int i = 0; i < _joints.Length; i++)
        {
            var joint = _joints[i];
            var go = new GameObject("JointLineRenderer " + i);
            go.transform.SetParent(parent.transform);
            var lr = go.AddComponent<LineRenderer>();
            
            UpdateRenderer(lr);
            _jointsRenderers[i] = new JointLineRenderer(joint, lr);
        }
    }

    void UpdateLrs()
    {
        if(_jointsRenderers == null) return;
        foreach (var jointRenderer in _jointsRenderers)
        {
            UpdateRenderer(jointRenderer.lineRenderer);
        }
    }

    private void UpdateRenderer(LineRenderer lr)
    {
        lr.material = lineMaterial;
        lr.useWorldSpace = true;
        lr.startWidth = lr.endWidth = width;
        lr.textureMode = LineTextureMode.Tile;
        lr.loop = false;
        lr.startColor = lr.endColor = lineColor;
        lr.sortingLayerName = sortingLayer;
        lr.sortingOrder = orderInLayer;
    }

    private void Update()
    {
        foreach (var jointRenderer in _jointsRenderers)
        {
            jointRenderer.Update(zOffset);
        }
    }

    private class JointLineRenderer
    {
        private readonly AnchoredJoint2D _joint2D;
        public readonly LineRenderer lineRenderer;
        
        public JointLineRenderer(AnchoredJoint2D joint2D, LineRenderer lineRenderer)
        {
            _joint2D = joint2D;
            this.lineRenderer = lineRenderer;
        }

        public void Update(float zOffset)
        {
            lineRenderer.positionCount = 2;
            var connectedAnchor = _joint2D.connectedAnchor;
            lineRenderer.SetPosition(0, _joint2D.connectedBody.transform.position + new Vector3(connectedAnchor.x, connectedAnchor.y, zOffset));
            var anchor = _joint2D.anchor;
            lineRenderer.SetPosition(1, _joint2D.transform.position + new Vector3(anchor.x, anchor.y, zOffset));
        }
    }
}