using System;
using Power.Steam.Network;
using UnityEngine;
using Zenject;

namespace Buildables
{
    public class HypergasGenerator : MonoBehaviour
    {
        private INetwork _steamNetwork;

        [Inject]
        public void InjectSteamNetwork(INetwork steamNetwork)
        {
            this._steamNetwork = steamNetwork;
        }

        private void Update()
        {
            if (_steamNetwork == null)
            {
                Debug.LogError($"Steam network was not injected into {name}",this);
            }
        }
    }
}