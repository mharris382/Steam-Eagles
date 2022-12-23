using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace PhysicsFun
{
    [ExecuteAlways]
    [RequireComponent(typeof(SoftBody2D))]
    public class SoftBodySpriteShape : MonoBehaviour
    {
        private const float SPLINE_OFFSET = .5f;
        public SpriteShapeController spriteShapeController;
      

        private SoftBody2D _softBody;
        public SoftBody2D softBody2D => _softBody ? _softBody : _softBody = GetComponent<SoftBody2D>();


        bool HasResources()
        {
            return spriteShapeController != null;
        }


        private void Awake()
        {
            if (!HasResources()) return;
            UpdateVertices();
        }

        private void Update()
        {
            if (!HasResources()) return;
            UpdateVertices();
        }

        private void UpdateVertices()
        {
            if (spriteShapeController.spline.GetPointCount() != transform.childCount)
            {
                spriteShapeController.spline.Clear();
                for (int i = 0; i < transform.childCount; i++)
                {
                    spriteShapeController.spline.InsertPointAt(i, transform.GetChild(i).position);
                    spriteShapeController.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
                }
            }
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                var vert = child.localPosition;
                var toCenter = -vert.normalized;
                var radius = softBody2D.bodyRadius;
                try
                {
                    spriteShapeController.spline.SetPosition(i, vert + (vert.normalized * radius));
                }
                catch (Exception e)
                {
                    Debug.Log("Spline points are too close together...", this);
                    spriteShapeController.spline.SetPosition(i, vert - (toCenter * (radius + SPLINE_OFFSET)));
                    throw;
                }
                spriteShapeController.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
                var lt = spriteShapeController.spline.GetLeftTangent(i);
                var newlt = Vector2.Perpendicular(toCenter) * lt.magnitude;
                var newrt = -newlt;
                spriteShapeController.spline.SetRightTangent(i, newrt);
                spriteShapeController.spline.SetLeftTangent(i, newlt);
            }
        }
    }
}