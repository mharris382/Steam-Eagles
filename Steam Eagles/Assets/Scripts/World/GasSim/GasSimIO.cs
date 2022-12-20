using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using World.GasSim;
#endif
namespace GasSim
{
    
    public class GasSimIO : MonoBehaviour
    {
        [SerializeField]
        private GasSimParticleSystem gasSim;
        public GasSimParticleSystem GasSim =>
            gasSim == null ? (gasSim = GetComponent<GasSimParticleSystem>()) : gasSim;

        public List<GameObject> sources;
        public List<GameObject> sinks;


        private List<GasIO> _blobs;
        
        
        private void Awake()
        {
            gasSim = GetComponent<GasSimParticleSystem>();
        }

        private void Start()
        {
            GasSource[] sources = GetComponentsInChildren<GasSource>();
            HashSet<GasSource> _registered = new HashSet<GasSource>();
            foreach (var gasSource in sources)
            {
                if (_registered.Contains(gasSource)) continue;
                if (gasSource is IGasSink)
                {
                    GasSim.AddGasSinkToSimulation(gasSource as IGasSink);
                }
                else
                {
                    GasSim.AddGasSourceToSimulation(gasSource);
                }

                _registered.Add(gasSource);
            }
            foreach (var source in sources.SelectMany(t => t.GetComponentsInChildren<GasSource>()))
            {
                if (_registered.Contains(source)) continue;
                GasSim.AddGasSourceToSimulation(source);
                _registered.Add(source);
            }

            foreach (var gasSink in sinks.SelectMany(t => t.GetComponentsInChildren<GasSink>()))
            {
                if (_registered.Contains(gasSink)) continue;
                GasSim.AddGasSinkToSimulation(gasSink);
                _registered.Add(gasSink);
            }
        }

        private void OnDestroy()
        {
            
            foreach (var source in sources.SelectMany(t => t.GetComponentsInChildren<GasSource>().Where(t => !(t is GasSink))))
            {
                GasSim.RemoveGasSourceFromSimulation(source);
            }
            
            foreach (var gasSink in sinks.SelectMany(t => t.GetComponentsInChildren<GasSink>()))
            {
                GasSim.RemoveGasSinkFromSimulation(gasSink);
            }
        }

        public void ApplyIOToPressure(IPressureGrid simGrid)
        {
            
        }
    }
    
    // #if UNITY_EDITOR
    // [CustomEditor(typeof(GasSimIO))]
    // public class GasSimIOEditor : Editor
    // {
    //     public override void OnInspectorGUI()
    //     {
    //         var simIO = target as GasSimIO;
    //         simIO.sources.RemoveAll(t => t == null);
    //         simIO.sinks.RemoveAll(t => t == null);
    //         var gasSources = simIO.sources.SelectMany(t => t.GetComponentsInChildren<GasSource>()).ToArray();
    //         var gasSinks = simIO.sinks.SelectMany(t => t.GetComponentsInChildren<GasSink>()).ToArray();
    //         string labelSinks = $"<b>{gasSinks.Length} Sinks</b>";
    //         string labelSource = $"<b>{gasSources.Length} Sources</b>";
    //         base.OnInspectorGUI();
    //         
    //         GUILayout.Box($"{labelSource}\n{labelSinks}", EditorStyles.helpBox, GUILayout.ExpandHeight(false), GUILayout.ExpandHeight(true));
    //     }
    // }
    // #endif
}