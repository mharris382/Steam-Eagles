using System;
using CoreLib;
using NaughtyAttributes;
using UnityEngine;

namespace PhysicsFun
{
    [CreateAssetMenu(menuName = "Steam Eagles/Atmosphere")]
    public class Atmosphere : ScriptableObject
    {
        const int X_MAX = 10000;
        const int X_MIN = -10000;
        public float spaceHeight = 100;
        public float seaLevelHeight = 0;
        public float oceanFloorHeight = -100;

        [Range(0.0001f, 10)]
        public float atmosphereDensityFalloff = 1;
        
        [SerializeField] bool defineX = true;
        
        [MinMaxRange(X_MIN, X_MAX)]
        [ShowIf(nameof(defineX))] [SerializeField] private Vector2Int xRange = new Vector2Int(-100, 100);

        [Header("Debug")]
        [SerializeField] private bool gizmos = true;
        
        [ShowIf(nameof(gizmos))]
        [Range(0, 100)]
        [SerializeField] private int atmosphereSegments = 25;
        
        
        public float GetXMin() => defineX ? xRange.x : -10000;
        public float GetXMax() => defineX ? xRange.y : 10000;
        public void DrawAtmosphere()
        {
            var spaceColor = Color.blue.Mix(Color.red, 0.4f).Darken(0.5f);
            var seaLevelColor = Color.blue.Mix(Color.cyan, 0.4f).Lighten(0.1f);
            var oceanFloorColor = Color.blue.Mix(Color.cyan, 0.1f).Darken(0.4f);
            
            DrawLine(spaceHeight, spaceColor);
            DrawLine(seaLevelHeight, seaLevelColor);
            DrawLine(oceanFloorHeight, oceanFloorColor);

            for (int i = 0; i < atmosphereSegments; i++)
            {
                float t = (i+1) / (float)atmosphereSegments;
                t = Mathf.Pow(t, atmosphereDensityFalloff);
                var color = Color.Lerp(spaceColor, seaLevelColor, t);
                float height = Mathf.Lerp(spaceHeight, seaLevelHeight, t);
                DrawLine(height, color);
            }
            
            var atmosphereRect = GetAtmosphereRect();
            atmosphereRect.yMax -= 4;
            atmosphereRect.yMin += 4;
            atmosphereRect.DrawGizmos();
            
            var oceanRect = GetOceanRect();
            oceanRect.yMax -= 4;
            oceanRect.yMin += 4;
            oceanRect.DrawGizmos();
        }

        public Rect GetAtmosphereRect() => CreateRect(seaLevelHeight, spaceHeight);
        public Rect GetOceanRect() => CreateRect(oceanFloorHeight, seaLevelHeight);

        public void DrawLine(float height, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawLine(new Vector2(GetXMin(), height), new Vector2(GetXMax(), height));
        }

        public Rect CreateRect(float minHeight, float maxHeight)
        {
            return Rect.MinMaxRect(GetXMin(), minHeight, GetXMax(), maxHeight);
        }
        

        [Serializable]
        public class WorldLayer
        {
            private readonly float yMin;
            private readonly float yMax;

            public WorldLayer()
            {
                
            }
            
            public WorldLayer(float yMin, float yMax)
            {
                this.yMin = yMin;
                this.yMax = yMax;
                
            }
        }
    }
}