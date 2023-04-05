using System.Text;

namespace GasSim.SimCore.DataStructures
{
    /// <summary>
    /// a priority queue data structure, internally backed by a binary min heap <seealso cref="BinaryHeap{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PriorityQueue<T>
    {
        private readonly BinaryHeap<T> _binaryHeap;

        private int Capacity => _binaryHeap.Capacity;
        public int Count => _binaryHeap.Count;

        public bool IsEmpty => Count == 0;
        public PriorityQueue(int capacity)
        {
            _binaryHeap = new BinaryHeap<T>();
            _binaryHeap.StartHeap(capacity);
        }

        public T PeekMin() => _binaryHeap.FindMin();
        
        public T ExtractMin() => _binaryHeap.ExtractMin();
        
        public bool Enqueue(T value, float key)
        {
            if (_binaryHeap.IsAtCapacity())
            {
                //queue is full
                return false;
            }
            _binaryHeap.Insert(value, key);
            return true;
        }

        public void UpdateKey(T value, float key) => _binaryHeap.ChangeKey(value, key);

        public void Remove(T value) => _binaryHeap.Delete(value);


        public BinaryHeap<T> GetInternalHeap()
        {
            return _binaryHeap;
        }

        public override string ToString()
        {
            var copy = BinaryHeap<T>.Clone(_binaryHeap);
            StringBuilder sb = new StringBuilder();
            while (copy.Count > 0)
            {
                sb.Append(copy.ExtractMin());
                sb.Append(", ");
            }
            return sb.ToString();
        }
    }
}