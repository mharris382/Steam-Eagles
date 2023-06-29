using UnityEngine;
using Zenject;

namespace PhysicsFun.Airships.Installer
{
    public class AirshipThruster : MonoBehaviour, IThrusters
    {
        public Rigidbody2D targetBody;
        
        private AirshipFlightConfig config;
        public float powerPerNotch = 100000;
        public float maxHorizontalAngle = 15;
        
        private Vector2 _inputDirection;
        private float _currentPower;
        [Inject]
        public void Inject(AirshipFlightConfig config)
        {
            this.config = config;
        }

        public void SetPower(float power)
        {
            _currentPower = power;
        }

        public void SetDirection(Vector2 direction)
        {
            _inputDirection = direction;
        }

        private void FixedUpdate()
        {
            float desiredForce = _inputDirection.x * (powerPerNotch * _currentPower) * Time.fixedDeltaTime;
            Vector2 force = new Vector2(desiredForce, 0);
            force = RotateForceDirection(force);
            Debug.DrawRay(transform.position, force, Color.red);
            ApplyForce(force);
        }

        void ApplyForce(Vector2 force)
        {
            targetBody.AddForceAtPosition(force, transform.position, ForceMode2D.Force);
        }
        Vector2 RotateForceDirection(Vector2 force)
        {
            var t = (_inputDirection.y + 2) / 2f;
            return Quaternion.Euler(0, 0,  Mathf.Lerp(-maxHorizontalAngle, maxHorizontalAngle, t)) * force;
        }
    }
}