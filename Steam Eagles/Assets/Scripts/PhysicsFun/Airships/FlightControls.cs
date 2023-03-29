using System;
using CoreLib.Interactions;
using UnityEngine;

namespace PhysicsFun.Airships
{
    public class FlightControls : MonoBehaviour
    {
        public AirshipControls controls;
        public AirshipFlightController flightController;
        public Rigidbody2D balloonBody;
        public Transform bodyPropeller;
        public Transform balloonPropeller;

        
        
        public float powerPerNotch = 100000;
        public float maxHorizontalAngle = 15;
        
        private void Awake()
        {
            if (flightController.heightTarget == null)
            {
                flightController.heightTarget = new GameObject($"Flight Controller for {controls}").transform;
            }
            flightController.heightTarget.position = flightController.airshipBody.position;
            Vector3 maxAngle = Quaternion.Euler(0, 0, 15) * Vector2.right;
            Vector3 minAngle = Quaternion.Euler(0, 0, -15) * Vector2.right;
            
        }

        private void Update()
        {
            if (controls.CurrentPilot == null)
            {
                AutoPilot();
                return;
            }

            Debug.Log($"Airship is piloted by {controls.CurrentPilot}!");
            float desiredForce = controls.CurrentPilot.XInput * (powerPerNotch * controls.thrusterPower) * Time.deltaTime;
            Vector2 force = new Vector2(desiredForce, 0);
            var balloonPropPos = balloonPropeller.position;
            var bodyPropPos = bodyPropeller.position;
            if (controls.CurrentPilot.YInput > 0)
            {
                //apply non rotated force to balloon
                ApplyForceToBalloonBody();
                //apply rotated force to ship
                force = RotateForceDirection(force);
                Debug.DrawRay(bodyPropPos, force, Color.red);
                ApplyForceToShipBody();
            }
            else if (controls.CurrentPilot.YInput < 0)
            {
                //apply rotated force to ship
                //apply rotated force to balloon
                force = RotateForceDirection(force);
                Debug.DrawRay(balloonPropPos, force, Color.red);
                ApplyForceToBalloonBody();
                ApplyForceToShipBody();
            }
            else
            {
                ApplyForceToBalloonBody();
                ApplyForceToShipBody();
            }
            

            void ApplyForceToBalloonBody()
            {
                balloonBody.AddForceAtPosition(force, balloonPropPos, ForceMode2D.Force);
            }

            void ApplyForceToShipBody()
            {
                flightController.airshipBody.AddForceAtPosition(force, bodyPropPos, ForceMode2D.Force);
            }
        }

        Vector2 RotateForceDirection(Vector2 force)
        {
            var t = (controls.CurrentPilot.YInput + 2) / 2f;
            return Quaternion.Euler(0, 0,  Mathf.Lerp(-maxHorizontalAngle, maxHorizontalAngle, t)) * force;
        }

        void AutoPilot()
        {
            flightController.heightTarget.position = new Vector3(0, 1000000, 0);
            Debug.Log($"Autoiloting");
        }
    }
}