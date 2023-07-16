using System;
using System.Collections;
using System.Collections.Generic;
using Buildings;
using Buildings.Rooms;
using UnityEngine;

namespace _EXP.PhysicsFun.ComputeFluid
{
    public class DynamicFluidIO : MonoBehaviour
    {
        public float rate = 0.25f;


        private Building _building;
        public Building Building => _building ? _building : _building = GetComponent<Building>();

        private GasTexture _gasTexture;
        public GasTexture GasTexture => _gasTexture ? _gasTexture : _gasTexture = GetComponent<GasTexture>();

        private RoomSimTextures _simTextures;
        public RoomSimTextures SimTextures => _simTextures ? _simTextures : _simTextures = GetComponent<RoomSimTextures>();
        private Room _room;
        public Room Room => _room ? _room : _room = GetComponentInParent<Room>();

        public List<DynamicIOObject> dynamicIOObject;

        private DynamicIOObject[] _ioObjects;
        private DynamicIOData[] _data;

        private void OnEnable()
        {
            ValidateArrays();
            //StartCoroutine(nameof(UpdateDynamicIO));
        }

        private Vector2Int SizePerCell => new Vector2Int(GasTexture.Resolution, GasTexture.Resolution);

        void ValidateArrays()
        {
            if (_ioObjects == null || _data == null || _ioObjects.Length != _data.Length || _ioObjects.Length != dynamicIOObject.Count)
            {
                int cnt = dynamicIOObject.Count;
                _ioObjects = new DynamicIOObject[cnt];
                _data = new DynamicIOData[cnt];
                for (int i = 0; i < cnt; i++)
                {
                    _ioObjects[i] = dynamicIOObject[i];
                    _data[i] = dynamicIOObject[i].GetDynamicIOData();
                }
            }   
        }

        void ObjectsToData()
        {
            ValidateArrays();
            for (int i = 0; i < _ioObjects.Length; i++)
            {
                _data[i] = _ioObjects[i].GetDynamicIOData();
            }
        }

        void DataToObjects()
        {
            ValidateArrays();
            for (int i = 0; i < _ioObjects.Length; i++)
            {
                var data = _data[i];
                if(data.deltaOut != 0) _ioObjects[i].SetDynamicIOData(data);
            }
        }

        public void DispatchDynamicIO()
        {
            ObjectsToData();
            DynamicGasIOCompute.ExecuteDynamicIO(GasTexture.RenderTexture, GasTexture.Velocity, ref _data);
            DataToObjects();
        }
        
        // IEnumerator UpdateDynamicIO()
        // {
        //     GasTexture.ResetTexture();
        //     SimTextures.Init();
        //     while (enabled)
        //     {
        //         DispatchDynamicIO();
        //         yield return new WaitForSeconds(rate);
        //     }
        // }
    }
}