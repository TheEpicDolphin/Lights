using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface INode
{
    List<INodeEdge> GetNeighborEdges();
}

public interface INodeEdge
{
    INode GetNode();
    int GetWeight();
}

public class Graph<T> where T : class, INode
{
    public T[] nodes;

    public Graph(T[] nodes)
    {
        this.nodes = nodes;
    }

    public List<T> DijkstrasAlgorithm(T start, T end)
    {
        Dictionary<T, T> backPointers = new Dictionary<T, T>();
        MinHeap<int, T> frontier = new MinHeap<int, T>();
        foreach (T node in nodes)
        {
            if (node == start)
            {
                frontier.Insert(0, node);
            }
            else
            {
                frontier.Insert(int.MaxValue, node);
            }
        }
        backPointers[start] = null;

        HashSet<T> sptSet = new HashSet<T>();
        HeapElement<int, T> curNode = frontier.ExtractMin();
        while (curNode != null)
        {
            sptSet.Add(curNode.value);
            List<INodeEdge> outgoingEdges = curNode.value.GetNeighborEdges();
            foreach (INodeEdge edge in outgoingEdges)
            {
                T neighbor = (T)edge.GetNode();
                int edgeWeight = edge.GetWeight();
                if (!sptSet.Contains(neighbor) && (frontier.FetchKeyFor(neighbor) > edgeWeight + curNode.key))
                {
                    frontier.Update(edgeWeight + curNode.key, neighbor);
                    backPointers[neighbor] = curNode.value;
                }
            }
            curNode = frontier.ExtractMin();
        }

        return TraceBackPointers(backPointers, end);
    }

    public List<List<T>> DijkstrasAlgorithm(T start, List<T> ends)
    {
        Dictionary<T, T> backPointers = new Dictionary<T, T>();
        MinHeap<int, T> frontier = new MinHeap<int, T>();
        foreach (T node in nodes)
        {
            if (node == start)
            {
                frontier.Insert(0, node);
            }
            else
            {
                frontier.Insert(int.MaxValue, node);
            }
        }
        backPointers[start] = null;

        HashSet<T> sptSet = new HashSet<T>();
        HeapElement<int, T> curNode = frontier.ExtractMin();
        while (curNode != null)
        {
            sptSet.Add(curNode.value);
            List<INodeEdge> outgoingEdges = curNode.value.GetNeighborEdges();
            foreach (INodeEdge edge in outgoingEdges)
            {
                T neighbor = (T)edge.GetNode();
                int edgeWeight = edge.GetWeight();
                if (!sptSet.Contains(neighbor) && (frontier.FetchKeyFor(neighbor) > edgeWeight + curNode.key))
                {
                    frontier.Update(edgeWeight + curNode.key, neighbor);
                    backPointers[neighbor] = curNode.value;
                }
            }
            curNode = frontier.ExtractMin();
        }

        List<List<T>> paths = new List<List<T>>();
        foreach(T end in ends)
        {
            paths.Add(TraceBackPointers(backPointers, end));
        }
        return paths;
    }

    public List<T> TraceBackPointers(Dictionary<T, T> backPointers, T end)
    {
        List<T> path = new List<T>();
        T curNode = end;
        while (curNode != null)
        {
            path.Add(curNode);
            curNode = backPointers[curNode];
        }
        path.Reverse();
        return path;
    }

}
