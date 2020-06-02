using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using VecUtils;

public class HeapElement<TKey, TValue> where TKey : IComparable
{

    public TKey key;
    public TValue value;

    public HeapElement(TKey key, TValue value)
    {
        this.key = key;
        this.value = value;
    }

}
public class MinHeap<TKey, TValue> where TKey : IComparable
{
    public List<HeapElement<TKey, TValue>> heap;
    public Dictionary<TValue, int> indexMap;

    //Assumes that it is already heapified
    public MinHeap()
    {
        this.heap = new List<HeapElement<TKey, TValue>>();
        this.indexMap = new Dictionary<TValue, int>();
    }

    public HeapElement<TKey, TValue> ExtractMin()
    {
        if (heap.Count == 0)
        {
            return null;
        }
        else if (heap.Count == 1)
        {
            HeapElement<TKey, TValue> min = heap[0];
            heap.RemoveAt(0);
            indexMap.Remove(min.value);
            return min;
        }
        else
        {
            int n = heap.Count;
            HeapElement<TKey, TValue> min = heap[0];
            indexMap[heap[n - 1].value] = 0;
            heap[0] = heap[n - 1];
            heap.RemoveAt(n - 1);
            indexMap.Remove(min.value);
            MinHeapifyDown(0);
            return min;
        }


    }

    public void Insert(TKey key, TValue value)
    {
        heap.Add(new HeapElement<TKey, TValue>(key, value));
        indexMap[value] = heap.Count - 1;
        MinHeapifyUp(heap.Count - 1);
    }

    public void Update(TKey key, TValue value)
    {
        int i = indexMap[value];
        if (heap[i].key.CompareTo(key) < 0)
        {
            heap[i].key = key;
            MinHeapifyDown(i);
        }
        else
        {
            heap[i].key = key;
            MinHeapifyUp(i);
        }

    }

    public TKey FetchKeyFor(TValue value)
    {
        int i = indexMap[value];
        return heap[i].key;
    }

    public bool ContainsValue(TValue value)
    {
        return indexMap.ContainsKey(value);
    }

    private void MinHeapifyUp(int i)
    {
        int p = Parent(i);

        if (heap[i].key.CompareTo(heap[p].key) < 0)
        {
            indexMap[heap[p].value] = i;
            indexMap[heap[i].value] = p;
            HeapElement<TKey, TValue> temp = heap[p];
            heap[p] = heap[i];
            heap[i] = temp;
            MinHeapifyUp(p);
        }
    }

    private void MinHeapifyDown(int i)
    {
        int l = Left(i);
        int r = Right(i);
        int c = heap[r].key.CompareTo(heap[l].key) < 0 ? r : l;

        if (heap[c].key.CompareTo(heap[i].key) < 0)
        {
            indexMap[heap[c].value] = i;
            indexMap[heap[i].value] = c;
            HeapElement<TKey, TValue> temp = heap[c];
            heap[c] = heap[i];
            heap[i] = temp;
            MinHeapifyDown(c);
        }

    }

    private int Parent(int i)
    {
        int p = (i - 1) / 2;
        return i > 0 ? p : i;
    }

    private int Left(int i)
    {
        int l = 2 * i + 1;
        return l < heap.Count ? l : i;
    }

    private int Right(int i)
    {
        int r = 2 * i + 2;
        return r < heap.Count ? r : i;
    }


}
