using System.Collections.Generic;

public class PriorityQueue<T>
{
    private readonly SortedList<float, Queue<T>> _elements = new SortedList<float, Queue<T>>();

    public int Count { get; private set; }

    public void Enqueue(T item, float priority)
    {
        if (!_elements.ContainsKey(priority))
        {
            _elements[priority] = new Queue<T>();
        }

        _elements[priority].Enqueue(item);
        Count++;
    }

    public T Dequeue()
    {
        if (Count == 0)
        {
            throw new System.InvalidOperationException("The queue is empty.");
        }

        var firstKey = _elements.Keys[0];
        var firstQueue = _elements[firstKey];
        var item = firstQueue.Dequeue();
        if (firstQueue.Count == 0)
        {
            _elements.Remove(firstKey);
        }

        Count--;
        return item;
    }
}