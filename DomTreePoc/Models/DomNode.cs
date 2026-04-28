using DomTreePoc.Collections;

namespace DomTreePoc.Models;

internal sealed class DomNode
{
    public DomNode(string tagName) {
        TagName = tagName;
        Children = new DynamicArray<DomNode>();
    }

    public string TagName { get; }
    public DomNode? Parent { get; private set; }
    public DynamicArray<DomNode> Children { get; }

    public void AddChild(DomNode child) {
        child.Parent = this;
        Children.Add(child);
    }
}
