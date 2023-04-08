using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreLib
{
    public class GrabBag<T>
    {
        private Queue<T> shuffleBag;
        private Queue<T> declined;
        private HashSet<T> declinedSet;
        private readonly IRandom _random;
        private int totalWeight;
        private List<T> items;

        public GrabBag() : this(new UnityRandom())
        {
        }
        public GrabBag(IRandom random)
        {
            
            _random = random;
        }

        public void Init(List<(T item, int weight)> items)
        {
            this.items = new List<T>();
            foreach (var valueTuple in items)
            {
                for (int i = 0; i < valueTuple.weight; i++)
                {
                    this.items.Add(valueTuple.item);
                }
            }
            shuffleBag = new Queue<T>();
            declined = new Queue<T>();
            declinedSet = new HashSet<T>();
            Reset();
        }

        public void Init(params (T item, int weight)[] items)
        {
            Init(items.ToList());
        }
        
        public void Reset()
        {
            if(items == null || items.Count == 0)
                throw new Exception("GrabBag not initialized");
            for (int i = items.Count-1; i > 0; i--)
            {
                int j = _random.NextInt(0, i + 1);
                (items[j], items[i]) = (items[i], items[j]);
            }
            shuffleBag = new Queue<T>(items);
            declined = new Queue<T>();
            declinedSet.Clear();
        }

        public IEnumerable<T> GetItems(Predicate<T> acceptItem)
        {
            while (shuffleBag.Count > 0)
            {
                var item = shuffleBag.Dequeue();
                if (declinedSet.Contains(item))
                {
                    declined.Enqueue(item);
                }
                else if (!acceptItem(item))
                {
                    declinedSet.Add(item);
                    declined.Enqueue(item);
                }
                else
                {
                    yield return item;
                    declinedSet.Clear();
                    while (declined.Count > 0)
                    {
                        shuffleBag.Enqueue(declined.Dequeue());
                    }
                }
            }
        }
    }
}
