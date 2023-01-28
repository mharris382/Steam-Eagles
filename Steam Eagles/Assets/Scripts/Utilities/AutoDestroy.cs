using System;
using System.Collections;
using UnityEngine;

namespace Utilities
{
    public class AutoDestroy : MonoBehaviour
    {
        public float delay = 5;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }
    }
}