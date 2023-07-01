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

        public bool useDepthLimit;
        public float depthRange = 0.2f;
        
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
            List<Gear> axelGears = new List<Gear>();

            void FindAxelParent(Gear gear)
            {
                var parent = FindGearColliderss(gear)
                    .Select(t => t.GetComponent<Gear>())
                    .Where(gear => gear != null && gear != gear).OrderBy(t => (gear.Center - t.Center).sqrMagnitude)
                    .FirstOrDefault();
                if (parent == null) return;
                gear.parentGear = parent;
                gear.parentGear.axelChildGears.Add(gear);
                gear.transform.parent = gear.parentGear.transform;
                var lp = gear.transform.localPosition;
                lp.x = 0;
                lp.y = 0;
                gear.transform.localPosition = lp;
                unsearchedGears.Push(gear);
            }
            unsearchedGears.Push(rootGear);
            
            while (unsearchedGears.Count > 0)
            {
                var currentGear = unsearchedGears.Pop();
                
                if (searchedGears.Contains(currentGear))
                    continue;
                
                searchedGears.Add(currentGear);
                var colls = FindGearColliderss(currentGear);

                foreach (var gear in colls  
                             .Select(t => t.GetComponent<Gear>())
                             .Where(gear => gear!=null && gear != currentGear)
                             .Where(gear => !searchedGears.Contains(gear)))
                {
                    if (gear.System != null && gear.System != this)
                    {
                        Debug.LogError("Gear Systems overlapping!", gear.System);
                        return;
                    }
                    
                    gear.System = this;
                    
                    var currentRadius = currentGear.Radius;
                    var gearRadius = gear.Radius;
                    var diff = gear.Center - currentGear.Center;
                    
                    
                    

                    if (gear == currentGear.parentGear) continue;

                    if (gear.axelConnection && gear.parentGear == null)
                    {
                        FindAxelParent(gear);
                    }
                    
                    if(gear.parentGear == null)
                    {
                        gear.parentGear = currentGear;
                    }
                   
                    if(!gear.parentGear.childGears.Contains(gear))
                        gear.parentGear.childGears.Add(gear);
                    unsearchedGears.Push(gear);
                }
                
            }

            gearsInSystem = searchedGears.ToList();
        }

        private Collider2D[] FindGearColliderss(Gear currentGear)
        {
            float minZ = 0, maxZ = 0;
            if (useDepthLimit)
            {
                minZ = currentGear.transform.position.z - depthRange;
                maxZ = currentGear.transform.position.z + depthRange;
            }

            var colls = !useDepthLimit
                ? Physics2D.OverlapCircleAll(
                    currentGear.Center,
                    currentGear.Radius + bufferRadius,
                    gearLayers)
                : Physics2D.OverlapCircleAll(
                    currentGear.Center,
                    currentGear.Radius + bufferRadius,
                    gearLayers, maxDepth: maxZ, minDepth: minZ
                );
            if (colls.Length == 0) return new Collider2D[0];
            return colls;
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