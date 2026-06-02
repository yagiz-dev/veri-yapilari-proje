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
