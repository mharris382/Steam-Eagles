using System;
using UnityEngine;

namespace PhysicsFun
{
    public class AtmosphereManager : MonoBehaviour
    {
        public Atmosphere atmosphere;

        
        
        
        
        void Start()
        {
            var atmosphereRect = atmosphere.GetAtmosphereRect();
            
            var oceanRect = atmosphere.GetOceanRect();
            
            var oceanGameObject = new GameObject("Ocean");
            oceanGameObject.transform.SetParent(transform);
            oceanGameObject.transform.position = Vector3.zero;
            
            
            Mesh oceanMesh = new Mesh()
            {
                name = "Ocean Mesh"
            };
            
            Vector3[] vertices = new Vector3[4];
            vertices[0] = new Vector3(oceanRect.xMin, oceanRect.yMin, 0);
            vertices[1] = new Vector3(oceanRect.xMax, oceanRect.yMin, 0);
            vertices[2] = new Vector3(oceanRect.xMax, oceanRect.yMax, 0);
            vertices[3] = new Vector3(oceanRect.xMin, oceanRect.yMax, 0);
            oceanMesh.vertices = vertices;
            
            int[] tris = new int[6];
            tris[0] = 0;
            tris[1] = 1;
            tris[2] = 2;
            tris[3] = 0;
            tris[4] = 2;
            tris[5] = 3;
            oceanMesh.triangles = tris;
            
            oceanMesh.normals = new Vector3[4];
            oceanMesh.tangents = new Vector4[4];
            for (int i = 0; i < oceanMesh.normals.Length; i++)
            {
                oceanMesh.normals[i] = Vector3.back;
                oceanMesh.tangents[i] = new Vector4(1, 0, 0, -1);
            }
            
            Vector2[] uvs = new Vector2[4];
            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(1, 0);
            uvs[2] = new Vector2(1, 1);
            uvs[3] = new Vector2(0, 1);
            oceanMesh.uv = uvs;
            
            MeshFilter meshFilter = oceanGameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = oceanMesh;
        }
        private void OnDrawGizmos()
        {
            if (atmosphere == null) return;
            atmosphere.DrawAtmosphere();
        }
    }
}