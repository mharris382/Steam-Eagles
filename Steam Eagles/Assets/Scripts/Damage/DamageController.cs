using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Damage
{
    public class DamageController : MonoBehaviour
    {
        
      //  [Range(0,1)] public float baselineProbability = 0.5f;
      //  [LabelWidth(75)] public DamageRate lowerRate = new DamageRate(1, 3);
      //  [LabelWidth(75)] public DamageRate upperRate = new DamageRate(20, 1);
      // 
      // [Min(1)] public int checksPerTurn = 4;

        public DamageCalculator calculator;
        private IDisposable _disposable;

        public int damageCount;
        public int missCount;
        public int batchCount;

        public int chanceCount;

        public float percent;
        public int chances;
        
        private void Awake()
        {
            var basicStrategy = new BasicDamageRollStrategy();
            calculator.RollsStrategy = basicStrategy;
            calculator.RollsStrategy = basicStrategy;
            
        }

        private void OnEnable()
        {
            damageCount = 0;
            var cd = new CompositeDisposable();
            _disposable = cd;
                calculator.RunDamageLoop()
               .Subscribe(damage =>
               {
                   damageCount++;
               })
               .AddTo(cd);
                calculator.Misses.Subscribe(misses =>
                {
                    missCount += misses;
                }).AddTo(cd);
                calculator.OnBatch.Subscribe(rp =>
                {
                    batchCount++;
                    percent = rp.Percent;
                    chances = (rp.Chances)+1;
                }).AddTo(cd);
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
            _disposable = null;
        }
    }
}