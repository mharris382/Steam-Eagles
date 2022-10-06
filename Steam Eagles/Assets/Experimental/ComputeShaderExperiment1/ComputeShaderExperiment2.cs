using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Random = UnityEngine.Random;

public struct Cube
{
    public Vector3 position;
    public Color color;
}
public class ComputeShaderExperiment2 : MonoBehaviour
{
    public ComputeShader computeShader;
    public Material material;
    public Mesh mesh;

    public int repetitions = 1;
    public int count = 50;
    public Cube[] data;
    public List<GameObject> objects = new List<GameObject>();
    

    void CreateCubes()
    {
        objects = new List<GameObject>();
        data = new Cube[count * count];
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                CreateCube(i,j);
            }
        }
    }
    void CreateCube(int x, int y)
    {
        GameObject cube = new GameObject($"Cube {x * count}, {y * count} ", typeof(MeshFilter), typeof(MeshRenderer));
        cube.GetComponent<MeshFilter>().mesh = mesh;
        cube.GetComponent<MeshRenderer>().material = new Material(material);
        cube.transform.position = new Vector3(x, y, Random.Range(-0.1f, 0.1f));
        Color color = Random.ColorHSV();
        cube.GetComponent<MeshRenderer>().material.SetColor("_Color", color);


        objects.Add(cube);
        var cubeData = new Cube();
        cubeData.position = cube.transform.position;
        cubeData.color = color;
        data[x * count + y] = cubeData;
    }

    public void OnRandomCPU()
    {
        OnRandomize();
    }

    public void OnRandomGPU()
    {
        int colorSize = sizeof(float) * 4;
        int vector3Size = sizeof(float) * 3;
        int totalSize = colorSize + vector3Size;
        ComputeBuffer cubeBuffer = new ComputeBuffer(data.Length, totalSize);
        cubeBuffer.SetData(data);
        computeShader.SetBuffer(0, "cubes", cubeBuffer);
        computeShader.SetFloat("resolution", data.Length);
        computeShader.SetFloat("repetitions", repetitions);
        computeShader.Dispatch(0, data.Length/10, 1, 1);

        cubeBuffer.GetData(data);
        for (int i = 0; i < objects.Count; i++)
        {
            GameObject obj = objects[i];
            Cube cube = data[i];
            obj.transform.position = cube.position;
            obj.GetComponent<MeshRenderer>().material.SetColor("_Color", cube.color);
        }
        cubeBuffer.Dispose();
    }

    void OnRandomize()
    {
        for (int i = 0; i < repetitions; i++)
        {
            for (int j = 0; j < objects.Count; j++)
            {
                GameObject obj = objects[j];
                obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y,
                    Random.Range(-0.1f, 0.1f));
                obj.GetComponent<MeshRenderer>().material.SetColor("_Color", Random.ColorHSV());
            }
        }
    }

    public void OnGUI()
    {
        if (objects == null || objects.Count == 0)
        {
            if (GUI.Button(new Rect(0, 0, 100, 50), "Create"))
            {
                CreateCubes();   
            }
        }
        else
        {
            if (GUI.Button(new Rect(0, 0, 100, 50), "Random CPU"))
            {
                var cpuStopwatch = new Stopwatch();
                cpuStopwatch.Start();
                for (int i = 0; i < repetitions; i++)
                {
                    OnRandomCPU();
                }
                cpuStopwatch.Stop();
                UnityEngine.Debug.Log($"# of Cubes={count*count}\t # of repetitions={repetitions}\t total cube operations ={(count*count)*repetitions}\n<b>OnRandomCPU took {cpuStopwatch.ElapsedMilliseconds} ms</b>");
            }
            else if (GUI.Button(new Rect(0, 50, 100, 50), "Random GPU"))
            {
                var gpuStopwatch = new Stopwatch();
                gpuStopwatch.Start();
                for (int i = 0; i < repetitions; i++)
                {
                    OnRandomGPU();
                }
                gpuStopwatch.Stop();
                UnityEngine.Debug.Log($"# of Cubes={count*count}\t # of repetitions={repetitions}\t total cube operations ={(count*count)*repetitions}\n<b>OnRandomGPU took {gpuStopwatch.ElapsedMilliseconds} ms</b>");
            }
        }
    }
}
