using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhysicsFun
{
    public class CounterBalanceController : MonoBehaviour
    {
        public Collider2D[] counterBalances;

        public float minDensity = 1;
        public float maxDensity = 4.9f;


        [Range(0, 1)]
        public float densityControl = 0.5f;
        public float densityControlSpeed = 0.1f;

        [ReadOnly, ProgressBar(0,1)]
        public float densityActual = 0;

        private float _targetDensity = 1;
        private float _currentDensity = 1;
        private float _currentVelocity = 0;

        private void Awake()
        {
            _targetDensity = Mathf.Lerp(minDensity, maxDensity, densityControl);
            _currentDensity = _targetDensity;
            UpdateDensities();
        }

        private void Update()
        {
            _targetDensity = Mathf.Lerp(minDensity, maxDensity, densityControl);
            _currentDensity = Mathf.SmoothDamp(_currentDensity, _targetDensity, ref _currentVelocity, densityControlSpeed);
            UpdateDensities();
            densityActual = Mathf.InverseLerp(minDensity, maxDensity, _currentDensity);
        }

        private void UpdateDensities()
        {
            foreach (var counterBalance in counterBalances)
            {
                counterBalance.density = _currentDensity;
            }
        }
    }
}