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
         private GlobalPCInfo _globalPCInfo;
         public override bool DestroyOnLoad => false;


         public int PlayerCount => _globalPCInfo.PCCount;

         public PCInstance GetPC(int player) => _globalPCInfo.GetInstance(player);
         public IPCTracker GetPCTracker(int player) => _globalPCInfo.GetTracker(player);

         [Inject]
         public void InjectMe(GlobalPCInfo globalPCInfo)
         {
             _globalPCInfo = globalPCInfo;
         }
     }
}