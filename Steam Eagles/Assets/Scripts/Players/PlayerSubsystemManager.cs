using System;
using CoreLib;
using UnityEngine;

namespace Players
{
    public class PlayerSubsystemManager : Singleton<PlayerSubsystemManager>
    {
        public override bool DestroyOnLoad => true;
        private void Start()
        {
            
        }
    }
}