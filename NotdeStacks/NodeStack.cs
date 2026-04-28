using DomTreePoc.Models;

namespace DomTreePoc.Collections;

internal sealed class NodeStack
{
    private StackItem? _top;

    public int Count { get; private set; }
    public bool IsEmpty => Count == 0;

    public void Push(DomNode node) {
        _top = new StackItem(node, _top);
        Count++;
    }

    public DomNode Pop() {
        if (IsEmpty) {
            throw new InvalidOperationException("Stack bosken pop yapilamaz.");
        }

        var top = _top!;
        var node = top.Value;
        _top = top.Next;
        Count--;
        return node;
    }

    public DomNode Peek() {
        if (IsEmpty) {
            throw new InvalidOperationException("Stack bosken peek yapilamaz.");
        }

        return _top!.Value;
    }

    private sealed class StackItem {
        public StackItem(DomNode value, StackItem? next)
        {
            Value = value;
            Next = next;
        }

        public DomNode Value { get; }
        public StackItem? Next { get; }
    }
}
