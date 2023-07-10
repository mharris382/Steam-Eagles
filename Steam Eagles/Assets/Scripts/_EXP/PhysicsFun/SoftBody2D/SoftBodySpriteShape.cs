﻿using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.U2D;

namespace PhysicsFun.SoftBody2D
{
    [ExecuteAlways]
    public class SoftBodySpriteShape : MonoBehaviour
    {
        private const float SPLINE_OFFSET = .5f;
        public SpriteShapeController spriteShapeController;
      


        public bool checkForBreaks = true;
        private CircleCollider2D _circle;

        bool HasResources()
        {
            return spriteShapeController != null;
        }


        private void Awake()
        {
            if (!HasResources()) return;
            UpdateVertices();
            _circle = GetComponent<CircleCollider2D>();
        }

        void CheckForBreaks()
        {
            
        }
        
        private void Update()
        {
            if (!HasResources()) return;
            UpdateVertices();
        }

        
        [Button]
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
                var radius = child.GetComponent<CircleCollider2D>().radius;
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