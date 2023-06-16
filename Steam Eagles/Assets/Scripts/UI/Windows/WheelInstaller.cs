using System.Collections.Generic;
using Sirenix.OdinInspector;
using UI.Wheel;
using UnityEngine;
using Zenject;

namespace UI
{
    public class WheelInstaller : MonoInstaller
    {
        [SerializeField, Required, AssetsOnly] private UIWheelBuilder wheelBuilder;

        public override void InstallBindings()
        {
            Container.Bind<UIWheelBuilder>().FromComponentInNewPrefab(wheelBuilder).AsSingle().NonLazy();
        }
    }

    public interface IWheelSegmentFactory : IFactory<UIWheelSelectable, UIWheelSegment> { }
}