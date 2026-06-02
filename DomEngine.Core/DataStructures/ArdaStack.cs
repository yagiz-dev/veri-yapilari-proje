using System;

namespace DomEngine.Core.DataStructures;

/// <summary>
/// Parser aşamasında HTML tag eşleşmelerini kontrol etmek için LIFO (Last In First Out) yapısı. Custom stack üstüne gelince bu açıklama gözüküyor.
/// </summary>
/// 
/// <typeparam name="T">Elemanların tipi.</typeparam> //T'nin üstüne gelince Bu açıklama gözüküyor


public class ArdaStack<T>
{
    private ArdaNode<T>? _top;
    private int _count;

    public int Count { get { return _count; } }

    public void Push(T item)
    {
        var newNode = new ArdaNode<T>(item);

        if (_top != null)
        {
            newNode.Next = _top;
            _top.Prev = newNode;
        }
        
        _top = newNode;
        _count++;
    }

    public T Pop()
    {
        if (_top == null)
        {
            throw new InvalidOperationException("Stack is empty.");
        }

        var data = _top.Data;
        _top = _top.Next;
        if (_top != null)
        {
            _top.Prev = null;
        }

        _count--;
        return data;
    }

    public T Peek()
    {
        if (_top == null)
        {
            throw new InvalidOperationException("Stack is empty.");
        }

        return _top.Data;
    }
}
