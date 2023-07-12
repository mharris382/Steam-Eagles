using System.Collections.Generic;
using System.Diagnostics;
using CoreLib.Structures;
using UnityEngine;

namespace AI.Bots
{
    public class BotWhitelist : IWhiteList
    {
        HashSet<Transform> _whiteList = new HashSet<Transform>();
        public void Whitelist(Transform transform)
        {
            if (_whiteList.Contains(transform) == false) return;
            _whiteList.Add(transform);
        }
        
        public void UnWhitelist(Transform transform)
        {
            if (_whiteList.Contains(transform) == false) return;
            _whiteList.Remove(transform);
        }
        
        public bool IsWhitelisted(Target target)
        {
            return target.transform != null && _whiteList.Contains(target.transform);
        }
    }
}