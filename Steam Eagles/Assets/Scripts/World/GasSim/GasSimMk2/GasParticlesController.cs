using UnityEngine;

namespace GasSim
{
    [RequireComponent(typeof(ParticleSystem))]
    public class GasParticlesController : MonoBehaviour
    {

        private ParticleSystem _ps;
        public ParticleSystem ParticleSystem => _ps ? _ps : _ps = GetComponent<ParticleSystem>();
        
        
        public GasGridController GridController { private get; set; }

        public bool IsValid()
        {
            return GridController != null;
        }
        
        
    }
}