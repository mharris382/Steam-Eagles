using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Utilities;

namespace PhysicsFun
{
    public class DamageOverTime : MonoBehaviour
    {
        [SerializeField] private int damage = 1;
        [SerializeField] private float damageInterval = 1;
        [SerializeField] private GameFX damageFx;
        private Dictionary<GameObject, Coroutine> _damageOverTimeCoroutines = new Dictionary<GameObject, Coroutine>();

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.TryGetComponent<Health>(out var health) && !_damageOverTimeCoroutines.ContainsKey(col.gameObject))
            {
                _damageOverTimeCoroutines.Add(col.gameObject, StartCoroutine(DamageRoutine(health)));
            }   
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_damageOverTimeCoroutines.ContainsKey(other.gameObject))
            {
                StopCoroutine(_damageOverTimeCoroutines[other.gameObject]);
                _damageOverTimeCoroutines.Remove(other.gameObject);
            }   
        }


        IEnumerator DamageRoutine(Health target)
        {
            while (true)
            {
                target.CurrentHealth -= damage;
                if(damageFx != null) damageFx.SpawnEffectFrom(target.transform);
                yield return new WaitForSeconds(damageInterval);
            }
        }
    }
}