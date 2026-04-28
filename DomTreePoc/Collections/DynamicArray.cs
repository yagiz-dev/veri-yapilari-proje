namespace DomTreePoc.Collections;

internal sealed class DynamicArray<T>
{
    private T[] _items = new T[4];

    public int Count { get; private set; }

    public T this[int index] => index >= 0 && index < Count
        ? _items[index]
        : throw new IndexOutOfRangeException();

    public void Add(T item) {
        if (Count == _items.Length) {
            Resize(_items.Length * 2);
        }

        _items[Count++] = item;
    }

    private void Resize(int newCapacity) {
        var resized = new T[newCapacity];
        for (var i = 0; i < Count; i++) {
            resized[i] = _items[i];
        }

        _items = resized;
    }
}
