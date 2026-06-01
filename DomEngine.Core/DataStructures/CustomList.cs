using System;
using System.Collections;
using System.Collections.Generic;

namespace DomEngine.Core.DataStructures;

/// <summary>
/// Çift yönlü bağlı liste mantığıyla çalışan özel liste yapısı.
/// .NET'in yerleşik koleksiyonları kullanılmadan sıfırdan yazılmıştır.
/// </summary>
/// <typeparam name="T">Elemanların tipi.</typeparam>
public class CustomList<T> : IEnumerable<T>
{
    private Node<T>? _head;
    private Node<T>? _tail;
    private int _count;

    public int Count => _count;

    // Indexer - Bağlı liste olduğu için O(n) zaman alır ancak kullanım kolaylığı sağlar.
    public T this[int index]
    {
        get
        {
            var node = GetNodeAt(index);
            return node.Data;
        }
        set
        {
            var node = GetNodeAt(index);
            node.Data = value;
        }
    }

    public void Add(T item)
    {
        var newNode = new Node<T>(item);

        if (_head == null)
        {
            _head = newNode;
            _tail = newNode;
        }
        else
        {
            _tail!.Next = newNode;
            newNode.Prev = _tail;
            _tail = newNode;
        }

        _count++;
    }

    public bool Remove(T item)
    {
        var current = _head;
        while (current != null)
        {
            if (EqualityComparer<T>.Default.Equals(current.Data, item))
            {
                if (current.Prev != null)
                {
                    current.Prev.Next = current.Next;
                }
                else
                {
                    _head = current.Next;
                }

                if (current.Next != null)
                {
                    current.Next.Prev = current.Prev;
                }
                else
                {
                    _tail = current.Prev;
                }

                _count--;
                return true;
            }
            current = current.Next;
        }
        return false;
    }

    private Node<T> GetNodeAt(int index)
    {
        if (index < 0 || index >= _count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }

        var current = _head;
        for (int i = 0; i < index; i++)
        {
            current = current!.Next;
        }

        return current!;
    }

    public void Clear()
    {
        _head = null;
        _tail = null;
        _count = 0;
    }

    // Foreach döngülerinde kullanılabilmesi için IEnumerable implementasyonu
    public IEnumerator<T> GetEnumerator()
    {
        var current = _head;
        while (current != null)
        {
            yield return current.Data;
            current = current.Next;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
