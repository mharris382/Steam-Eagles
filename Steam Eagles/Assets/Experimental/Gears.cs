using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Experimental
{
    public class Gears : MonoBehaviour
    {
        public Gear rootGear;
        public LayerMask gearLayers;
        public float bufferRadius = 0.2f;
        public float gearRPM = 6;

        private float _currentRpm;
        private float _rpmT;
        
        public List<Gear> gearsInSystem;
        private void Start()
        {
            rootGear = GetComponent<Gear>();
            gearsInSystem.Clear();
            SolveGearChain();
        }

        public void SolveGearChain()
        {
            
            HashSet<Gear> searchedGears = new HashSet<Gear>();
            Stack<Gear> unsearchedGears = new Stack<Gear>();
            
            unsearchedGears.Push(rootGear);
            
            while (unsearchedGears.Count > 0)
            {
                var currentGear = unsearchedGears.Pop();
                
                if (searchedGears.Contains(currentGear))
                    continue;
                
                searchedGears.Add(currentGear);
                foreach (var gear in  Physics2D.OverlapCircleAll(currentGear.Center, currentGear.Radius+bufferRadius, gearLayers).Select(t => t.GetComponent<Gear>()))
                {
                    if (gear == null)
                        continue;
                    if (searchedGears.Contains(gear)) continue;
                    if (gear.System != null && gear.System != this)
                    {
                        Debug.LogError("Gear Systems overlapping!", gear.System);
                        return;
                    }

                    if (gear == currentGear.parentGear) continue;
                    if (gear == currentGear) continue;

                    gear.parentGear = currentGear;
                    
                    currentGear.childGears.Add(gear);
                    gear.System = this;
                    unsearchedGears.Push(gear);
                }
                
            }

            gearsInSystem = searchedGears.ToList();
        }

        private void Update()
        {
            MoveGearChain();
        }

        public void MoveGearChain()
        {
           _currentRpm = Mathf.SmoothDamp(_currentRpm, gearRPM, ref _rpmT, Time.deltaTime);
           float radPerSec = _currentRpm * 0.10472f;
           float degPerSec = radPerSec * Mathf.Rad2Deg;
           rootGear.AngularVelocity = degPerSec;
        }
    }
}