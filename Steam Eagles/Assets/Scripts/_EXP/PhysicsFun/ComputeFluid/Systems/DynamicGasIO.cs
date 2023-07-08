using System;
using System.Collections;
using UnityEngine;
using Zenject;
using UniRx;

namespace _EXP.PhysicsFun.ComputeFluid
{
    public class DynamicGasIO : IInitializable, IDisposable
    {
        private readonly DynamicIObjects _iObjects;
        private readonly CoroutineCaller _coroutineCaller;
        private readonly GasTexture _gasTexture;
        private RoomGasSimConfig _config;
        private CompositeDisposable _cd = new();
        private DynamicIOData[] _dynamicIOData;
        private DynamicIOObject[] _dynamicIOObjects;
        private Coroutine _updateCoroutine;
        public DynamicGasIO(RoomGasSimConfig config, DynamicIObjects iObjects, CoroutineCaller coroutineCaller, GasTexture gasTexture)
        {
            _config = config;
            _iObjects = iObjects;
            _coroutineCaller = coroutineCaller;
            _gasTexture = gasTexture;
        }
        public void Initialize()
        {
            _updateCoroutine = _coroutineCaller.StartCoroutine(DoUpdateRoutine());

            _iObjects.OnValueAdded.Subscribe(OnAdded).AddTo(_cd);
            _iObjects.OnValueRemoved.Subscribe(OnRemoved).AddTo(_cd);

            void OnAdded(DynamicIOObject obj) => RebuildData();
            void OnRemoved(DynamicIOObject obj) => RebuildData();
            RebuildData();
        }

        void RebuildData()
        {
            var cnt = _iObjects.ListCount;
            if (cnt == 0)
            {
                _dynamicIOData = null;
                _dynamicIOObjects = null;
                return;
            }
            _dynamicIOData = new DynamicIOData[cnt];
            _dynamicIOObjects = new DynamicIOObject[cnt];
            for (int i = 0; i < cnt; i++)
            {
                _dynamicIOObjects[i] = _iObjects[i];
                _dynamicIOData[i] = _iObjects[i].GetDynamicIOData();
            }
        }
        IEnumerator DoUpdateRoutine()
        {
            while (true)
            {
                if (_dynamicIOData == null || _dynamicIOObjects == null)
                {
                    RebuildData();
                    continue;
                }
                if(_dynamicIOData.Length == _dynamicIOObjects.Length)
                    DynamicIO();
                yield return new WaitForSeconds(_config.dynamicIORate);
            }
        }


        void ObjectToData()
        {
            for (int i = 0; i < _dynamicIOData.Length; i++)
            {
                _dynamicIOData[i] = _dynamicIOObjects[i].GetDynamicIOData();
            }
        }

        void DataToObjects()
        {
            for (int i = 0; i < _dynamicIOData.Length; i++)
            {
                _dynamicIOObjects[i].SetDynamicIOData(_dynamicIOData[i]);
            }
        }
        private void DynamicIO()
        {
            ObjectToData();
            DynamicGasIOCompute.ExecuteDynamicIO(_gasTexture.RenderTexture, ref _dynamicIOData);
            DataToObjects();
        }

        public void Dispose()
        {
            _cd.Dispose();
            if(_coroutineCaller != null && _updateCoroutine != null)
                _coroutineCaller.StopCoroutine(_updateCoroutine);
        }
    }
}