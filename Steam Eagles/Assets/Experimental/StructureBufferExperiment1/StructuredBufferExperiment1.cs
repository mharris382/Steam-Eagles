using System;
using System.Collections;
using System.Collections.Generic;
using Experimental.ComputeShaderExperiment2;
using UnityEngine;

namespace Experimental.StructureBufferExperiment1
{
    public class StructuredBufferExperiment1 : Experimenter
    {
        public struct Cell
        {
            public Color color;
            public Vector3 position;
        }

        public float scrollSpeed = 10;

        private Cell[] _data1;
        private Cell[] _data2;
        
        public GameObject prefab;
        private ICell[] cellObjects;

        public ComputeShader computeShader;

        public float mouseRadius = 1;
        public int gridSize = 100;
        private int cnt = 0;

        void CreateGrid()
        {
            _data1 = new Cell[gridSize * gridSize];
            _data2 = new Cell[gridSize * gridSize];
            cellObjects = new ICell[gridSize * gridSize];
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    _data1[x * gridSize + y] =_data2[x * gridSize + y] =  new Cell()
                    {
                        color = UnityEngine.Random.ColorHSV(),
                        position = new Vector3(x, y)
                    };
                    
                    var go = Instantiate(prefab);
                    go.name = $"{prefab.name}_({x},{y})";
                    go.transform.rotation = prefab.transform.rotation;
                    var cellObj =go.GetComponent<ICell>();
                    cellObj.Position = new Vector3(x, y);
                    cellObj.Color = _data1[x * gridSize + y].color;
                    cellObjects[x * gridSize + y] = cellObj;
                }
            }
        }

        void CopyDataToObjects()
        {
            for (int i = 0; i < _data1.Length; i++)
            {
                cellObjects[i].Color = _data1[i].color;
            }
        }


        private Vector3 lastMP;

        private void Update()
        {
            int mouseButton = 1;
            if (Input.GetMouseButton(mouseButton))
            {
                lastMP = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }

            if (Mathf.Abs(Input.mouseScrollDelta.y) > 0.1f)
            {
                mouseRadius += Input.mouseScrollDelta.y * scrollSpeed;
                mouseRadius = Mathf.Clamp(mouseRadius, 1, 100);
            }

            if (Input.GetMouseButtonUp(mouseButton))
            {
                RandomizeColors();
            }
        }

     

        void RandomizeColors()
        {
            StopAllCoroutines();
            int colorSize = sizeof(float) * 4;
            int positionSize = sizeof(float) * 3;
            int totalSize = colorSize + positionSize;
            
            var data = cnt % 2== 0 ? _data1 : _data2;
            ComputeBuffer cellBuffer = new ComputeBuffer(data.Length, totalSize);
            cellBuffer.SetData(data);
            computeShader.SetBuffer(0, "cells", cellBuffer);
            computeShader.SetFloat("gridSize", gridSize);
            computeShader.SetFloat("mouseRadius", mouseRadius);
            computeShader.SetFloat("time", Time.realtimeSinceStartup);
            var mp = Application.isFocused ? lastMP : Camera.main.ViewportToWorldPoint(new Vector3(0.5f,0.5f));
            
            computeShader.SetVector("mousePosition", new Vector4(mp.x, mp.y, mp.z, 0f));
            computeShader.Dispatch(0, _data1.Length / 10, 1,1);
            
            
            cellBuffer.GetData(data);
            for (int i = 0; i < cellObjects.Length; i++)
            {
                Cell cell = data[i];
                ICell obj = cellObjects[i];
                obj.Color = cell.color;
            }
            cellBuffer.Dispose();
            cnt++;
        }

        void RandomizeColorsWithLerp()
        {
            RandomizeColors();
            StartCoroutine(LerpColors(cnt));
        }
        IEnumerator LerpColors(int cnt, float duration = 1)
        {
            var fromData = cnt % 2 == 0 ? _data1 : _data2;
            var toData = cnt % 2 == 0 ? _data2 : _data1;
            for (int i = 0; i < cellObjects.Length; i++)
            {
                cellObjects[i].Color = fromData[i].color;
            }
            for (float t = 0; t <= 1; t += Time.deltaTime / duration)
            {
                for (int i = 0; i < cellObjects.Length; i++)
                {
                    cellObjects[i].Color = Color.Lerp(fromData[i].color, toData[i].color, t);
                }
                yield return null;
            }
        }
        void ExecuteAndExtractBuffer(Action<Cell> foreachCell)
        {

            int stide = sizeof(float) * 4;
            ComputeBuffer buffer = new ComputeBuffer(_data1.Length, stide);
            buffer.SetData(_data1);
            computeShader.SetBuffer(0, "cells", buffer);
            
        }

        protected override IEnumerable<(string, Action)> GetButtonActions()
        {
            yield return ("RandomizeColors", RandomizeColors);
            yield return ("Randomize Colors Animate", RandomizeColorsWithLerp);
            yield return ("Reset Colors", ResetColors);
            //yield return ("CopyDataToObjects", CopyDataToObjects);
        }

        void ResetColors()
        {
            for (int i = 0; i < _data1.Length; i++)
            {
                var cell = _data1[i];
                cell.color = UnityEngine.Random.ColorHSV();
                _data1[i] = _data2[i] = cell;
            }
        }

        protected override void Init()
        {
            CreateGrid();
        }
    }
}