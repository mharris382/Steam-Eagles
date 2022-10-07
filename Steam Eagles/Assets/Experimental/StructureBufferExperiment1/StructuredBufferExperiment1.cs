using System;
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
        }

        private Cell[] _data;
        public GameObject prefab;
        private ICell[] cellObjects;

        public ComputeShader computeShader;

        public int gridSize = 100;

        void CreateGrid()
        {
            _data = new Cell[gridSize * gridSize];
            cellObjects = new ICell[gridSize * gridSize];
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    _data[x * gridSize + y] = new Cell()
                    {
                        color = UnityEngine.Random.ColorHSV()
                    };
                    var go = Instantiate(prefab);
                    go.name = $"{prefab.name}_({x},{y})";
                    go.transform.rotation = prefab.transform.rotation;
                    go.GetComponent<ICell>().Position = new Vector3(x, y);
                    cellObjects[x * gridSize + y] = go.GetComponent<ICell>();
                }
            }
        }

        void CopyDataToObjects()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                cellObjects[i].Color = _data[i].color;
            }
        }


        void RandomizeColors()
        {
            int colorSize = sizeof(float) * 4;
            
            ComputeBuffer cellBuffer = new ComputeBuffer(_data.Length, colorSize);
            cellBuffer.SetData(_data);
            computeShader.SetBuffer(0, "cells", cellBuffer);
            computeShader.SetFloat("gridSize", gridSize);
            computeShader.Dispatch(0, _data.Length / 10, 1,1);
            
            
            cellBuffer.GetData(_data);
            for (int i = 0; i < cellObjects.Length; i++)
            {
                Cell cell = _data[i];
                ICell obj = cellObjects[i];
                obj.Color = cell.color;
            }
            cellBuffer.Dispose();
            
        }

        protected override IEnumerable<(string, Action)> GetButtonActions()
        {
            yield return ("RandomizeColors", RandomizeColors);
            //yield return ("CopyDataToObjects", CopyDataToObjects);
        }

        protected override void Init()
        {
            CreateGrid();
        }
    }
}