using AI.Bots;
using UnityEngine;
using Zenject;

namespace a
{
    public class Whitelisted : MonoBehaviour
    {
        private BotWhitelist _whitelist;

        [Inject] private void Install(BotWhitelist whitelist)
        {
            this._whitelist = whitelist;
            if(enabled)_whitelist.Whitelist(transform);
        }
        
        private void OnEnable()
        {
            //_whitelist?.Whitelist(transform);
        }
        void OnDisable()
        {
            //_whitelist.UnWhitelist(transform);
        }
    }
}