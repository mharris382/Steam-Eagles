using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace GasSim
{
    public class ValveFlowVisual : MonoBehaviour
    {
        public Transform valve;
        
        public float maxAngle = 360;
        public float minAngle = -360;
        
        public float durationPerIncrement = 1;
        public Ease ease = Ease.Linear;
        
        private float _targetAngle;
        private float CurrentAngle
        {
            get=> valve.localEulerAngles.z;
            set=> valve.localEulerAngles = new Vector3(valve.localEulerAngles.x, valve.localEulerAngles.y, value);
        }
        
        
        public void OnFlowChanged(int flow)
        {
            var t = Mathf.InverseLerp(-15, 15, flow);
            var lastAngle = _targetAngle;
            _targetAngle = Mathf.Lerp(minAngle, maxAngle, t);
            var duration = Mathf.Abs(_targetAngle - lastAngle) * durationPerIncrement;
            var x = valve.localRotation.x;
            var y = valve.localRotation.y;
            valve.DOKill(valve);
            valve.DOLocalRotate(new Vector3(x, y, _targetAngle), duration, RotateMode.FastBeyond360).SetEase(ease).SetAutoKill(true).Play();
        }
        
        
    }
}