using System;
using DG.DemiEditor;
using UnityEngine;
using Zenject;

namespace Weather.Storms
{
    public class StormDrawer : MonoBehaviour
    {
        private Color[] _randomColors;
        private StormRegistry _registry;


        private void Awake()
        {
            _randomColors = new Color[100];
            for (int i = 0; i < 100; i++)
            {
                _randomColors[i] = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            }
        }


        [Inject]
        public void Assign(StormRegistry registry)
        {
            this._registry = registry;
        }


        private void OnDrawGizmos()
        {
            if (_registry == null) return;
            for (int i = 0; i < _registry.Count; i++)
            {
                var storm = _registry.GetStorm(i);
                DrawStormGizmos(storm, i);
            }
        }

        private void DrawStormGizmos(Storm storm, int index)
        {
            var color = _randomColors[index];
            Gizmos.color = color;
            var bounds = storm.InnerBoundsWs;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            Gizmos.color = color.SetAlpha(0.5f);
            bounds = storm.OuterBoundsWs;
            Gizmos.DrawCube(bounds.center, bounds.size);
        }
    }
}