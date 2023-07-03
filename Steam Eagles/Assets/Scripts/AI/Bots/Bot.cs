using Sirenix.OdinInspector;
using UnityEngine;

namespace AI.Bots
{
    public class Bot : MonoBehaviour
    {
        [Required] public Transform pivot;
        [Required] public GameObject weapon;

        private float _targetAngle;
        private float _currentAngle;
        private float _angleVelocity;


        public bool IsShooting
        {
            get => weapon.activeSelf;
            set => weapon.SetActive(value);
        }

        public float CurrentAngle
        {
            get => _currentAngle;
            set => _currentAngle = value;
        }
        
        public void RotateTowards(Vector3 position, float speed)
        {
            var dir =position - pivot.transform.position;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            _targetAngle = angle;
            _currentAngle = Mathf.SmoothDampAngle(_currentAngle, _targetAngle, ref _angleVelocity, speed);
            pivot.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}