using System;
using CoreLib;
using UnityEngine;

namespace Utilities
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class Gun2D : MonoBehaviour
    {
        
        private Rigidbody2D _rb;
        public Rigidbody2D rb => _rb ? _rb : _rb = GetComponent<Rigidbody2D>();

        public abstract Transform FirePoint { get; set; }

        public float AimDirection
        {
            get => rb.rotation;
            set => rb.rotation = value;
        }

        public bool drawGizmos = true;
        public Color gizmoGolor = Color.yellow;
        private void OnDrawGizmos()
        {
            if(!drawGizmos)return;
        }
    }

    //
    //
    // [RequireComponent(typeof(Rigidbody2D))]
    // [RequireComponent(typeof(ParticleSystem))]
    // public class ParticleGun2D : Gun2D
    // {
    //     private ParticleSystem _ps;
    //     public ParticleSystem ps => _ps ? _ps : _ps = GetComponent<ParticleSystem>();
    //     
    //     
    //     
    //     
    //     
    //     [Serializable]
    //     public class GunShooter
    //     {
    //         public float timeBetweenShots = 0.5f;
    //         
    //         [Header("Debug Values")] [NonSerialized] 
    //         public float timeListFired;
    //         
    //         public bool CanFire()
    //         {
    //             return Time.time - timeListFired > timeBetweenShots;
    //         }
    //         
    //         public void Fire(ParticleGun2D particleGun)
    //         {
    //             timeListFired = Time.time;
    //         }
    //     }
    //
    //     
    //     [Serializable]
    //     public class GunAim
    //     {
    //         
    //         public float aimSpeed = 1f;
    //         public float aimAngleOffset = 0f;
    //         
    //         
    //     }
    //     
    //     
    //     public void Shoot(float angle, float speed)
    //     {
    //         rb.rotation = angle;
    //         var main = ps.main;
    //         main.simulationSpeed = speed;
    //         ps.Emit(1);
    //     }
    // }
    //
    // public class ParticleGun2DTester : ParticleGun2D
    // {
    //     public Camera camera;
    //     
    //     
    // }
}