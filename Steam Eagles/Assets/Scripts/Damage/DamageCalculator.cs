using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Buildings;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Damage
{
    [System.Serializable]
    public class DamageCalculator
    {
        
        [MinMaxSlider(0,1, true)]
        public Vector2 hitRatioRange = new Vector2(0.2f, 0.8f);
        [Range(0,1)]
        public float desiredHitRatio = 0.5f;

        public StrategyMode strategyMode;


        [Min(1)]
       public int rollsPerBatch = 10;
       

       [Min(0.1f),SuffixLabel("sec")]
       public float durationBetweenBatches = 1;

       [Min(0)]
       public int maxReRolls = 3;

       public float minHitRatio => hitRatioRange.x;
       public float maxHitRatio => hitRatioRange.y;


       private float _hitPercent = 0.5f;
       private int _numHits=0;
       private int _numRollsSoFar=0;
       private int _rerolls = 0;

       private List<IEnumerable<Vector3Int>> _multiHits = new List<IEnumerable<Vector3Int>>();
       private List<Vector3Int> _hits = new List<Vector3Int>();

       private Queue<Vector3Int> _queuedHits = new Queue<Vector3Int>();

       public enum StrategyMode
       {
           MULTI_HIT_PER_ROLL,
           SINGLE_HIT_PER_ROLL,
           ADDITIVE,
           SINGLE_TO_MULTI
       }

       public IDamageRollsStrategy RollsStrategy
       {
           get => _rollsStrategy ??= new BasicDamageRollStrategy();
           set
           {
               if (value == null)
               {
                   _rollsStrategy = new BasicDamageRollStrategy();
               }
               else
               {
                   _rollsStrategy = value;
               }
           }
       }
       public IDamageRollStrategy RollStrategy
       {
           get => _rollStrategy ??= new BasicDamageRollStrategy();
           set
           {
               if (value == null)
               {
                   _rollStrategy = new BasicDamageRollStrategy();
               }
               else
               {
                   _rollStrategy = value;
               }
           }
       }
       
       private IDamageRollStrategy _rollStrategy;
       private IDamageRollsStrategy _rollsStrategy;


       private Subject<Vector3Int> _hitSubject = new Subject<Vector3Int>();
       private Subject<RollParameters> _onBatch;
       public IObservable<RollParameters> OnBatch => _onBatch;
       private Subject<int> _misses;
       public IObservable<int> Misses => _misses;
       public void NotifyHit(Vector3Int hit)
       {
           _numHits++;
           _queuedHits.Enqueue(hit);
           if (_hitSubject == null) return;
           _hitSubject.OnNext(hit);
       }

       public IObservable<Vector3Int> RunDamageLoop()
       {
           _onBatch = new Subject<RollParameters>();
           _misses = new Subject<int>();
           return Observable.FromCoroutine<Vector3Int>(RunDamageLoop);
       }

       public IEnumerable<Vector3Int> Loop()
       {
           Debug.Log("Starting Calculator Loop");
           var parameters = ComputeParameters(_numHits, _numRollsSoFar);
           var rollP = new RollParameters(parameters.percent, parameters.rerolls);
           if(_onBatch == null) _onBatch = new Subject<RollParameters>();
           _onBatch.OnNext(rollP);
           int localHits = 0;
           using (_hitSubject.Subscribe(_ => localHits++))
           {
               SingleBatch(rollP);
           }
            _numRollsSoFar += rollsPerBatch;
            _numHits += localHits;
            if (localHits == 0)
            {
                Debug.Log("No hits this time");
            }
            else
            {
                Debug.Log($"Got {localHits} hits this time");
            }
            for (int i = 0; i < localHits; i++)
            {
                yield return Vector3Int.zero;
            }
       }
       
       public IEnumerator RunDamageLoop(IObserver<Vector3Int> observer,
            CancellationToken ct)
        {
            _hitSubject?.Dispose();
            _hitSubject = new Subject<Vector3Int>();
            
            var d = _hitSubject.Subscribe(observer);
            _numHits = 0;
            _numRollsSoFar = 0;
            while (!ct.IsCancellationRequested)
            {
                var parameters = ComputeParameters(_numHits, _numRollsSoFar);
                var rollP = new RollParameters(parameters.percent, parameters.rerolls);
                
                var timeBatchStart =Time.realtimeSinceStartup;
                
                SingleBatch(rollP);
                _numRollsSoFar += rollsPerBatch;
                
                var batchDuration = Time.realtimeSinceStartup - timeBatchStart;
                var waitTime = Mathf.Max(0, durationBetweenBatches - (batchDuration));
                yield return new WaitForSeconds(waitTime);
                if (!ct.IsCancellationRequested )
                {
                    int chances = parameters.rerolls * rollsPerBatch;
                    int hits = _queuedHits.Count;
                    if (_queuedHits.Count > 0)
                    {
                        int cnt = 0;
                        while (_queuedHits.Count > 0)
                        {
                            observer.OnNext(_queuedHits.Dequeue());
                            if (cnt > chances)
                            {
                                break;
                            }
                            cnt++;
                        }
                        _queuedHits.Clear();
                    }

                    
                    int misses = chances - hits;
                    _misses.OnNext(misses);
                    _onBatch.OnNext(rollP);
                }
            }
            _misses.OnCompleted();
            _onBatch.OnCompleted();
            _hitSubject.OnCompleted();
            _hitSubject = null;
        }

       private  (float percent, int rerolls) ComputeParameters(int numHits, int numRollsSoFar)
       {
           var currentHitRatio = numHits / (float)numRollsSoFar;

           if (currentHitRatio < desiredHitRatio)
           {
               _hitPercent = Mathf.Max(minHitRatio, 1 - ((1 - desiredHitRatio) / currentHitRatio) * (1 - maxHitRatio));
           }
           else
           {
               _hitPercent = Mathf.Min(maxHitRatio,
                   ((desiredHitRatio - minHitRatio) / (1 - currentHitRatio)) + minHitRatio);
           }

           if (_hitPercent == 1)
           {
               _rerolls = 0;
           }
           else if (_hitPercent == minHitRatio)
           {
               _rerolls = maxReRolls;
           }
           else
           {
               _rerolls = Mathf.CeilToInt(((_hitPercent - minHitRatio) / (maxHitRatio - minHitRatio)) * maxReRolls);
           }

           return (_hitPercent, _rerolls);
       }

       private void SingleBatch(RollParameters rollParameters)
       {
           for (int i = 0; i < rollsPerBatch; i++)
           {
               switch (strategyMode)
               {
                   case StrategyMode.MULTI_HIT_PER_ROLL:
                       RollForMultiHits(rollParameters);
                       break;
                   
                   case StrategyMode.SINGLE_HIT_PER_ROLL:
                       RollForSingleHit(rollParameters);
                       break;
                   
                   case StrategyMode.ADDITIVE:
                       RollForMultiHits(rollParameters);
                       RollForSingleHit(rollParameters);
                       break;
                   
                   case StrategyMode.SINGLE_TO_MULTI:
                       if (RollForSingleHit(rollParameters))
                       {
                           RollForMultiHits(rollParameters);
                       }
                       break;
                   
                   default:
                       throw new ArgumentOutOfRangeException();
               }
           }
       }
       
       private void RollForMultiHits(RollParameters rollParameters)
       {
           int hits = RollsStrategy.RollForHits(rollParameters, out var rollHits);
           
           if (hits > 0)
           {
               var loopHits = new Vector3Int[hits];
               int index = 0;
               foreach (var cellPosition in rollHits)
               {
                   loopHits[index] = cellPosition;
                   NotifyHit(cellPosition);
                   index++;
               }
               _multiHits.Add(loopHits);
           }
       }
       
       private bool RollForSingleHit(RollParameters rollP)
       {
           if (RollStrategy.RollForHit(rollP, out var rollHit))
           {
               _hits.Add(rollHit);
               NotifyHit(rollHit);
               
               return true;
           }

           return false;
       }
    }
}