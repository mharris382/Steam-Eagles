#if GODOT
using Godot;
#elif UNITY_5_3_OR_NEWER
using UnityEngine;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GasSim.SimCore.DataStructures
{
    /// <summary>
    /// binary MIN heap data structure has O(n log n) for insertion/removal on a sorted set of data.  
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BinaryHeap<T> : IEnumerable<T>, IEnumerable
    {
        #region [FIELDS]

        /// <summary>
        /// internal array of (item,weight) tuple
        /// </summary>
        private (T item,float key)[] _items;
        
        /// <summary>
        /// size of internal array
        /// </summary>
        private int _capacity;
        
        /// <summary>
        /// stores the number of items in the heap, not the size of the array
        /// </summary>
        private int _count;
        /// <summary>
        /// lookup table so the index of any given element can be found in O(1) time
        /// </summary>
        private Dictionary<T, int> _indexLookup; 

        #endregion

        #region [PROPERTIES]

        /// <summary>
        /// the maximum number of items that can be stored, not the size of the array <seealso cref="Count"/>
        /// </summary>
        public int Capacity => _capacity;
        
        /// <summary>
        /// number of items in the heap, not the size of the array <seealso cref="Capacity"/>
        /// </summary>
        public int Count => _count;
        
        /// <summary>
        /// Minimum item in the array.
        /// </summary>
        public T Root => FindMin();

        #endregion

        #region [STATIC METHODS]

        public static BinaryHeap<T> Clone(BinaryHeap<T> original)
        {
            var bh = new BinaryHeap<T>();
            bh.StartHeap(original._capacity);
            for (int i = 0; i < original._count; i++)
            {
                bh.Insert(original._items[i].item, original._items[i].key);
            }
            return bh;
        }
        

        #endregion



        #region [PUBLIC METHODS]

        public bool Contains(T item) => _indexLookup.ContainsKey(item);

        public bool Remove(T item)
        {
            if (!Contains(item)) return false;
            Delete(item);
            return true;
        }
        
        /// <summary>
        /// initializes an empty heap that is set up to store at most N elements.
        /// This operation takes O(N) time, as it involves initializing the array that will hold the heap
        /// </summary>
        /// <param name="maxItems"></param>
        public void StartHeap(int maxItems)
        {
            _capacity = maxItems;
            _items = new (T, float)[maxItems];
            _count = 0;
            _indexLookup = new Dictionary<T, int>();
        }

        /// <summary>
        /// inserts the item, item, with an ordering value, value, into the heap at index 0,
        /// then uses HeapifyDown to position the item so as to maintain the heap order.
        /// If the heap currently has n elements, this takes O(log n) time.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="key">ordering value</param>
        public bool Insert(T item, float key)
        {
            if (IsAtCapacity())
            {
                return false;
            }
            
            int index = _count;
            _items[index] =  (item, key);
            _indexLookup.Add(item, index);
            _count += 1;
            
            int i = index;
            while (i != 0 && GetKey(i) < GetParentKey(i))
            {
                int parent = GetParentIndex(i);
                Swap(i, parent);
                i = parent;
            }
            
            return true;
        }

        /// <summary>
        /// identifies the minimum element in the heap, which is located at index 1, but does not remove it. This takes O(1) time.
        /// </summary>
        /// <returns></returns>
        public T FindMin()
        {
            return _items[0].item;
        }

        /// <summary>
        /// deletes the element in the specified heap position by moving the item in the last array position to index,
        /// then using Heapify_Down to reposition that item. This is implemented in O(log n) time for heaps that have n elements.
        /// </summary>
        /// <param name="index"></param>
        public void Delete(int index)
        {
            
            ChangeKey(_items[index].item, float.MinValue);
            ExtractMin();
        }

        /// <summary>
        /// deletes the element item form the heap. This can be implemented as a call to Delete(Position[item]), which operates in O(log n) time for heaps
        /// that have n elements provided Position allows the index of v to be returned in O(1) time     
        /// </summary>
        /// <param name="item"></param>
        public void Delete(T item)
        {
            Delete(_indexLookup[item]);
        }

        /// <summary>
        /// identifies and deletes the element with the minimum key value, located at index 1, from the heap. This is a combination of the preceding two
        /// operations, and so it takes O(log n) time
        /// </summary>
        public T ExtractMin()
        {
            if (_count <= 0)
            {
                _count = 0;
                return default(T);
            }

            if (_count == 1)
            {
                _count--;
                return _items[0].item;
            }
            
            T minItem = _items[0].item;
            
            
            _items[0] = _items[_count - 1];
            _count--;
            HeapifyDown(0);
            
            return minItem;
        }

        /// <summary>
        /// which changes the key value of element v to newValue. To implement this operation in O(log n) time, we first need to be able to identify the position
        /// of element v in the array, which we do by using the Position structure. Once we have identified the position of element v, we change the key and
        /// then apply Heapify-up or Heapify-down as appropriate
        /// </summary>
        /// <param name="item"></param>
        /// <param name="key"></param>
        public void ChangeKey(T item, float key)
        {

            if (!_indexLookup.ContainsKey(item))
                throw new Exception("throw new NodeNotFoundException($\"the item: ({item}) was not found in the heap\");");
            
            var curIndex = _indexLookup[item];
            var oldKey = _items[curIndex].key;
            _items[curIndex] = (item, key);
           
            
            
            if(Math.Abs(oldKey - key) < Mathf.Epsilon)  //key did not change, so position is same
                return;
            
            if (oldKey < key) //key was increased
                HeapifyDown(curIndex);
            else // key was decreased
                HeapifyUp(curIndex);
        }
        
        public bool IsAtCapacity() => _count >= _capacity;

        public bool GetRightChild(T item, out T child)
        {
            var index = _indexLookup[item];
            try
            {
                var rightChild = GetRightChild(index);
                if (rightChild < _count)
                {
                    child = _items[rightChild].item;
                    return true;
                }

            }
            catch (Exception e)
            {
                child = default(T);
                return false;    
            }
            child = default(T);
            return false;
        }

        public bool GetLeftChild(T item, out T child)
        {
            try
            {
                var index = _indexLookup[item];
                var leftChild = GetLeftChild(index);
                if (leftChild < _count)
                {
                    child = _items[leftChild].item;
                    return true;
                }

            }
            catch (Exception e)
            {
                child = default(T);
                return false;
            }
            child = default(T);
            return false;
        }

        #endregion


        #region [PRIVATE METHODS]

        /// <summary>
        /// moves an element located at the specified index upwards in the heap to correctly reposition an element
        /// whose value is less than the value of its parent.
        /// This condition may result from removing an element or from changing an element’s value.
        /// This method is described on pages 60-61 of the text, and pseudocode is provided on page 61.
        /// </summary>
        /// <param name="index"></param>
        void HeapifyUp(int index)
        {
            
            int curIndex = index;
            float key = _items[curIndex].key;
            
           
            while (curIndex != 0 && key < ParentKey())
            {
                SwapCurrentWithParent();
                void SwapCurrentWithParent()
                {
                    int parentIndex = GetParentIndex(curIndex);
                    Swap(curIndex, parentIndex);
                    curIndex = parentIndex;
                }
            }
            float ParentKey() => _items[GetParentIndex(curIndex)].key;
        }

        
        /// <summary>
        /// moves an element located at the specified index downwards in the heap to correctly reposition an element whose value is
        /// greater than the value of either of its children. This condition may result from removing an element or from changing an
        /// element’s value.
        /// </summary>
        /// <param name="index"></param>
        void HeapifyDown(int index)
        {
            
            int n = _count;
            int j, lIndex, rIndex;

            lIndex =  GetLeftChild(index);
            rIndex = GetRightChild(index);
            
            
            float key = GetKey(index);
            int smallestIndex = index;
            float smallestKey = key;
            if (lIndex < _count &&  GetKey(lIndex) < smallestKey)
            {
                smallestKey = GetKey(lIndex);
                smallestIndex = lIndex;
            }
            if (rIndex < _count &&  GetKey(rIndex) < smallestKey)
            {
                smallestIndex = rIndex;
            }

            if (index != smallestIndex)
            {
                Swap(index, smallestIndex);
                HeapifyDown(smallestIndex);
            }
        }
        
        
        void Swap(int i, int j)
        {
            (_items[i], _items[j]) = (_items[j], _items[i]);
            _indexLookup[_items[i].item] = i;
            _indexLookup[_items[j].item] = j;
        }

        void Swap(T a, T b)
        {
            int index1 = _indexLookup[a];
            int index2 = _indexLookup[b];
            Swap(index1, index2);
        }

        
        int GetParentIndex(int index)
        {
            if (index < 0 || index > _count - 1)
                throw new IndexOutOfRangeException($"Must index between 0,{_count}");
            else if (index == 0)
                throw new InvalidOperationException("root node has no parent");
    
            return (index - 1) / 2;
        }

        
        int GetLeftChild(int index)
        {
            if (index < 0 || index > _count - 1)
                throw new IndexOutOfRangeException($"Must index between 0,{_count}");
            return (2 * index) + 1;
        }

        
        int GetRightChild(int index)
        {
            if (index < 0 || index > _count - 1)
                throw new IndexOutOfRangeException($"Must index between 0,{_count}");
            return (2 * index) + 2;
        }

        float GetKey(int index) => _items[index].key;
        
        float GetParentKey(int index) => GetKey(GetParentIndex(index));
        
        float GetLeftKey(int index) => GetKey(GetLeftChild(index));
        
        float GetRightKey(int index) => GetKey(GetRightChild(index));

        private void PrintDictionary()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in _indexLookup)
            {
                sb.Append($"[{kvp.Key}, {kvp.Value}]");
                sb.Append(", ");
            }
            Debug.Log(sb.ToString());
        }

        #endregion

       
        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < _count; i++)
            {
                sb.Append(i);
                sb.Append(":[");
                var value = _items[i].Item1;
                var key =_items[i].Item2;
                sb.Append(value.ToString());
                sb.Append(',');
                sb.Append($"{key:F2}");
                sb.Append("], ");
            }

            return sb.ToString();
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            if (this.Count == 0)
                yield break;
            foreach (var item in InOrder(Root))
            {
                yield return item;
            }
        }

        public IEnumerable<T> InOrder() => InOrder(Root);
        public IEnumerable<(T,int)> InOrderWithDepth() => InOrder(Root, 0);

        private IEnumerable<T> InOrder(T item)
        {
            if(GetLeftChild(item, out var leftChild))
            {
                foreach (var child in InOrder(leftChild))
                {
                    yield return child;
                }
            }
            yield return item;
            if(GetRightChild(item, out var rightChild))
            {
                foreach (var child in InOrder(rightChild))
                {
                    yield return child;
                }
            }
        }

        private IEnumerable<(T, int)> InOrder(T item, int depth)
        {
            yield return (item,depth);
            if(GetLeftChild(item, out var leftChild))
            {
                foreach (var child in InOrder(leftChild, depth+1))
                {
                    yield return child;
                }
            }
            if(GetRightChild(item, out var rightChild))
            {
                foreach (var child in InOrder(rightChild, depth+1))
                {
                    yield return child;
                }
            }
        }
        public IEnumerable<T> DepthFirst() => DepthFirst(Root);
        public IEnumerable<(T,int)> DepthFirstWithDepth() => DepthFirst(Root, 0);
        private IEnumerable<T> DepthFirst(T item)
        {
            if(GetLeftChild(item, out var leftChild))
            {
                foreach (var child in DepthFirst(leftChild))
                {
                    yield return child;
                }
            }
            if(GetRightChild(item, out var rightChild))
            {
                foreach (var child in DepthFirst(rightChild))
                {
                    yield return child;
                }
            }
            yield return item;
        }
        public IEnumerable<(T, int)> DepthFirst(T item, int depth)
        {
            yield return (item,depth);
            if(GetLeftChild(item, out var leftChild))
            {
                foreach (var child in DepthFirst(leftChild, depth+1))
                {
                    yield return child;
                }
            }
            if(GetRightChild(item, out var rightChild))
            {
                foreach (var child in DepthFirst(rightChild, depth+1))
                {
                    yield return child;
                }
            }
        }
        public IEnumerable<(T,int)> BreadthFirstWithDepth() => BreadthFirst(Root, 0);
        public IEnumerable<T> BreadthFirst() => BreadthFirst(Root);

        private IEnumerable<T> BreadthFirst(T item)
        {
            yield return item;
            if(GetLeftChild(item, out var leftChild))
            {
                foreach (var child in BreadthFirst(leftChild))
                {
                    yield return child;
                }
            }
            if(GetRightChild(item, out var rightChild))
            {
                foreach (var child in BreadthFirst(rightChild))
                {
                    yield return child;
                }
            }
        }
        public IEnumerable<(T, int)> BreadthFirst(T item, int depth)
        {
            yield return (item,depth);
            if(GetLeftChild(item, out var leftChild))
            {
                foreach (var child in BreadthFirst(leftChild, depth+1))
                {
                    yield return child;
                }
            }
            if(GetRightChild(item, out var rightChild))
            {
                foreach (var child in BreadthFirst(rightChild, depth+1))
                {
                    yield return child;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            foreach (var i in _indexLookup)
            {
                _items[i.Value] = default;
            }
            _indexLookup.Clear();
            _count = 0;
        }
    }
}