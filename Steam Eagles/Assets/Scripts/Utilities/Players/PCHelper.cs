using CoreLib;
using CoreLib.Signals;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Tools
{
    [InfoBox("this singleton is basically a hacky workaround that allows certain player systems like tools to resolve dependencies which are managed by extenject, since tools are not managed by extenject currently")]
     public class PCHelper : Singleton<PCHelper>
     {
         private PCRegistry _pcRegistry;
         public override bool DestroyOnLoad => false;


         public int PlayerCount => _pcRegistry.PCCount;

         public PCInstance GetPC(int player) => _pcRegistry.GetInstance(player);
         public IPCTracker GetPCTracker(int player) => _pcRegistry.GetTracker(player);

         [Inject]
         public void InjectMe(PCRegistry pcRegistry)
         {
             _pcRegistry = pcRegistry;
         }
     }
}