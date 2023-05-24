using CoreLib.Signals;
using UnityEngine;
using Zenject;

namespace Weather.Storms
{
    /// <summary>
    /// because only PCStormSubjects require rendering the entry point for these storm subjects should
    /// be separate from the actual character entity's subject.  This allows us to easily separate the
    /// view layers from all other game logic
    /// </summary>
    public class PCStormSubjectManager : IInitializable, IGlobalStormSystem
    {
        private readonly GlobalPCInfo _pcInfo;


        public PCStormSubjectManager(GlobalPCInfo pcInfo)
        {
            _pcInfo = pcInfo;
        }

        public void Initialize()
        {
            Debug.Log("Initialized PC Storm Subject Manager");
        }
    }
}