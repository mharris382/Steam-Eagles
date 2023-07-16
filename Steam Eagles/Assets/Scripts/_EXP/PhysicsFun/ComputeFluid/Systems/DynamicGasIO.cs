using System;
using System.Collections;
using UnityEngine;
using Zenject;
using UniRx;

namespace _EXP.PhysicsFun.ComputeFluid
{
    public class DynamicGasIO : IInitializable, IDisposable
    {
        public DynamicForceInputObjects ForceInputObjects { get; }
        private readonly DynamicIObjects _iObjects;
        private readonly CoroutineCaller _coroutineCaller;
        private readonly GasTexture _gasTexture;
        private RoomGasSimConfig _config;
        private CompositeDisposable _cd = new();
        
        private DynamicIOData[] _dynamicIOData;
        private DynamicIOObject[] _dynamicIOObjects;

        private DynamicForceInputObject[] _forceInputObjects;
        private DynamicForceInput[] _dynamicForceInputData;
        
        private Coroutine _updateCoroutine;
        public DynamicGasIO(RoomGasSimConfig config, DynamicIObjects iObjects, DynamicForceInputObjects forceInputObjects, CoroutineCaller coroutineCaller, GasTexture gasTexture)
        {
            ForceInputObjects = forceInputObjects;
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

        void RebuildForceData()
        {
            _forceInputObjects = null;
            _dynamicForceInputData = null;
            int cnt = ForceInputObjects.ListCount;
            if(cnt == 0) return;
            _forceInputObjects = new DynamicForceInputObject[cnt];
            _dynamicForceInputData = new DynamicForceInput[cnt];
            for (int i = 0; i < cnt; i++)
            {

                _dynamicForceInputData[i] = (_forceInputObjects[i] = ForceInputObjects[i]).GetForceData();
            }
        }

        void RebuildIOData()
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
        void RebuildData()
        {
           RebuildForceData();
           RebuildIOData();
        }
        
        IEnumerator DoUpdateRoutine()
        {
            float timeLastUpdate = Time.time;
            while (true)
            {
                timeLastUpdate = Time.time;
                UpdateIO();
                UpdateForceInput();
                float deltaTime = Time.time - timeLastUpdate;
                float waitTime = _config.dynamicIORate - deltaTime;
                if (waitTime > 0) yield return new WaitForSeconds(waitTime);
            }


            void UpdateIO()
            {
                if (_dynamicIOData == null || _dynamicIOObjects == null) RebuildIOData();
                if (_dynamicIOData == null) return;

                if (_dynamicIOData.Length == _dynamicIOObjects.Length)
                {
                    DynamicIO();
                }
            }

            void UpdateForceInput()
            {
                if(_dynamicForceInputData == null || _forceInputObjects == null) RebuildForceData();
                if(_dynamicForceInputData == null) return;
                if(_dynamicForceInputData.Length == _forceInputObjects.Length)
                {
                    DynamicForce();
                }
            }
        }

        private void DynamicForce()
        {
            ForceObjectsToForceData();
            DynamicGasIOCompute.ExecuteDynamicForce(_gasTexture.Velocity, _dynamicForceInputData);
            
            void ForceObjectsToForceData()
            {
                for (int i = 0; i < _dynamicForceInputData.Length; i++)
                {
                    _dynamicForceInputData[i] = _forceInputObjects[i].GetForceData();
                }            
            }
        }
        
        private void DynamicIO()
        {
            ObjectToData();
            if (_gasTexture.Velocity != null)
                DynamicGasIOCompute.ExecuteDynamicIO(_gasTexture.RenderTexture, _gasTexture.Velocity,
                    ref _dynamicIOData);
            DataToObjects();
            
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
        }

        public void Dispose()
        {
            _cd.Dispose();
            if(_coroutineCaller != null && _updateCoroutine != null)
                _coroutineCaller.StopCoroutine(_updateCoroutine);
        }
    }
}