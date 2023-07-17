using System;
using Sirenix.OdinInspector;
using UnityEngine.VFX;
using Zenject;

namespace _EXP.PhysicsFun.ComputeFluid.Engine2
{
    [Serializable]
    public class SimEffect : IInitializable, IDisposable
    {
        
        [Required, ValidateInput(nameof(IsVEffectValid))] public VisualEffect effect;
        [FoldoutGroup("Parameters"), ValidateInput(nameof(ValidateParamNameTex))] public string textureParameter = "Texture";
        [FoldoutGroup("Parameters"), ValidateInput(nameof(ValidateParamNameV3))] public string sizeParameter = "BoundsSize";
        [FoldoutGroup("Parameters"), ValidateInput(nameof(ValidateParamNameV3))] public string centerParameter = "BoundsCenter";
        [FoldoutGroup("Parameters"), ValidateInput(nameof(ValidateParamNameInt))] public string resolutionParameter = "Resolution";

        [Inject]
        void Install(VisualEffect ve)
        {
            effect ??= ve;
        }
        public VisualEffect VisualEffect => effect;

        public bool Initialized { get; private set; }

        #region [Validation]

        public bool IsEffectValid(ref string error)
        {

            if (!IsVEffectValid(effect))
            {
                error = "Effect is null";
                return false;
            }

            bool res = true;
            if (!ValidateParamNameTex(textureParameter) )
            {
                error = $"Texture parameter is invalid: no texture with name {textureParameter} exists in the effect";
                return false;
            }
            
            if (!ValidateParamNameV3(sizeParameter))
            {
                error = $"Size parameter is invalid: no Vector3 with name {sizeParameter} exists in the effect";
                return false;
            }

            if (!ValidateParamNameV3(centerParameter))
            {
                error = $"Center parameter is invalid: no Vector3 with name {centerParameter} exists in the effect";
                return false;
            }

            if (!ValidateParamNameInt(resolutionParameter))
            {
                error = $"resolution parameter is invalid: no int with name {resolutionParameter} exists in the effect";
                return false;
            }
            return true;
        }
        bool IsVEffectValid(VisualEffect e)
        {
            if (e == null) return false;
            return true;
        }

        bool ValidateParamNameTex(string p)
        {
            if (string.IsNullOrEmpty(p))
            {
                return false;
            }

            try
            {
                var v = VisualEffect.GetTexture(p);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        bool ValidateParamNameV3(string p)
        {
            if (string.IsNullOrEmpty(p))
            {
                return false;
            }

            try
            {
                var v = VisualEffect.GetVector3(p);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        bool ValidateParamNameInt(string p)
        {
            if (string.IsNullOrEmpty(p))
            {
                return false;
            }

            try
            {
                var v = VisualEffect.GetInt(p);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        #endregion

        private int _textureParamId;

        public void Initialize()
        {
            Initialized = true;
        }

        public void Dispose()
        {
            if(effect)
                effect.Stop();
        }
    }
}