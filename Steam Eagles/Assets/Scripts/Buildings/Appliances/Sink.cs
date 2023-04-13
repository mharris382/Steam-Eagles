using System;
using System.Collections;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Buildings.Appliances
{
    public class Sink : BuildingAppliance
    {
        [SerializeField, Required, ChildGameObjectsOnly]
        private ParticleSystem waterParticles;
        [SerializeField, ChildGameObjectsOnly]
        private AudioSource waterSound;

        private IEnumerator Start()
        {
            while(!IsEntityInitialized)
                yield return null;
            OnApplianceOnStateChanged(IsOn);
            IsApplianceOn.Subscribe(OnApplianceOnStateChanged).AddTo(this);
        }

        void OnApplianceOnStateChanged(bool isOn)
        {
            if (isOn)
            {
                waterParticles.Play(true);
                if(waterSound != null) waterSound.Play();
            }
            else
            {
                waterParticles.Stop();
                if(waterSound != null) waterSound.Stop();
            }
        }
        
    }
}