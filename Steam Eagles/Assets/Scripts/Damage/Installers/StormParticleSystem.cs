using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class StormParticleSystem : MonoBehaviour, IStormViewLayer
{
    private ParticleSystem _particleSystem;
    private ParticleSystemRenderer _particleSystemRenderer;
    
    
    [Tooltip("Is the particle system active regardless of intensity or is it only active at specific intensities")]
    public bool alwaysActive = true;

    public Vector2Int activeIntensityRange = new Vector2Int(0, 5);
    
    
    public bool lerpEmmisionRate = true;
    private ParticleSystem.MinMaxCurve _rateMax;
    private Coroutine _emissionTransitionCoroutine;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _particleSystemRenderer=_particleSystem.GetComponent<ParticleSystemRenderer>();
        _rateMax = _particleSystem.emission.rateOverTime;
    }

    public void SetActiveIntensity(int intensity)
    {
        if (!alwaysActive)
        {
            if (intensity > activeIntensityRange.x && intensity < activeIntensityRange.y)
                _particleSystem.Play();
            else
                _particleSystem.Stop();
        }
        else if(_particleSystem.isPlaying==false)
        {
            _particleSystem.Play();
        }
    }

    public void SetVisible(bool visible)
    {
        if (_particleSystem.isPlaying)
        {
            _particleSystem.Stop();
        }
        else
        {
            _particleSystemRenderer.enabled = visible;
        }
    }

    public void StormStarting(float startDuration)
    {
        if(_emissionTransitionCoroutine != null)StopCoroutine(_emissionTransitionCoroutine);
        if (lerpEmmisionRate)
        {
            _emissionTransitionCoroutine = StartCoroutine(LerpEmissionToMax(startDuration));
        }
    }

    public void StormStopping(float stopDuration)
    {
        if(_emissionTransitionCoroutine != null)StopCoroutine(_emissionTransitionCoroutine);
        if (lerpEmmisionRate)
        {
            _emissionTransitionCoroutine = StartCoroutine(LerpEmissionToZero(stopDuration));
        }
    }
    

    IEnumerator LerpEmissionToMax(float duration)
    {
        var emissionModule = _particleSystem.emission;
        var rateOverTime = _rateMax;
        var fullRate = rateOverTime;

        Func<float, Vector2> lerpFunc = t => {
            var max =Mathf.Lerp(0, _rateMax.constantMax, t);
            var min = Mathf.Lerp(0, _rateMax.constantMin, t);
            return new Vector2(min, max);
        };
        
        yield return LerpEmission(duration, rateOverTime, emissionModule, lerpFunc);
        emissionModule.rateOverTime = _rateMax;
        _emissionTransitionCoroutine = null;
    }

    IEnumerator LerpEmissionToZero(float duration)
    {
        var em = _particleSystem.emission;
        
        var rateOverTime = _rateMax;
        var constantMax = rateOverTime.constantMax;
        var constantMin = rateOverTime.constantMin;
        
        Func<float, Vector2> lerpFunc = t => {
            var max =Mathf.Lerp(constantMax, 0, t);
            var min = Mathf.Lerp( constantMin, 0, t);
            return new Vector2(min, max);
        };
        yield return LerpEmission(duration, rateOverTime,em, lerpFunc);
        em.rateOverTime = new ParticleSystem.MinMaxCurve(0);
        _emissionTransitionCoroutine = null;
    }

    IEnumerator LerpEmission(float duration, 
        ParticleSystem.MinMaxCurve rateOverTime, 
        ParticleSystem.EmissionModule emission,
        Func<float, Vector2> lerpFunc)
    {
        for (float t = 0; t < duration; t+=Time.deltaTime/duration)
        {
            var currentMinMax = lerpFunc(t);
            rateOverTime.constantMin = currentMinMax.x;
            rateOverTime.constantMax = currentMinMax.y;
            emission.rateOverTime = rateOverTime;
            yield return null;
        }
    }
}