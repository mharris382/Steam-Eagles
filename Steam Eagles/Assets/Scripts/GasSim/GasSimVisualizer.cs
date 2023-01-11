using System;
using System.Collections.Generic;
using UnityEngine;

namespace GasSim
{
    [RequireComponent(typeof(ParticleSystem), typeof(Grid))]
    public class GasSimVisualizer : MonoBehaviour
    {
        public uint seed = 100;
        ParticleSystem ps;
        ParticleSystem PS => ps ? ps : ps = GetComponent<ParticleSystem>();
        
        private Grid _grid;
        private Grid Grid => _grid ? _grid : _grid = GetComponent<Grid>();

        ParticleSystem.Particle GetParticleForPressureCell(Vector2Int cell, int pressure, float updateRate)
        {
            float pressureOpacity = Mathf.InverseLerp(0, 16, pressure);
            Vector3Int cellPos = new Vector3Int(cell.x, cell.y, 0);
            Color particleColor = Color.white;
            float lifetime = updateRate + 0.1f;
            particleColor.a = pressureOpacity;
            return new ParticleSystem.Particle
            {
                position = Grid.GetCellCenterWorld(cellPos),
                startColor = particleColor,
                startSize = Grid.cellSize.x,
                startLifetime = lifetime,
                remainingLifetime = lifetime,
                randomSeed = seed
            };
        }

        public void UpdateParticlesFromGridData(Dictionary<Vector2Int, int> cells, float updateRate)
        {
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[cells.Count];
            int i = 0;
            foreach (var kvp in cells)
            {
                var coord = kvp.Key;
                var pressure = kvp.Value;
                particles[i] = GetParticleForPressureCell(coord, pressure, updateRate);
                i++;
            }
            PS.Emit(cells.Count);
            PS.SetParticles(particles, particles.Length);
        }
    }
}