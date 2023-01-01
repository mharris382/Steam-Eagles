using System;
using UniRx;
using UnityEngine;

namespace PhysicsFun
{
    public class JointDisabler : MonoBehaviour
    {
        public Joint2D joint2D;


        private void Start()
        {
            if (joint2D == null) return;
            joint2D.enabled = joint2D.attachedRigidbody != null;
            joint2D
                .ObserveEveryValueChanged(t => t.attachedRigidbody)
                .Select(t => t != null).Subscribe(t => joint2D.enabled = t)
                .AddTo(this);
        }
    }
}