using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.VFX;

namespace _EXP.PhysicsFun.ComputeFluid.Extras
{
    public abstract class VFXBase : SerializedMonoBehaviour
    {
        [SerializeField] VisualEffect effect;
        public VisualEffect Effect => effect ??= GetComponent<VisualEffect>();
    }
}