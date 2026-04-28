namespace DomTreePoc.Models;

internal sealed class DomDocument
{
    public DomDocument() {
        Root = new DomNode("#document");
    }

    public DomNode Root { get; }

    public void PrintTree() {
        PrintNode(Root, depth: 0);
    }

    private static void PrintNode(DomNode node, int depth) {
        Console.WriteLine($"{new string(' ', depth * 2)}{node.TagName}");

        for (var i = 0; i < node.Children.Count; i++) {
            PrintNode(node.Children[i], depth + 1);
        }
    }
}
