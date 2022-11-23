using System.Collections;
using UnityEngine;
using Rand = UnityEngine.Random;

namespace Utilities
{
    [CreateAssetMenu(fileName = "New Destruction", menuName = "Steam Eagles/Game FX")]
    public class GameFX : ScriptableObject
    {
        [SerializeField] private SpawnedEffect[] effects;


        [System.Serializable]
        public class SpawnedEffect
        {
            public string effectName = "New Effect";

            
            public GameObject fxPrefab;

            [Tooltip("if true, the effect will be spawned as a child of the object it is spawned on. " +
                     " Otherwise it will be spawned at the position of the object it is spawned on")]
            public bool spawnAsChild;

            public float scalePosition = 1;
            public ParticleSystem.MinMaxCurve xOffset = new ParticleSystem.MinMaxCurve() {
                mode = ParticleSystemCurveMode.Constant,
                constant = 0,
                constantMin = -1,
                constantMax = 1,
                curve = AnimationCurve.Linear(0,-1,1,1),
                curveMin = AnimationCurve.Linear(0,0,-1,-1),
                curveMax = AnimationCurve.Linear(0,0,1,1),
                curveMultiplier = 1,
            };
            public ParticleSystem.MinMaxCurve yOffset = new ParticleSystem.MinMaxCurve() {
                mode = ParticleSystemCurveMode.Constant,
                constant = 0,
                constantMin = -1,
                constantMax = 1,
                curve = AnimationCurve.Linear(0,-1,1,1),
                curveMin = AnimationCurve.Linear(0,0,-1,-1),
                curveMax = AnimationCurve.Linear(0,0,1,1),
                curveMultiplier = 1,
            };
            public ParticleSystem.MinMaxCurve zOffset = new ParticleSystem.MinMaxCurve() {
                mode = ParticleSystemCurveMode.Constant,
                constant = 0,
                constantMin = -1,
                constantMax = 1,
                curve = AnimationCurve.Linear(0,-1,1,1),
                curveMin = AnimationCurve.Linear(0,0,-1,-1),
                curveMax = AnimationCurve.Linear(0,0,1,1),
                curveMultiplier = 1,
            };
            [Range(-180, 180)] public float fixedRotationOffset= 0.0f;
            public ParticleSystem.MinMaxCurve rotationOffset = new ParticleSystem.MinMaxCurve() {
                mode = ParticleSystemCurveMode.Constant,
                constant = 0,
                constantMin = -15,
                constantMax = 15,
                curve = AnimationCurve.Linear(0,0,1,1),
                curveMultiplier = 1,
                curveMin = AnimationCurve.Linear(0,0,0.9f,0.9f),
                curveMax = AnimationCurve.Linear(0.1f,0.1f,1,1)
            };

        [Tooltip("If true, the spawned effect will destroyed after this amount of time")]
            [Min(0)] public float autoDestroyDelay;
            
            public void Spawn(Transform fxCaller)
            {
                var fxInstance = spawnAsChild ? Instantiate(fxPrefab, fxCaller.transform) : Instantiate(fxPrefab);
                fxInstance.transform.position = fxCaller.transform.position + GetPositionOffset();
                fxInstance.transform.rotation = fxCaller.transform.rotation * GetRotationOffset();
                if (autoDestroyDelay > 0)
                {
                    GameFxManager.Instance.StartCoroutine(AutoDestroy(fxInstance, autoDestroyDelay));
                }
            }

            private Quaternion GetRotationOffset()
            {
                var z = GetPositionOffset(rotationOffset);
                
                return Quaternion.Euler(0,0,z + fixedRotationOffset);
            }
            private Vector3 GetPositionOffset()
            {
                var x = GetPositionOffset(xOffset);
                var y = GetPositionOffset(yOffset);
                var z = GetPositionOffset(zOffset);
                return new Vector3(x, y, 1);
            }
            
            private float GetPositionOffset(ParticleSystem.MinMaxCurve minMaxCurve)
            {
                var randValue = Rand.value;
                switch (minMaxCurve.mode)
                {
                    case ParticleSystemCurveMode.Constant:
                        return minMaxCurve.constant;
                    case ParticleSystemCurveMode.TwoConstants:
                        return Rand.Range(minMaxCurve.constantMin, minMaxCurve.constantMax);
                    case ParticleSystemCurveMode.Curve:
                        return minMaxCurve.curve.Evaluate(randValue);
                    case ParticleSystemCurveMode.TwoCurves:
                        return Rand.Range(minMaxCurve.curveMin.Evaluate(randValue), minMaxCurve.curveMax.Evaluate(randValue));
                    default:
                        return 0;
                }
            }

            private IEnumerator AutoDestroy(GameObject instance, float delay)
            {
                yield return new WaitForSeconds(delay);
                if(instance) Destroy(instance);
            }
        }

        public void SpawnEffectFrom(Vector3 spawnPosition, float rotation)
        {
            var spawnPoint = new GameObject($"{name} (FX Instance)");
            spawnPoint.transform.SetParent(GameFxManager.Instance.transform);
            spawnPoint.transform.position = spawnPosition;
            spawnPoint.transform.rotation = Quaternion.Euler(0,0,rotation);
            SpawnEffectFrom(spawnPoint.transform);
        }
        public void SpawnEffectFrom(Transform spriteRendererTransform)
        {
            foreach (var spawnedEffect in effects)
            {
                spawnedEffect.Spawn(spriteRendererTransform);
            }
        }
    }
}