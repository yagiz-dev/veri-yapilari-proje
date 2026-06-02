using System;

namespace DomEngine.Core.DataStructures;

/// <summary>
/// Ağaç üzerinde BFS (Genişlik Öncelikli Arama) aramaları yapmak için FIFO (First In First Out) yapısı.
/// </summary>
/// <typeparam name="T">Elemanların tipi.</typeparam>
public class ArdaQueue<T>
{
    private ArdaNode<T>? _head;
    private ArdaNode<T>? _tail;
    private int _count;

    public int Count => _count;

    public void Enqueue(T item)
    {
        var newNode = new ArdaNode<T>(item);

        if (_tail == null)
        {
            _head = newNode;
            _tail = newNode;
        }
        else
        {
            _tail.Next = newNode;
            newNode.Prev = _tail;
            _tail = newNode;
        }

        _count++;
    }

    public T Dequeue()
    {
        if (_head == null)
        {
            throw new InvalidOperationException("Queue is empty.");
        }

        var data = _head.Data;
        _head = _head.Next;

        if (_head == null)
        {
            _tail = null;
        }
        else
        {
            _head.Prev = null;
        }

        _count--;
        return data;
    }

    public T Peek()
    {
        if (_head == null)
        {
            throw new InvalidOperationException("Queue is empty.");
        }

        return _head.Data;
    }
}
