using System;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Buildings.Damage
{
    public class DamageOptionTester : MonoBehaviour
    {

        [InfoBox("$Info")]
        public bool test;

        private IDamageOptionProvider _damageOptionProvider;
        private Vector3Int[] _lastOptions;
        private BuildingLayers _lastOptionLayer;

        [Inject]
        public void Inject(IDamageOptionProvider damageOptionProvider)
        {
            
            this._damageOptionProvider = damageOptionProvider;
        }

        string Info
        {
            get
            {
                if (_damageOptionProvider == null)
                {
                    return "Waiting for Injection...";
                }
                var sb = new StringBuilder();
                var boundType = _damageOptionProvider.GetType().Name;
                sb.AppendLine($"Damage Provider Bound to {boundType}");
                if (_lastOptions != null)
                {
                    sb.AppendLine($"Layer Checked: {_lastOptionLayer}");
                    sb.AppendLine($"Options: {_lastOptions.Length}");
                }
                return sb.ToString();
            }
        }



        [Button, HideInEditorMode]
        public void Test(BuildingLayers buildingLayers)
        {
            var prev = _lastOptionLayer;
            if(_damageOptionProvider == null)return;
            try
            {
                this._lastOptionLayer = buildingLayers;
                this._lastOptions = _damageOptionProvider.GetOptionsForLayer(buildingLayers).ToArray();
            }
            catch (InvalidOperationException e)
            {
                Debug.LogError(e);
                this._lastOptionLayer = prev;
                _lastOptions = null;
            }
        }
    }
}