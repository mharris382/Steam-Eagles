using System.Collections.Generic;
using ObjectLabelMapping;
using UnityEngine;
using Zenject;

public class ParameterMapInstaller : Installer<ParameterMapInstaller>
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<GlobalObjectMap>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<GlobalValueMap>().AsSingle().NonLazy();
         Container.BindInterfacesAndSelfTo<TagToSurfaceParameters>().AsSingle().NonLazy();
         Container.BindInterfacesAndSelfTo<LayerMapToSurfaceParameters>().AsSingle().NonLazy();
    }

    private class TagToSurfaceParameters : ParameterMapBase<string>
    {
        private GameObject _gameObject;
        protected override string GetParameterName() => "Surface";
    
        protected override IEnumerable<(string value, string label)> GetLabelMappings()
        {
            yield return ("Blocking Tilemap", Surfaces.FOUNDATION);
            yield return ("Pipe Tilemap", Surfaces.PIPE);
            yield return ("Solid Tilemap", Surfaces.SOLID);
            yield return ("Platform Tilemap", Surfaces.SOLID);
        }
    }
    
    private class LayerMapToSurfaceParameters : ParameterMapBase<int>
    {
        protected override string GetParameterName() => "Surface";
    
        protected override IEnumerable<(int value, string label)> GetLabelMappings()
        {
            yield return (LayerMask.NameToLayer("Solids"), Surfaces.SOLID);
            yield return (LayerMask.NameToLayer("Pipes"), Surfaces.PIPE);
            yield return (LayerMask.NameToLayer("Ground"), Surfaces.GRASS);
        }
    }
}