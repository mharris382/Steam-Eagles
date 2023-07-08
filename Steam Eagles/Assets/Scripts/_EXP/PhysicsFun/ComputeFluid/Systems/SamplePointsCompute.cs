using System;
using System.Collections;
using UnityEngine;
using Zenject;
using UniRx;

namespace _EXP.PhysicsFun.ComputeFluid
{
    public class SamplePointsCompute : IInitializable, IDisposable
    {
        private readonly GasTexture _gasTexture;
        private readonly CoroutineCaller _coroutineCaller;
        private readonly SamplePoints _samplePoints;
        private Coroutine _samplePointsRoutine;
        private CompositeDisposable _cd;

        private SamplePoint[] _samplePointsArray;
        private GasSampleData[] _sampleDatas;
        
        public SamplePointsCompute(GasTexture gasTexture, CoroutineCaller coroutineCaller, SamplePoints samplePoints)
        {
            _gasTexture = gasTexture;
            _coroutineCaller = coroutineCaller;
            _samplePoints = samplePoints;
            _cd = new();
        }

        public void Initialize()
        {
            _samplePoints.OnValueAdded.Subscribe(t => AddSamplePoint(t)).AddTo(_cd);
            _samplePoints.OnValueRemoved.Subscribe(t => RemoveSamplePoint(t)).AddTo(_cd);
            RebuildArrays();
        }

        private void AddSamplePoint(SamplePoint o)
        {
            RebuildArrays();
        }

        private void RemoveSamplePoint(SamplePoint o)
        {
            RebuildArrays();
        }

        void RebuildArrays()
        {
            int cnt = _samplePoints.ListCount;
            _samplePointsArray = new SamplePoint[cnt];
            _sampleDatas = new GasSampleData[cnt];
            for (int i = 0; i < cnt; i++)
            {
                _samplePointsArray[i] = _samplePoints[i];
            }

            if (cnt <= 0)
            {
                StopSampling();
            }
            else
            {
                StartSampling();
            }
        }

        void StartSampling()
        {
            if(_samplePointsRoutine != null) return;
            _samplePointsRoutine = _coroutineCaller.StartCoroutine(SampleRoutine());
        }

        IEnumerator SampleRoutine()
        {
            while (true)
            {
                Sample();
                yield return null;
            }
        }

        void Sample()
        {
            for (int i = 0; i < _samplePointsArray.Length; i++)
            {
                _sampleDatas[i] = _samplePointsArray[i].GetSampleData();
            }
            SimCompute.AssignGasSampler(_gasTexture.RenderTexture, ref _sampleDatas);
            for (int i = 0; i < _samplePointsArray.Length; i++)
            {
                _samplePointsArray[i].SetSampleData(_sampleDatas[i]);
            }
        }
        
        void StopSampling()
        {
            if (_samplePointsRoutine != null && _coroutineCaller != null)
            {
                _coroutineCaller.StopCoroutine(_samplePointsRoutine);
                _samplePointsRoutine = null;
            }
        }
        public void Dispose()
        {
            _cd?.Dispose();
        }
    }
}