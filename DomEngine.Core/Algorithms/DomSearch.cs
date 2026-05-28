using System;
using DomEngine.Core.DataStructures;
using DomEngine.Core.Topology;

namespace DomEngine.Core.Algorithms;

/// <summary>
/// DOM Ağacı üzerinde arama, gezinti (Traversal) ve analiz algoritmaları barındırır.
/// </summary>
public static class DomSearch
{
    private static bool IsMatch(DomNode node, string query)
    {
        query = query.ToLower();
        
        // 1. Etiket ismine göre eşleştir
        if (!string.IsNullOrEmpty(node.TagName) && node.TagName.ToLower().Contains(query)) return true;

        // 2. Etiket ismine göre eşleşmiyorsa attribute'lara göre eşleştir
        var attrs = node.Attributes.GetAllPairs();
        foreach (var attr in attrs)
        {
            if (!string.IsNullOrEmpty(attr.Key) && attr.Key.ToLower().Contains(query)) return true;
            if (!string.IsNullOrEmpty(attr.Value) && attr.Value.ToLower().Contains(query)) return true;
        }

        return false;
    }

    /// <summary>
    /// Genişlik Öncelikli Arama (BFS - Breadth First Search) kullanarak arama yapar.
    /// Zaman Karmaşıklığı: O(N)
    /// </summary>
    public static CustomList<DomNode> SearchBFS(NaryTree tree, string query)
    {
        var result = new CustomList<DomNode>();
        if (tree.Root == null || string.IsNullOrWhiteSpace(query)) return result;

        var queue = new CustomQueue<DomNode>();
        queue.Enqueue(tree.Root);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (IsMatch(current, query))
            {
                result.Add(current);
            }

            foreach (var child in current.Children)
            {
                queue.Enqueue(child);
            }
        }

        return result;
    }

    /// <summary>
    /// Derinlik Öncelikli Arama (DFS - Depth First Search) kullanarak arama yapar.
    /// Zaman Karmaşıklığı: O(N)
    /// </summary>
    public static CustomList<DomNode> SearchDFS(NaryTree tree, string query)
    {
        var result = new CustomList<DomNode>();
        if (tree.Root == null || string.IsNullOrWhiteSpace(query)) return result;

        var stack = new CustomStack<DomNode>();
        stack.Push(tree.Root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            if (IsMatch(current, query))
            {
                result.Add(current);
            }

            // Stack'e eklerken sağdan sola eklersek, çıkarırken soldan sağa işlenmiş olur
            foreach (var child in current.Children)
            {
                stack.Push(child);
            }
        }

        return result;
    }
}
