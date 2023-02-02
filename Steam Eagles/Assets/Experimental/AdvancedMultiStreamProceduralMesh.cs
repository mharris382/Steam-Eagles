#define NOTES
using System;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;

namespace Experimental
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class AdvancedMultiStreamProceduralMesh : MonoBehaviour
    {
        private void OnEnable()
        {
            int vertexAttributeCount = 4;
            int vertexCount = 4;
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];
            //Each vertex of our mesh has four attributes: a position, a normal, a tangent, and a set of texture coordinates. We'll describe these by allocating a temporary native array with VertexAttributeDescriptor elements.

#if NOTES

            var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
                vertexAttributeCount, Allocator.Temp, 
                //We can optimize our usage of the native array a bit more by skipping its memory initialization step. By default Unity fills the allocated memory block with zeros, to guard against weird values. We can skip this step by passing NativeArrayOptions.UninitializedMemory as a third argument to the NativeArray constructor.
                //This means that the contents of the array are arbitrary and can be invalid, but we overwrite it all so that doesn't matter.
                NativeArrayOptions.UninitializedMemory
            );
            
            /*
             *We begin with the position. 
             *The constructor of VertexAttributeDescriptor has four optional parameters, to describe the attribute type, format, dimensionality, and the index of the stream that contains it. The default values are correct for our position, but we have to
             * provide at least a single argument otherwise we end up using the constructor without arguments, which would be invalid. So let's explicitly set the dimension argument to 3, which indicates that it consists of three component values.
             */
            vertexAttributes[0] = new VertexAttributeDescriptor(dimension: 3);
            vertexAttributes[1] = new VertexAttributeDescriptor(attribute:VertexAttribute.Normal, dimension: 3, stream:1);
            vertexAttributes[2] = new VertexAttributeDescriptor(attribute:VertexAttribute.Tangent, dimension: 4, stream:2);
            vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2, stream: 3);
            
            //The vertex streams of the mesh are then allocated by invoking SetVertexBufferParams on the mesh data, with the vertex count and the attribute definitions as arguments.
            meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
            
            //After that we no longer need the attribute definition, so we dispose of it.
            vertexAttributes.Dispose();
#else
            var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(vertexAttributeCount, Allocator.Temp,NativeArrayOptions.UninitializedMemory);
            vertexAttributes[0] = new VertexAttributeDescriptor(dimension: 3);
            vertexAttributes[1] = new VertexAttributeDescriptor(attribute:VertexAttribute.Normal, dimension: 3, stream:1);
            vertexAttributes[2] = new VertexAttributeDescriptor(attribute:VertexAttribute.Tangent, dimension: 4, stream:2);
            vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2, stream: 3);
            meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
            vertexAttributes.Dispose();
#endif
            
            var mesh = new Mesh()
            {
                name = "Procedural Mesh",
            };
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
            GetComponent<MeshFilter>().mesh = mesh;
            
        }
    }
}