/*
 * Copyright (c) 2020 Razeware LLC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * Notwithstanding the foregoing, you may not use, copy, modify, merge, publish, 
 * distribute, sublicense, create a derivative work, and/or sell copies of the 
 * Software in any work that is designed, intended, or marketed for pedagogical or 
 * instructional purposes related to programming, coding, application development, 
 * or information technology.  Permission for such use, copying, modification,
 * merger, publication, distribution, sublicensing, creation of derivative works, 
 * or sale is expressly withheld.
 *    
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Jobs;

using math = Unity.Mathematics.math;
using random = Unity.Mathematics.Random;
using Rand = UnityEngine.Random;
public class FishGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform waterObject;
    public Transform objectPrefab;

    [Header("Spawn Settings")]
    public int amountOfFish;
    public Vector3 spawnBounds;
    public float spawnHeight;
    public int swimChangeFrequency;

    [Header("Settings")]
    public float swimSpeed;
    public float turnSpeed;

    private NativeArray<Vector3> velocities;
// 2
    private TransformAccessArray transformAccessArray;
    private PositionUpdateJob positionUpdateJob;

    private JobHandle positionUpdateJobHandle;
    private void Start()
    {
        velocities = new NativeArray<Vector3>(amountOfFish, Allocator.Persistent);
        transformAccessArray = new TransformAccessArray(amountOfFish);

        for (int i = 0; i < amountOfFish; i++)
        {
            float distanceX = Rand.Range(-spawnBounds.x /2f, spawnBounds.x /2f);
            float distanceZ = Rand.Range(-spawnBounds.z /2f, spawnBounds.z /2f);
            
            Vector3 spawnPosition = (transform.position + Vector3.up * spawnHeight) + new Vector3(distanceX, spawnHeight, distanceZ);
            Transform t = (Transform)Instantiate(objectPrefab, spawnPosition, Quaternion.identity);
            
            transformAccessArray.Add(t);
        }
    }

    private void Update()
    {
        // 1
        positionUpdateJob = new PositionUpdateJob()
        {
            objectVelocities = velocities,
            jobDeltaTime = Time.deltaTime,
            swimSpeed = this.swimSpeed,
            turnSpeed = this.turnSpeed,
            time = Time.time,
            swimChangeFrequency = this.swimChangeFrequency,
            center = waterObject.position,
            bounds = spawnBounds,
            seed = System.DateTimeOffset.Now.Millisecond
        };
        positionUpdateJobHandle= positionUpdateJob.Schedule(transformAccessArray);
    }

    private void LateUpdate()
    {
        positionUpdateJobHandle.Complete(); 
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position + Vector3.up * spawnHeight, spawnBounds);
    }

    private void OnDestroy()
    {
        transformAccessArray.Dispose();
        velocities.Dispose();
    }
    
    
    
    [BurstCompile]
    struct PositionUpdateJob : IJobParallelForTransform
    {
        public NativeArray<Vector3> objectVelocities;

        public Vector3 bounds;
        public Vector3 center;

        public float jobDeltaTime;
        public float time;
        public float swimSpeed;
        public float turnSpeed;
        public int swimChangeFrequency;

        public float seed;

        public void Execute(int i, TransformAccess transform)
        {
            // 1
            Vector3 currentVelocity = objectVelocities[i];

// 2            
            random randomGen = new random((uint)(i * time + 1 + seed));

// 3
            transform.position += 
                transform.localToWorldMatrix.MultiplyVector(new Vector3(0, 0, 1)) * 
                swimSpeed * 
                jobDeltaTime * 
                randomGen.NextFloat(0.3f, 1.0f);

// 4
            if (currentVelocity != Vector3.zero)
            {
                transform.rotation = 
                    Quaternion.Lerp(transform.rotation, 
                        Quaternion.LookRotation(currentVelocity), turnSpeed * jobDeltaTime);
            }
            
            
            Vector3 currentPosition = transform.position;

            bool randomise = true;

// 1
            if (currentPosition.x > center.x + bounds.x / 2 || 
                currentPosition.x < center.x - bounds.x/2 || 
                currentPosition.z > center.z + bounds.z / 2 || 
                currentPosition.z < center.z - bounds.z / 2)
            {
                Vector3 internalPosition = new Vector3(center.x + 
                                                       randomGen.NextFloat(-bounds.x / 2, bounds.x / 2)/1.3f, 
                    0, 
                    center.z + randomGen.NextFloat(-bounds.z / 2, bounds.z / 2)/1.3f);

                currentVelocity = (internalPosition- currentPosition).normalized;

                objectVelocities[i] = currentVelocity;

                transform.rotation = Quaternion.Lerp(transform.rotation, 
                    Quaternion.LookRotation(currentVelocity), 
                    turnSpeed * jobDeltaTime * 2);

                randomise = false;
            }

// 2
            if (randomise)
            {
                if (randomGen.NextInt(0, swimChangeFrequency) <= 2)
                {
                    objectVelocities[i] = new Vector3(randomGen.NextFloat(-1f, 1f), 
                        0, randomGen.NextFloat(-1f, 1f));
                }
            }
        }
    }
}