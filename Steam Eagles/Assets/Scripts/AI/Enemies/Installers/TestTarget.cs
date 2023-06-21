using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace AI.Enemies.Installers
{
    [InfoBox("Creates a dummy target to be attacked by enemies, for testing purposes. ")]
    public class TestTarget : MonoBehaviour
    {
        private TestTargets _testTargets;

        [Inject] void InjectMe(TestTargets testTargets)
        {
            this._testTargets = testTargets;
            _testTargets.AddTestTarget(transform);
        }

        private void OnEnable()
        {
            _testTargets.AddTestTarget(transform);
        }

        private void OnDisable()
        {
            _testTargets.RemoveTestTarget(transform);
        }
    }
}